<script setup lang="ts">
import { onBeforeUnmount, onMounted, ref } from "vue";
import { PocTilesScene } from "./PocTilesScene";
import type { PocLifecycleRecord, PocRuntimeDiagnostics } from "./pocDiagnostics";
import type { PocTilesState } from "./pocTilesRuntime";

const emit = defineEmits<{
  tilesState: [state: PocTilesState];
  glbStatus: [message: string];
  glbSelection: [name: string | undefined];
  diagnostics: [diagnostics: PocRuntimeDiagnostics];
  disposed: [record: Omit<PocLifecycleRecord, "round">];
}>();

const viewport = ref<HTMLElement>();
let pocScene: PocTilesScene | undefined;
let enteredAt = "";
let glbStatus = "等待独立 POC 场景初始化。";
let tilesStatus: PocTilesState["phase"] = "idle";

onMounted(async () => {
  if (!viewport.value) {
    return;
  }

  enteredAt = new Date().toISOString();
  pocScene = new PocTilesScene(viewport.value, {
    onTilesStateChange: (state) => {
      tilesStatus = state.phase;
      emit("tilesState", state);
    },
    onGlbStatusChange: (message) => {
      glbStatus = message;
      emit("glbStatus", message);
    },
    onGlbSelectionChange: (name) => emit("glbSelection", name),
    onDiagnosticsChange: (diagnostics) => emit("diagnostics", diagnostics),
  });
  await pocScene.init();
});

onBeforeUnmount(() => {
  const diagnostics = pocScene?.getDiagnostics();
  pocScene?.dispose();
  emit("disposed", {
    enteredAt,
    readyAt: diagnostics?.readyTime ?? undefined,
    exitedAt: new Date().toISOString(),
    canvasCount: diagnostics?.canvasCount,
    rendererCount: diagnostics?.rendererCount,
    animationLoopActive: diagnostics?.animationLoopActive,
    glbStatus,
    tilesStatus,
    released: true,
    errors: (diagnostics?.networkErrors.length ?? 0) + (diagnostics?.parseErrors.length ?? 0),
    memoryMb: diagnostics?.memoryMb ?? null,
  });
  pocScene = undefined;
});

defineExpose({
  reloadLocalFixture: () => pocScene?.reloadLocalFixture(),
  loadFailureFixture: () => pocScene?.loadFailureFixture(),
  loadChildFailureFixture: () => pocScene?.loadChildFailureFixture(),
  loadInvalidJsonFixture: () => pocScene?.loadInvalidJsonFixture(),
  loadSyntheticPocLifter: () => pocScene?.loadSyntheticPocLifter(),
  setSyntheticPlatformWorldZ: (targetZ: number) => pocScene?.setSyntheticPlatformWorldZ(targetZ),
});
</script>

<template>
  <div ref="viewport" class="poc-viewport" aria-label="POC 3D Tiles scene" />
</template>
