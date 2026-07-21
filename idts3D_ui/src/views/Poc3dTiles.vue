<script setup lang="ts">
import { nextTick, ref } from "vue";
import PocTilesViewport from "../poc/PocTilesViewport.vue";
import {
  createPocEvidencePayload,
  createPocRuntimeDiagnostics,
  type PocLifecycleRecord,
  type PocRuntimeDiagnostics,
} from "../poc/pocDiagnostics";
import type { PocTilesState } from "../poc/pocTilesRuntime";
import { createPocReadyGate } from "../poc/pocLifecycle";

const sceneHost = ref<InstanceType<typeof PocTilesViewport>>();
const sceneMounted = ref(true);
const tilesState = ref<PocTilesState>({ phase: "idle" });
const glbStatus = ref("等待独立 POC 场景初始化。");
const selectedGlbObject = ref<string>();
const diagnostics = ref<PocRuntimeDiagnostics>(createPocRuntimeDiagnostics());
const lifecycle = ref<PocLifecycleRecord[]>([]);
const lifecycleBusy = ref(false);
const lifecycleError = ref<string>();
const readyGate = createPocReadyGate();

function onTilesStateChange(state: PocTilesState): void {
  tilesState.value = state;
  readyGate.notify(state.phase);
}

function onDiagnosticsChange(next: PocRuntimeDiagnostics): void {
  diagnostics.value = { ...next, lifecycle: [...lifecycle.value] };
}

function onDisposed(record: Omit<PocLifecycleRecord, "round">): void {
  lifecycle.value = [
    ...lifecycle.value,
    { round: lifecycle.value.length + 1, ...record },
  ];
  diagnostics.value = { ...diagnostics.value, lifecycle: [...lifecycle.value] };
}

async function runNextLifecycleRound(): Promise<boolean> {
  if (lifecycleBusy.value) {
    return false;
  }

  lifecycleBusy.value = true;
  lifecycleError.value = undefined;
  readyGate.reset();
  sceneMounted.value = false;
  await nextTick();
  await new Promise((resolve) => window.setTimeout(resolve, 250));
  sceneMounted.value = true;
  await nextTick();
  const reachedReady = await readyGate.waitForReady(30_000);
  if (!reachedReady) {
    lifecycleError.value = "本轮未在 30 秒内达到 Tiles: ready，已停止后续生命周期轮次。";
  }
  lifecycleBusy.value = false;
  return reachedReady;
}

async function runTenLifecycleRounds(): Promise<void> {
  for (let index = 0; index < 10; index += 1) {
    const reachedReady = await runNextLifecycleRound();
    if (!reachedReady) {
      return;
    }
    await new Promise((resolve) => window.setTimeout(resolve, 250));
  }
}

function setSyntheticWorldZ(target: number): void {
  sceneHost.value?.setSyntheticPlatformWorldZ(target);
}

function exportEvidence(): void {
  const payload = createPocEvidencePayload({
    ...diagnostics.value,
    lifecycle: [...lifecycle.value],
  });
  const blob = new Blob([JSON.stringify(payload, null, 2)], { type: "application/json" });
  const link = document.createElement("a");
  const stamp = new Date().toISOString().replace(/[-:]/g, "").replace(/\.\d{3}Z$/, "");
  link.href = URL.createObjectURL(blob);
  link.download = `POC-3DT-01-evidence-${stamp}.json`;
  link.click();
  URL.revokeObjectURL(link.href);
}
</script>

<template>
  <main class="poc-shell">
    <header class="poc-header">
      <div>
        <p>POC-3DT-01 · 非生产独立入口</p>
        <h1>3D Tiles + GLB 最小技术验证</h1>
      </div>
      <span :class="['poc-state', tilesState.phase]">Tiles：{{ tilesState.phase }}</span>
    </header>

    <section class="poc-workbench">
      <PocTilesViewport
        v-if="sceneMounted"
        ref="sceneHost"
        @tiles-state="onTilesStateChange"
        @glb-status="glbStatus = $event"
        @glb-selection="selectedGlbObject = $event"
        @diagnostics="onDiagnosticsChange"
        @disposed="onDisposed"
      />

      <aside class="poc-panel">
        <p>{{ glbStatus }}</p>
        <p>GLB 拾取：{{ selectedGlbObject ?? "未选择" }}</p>
        <p v-if="tilesState.error" class="poc-error">{{ tilesState.error }}</p>
        <button type="button" @click="sceneHost?.reloadLocalFixture()">加载本地最小 Tileset</button>
        <button type="button" @click="sceneHost?.loadFailureFixture()">注入严格 HTTP 404</button>
        <button type="button" @click="sceneHost?.loadChildFailureFixture()">注入子资源失败</button>
        <button type="button" @click="sceneHost?.loadInvalidJsonFixture()">注入 Tileset JSON 解析错误</button>
        <button type="button" @click="sceneHost?.loadSyntheticPocLifter()">加载合成 POC worldZ 夹具</button>
        <div class="poc-button-row" aria-label="合成夹具 worldZ 控制">
          <button type="button" @click="setSyntheticWorldZ(0)">worldZ 低位 0</button>
          <button type="button" @click="setSyntheticWorldZ(6)">worldZ 中位 6</button>
          <button type="button" @click="setSyntheticWorldZ(12)">worldZ 高位 12</button>
        </div>
        <button type="button" :disabled="lifecycleBusy" @click="runNextLifecycleRound">
          执行下一轮真实生命周期
        </button>
        <button type="button" :disabled="lifecycleBusy" @click="runTenLifecycleRounds">
          连续执行 10 轮真实生命周期
        </button>
        <button type="button" @click="exportEvidence">导出当前 POC 证据 JSON</button>
        <p v-if="lifecycleError" class="poc-error">{{ lifecycleError }}</p>
        <p class="poc-note">
          Tiles 是静态厂房基座，GLB 与 Tiles 使用同一 Canvas、同一相机与控制器，Tiles 不参与 GLB 拾取。真实 GLB 无经验证的内部升降台节点；worldZ 按钮仅用于明确标注的合成 POC 夹具。
        </p>
      </aside>
    </section>

    <section class="poc-diagnostics" aria-label="POC 运行诊断">
      <h2>POC 运行诊断</h2>
      <dl>
        <div><dt>Tiles / 地址</dt><dd>{{ diagnostics.tilesPhase }} · {{ diagnostics.tilesetUrl || "未加载" }}</dd></div>
        <div><dt>GLB / Canvas / Renderer</dt><dd>{{ diagnostics.glbUrl || "未加载" }} · {{ diagnostics.canvasCount }} / {{ diagnostics.rendererCount }}</dd></div>
        <div><dt>动画 / FPS / Draw calls</dt><dd>{{ diagnostics.animationLoopActive ? "运行" : "停止" }} · {{ diagnostics.fps?.toFixed(1) ?? "未测量" }} · {{ diagnostics.drawCalls ?? "未测量" }}</dd></div>
        <div><dt>瓦片（已加载 / 活动）</dt><dd>{{ diagnostics.loadedTiles ?? "未测量" }} / {{ diagnostics.activeTiles ?? "未测量" }}</dd></div>
        <div><dt>选中对象 / worldZ</dt><dd>{{ diagnostics.selectedObject ?? "未选择" }} / {{ diagnostics.worldZ ?? "未测量" }}</dd></div>
        <div><dt>GLB 节点 / 明确可动节点</dt><dd>{{ diagnostics.glbNodes.length }} / {{ diagnostics.glbNodes.filter((node) => node.isExplicitMovablePart).map((node) => node.name).join(", ") || "无" }}</dd></div>
        <div><dt>网络 / 解析错误</dt><dd>{{ diagnostics.networkErrors.length }} / {{ diagnostics.parseErrors.length }}</dd></div>
        <div><dt>生命周期 / 最近 ready / 加载耗时</dt><dd>{{ lifecycle.length }} / {{ diagnostics.readyTime ?? "未就绪" }} / {{ diagnostics.loadDurationMs?.toFixed(1) ?? "未测量" }} ms</dd></div>
      </dl>
    </section>
  </main>
</template>
