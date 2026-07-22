import { defineConfig, devices } from "@playwright/test";

export default defineConfig({
  testDir: "./tests/e2e",
  testMatch: "Poc3dTilesCache.spec.ts",
  timeout: 30 * 60_000,
  expect: { timeout: 120_000 },
  workers: 1,
  outputDir: "./test-results/poc-3dt-cache",
  use: {
    trace: "off",
    screenshot: "off",
    video: "off",
  },
  projects: [
    {
      name: "chrome-cache",
      use: {
        ...devices["Desktop Chrome"],
        channel: "chrome",
        headless: false,
        viewport: { width: 1920, height: 1080 },
        deviceScaleFactor: 1,
      },
    },
  ],
  webServer: [
    {
      command: "npm run dev -- --port 5184",
      url: "http://127.0.0.1:5184/poc-3dtiles.html",
      timeout: 120_000,
      reuseExistingServer: false,
    },
    {
      command: "npm run preview -- --port 5185",
      url: "http://127.0.0.1:5185/poc-3dtiles.html",
      timeout: 120_000,
      reuseExistingServer: false,
    },
  ],
});
