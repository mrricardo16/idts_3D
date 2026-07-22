<script setup lang="ts">
import { onBeforeUnmount, onMounted, ref } from "vue";
import { PocTilesScene, type PocPerformanceSnapshot } from "./PocTilesScene";
import type { PocCameraPresetName } from "./pocCameraPresets";
import type { PocLifecycleRecord, PocRuntimeDiagnostics } from "./pocDiagnostics";
import type { PocPerformanceScenarioConfig } from "./pocPerformanceScenario";
import type { PocCachePageState } from "./pocCacheRuntime";
import type { PocTilesState } from "./pocTilesRuntime";

declare global {
  interface Window {
    __idtsPocPerformanceProbe?: {
      getSnapshot: () => PocPerformanceSnapshot | undefined;
    };
  }
}

const props = defineProps<{
  performanceScenario?: PocPerformanceScenarioConfig;
}>();

const emit = defineEmits<{
  tilesState: [state: PocTilesState];
  glbStatus: [message: string];
  glbSelection: [name: string | undefined];
  cacheState: [state: PocCachePageState];
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
    onCacheStateChange: (state) => emit("cacheState", state),
    onDiagnosticsChange: (diagnostics) => emit("diagnostics", diagnostics),
  }, { performanceScenario: props.performanceScenario });
  if (new URLSearchParams(window.location.search).has("pocPerfProbe")) {
    window.__idtsPocPerformanceProbe = {
      getSnapshot: () => pocScene?.getPerformanceSnapshot(),
    };
  }
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
  delete window.__idtsPocPerformanceProbe;
  pocScene = undefined;
});

defineExpose({
  reloadLocalFixture: () => pocScene?.reloadLocalFixture(),
  loadFailureFixture: () => pocScene?.loadFailureFixture(),
  loadChildFailureFixture: () => pocScene?.loadChildFailureFixture(),
  loadInvalidJsonFixture: () => pocScene?.loadInvalidJsonFixture(),
  loadSyntheticPocLifter: () => pocScene?.loadSyntheticPocLifter(),
  setSyntheticPlatformWorldZ: (targetZ: number) => pocScene?.setSyntheticPlatformWorldZ(targetZ),
  applyCameraPreset: (name: PocCameraPresetName) => pocScene?.applyCameraPreset(name),
});
</script>

<template>
  <div ref="viewport" class="poc-viewport" aria-label="POC 3D Tiles scene" />
</template>
