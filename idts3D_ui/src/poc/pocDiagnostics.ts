import type { PocTilesPhase } from "./pocTilesRuntime";
import type { PocGlbNodeRecord } from "./pocGlbNodes";

export interface PocLifecycleRecord {
  round: number;
  enteredAt: string;
  readyAt?: string;
  exitedAt?: string;
  canvasCount?: number;
  rendererCount?: number;
  animationLoopActive?: boolean;
  glbStatus?: string;
  tilesStatus?: PocTilesPhase;
  released?: boolean;
  errors?: number;
  memoryMb?: number | null;
}

export interface PocRuntimeDiagnostics {
  browser: string;
  userAgent: string;
  webglAvailable: boolean;
  tilesPhase: PocTilesPhase;
  tilesetUrl: string;
  glbUrl: string;
  canvasCount: number;
  rendererCount: number;
  animationLoopActive: boolean;
  fps: number | null;
  loadedTiles: number | null;
  activeTiles: number | null;
  selectedObject: string | null;
  worldZ: number | null;
  networkErrors: string[];
  parseErrors: string[];
  lifecycle: PocLifecycleRecord[];
  loadStartTime: string | null;
  firstVisibleTime: string | null;
  readyTime: string | null;
  loadDurationMs: number | null;
  drawCalls: number | null;
  memoryMb: number | null;
  glbNodes: PocGlbNodeRecord[];
}

export function createPocRuntimeDiagnostics(
  values: Partial<PocRuntimeDiagnostics> = {},
): PocRuntimeDiagnostics {
  return {
    browser: values.browser ?? "unknown",
    userAgent: values.userAgent ?? "unknown",
    webglAvailable: values.webglAvailable ?? false,
    tilesPhase: values.tilesPhase ?? "idle",
    tilesetUrl: values.tilesetUrl ?? "",
    glbUrl: values.glbUrl ?? "",
    canvasCount: values.canvasCount ?? 0,
    rendererCount: values.rendererCount ?? 0,
    animationLoopActive: values.animationLoopActive ?? false,
    fps: values.fps ?? null,
    loadedTiles: values.loadedTiles ?? null,
    activeTiles: values.activeTiles ?? null,
    selectedObject: values.selectedObject ?? null,
    worldZ: values.worldZ ?? null,
    networkErrors: values.networkErrors ?? [],
    parseErrors: values.parseErrors ?? [],
    lifecycle: values.lifecycle ?? [],
    loadStartTime: values.loadStartTime ?? null,
    firstVisibleTime: values.firstVisibleTime ?? null,
    readyTime: values.readyTime ?? null,
    loadDurationMs: values.loadDurationMs ?? null,
    drawCalls: values.drawCalls ?? null,
    memoryMb: values.memoryMb ?? null,
    glbNodes: values.glbNodes ?? [],
  };
}

export function createPocEvidencePayload(diagnostics: PocRuntimeDiagnostics): Record<string, unknown> {
  return {
    browser: diagnostics.browser,
    userAgent: diagnostics.userAgent,
    webglAvailable: diagnostics.webglAvailable,
    canvasCount: diagnostics.canvasCount,
    rendererCount: diagnostics.rendererCount,
    tilesetUrl: diagnostics.tilesetUrl,
    glbUrl: diagnostics.glbUrl,
    loadStartTime: diagnostics.loadStartTime,
    firstVisibleTime: diagnostics.firstVisibleTime,
    readyTime: diagnostics.readyTime,
    loadDurationMs: diagnostics.loadDurationMs,
    selectedObject: diagnostics.selectedObject,
    worldZ: {
      low: diagnostics.worldZ === 0 ? 0 : null,
      middle: diagnostics.worldZ === 6 ? 6 : null,
      high: diagnostics.worldZ === 12 ? 12 : null,
    },
    networkErrors: diagnostics.networkErrors,
    parseErrors: diagnostics.parseErrors,
    lifecycle: diagnostics.lifecycle,
    performance: {
      fps: diagnostics.fps,
      drawCalls: diagnostics.drawCalls,
      memoryMb: diagnostics.memoryMb,
      loadedTiles: diagnostics.loadedTiles,
      activeTiles: diagnostics.activeTiles,
    },
    glbNodes: diagnostics.glbNodes,
    result: diagnostics.tilesPhase === "ready" ? "runtime-ready" : "not-ready",
  };
}
