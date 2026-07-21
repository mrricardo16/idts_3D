import { expect, test, type Browser, type BrowserContext, type Page } from "@playwright/test";
import { writeFile } from "node:fs/promises";

interface PocPerformanceSample {
  cacheMode: "cold" | "warm";
  round: number;
  navigationReadyMs: number;
  firstVisibleAndReadyMs: number | null;
  requestCount: number;
  transferBytes: number;
  encodedBodyBytes: number;
  fps: number | null;
  memoryMb: number | null;
  drawCalls: number | null;
  activeTiles: number | null;
  consoleErrors: string[];
  ignoredConsoleErrors: string[];
  pageErrors: string[];
  networkEntries: Array<{ name: string; transferBytes: number; encodedBodyBytes: number }>;
}

interface PocPerformanceEvidence {
  browser: string;
  generatedAt: string;
  samples: PocPerformanceSample[];
}

async function openReadyPoc(page: Page): Promise<void> {
  const consoleErrors: string[] = [];
  page.on("console", (message) => {
    if (message.type() === "error") {
      consoleErrors.push(message.text());
    }
  });
  await page.goto("/poc-3dtiles.html");
  try {
    await expect(page.locator(".poc-state")).toHaveText("Tiles：ready", { timeout: 90_000 });
  } catch (error) {
    console.log(JSON.stringify({
      state: await page.locator(".poc-state").textContent(),
      diagnostics: await page.getByLabel("POC 运行诊断").textContent(),
      viewport: await page.locator(".poc-viewport").evaluate((element) => ({
        width: element.clientWidth,
        height: element.clientHeight,
      })),
      webglVendor: await page.evaluate(() => {
        const context = document.createElement("canvas").getContext("webgl");
        const extension = context?.getExtension("WEBGL_debug_renderer_info");
        return extension ? context?.getParameter(extension.UNMASKED_RENDERER_WEBGL) : null;
      }),
      resources: await page.evaluate(() => performance.getEntriesByType("resource")
        .map((entry) => entry.name)
        .filter((name) => name.includes("poc-3dtiles") || name.includes("lifter.glb"))),
      consoleErrors,
    }));
    throw error;
  }
  await expect(page.locator(".poc-viewport canvas")).toHaveCount(1);
  await expect(page.getByText("GLB 已加载：/models/lifter.glb")).toBeVisible();
}

async function capturePerformanceSample(
  page: Page,
  cacheMode: PocPerformanceSample["cacheMode"],
  round: number,
  navigate: () => Promise<unknown>,
): Promise<PocPerformanceSample> {
  const consoleErrors: string[] = [];
  const ignoredConsoleErrors: string[] = [];
  const pageErrors: string[] = [];
  page.on("console", (message) => {
    if (message.type() === "error") {
      const location = message.location().url;
      const formatted = location ? `${message.text()} (${location})` : message.text();
      if (location.endsWith("/favicon.ico")) {
        ignoredConsoleErrors.push(formatted);
      } else {
        consoleErrors.push(formatted);
      }
    }
  });
  page.on("pageerror", (error) => pageErrors.push(error.message));

  const startedAt = performance.now();
  await navigate();
  await expect(page.locator(".poc-state")).toHaveText("Tiles：ready", { timeout: 90_000 });
  await expect(page.getByText("GLB 已加载：/models/lifter.glb")).toBeVisible();
  await page.waitForTimeout(1_100);

  const diagnostics = await page.getByLabel("POC 运行诊断").locator("dl > div").evaluateAll((rows) =>
    Object.fromEntries(rows.map((row) => [
      row.querySelector("dt")?.textContent?.trim() ?? "",
      row.querySelector("dd")?.textContent?.trim() ?? "",
    ])),
  ) as Record<string, string>;
  const networkEntries = await page.evaluate(() => performance.getEntriesByType("resource")
    .map((entry) => entry as PerformanceResourceTiming)
    .filter((entry) =>
      entry.name.includes("/poc-3dtiles/") ||
      entry.name.includes("/models/lifter") ||
      entry.name.includes("/model-configs/lifter.json") ||
      entry.name.includes("/models/manifest.json"),
    )
    .map((entry) => ({
      name: new URL(entry.name).pathname,
      transferBytes: entry.transferSize,
      encodedBodyBytes: entry.encodedBodySize,
    })));
  const memoryMb = await page.evaluate(() => {
    const performanceWithMemory = performance as Performance & {
      memory?: { usedJSHeapSize?: number };
    };
    const usedJSHeapSize = performanceWithMemory.memory?.usedJSHeapSize;
    return typeof usedJSHeapSize === "number" ? usedJSHeapSize / (1024 * 1024) : null;
  });
  const parseMetric = (source: string | undefined, pattern: RegExp): number | null => {
    const match = source?.match(pattern);
    return match ? Number(match[1]) : null;
  };
  const fpsAndDrawCalls = diagnostics["动画 / FPS / Draw calls"]?.split(" · ") ?? [];
  const loadedAndActiveTiles = diagnostics["瓦片（已加载 / 活动）"]?.split(" / ") ?? [];

  return {
    cacheMode,
    round,
    navigationReadyMs: performance.now() - startedAt,
    firstVisibleAndReadyMs: parseMetric(
      diagnostics["生命周期 / 最近 ready / 加载耗时"],
      /\/ ([0-9.]+) ms$/,
    ),
    requestCount: networkEntries.length,
    transferBytes: networkEntries.reduce((total, entry) => total + entry.transferBytes, 0),
    encodedBodyBytes: networkEntries.reduce((total, entry) => total + entry.encodedBodyBytes, 0),
    fps: parseMetric(fpsAndDrawCalls[1], /^([0-9.]+)$/),
    memoryMb,
    drawCalls: parseMetric(fpsAndDrawCalls[2], /^([0-9.]+)$/),
    activeTiles: parseMetric(loadedAndActiveTiles[1], /^([0-9.]+)$/),
    consoleErrors,
    ignoredConsoleErrors,
    pageErrors,
    networkEntries,
  };
}

async function createColdCacheContext(browser: Browser): Promise<BrowserContext> {
  return browser.newContext({ viewport: { width: 1440, height: 1000 } });
}

test("provides deterministic POC camera presets in local Chrome", async ({ page }) => {
  await page.goto("/poc-3dtiles.html");

  await expect(page.getByRole("button", { name: "厂房外观视角" })).toBeVisible({ timeout: 2_000 });
  await expect(page.getByRole("button", { name: "高门进入视角" })).toBeVisible({ timeout: 2_000 });
  await expect(page.getByRole("button", { name: "厂房内部视角" })).toBeVisible({ timeout: 2_000 });
});

test("starts from the deterministic factory exterior camera preset", async ({ page }) => {
  await page.goto("/poc-3dtiles.html");

  await expect(page.getByText("相机预设：厂房外观（初始）")).toBeVisible({ timeout: 2_000 });
});

test("reports the deterministic camera preset selected for browser evidence", async ({ page }) => {
  await page.goto("/poc-3dtiles.html");

  await page.getByRole("button", { name: "厂房内部视角" }).click();
  await expect(page.getByText("相机预设：厂房内部")).toBeVisible({ timeout: 2_000 });
});

test("loads the local factory and real GLB into one local Chrome canvas", async ({ page }) => {
  await openReadyPoc(page);

  await expect(page.getByLabel("POC 运行诊断")).toContainText("/models/lifter.glb · 1 / 1");
  await expect(page.getByLabel("POC 运行诊断")).toContainText("1 / 1");
});

test("isolates strict root 404 and recovers without adding a canvas", async ({ page }) => {
  await openReadyPoc(page);

  const [response] = await Promise.all([
    page.waitForResponse((candidate) => candidate.url().includes("/__poc_3dt__/missing/tileset.json")),
    page.getByRole("button", { name: "注入严格 HTTP 404" }).click(),
  ]);

  expect(response.status()).toBe(404);
  expect(response.headers()["content-type"]).toContain("application/json");
  await expect(page.locator(".poc-state")).toHaveText("Tiles：failed");
  await expect(page.getByText("GLB 已加载：/models/lifter.glb")).toBeVisible();
  await expect(page.locator(".poc-viewport canvas")).toHaveCount(1);

  await page.getByRole("button", { name: "加载本地最小 Tileset" }).click();
  await expect(page.locator(".poc-state")).toHaveText("Tiles：ready", { timeout: 90_000 });
  await expect(page.locator(".poc-viewport canvas")).toHaveCount(1);
});

test("distinguishes invalid JSON and child-resource HTTP 404 from a root 404", async ({ page }) => {
  await openReadyPoc(page);

  const [invalidJsonResponse] = await Promise.all([
    page.waitForResponse((candidate) => candidate.url().includes("/poc-3dtiles/invalid-json/tileset.json")),
    page.getByRole("button", { name: "注入 Tileset JSON 解析错误" }).click(),
  ]);
  expect(invalidJsonResponse.status()).toBe(200);
  expect(invalidJsonResponse.headers()["content-type"]).toContain("application/json");
  await expect(page.locator(".poc-state")).toHaveText("Tiles：failed");
  await expect(page.getByText(/Tileset 解析失败/)).toBeVisible();

  await page.getByRole("button", { name: "加载本地最小 Tileset" }).click();
  await expect(page.locator(".poc-state")).toHaveText("Tiles：ready", { timeout: 90_000 });

  const [childResponse] = await Promise.all([
    page.waitForResponse((candidate) => candidate.url().includes("/__poc_3dt__/missing/child/missing-child.gltf")),
    page.getByRole("button", { name: "注入子资源失败" }).click(),
  ]);
  expect(childResponse.status()).toBe(404);
  expect(childResponse.headers()["content-type"]).toContain("application/json");
  await expect(page.locator(".poc-state")).toHaveText("Tiles：ready", { timeout: 90_000 });
  await expect(page.getByLabel("POC 运行诊断")).toContainText("1 / 1", { timeout: 2_000 });
});

test("drives the synthetic lifter platform through the three worldZ evidence points", async ({ page }) => {
  await openReadyPoc(page);

  await page.getByRole("button", { name: "加载合成 POC worldZ 夹具" }).click();
  await expect(page.getByText("已加载合成 POC worldZ 夹具：/poc-3dtiles/poc-lifter/poc-lifter.gltf")).toBeVisible();

  for (const [buttonName, worldZ] of [["worldZ 低位 0", "0"], ["worldZ 中位 6", "6"], ["worldZ 高位 12", "12"]] as const) {
    await page.getByRole("button", { name: buttonName }).click();
    await expect(page.getByLabel("POC 运行诊断")).toContainText(`未选择 / ${worldZ}`);
  }
});

test("completes ten real Vue lifecycle rounds without accumulating canvases", async ({ page }) => {
  test.setTimeout(20 * 60_000);
  await openReadyPoc(page);

  const runTenRounds = page.getByRole("button", { name: "连续执行 10 轮真实生命周期" });
  await runTenRounds.click();
  await expect(runTenRounds).toBeEnabled({ timeout: 20 * 60_000 });
  await expect(page.getByLabel("POC 运行诊断")).toContainText("10 /");
  await expect(page.locator(".poc-error")).toHaveCount(0);
  await expect(page.locator(".poc-viewport canvas")).toHaveCount(1);
});

test("picks a mesh from the real lifter GLB through Canvas pointer events", async ({ page }, testInfo) => {
  await openReadyPoc(page);

  const canvas = page.locator(".poc-viewport canvas");
  const box = await canvas.boundingBox();
  expect(box).not.toBeNull();
  if (!box) {
    return;
  }

  const selection = page.getByText(/^GLB 拾取：/);
  await expect(selection).toHaveText("GLB 拾取：未选择");

  const candidateOffsets = [
    [0.5, 0.3], [0.5, 0.4], [0.5, 0.5], [0.5, 0.6], [0.5, 0.7],
    [0.4, 0.4], [0.4, 0.5], [0.4, 0.6], [0.6, 0.4], [0.6, 0.5], [0.6, 0.6],
  ] as const;
  let selectedText: string | null = null;
  for (const [xRatio, yRatio] of candidateOffsets) {
    await canvas.click({ position: { x: box.width * xRatio, y: box.height * yRatio } });
    const text = await selection.textContent();
    if (text && text !== "GLB 拾取：未选择") {
      selectedText = text;
      break;
    }
  }

  expect(selectedText).toMatch(/^GLB 拾取：.+/);
  const screenshotPath = testInfo.outputPath("poc-3dt-real-glb-pick.png");
  await page.screenshot({ path: screenshotPath, fullPage: true });
  await testInfo.attach("real-glb-pick", { path: screenshotPath, contentType: "image/png" });
});

test("records three cold-cache and three warm-cache POC performance samples", async ({ browser }, testInfo) => {
  test.setTimeout(10 * 60_000);
  const evidence: PocPerformanceEvidence = {
    browser: testInfo.project.name,
    generatedAt: new Date().toISOString(),
    samples: [],
  };

  for (let round = 1; round <= 3; round += 1) {
    const context = await createColdCacheContext(browser);
    const page = await context.newPage();
    const cdp = await context.newCDPSession(page);
    await cdp.send("Network.setCacheDisabled", { cacheDisabled: true });
    await context.tracing.start({ screenshots: true, snapshots: true, sources: false });
    try {
      const sample = await capturePerformanceSample(page, "cold", round, () =>
        page.goto("/poc-3dtiles.html", { waitUntil: "domcontentloaded" }),
      );
      evidence.samples.push(sample);
      const screenshotPath = testInfo.outputPath(`poc-3dt-cold-cache-${round}.png`);
      await page.screenshot({ path: screenshotPath, fullPage: true });
      await testInfo.attach(`cold-cache-${round}`, { path: screenshotPath, contentType: "image/png" });
    } finally {
      await context.tracing.stop({ path: testInfo.outputPath(`poc-3dt-cold-cache-${round}.zip`) });
      await context.close();
    }
  }

  const warmContext = await browser.newContext({ viewport: { width: 1440, height: 1000 } });
  const warmPage = await warmContext.newPage();
  await warmContext.tracing.start({ screenshots: true, snapshots: true, sources: false });
  try {
    await openReadyPoc(warmPage);
    for (let round = 1; round <= 3; round += 1) {
      const sample = await capturePerformanceSample(warmPage, "warm", round, () =>
        warmPage.reload({ waitUntil: "domcontentloaded" }),
      );
      evidence.samples.push(sample);
      const screenshotPath = testInfo.outputPath(`poc-3dt-warm-cache-${round}.png`);
      await warmPage.screenshot({ path: screenshotPath, fullPage: true });
      await testInfo.attach(`warm-cache-${round}`, { path: screenshotPath, contentType: "image/png" });
    }
  } finally {
    await warmContext.tracing.stop({ path: testInfo.outputPath("poc-3dt-warm-cache.zip") });
    await warmContext.close();
  }

  expect(evidence.samples).toHaveLength(6);
  const evidencePath = testInfo.outputPath("poc-3dt-performance-baseline.json");
  await writeFile(evidencePath, JSON.stringify(evidence, null, 2), "utf8");
  await testInfo.attach("performance-baseline", { path: evidencePath, contentType: "application/json" });
  for (const sample of evidence.samples) {
    expect(sample.navigationReadyMs).toBeGreaterThan(0);
    expect(sample.firstVisibleAndReadyMs).not.toBeNull();
    expect(sample.requestCount).toBeGreaterThan(0);
    expect(sample.fps).not.toBeNull();
    expect(sample.drawCalls).not.toBeNull();
    expect(sample.activeTiles).toBe(1);
    expect(sample.consoleErrors).toEqual([]);
    expect(sample.pageErrors).toEqual([]);
  }
});
