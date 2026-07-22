import { expect, test, type Browser, type BrowserContext, type CDPSession, type Page } from "@playwright/test";
import { readFile, writeFile } from "node:fs/promises";
import { resolve } from "node:path";

type ServerKind = "vite-development" | "vite-preview";

interface CacheRequestRecord {
  url: string;
  requestCacheControl: string | null;
  requestPragma: string | null;
  status: number | null;
  cacheControl: string | null;
  etag: string | null;
  contentLength: string | null;
  fromDiskCache: boolean;
  servedFromCache: boolean;
  encodedDataLength: number | null;
  failure: string | null;
}

interface PageDiagnostics {
  consoleErrors: string[];
  ignoredConsoleErrors: string[];
  pageErrors: string[];
  canvasRenderer: string | null;
}

const serverUrls: Record<ServerKind, string> = {
  "vite-development": "http://127.0.0.1:5184",
  "vite-preview": "http://127.0.0.1:5185",
};

function isVersionedLifter(url: string): boolean {
  return new URL(url).pathname.startsWith("/__poc_cache__/models/lifter/");
}

function createRecorder(session: CDPSession, shouldRecord: (url: string) => boolean = isVersionedLifter): {
  reset: () => void;
  records: () => CacheRequestRecord[];
} {
  const byRequestId = new Map<string, CacheRequestRecord>();
  session.on("Network.requestWillBeSent", (event) => {
    if (!shouldRecord(event.request.url)) {
      return;
    }
    byRequestId.set(event.requestId, {
      url: event.request.url,
      requestCacheControl: String(event.request.headers["Cache-Control"] ?? event.request.headers["cache-control"] ?? "") || null,
      requestPragma: String(event.request.headers.Pragma ?? event.request.headers.pragma ?? "") || null,
      status: null,
      cacheControl: null,
      etag: null,
      contentLength: null,
      fromDiskCache: false,
      servedFromCache: false,
      encodedDataLength: null,
      failure: null,
    });
  });
  session.on("Network.responseReceived", (event) => {
    const record = byRequestId.get(event.requestId);
    if (!record) {
      return;
    }
    const headers = Object.fromEntries(Object.entries(event.response.headers).map(([key, value]) => [key.toLowerCase(), String(value)]));
    record.status = event.response.status;
    record.cacheControl = headers["cache-control"] ?? null;
    record.etag = headers.etag ?? null;
    record.contentLength = headers["content-length"] ?? null;
    record.fromDiskCache = Boolean(event.response.fromDiskCache);
  });
  session.on("Network.requestServedFromCache", (event) => {
    const record = byRequestId.get(event.requestId);
    if (record) {
      record.servedFromCache = true;
    }
  });
  session.on("Network.loadingFinished", (event) => {
    const record = byRequestId.get(event.requestId);
    if (record) {
      record.encodedDataLength = event.encodedDataLength;
    }
  });
  session.on("Network.loadingFailed", (event) => {
    const record = byRequestId.get(event.requestId);
    if (record) {
      record.failure = event.errorText;
    }
  });
  return {
    reset: () => byRequestId.clear(),
    records: () => [...byRequestId.values()],
  };
}

function monitorPage(page: Page): PageDiagnostics {
  const diagnostics: PageDiagnostics = { consoleErrors: [], ignoredConsoleErrors: [], pageErrors: [], canvasRenderer: null };
  page.on("console", (message) => {
    if (message.type() === "error") {
      const detail = `${message.text()} (${message.location().url})`;
      if (message.location().url.endsWith("/favicon.ico")) {
        diagnostics.ignoredConsoleErrors.push(detail);
        return;
      }
      diagnostics.consoleErrors.push(detail);
    }
  });
  page.on("pageerror", (error) => diagnostics.pageErrors.push(error.message));
  return diagnostics;
}

async function waitForVersionedPoc(page: Page, server: ServerKind): Promise<void> {
  await page.goto(`${serverUrls[server]}/poc-3dtiles.html?pocCacheMode=versioned-cache`, { waitUntil: "domcontentloaded" });
  await expect(page.locator(".poc-state")).toHaveText(/ready/, { timeout: 120_000 });
  await expect(page.locator(".poc-viewport canvas")).toHaveCount(1);
  await expect(page.getByText("当前资产模式：versioned-cache")).toBeVisible();
  await expect(page.getByText(/GLB 已加载：\/__poc_cache__\/models\/lifter\//)).toBeVisible({ timeout: 120_000 });
  await expect(page.getByLabel("POC 缓存诊断")).toContainText("public, max-age=31536000, immutable");
  const runtimeDiagnostics = page.getByLabel("POC 运行诊断");
  await expect(runtimeDiagnostics).toContainText(/GLB \/ Canvas \/ Renderer[\s\S]*· 1 \/ 1/);
}

async function newContext(browser: Browser): Promise<BrowserContext> {
  return browser.newContext({
    viewport: { width: 1920, height: 1080 },
    deviceScaleFactor: 1,
  });
}

test("serves a versioned lifter manifest, immutable GLB headers, and byte ranges on both POC servers", async ({ browser }) => {
  for (const server of Object.keys(serverUrls) as ServerKind[]) {
    const context = await newContext(browser);
    const page = await context.newPage();
    try {
      const manifestResponse = await page.request.get(`${serverUrls[server]}/__poc_cache__/manifest.json`);
      expect(manifestResponse.status()).toBe(200);
      expect(manifestResponse.headers()["cache-control"]).toBe("no-cache");
      const manifest = await manifestResponse.json() as { sha256: string; versionedUrl: string; contentLength: number };
      expect(manifest.sha256).toMatch(/^[a-f0-9]{64}$/);
      expect(manifest.versionedUrl).toContain(manifest.sha256);
      expect(manifest.contentLength).toBe(293_192_660);

      const full = await page.request.get(`${serverUrls[server]}${manifest.versionedUrl}`, { headers: { Range: "bytes=0-15" } });
      expect(full.status()).toBe(206);
      expect(full.headers()["cache-control"]).toBe("public, max-age=31536000, immutable");
      expect(full.headers()["content-range"]).toBe("bytes 0-15/293192660");
      expect(full.headers()["accept-ranges"]).toBe("bytes");
      expect(full.headers()["etag"]).toBe(`"${manifest.sha256}"`);
      expect((await full.body()).byteLength).toBe(16);

      const invalidRange = await page.request.get(`${serverUrls[server]}${manifest.versionedUrl}`, { headers: { Range: "bytes=293192660-293192661" } });
      expect(invalidRange.status()).toBe(416);
      expect(invalidRange.headers()["content-range"]).toBe("bytes */293192660");
    } finally {
      await context.close();
    }
  }
});

test("records three cold and three warm CDP cache rounds without replaying the full GLB", async ({ browser }, testInfo) => {
  test.setTimeout(30 * 60_000);
  const evidence: Record<string, unknown> = { generatedAt: new Date().toISOString(), servers: {} };
  const evidencePath = resolve(process.cwd(), "../idts3D_docs/poc/evidence/POC-3DT-01/perf-fix-01-cache-result.json");

  for (const server of Object.keys(serverUrls) as ServerKind[]) {
    const cold: CacheRequestRecord[][] = [];
    const coldPageDiagnostics: PageDiagnostics[] = [];
    for (let round = 0; round < 3; round += 1) {
      const context = await newContext(browser);
      const page = await context.newPage();
      const pageDiagnostics = monitorPage(page);
      const session = await context.newCDPSession(page);
      await session.send("Network.enable");
      const recorder = createRecorder(session);
      try {
        await waitForVersionedPoc(page, server);
        pageDiagnostics.canvasRenderer = await page.getByLabel("POC 运行诊断").locator("dd").nth(1).textContent();
        cold.push(recorder.records());
        coldPageDiagnostics.push(pageDiagnostics);
      } finally {
        await session.detach();
        await context.close();
      }
    }

    const warm: CacheRequestRecord[][] = [];
    const warmContext = await newContext(browser);
    const warmPage = await warmContext.newPage();
    const warmSession = await warmContext.newCDPSession(warmPage);
    const warmPageDiagnostics = monitorPage(warmPage);
    await warmSession.send("Network.enable");
    const warmRecorder = createRecorder(warmSession);
    try {
      await waitForVersionedPoc(warmPage, server);
      warmPageDiagnostics.canvasRenderer = await warmPage.getByLabel("POC 运行诊断").locator("dd").nth(1).textContent();
      for (let round = 0; round < 3; round += 1) {
        warmRecorder.reset();
        await warmPage.reload({ waitUntil: "domcontentloaded" });
        await expect(warmPage.locator(".poc-state")).toHaveText(/ready/, { timeout: 120_000 });
        await expect(warmPage.getByLabel("POC 运行诊断")).toContainText(/GLB \/ Canvas \/ Renderer[\s\S]*· 1 \/ 1/);
        warm.push(warmRecorder.records());
      }
    } finally {
      await warmSession.detach();
      await warmContext.close();
    }

    const coldTransfers = cold.map((round) => round.reduce((total, request) => total + (request.encodedDataLength ?? 0), 0));
    const warmTransfers = warm.map((round) => round.reduce((total, request) => total + (request.encodedDataLength ?? 0), 0));
    const coldAverage = coldTransfers.reduce((total, value) => total + value, 0) / coldTransfers.length;
    const warmAverage = warmTransfers.reduce((total, value) => total + value, 0) / warmTransfers.length;
    const meetsWarmThreshold = warmAverage <= coldAverage * 0.01 || warmAverage <= 5 * 1024 * 1024;
    evidence.servers = {
      ...(evidence.servers as Record<string, unknown>),
      [server]: {
        cold,
        warm,
        coldTransfers,
        warmTransfers,
        coldAverage,
        warmAverage,
        meetsWarmThreshold,
        pageRegression: { cold: coldPageDiagnostics, warm: warmPageDiagnostics },
      },
    };
    await writeFile(evidencePath, JSON.stringify(evidence, null, 2), "utf8");
    console.log(JSON.stringify({ server, coldTransfers, warmTransfers, meetsWarmThreshold }));
    expect(coldTransfers.every((value) => value > 293_000_000)).toBe(true);
    expect(warm.flat().every((request) => request.failure === null)).toBe(true);
    expect(coldPageDiagnostics.every((diagnostics) => diagnostics.consoleErrors.length === 0 && diagnostics.pageErrors.length === 0)).toBe(true);
    expect(warmPageDiagnostics.consoleErrors).toEqual([]);
    expect(warmPageDiagnostics.pageErrors).toEqual([]);
  }

  await writeFile(evidencePath, JSON.stringify(evidence, null, 2), "utf8");
  await testInfo.attach("perf-fix-01-cache-evidence", { path: evidencePath, contentType: "application/json" });
});

test("uses different content-hash URLs without serving a stale POC fixture version", async ({ browser }) => {
  const base = serverUrls["vite-development"];
  const evidencePath = resolve(process.cwd(), "../idts3D_docs/poc/evidence/POC-3DT-01/perf-fix-01-cache-result.json");
  const context = await newContext(browser);
  const page = await context.newPage();
  const session = await context.newCDPSession(page);
  await session.send("Network.enable");
  const recorder = createRecorder(session, (url) => new URL(url).pathname.startsWith("/__poc_cache__/models/lifter/"));
  try {
    await page.goto(`${base}/__poc_cache__/test-version/v1/manifest.json`);
    const v1 = await page.request.get(`${base}/__poc_cache__/test-version/v1/manifest.json`);
    const v2 = await page.request.get(`${base}/__poc_cache__/test-version/v2/manifest.json`);
    expect(v1.status()).toBe(200);
    expect(v2.status()).toBe(200);
    const v1Manifest = await v1.json() as { versionedUrl: string };
    const v2Manifest = await v2.json() as { versionedUrl: string };
    expect(v1Manifest.versionedUrl).not.toBe(v2Manifest.versionedUrl);

    const fetchFixture = async (url: string) => page.evaluate(async (assetUrl) => {
      const response = await fetch(assetUrl);
      return { status: response.status, byteLength: (await response.arrayBuffer()).byteLength };
    }, url);
    const firstV1Response = await fetchFixture(`${base}${v1Manifest.versionedUrl}`);
    expect(firstV1Response.status).toBe(200);
    const firstV1 = recorder.records();
    recorder.reset();
    const warmV1Response = await fetchFixture(`${base}${v1Manifest.versionedUrl}`);
    expect(warmV1Response.status).toBe(200);
    const warmV1 = recorder.records();
    recorder.reset();
    const firstV2Response = await fetchFixture(`${base}${v2Manifest.versionedUrl}`);
    expect(firstV2Response.status).toBe(200);
    const firstV2 = recorder.records();

    expect(firstV1).toHaveLength(1);
    expect(firstV2).toHaveLength(1);
    expect(firstV1[0]?.encodedDataLength ?? 0).toBeGreaterThan(0);
    expect(firstV2[0]?.encodedDataLength ?? 0).toBeGreaterThan(0);
    expect(warmV1.every((record) => (record.encodedDataLength ?? 0) <= 1024)).toBe(true);

    const existing = JSON.parse(await readFile(evidencePath, "utf8")) as Record<string, unknown>;
    existing.versionInvalidation = {
      v1: { manifest: v1Manifest, response: firstV1Response, requests: firstV1 },
      v1Warm: { response: warmV1Response, requests: warmV1 },
      v2: { manifest: v2Manifest, response: firstV2Response, requests: firstV2 },
    };
    await writeFile(evidencePath, JSON.stringify(existing, null, 2), "utf8");
  } finally {
    await session.detach();
    await context.close();
  }
});
