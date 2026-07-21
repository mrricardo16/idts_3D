import { expect, test, type Page } from "@playwright/test";

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
