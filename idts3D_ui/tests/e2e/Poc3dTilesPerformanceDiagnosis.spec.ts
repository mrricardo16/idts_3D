import { expect, test, type Browser, type BrowserContext, type CDPSession, type Page } from "@playwright/test";
import { mkdir, writeFile } from "node:fs/promises";
import { dirname, resolve } from "node:path";

type ServerKind = "vite-development" | "vite-preview";
type CacheMode = "cold" | "warm";
type ScenarioName = "factory-only" | "glb-only" | "hybrid" | "synthetic" | "hybrid-throttled";

interface NetworkRequestRecord {
  requestId: string;
  url: string;
  resourceType: string;
  status: number | null;
  cacheControl: string | null;
  etag: string | null;
  lastModified: string | null;
  contentLength: string | null;
  fromDiskCache: boolean;
  fromServiceWorker: boolean;
  servedFromCache: boolean;
  encodedDataLength: number | null;
  failure: string | null;
}

interface NetworkAudit {
  requests: NetworkRequestRecord[];
  totals: {
    encodedDataLength: number;
    responseCount: number;
    status304Count: number;
    memoryCacheHitCount: number;
    diskCacheHitCount: number;
    requestFailureCount: number;
  };
}

interface FpsStatistics {
  average: number;
  median: number;
  p5: number;
  minimum: number;
  maximum: number;
  standardDeviation: number;
  sampleCount: number;
  durationMs: number;
}

interface PerformanceProbeSnapshot {
  scenario: ScenarioName;
  diagnostics: {
    tilesPhase: string;
    loadedTiles: number | null;
    activeTiles: number | null;
    memoryMb: number | null;
    canvasCount: number;
    rendererCount: number;
  };
  renderer: {
    calls: number;
    triangles: number;
    points: number;
    lines: number;
    geometries: number;
    textures: number;
  };
  sceneObjects: number;
  glbMeshes: number;
  tileMeshes: number;
  materialCount: number;
  renderLoopStarts: number;
  controlsUpdates: number;
  diagnosticsPublishes: number;
}

interface SceneRun {
  server: ServerKind;
  scenario: ScenarioName;
  round: number;
  fps: FpsStatistics;
  probe: PerformanceProbeSnapshot;
  jsHeapMb: number | null;
  longTaskCount: number | null;
  longestLongTaskMs: number | null;
  consoleErrors: string[];
  pageErrors: string[];
}

interface CacheRun {
  server: ServerKind;
  cacheMode: CacheMode;
  round: number;
  contextId: string;
  browserReusedForWarmCache: boolean;
  cacheCleared: boolean;
  network: NetworkAudit;
}

interface InterferenceRun {
  server: ServerKind;
  measurement: "trace-and-snapshots" | "request-interception";
  baseline?: FpsStatistics;
  measured?: FpsStatistics;
  cacheWithoutInterception?: NetworkAudit;
  cacheWithInterception?: NetworkAudit;
  notes: string[];
}

interface EvidencePayload {
  schemaVersion: 1;
  generatedAt: string;
  browser: {
    project: string;
    headed: true;
    viewport: { width: number; height: number };
    deviceScaleFactor: number;
    userAgent: string;
    webglRenderer: string | null;
  };
  methodology: {
    fpsWarmupMs: number;
    fpsDurationMs: number;
    roundsPerScenario: number;
    officialSamplingTrace: "off";
    officialSamplingScreenshot: "off";
    officialSamplingVideo: "off";
    transferMetric: "Network.loadingFinished.encodedDataLength";
  };
  cacheRuns: CacheRun[];
  sceneRuns: SceneRun[];
  interferenceRuns: InterferenceRun[];
  runtimeAudit: Array<{
    server: ServerKind;
    snapshot: PerformanceProbeSnapshot;
    canvasCount: number;
    notes: string[];
  }>;
}

const SERVER_URLS: Record<ServerKind, string> = {
  "vite-development": "http://127.0.0.1:5182",
  "vite-preview": "http://127.0.0.1:5183",
};

const SCENARIOS: ScenarioName[] = [
  "factory-only",
  "glb-only",
  "hybrid",
  "synthetic",
  "hybrid-throttled",
];

const EVIDENCE_PATH = resolve(
  process.cwd(),
  "../idts3D_docs/poc/evidence/POC-3DT-01/automated-chrome-performance-diagnosis.json",
);

function isPocResource(url: string): boolean {
  const path = new URL(url).pathname;
  return path.includes("/poc-3dtiles/")
    || path === "/models/lifter.glb"
    || path === "/models/lifter.json"
    || path === "/models/manifest.json";
}

function lowerCaseHeaders(headers: Record<string, string>): Record<string, string> {
  return Object.fromEntries(Object.entries(headers).map(([key, value]) => [key.toLowerCase(), value]));
}

class CdpNetworkRecorder {
  private readonly requests = new Map<string, NetworkRequestRecord>();

  constructor(session: CDPSession) {
    session.on("Network.requestWillBeSent", (event) => {
      if (!isPocResource(event.request.url)) {
        return;
      }
      this.requests.set(event.requestId, {
        requestId: event.requestId,
        url: event.request.url,
        resourceType: event.type,
        status: null,
        cacheControl: null,
        etag: null,
        lastModified: null,
        contentLength: null,
        fromDiskCache: false,
        fromServiceWorker: false,
        servedFromCache: false,
        encodedDataLength: null,
        failure: null,
      });
    });
    session.on("Network.responseReceived", (event) => {
      const request = this.requests.get(event.requestId);
      if (!request) {
        return;
      }
      const headers = lowerCaseHeaders(event.response.headers as Record<string, string>);
      request.status = event.response.status;
      request.cacheControl = headers["cache-control"] ?? null;
      request.etag = headers.etag ?? null;
      request.lastModified = headers["last-modified"] ?? null;
      request.contentLength = headers["content-length"] ?? null;
      request.fromDiskCache = Boolean(event.response.fromDiskCache);
      request.fromServiceWorker = Boolean(event.response.fromServiceWorker);
    });
    session.on("Network.requestServedFromCache", (event) => {
      const request = this.requests.get(event.requestId);
      if (request) {
        request.servedFromCache = true;
      }
    });
    session.on("Network.loadingFinished", (event) => {
      const request = this.requests.get(event.requestId);
      if (request) {
        request.encodedDataLength = event.encodedDataLength;
      }
    });
    session.on("Network.loadingFailed", (event) => {
      const request = this.requests.get(event.requestId);
      if (request) {
        request.failure = event.errorText;
      }
    });
  }

  reset(): void {
    this.requests.clear();
  }

  snapshot(): NetworkAudit {
    const requests = [...this.requests.values()].sort((left, right) => left.url.localeCompare(right.url));
    return {
      requests,
      totals: {
        encodedDataLength: requests.reduce((total, request) => total + (request.encodedDataLength ?? 0), 0),
        responseCount: requests.filter((request) => request.status !== null).length,
        status304Count: requests.filter((request) => request.status === 304).length,
        memoryCacheHitCount: requests.filter((request) => request.servedFromCache && !request.fromDiskCache).length,
        diskCacheHitCount: requests.filter((request) => request.fromDiskCache).length,
        requestFailureCount: requests.filter((request) => request.failure !== null).length,
      },
    };
  }
}

function percentile(values: number[], fraction: number): number {
  const sorted = [...values].sort((left, right) => left - right);
  const position = (sorted.length - 1) * fraction;
  const lower = Math.floor(position);
  const upper = Math.ceil(position);
  return sorted[lower] + ((sorted[upper] - sorted[lower]) * (position - lower));
}

function buildFpsStatistics(frameIntervals: number[], durationMs: number): FpsStatistics {
  const samples = frameIntervals.filter((interval) => interval > 0).map((interval) => 1000 / interval);
  const average = samples.reduce((total, sample) => total + sample, 0) / samples.length;
  const standardDeviation = Math.sqrt(samples.reduce((total, sample) => total + ((sample - average) ** 2), 0) / samples.length);
  return {
    average,
    median: percentile(samples, 0.5),
    p5: percentile(samples, 0.05),
    minimum: Math.min(...samples),
    maximum: Math.max(...samples),
    standardDeviation,
    sampleCount: samples.length,
    durationMs,
  };
}

async function startLongTaskObserver(page: Page): Promise<void> {
  await page.evaluate(() => {
    const performanceWithLongTasks = performance as Performance & { __pocLongTasks?: number[] };
    performanceWithLongTasks.__pocLongTasks = [];
    if (typeof PerformanceObserver === "undefined") {
      return;
    }
    try {
      const observer = new PerformanceObserver((entries) => {
        performanceWithLongTasks.__pocLongTasks?.push(...entries.getEntries().map((entry) => entry.duration));
      });
      observer.observe({ type: "longtask", buffered: true });
    } catch {
      // The Long Tasks API is not available in every browser build.
    }
  });
}

async function sampleFps(page: Page): Promise<FpsStatistics> {
  await page.waitForTimeout(5_000);
  const frameIntervals = await page.evaluate(async () => new Promise<number[]>((resolveFrames) => {
    const intervals: number[] = [];
    const startedAt = performance.now();
    let previous = startedAt;
    const collect = (now: number): void => {
      intervals.push(now - previous);
      previous = now;
      if (now - startedAt >= 30_000) {
        resolveFrames(intervals);
        return;
      }
      requestAnimationFrame(collect);
    };
    requestAnimationFrame(collect);
  }));
  return buildFpsStatistics(frameIntervals, 30_000);
}

async function getSnapshot(page: Page): Promise<PerformanceProbeSnapshot> {
  return page.evaluate(() => {
    const probe = window.__idtsPocPerformanceProbe;
    if (!probe) {
      throw new Error("POC performance probe was not registered.");
    }
    return probe.getSnapshot();
  }) as Promise<PerformanceProbeSnapshot>;
}

async function getHeapAndLongTasks(page: Page): Promise<{ jsHeapMb: number | null; longTaskCount: number | null; longestLongTaskMs: number | null }> {
  return page.evaluate(() => {
    const performanceWithMemory = performance as Performance & {
      memory?: { usedJSHeapSize?: number };
      __pocLongTasks?: number[];
    };
    const usedHeap = performanceWithMemory.memory?.usedJSHeapSize;
    const longTasks = performanceWithMemory.__pocLongTasks;
    return {
      jsHeapMb: typeof usedHeap === "number" ? usedHeap / (1024 * 1024) : null,
      longTaskCount: longTasks ? longTasks.length : null,
      longestLongTaskMs: longTasks?.length ? Math.max(...longTasks) : null,
    };
  });
}

async function navigateToScenario(page: Page, server: ServerKind, scenario: ScenarioName): Promise<void> {
  await page.goto(`${SERVER_URLS[server]}/poc-3dtiles.html?pocPerfScenario=${scenario}&pocPerfProbe=1`, {
    waitUntil: "domcontentloaded",
  });
  await expect(page.locator(".poc-viewport canvas")).toHaveCount(1);
  if (scenario !== "glb-only") {
    await expect(page.locator(".poc-state")).toHaveText(/ready/, { timeout: 90_000 });
  }
  if (scenario === "factory-only") {
    await expect(page.getByText("GLB 已按 POC 性能场景禁用。")).toBeVisible();
  } else if (scenario === "synthetic") {
    await expect(page.getByText("已加载合成 POC worldZ 夹具")).toBeVisible();
  } else {
    await expect(page.getByText("GLB 已加载：/models/lifter.glb")).toBeVisible({ timeout: 90_000 });
  }
  await expect.poll(() => page.evaluate(() => Boolean(window.__idtsPocPerformanceProbe))).toBe(true);
}

async function newPerformanceContext(browser: Browser): Promise<BrowserContext> {
  return browser.newContext({
    viewport: { width: 1920, height: 1080 },
    deviceScaleFactor: 1,
  });
}

async function captureSceneRun(browser: Browser, server: ServerKind, scenario: ScenarioName, round: number): Promise<SceneRun> {
  const context = await newPerformanceContext(browser);
  const page = await context.newPage();
  const consoleErrors: string[] = [];
  const pageErrors: string[] = [];
  page.on("console", (message) => {
    if (message.type() === "error" && !message.location().url.endsWith("/favicon.ico")) {
      consoleErrors.push(message.text());
    }
  });
  page.on("pageerror", (error) => pageErrors.push(error.message));
  try {
    await navigateToScenario(page, server, scenario);
    await startLongTaskObserver(page);
    const fps = await sampleFps(page);
    const probe = await getSnapshot(page);
    const heapAndLongTasks = await getHeapAndLongTasks(page);
    return {
      server,
      scenario,
      round,
      fps,
      probe,
      ...heapAndLongTasks,
      consoleErrors,
      pageErrors,
    };
  } finally {
    await context.close();
  }
}

async function captureCacheRun(
  browser: Browser,
  server: ServerKind,
  cacheMode: CacheMode,
  round: number,
  context?: BrowserContext,
): Promise<CacheRun> {
  const targetContext = context ?? await newPerformanceContext(browser);
  const ownsContext = !context;
  const page = await targetContext.newPage();
  const session = await targetContext.newCDPSession(page);
  await session.send("Network.enable");
  if (cacheMode === "cold") {
    await session.send("Network.setCacheDisabled", { cacheDisabled: true });
  }
  const recorder = new CdpNetworkRecorder(session);
  try {
    await navigateToScenario(page, server, "hybrid");
    return {
      server,
      cacheMode,
      round,
      contextId: `${server}-${cacheMode}-${round}`,
      browserReusedForWarmCache: cacheMode === "warm",
      cacheCleared: false,
      network: recorder.snapshot(),
    };
  } finally {
    await session.detach();
    await page.close();
    if (ownsContext) {
      await targetContext.close();
    }
  }
}

async function captureWarmCacheRuns(browser: Browser, server: ServerKind): Promise<CacheRun[]> {
  const context = await newPerformanceContext(browser);
  const page = await context.newPage();
  const session = await context.newCDPSession(page);
  await session.send("Network.enable");
  const recorder = new CdpNetworkRecorder(session);
  try {
    await navigateToScenario(page, server, "hybrid");
    const results: CacheRun[] = [];
    for (let round = 1; round <= 3; round += 1) {
      recorder.reset();
      await page.reload({ waitUntil: "domcontentloaded" });
      await expect(page.locator(".poc-state")).toHaveText(/ready/, { timeout: 90_000 });
      await expect(page.getByText("GLB 已加载：/models/lifter.glb")).toBeVisible({ timeout: 90_000 });
      results.push({
        server,
        cacheMode: "warm",
        round,
        contextId: `${server}-warm-shared-context`,
        browserReusedForWarmCache: true,
        cacheCleared: false,
        network: recorder.snapshot(),
      });
    }
    return results;
  } finally {
    await session.detach();
    await context.close();
  }
}

async function captureTraceInterference(browser: Browser, server: ServerKind): Promise<InterferenceRun> {
  const baselineRun = await captureSceneRun(browser, server, "hybrid", 0);
  const context = await newPerformanceContext(browser);
  const page = await context.newPage();
  const tracePath = resolve(
    dirname(EVIDENCE_PATH),
    "performance-diagnosis",
    `${server}-trace-and-snapshots.zip`,
  );
  try {
    await mkdir(dirname(tracePath), { recursive: true });
    await context.tracing.start({ screenshots: true, snapshots: true, sources: false });
    await navigateToScenario(page, server, "hybrid");
    await startLongTaskObserver(page);
    const measured = await sampleFps(page);
    return {
      server,
      measurement: "trace-and-snapshots",
      baseline: baselineRun.fps,
      measured,
      notes: [
        "The traced run is an interference audit only and is excluded from the five-scenario official FPS matrix.",
        "Trace was enabled with screenshots and DOM snapshots, matching the former baseline test's interference source.",
      ],
    };
  } finally {
    await context.tracing.stop({ path: tracePath });
    await context.close();
  }
}

async function captureInterceptionInterference(browser: Browser, server: ServerKind): Promise<InterferenceRun> {
  const context = await newPerformanceContext(browser);
  const page = await context.newPage();
  const session = await context.newCDPSession(page);
  await session.send("Network.enable");
  const recorder = new CdpNetworkRecorder(session);
  try {
    await navigateToScenario(page, server, "hybrid");
    recorder.reset();
    await page.reload({ waitUntil: "domcontentloaded" });
    await expect(page.locator(".poc-state")).toHaveText(/ready/, { timeout: 90_000 });
    const cacheWithoutInterception = recorder.snapshot();
    recorder.reset();
    await context.route("**/*", (route) => route.continue());
    await page.reload({ waitUntil: "domcontentloaded" });
    await expect(page.locator(".poc-state")).toHaveText(/ready/, { timeout: 90_000 });
    const cacheWithInterception = recorder.snapshot();
    await context.unroute("**/*");
    return {
      server,
      measurement: "request-interception",
      cacheWithoutInterception,
      cacheWithInterception,
      notes: [
        "Request interception was installed only for this cache-behaviour audit.",
        "The official cold/warm cache measurements and official FPS matrix have no request routing.",
      ],
    };
  } finally {
    await session.detach();
    await context.close();
  }
}

async function captureRuntimeAudit(browser: Browser, server: ServerKind): Promise<EvidencePayload["runtimeAudit"][number]> {
  const context = await newPerformanceContext(browser);
  const page = await context.newPage();
  try {
    await navigateToScenario(page, server, "hybrid");
    const snapshot = await getSnapshot(page);
    return {
      server,
      snapshot,
      canvasCount: await page.locator(".poc-viewport canvas").count(),
      notes: [
        "One PocTilesScene owns one renderer, one canvas, and one requestAnimationFrame render loop.",
        "controlsUpdates is expected to advance once per render frame; renderLoopStarts is expected to be exactly one.",
      ],
    };
  } finally {
    await context.close();
  }
}

test("records CDP cache evidence and a headed-Chrome POC performance diagnosis", async ({ browser }, testInfo) => {
  test.setTimeout(60 * 60_000);
  const probeContext = await newPerformanceContext(browser);
  const probePage = await probeContext.newPage();
  const browserDetails = await probePage.evaluate(() => {
    const canvas = document.createElement("canvas");
    const context = canvas.getContext("webgl");
    const extension = context?.getExtension("WEBGL_debug_renderer_info");
    return {
      userAgent: navigator.userAgent,
      webglRenderer: extension ? context?.getParameter(extension.UNMASKED_RENDERER_WEBGL) ?? null : null,
    };
  });
  await probeContext.close();

  const evidence: EvidencePayload = {
    schemaVersion: 1,
    generatedAt: new Date().toISOString(),
    browser: {
      project: testInfo.project.name,
      headed: true,
      viewport: { width: 1920, height: 1080 },
      deviceScaleFactor: 1,
      ...browserDetails,
    },
    methodology: {
      fpsWarmupMs: 5_000,
      fpsDurationMs: 30_000,
      roundsPerScenario: 3,
      officialSamplingTrace: "off",
      officialSamplingScreenshot: "off",
      officialSamplingVideo: "off",
      transferMetric: "Network.loadingFinished.encodedDataLength",
    },
    cacheRuns: [],
    sceneRuns: [],
    interferenceRuns: [],
    runtimeAudit: [],
  };

  for (const server of Object.keys(SERVER_URLS) as ServerKind[]) {
    for (let round = 1; round <= 3; round += 1) {
      evidence.cacheRuns.push(await captureCacheRun(browser, server, "cold", round));
    }
    evidence.cacheRuns.push(...await captureWarmCacheRuns(browser, server));

    for (const scenario of SCENARIOS) {
      for (let round = 1; round <= 3; round += 1) {
        evidence.sceneRuns.push(await captureSceneRun(browser, server, scenario, round));
      }
    }

    evidence.interferenceRuns.push(await captureTraceInterference(browser, server));
    evidence.interferenceRuns.push(await captureInterceptionInterference(browser, server));
    evidence.runtimeAudit.push(await captureRuntimeAudit(browser, server));
  }

  await mkdir(dirname(EVIDENCE_PATH), { recursive: true });
  await writeFile(EVIDENCE_PATH, JSON.stringify(evidence, null, 2), "utf8");
  expect(evidence.cacheRuns).toHaveLength(12);
  expect(evidence.sceneRuns).toHaveLength(30);
  expect(evidence.sceneRuns.every((run) => run.consoleErrors.length === 0 && run.pageErrors.length === 0)).toBe(true);
  expect(evidence.sceneRuns.every((run) => run.fps.sampleCount > 1_000)).toBe(true);
});
