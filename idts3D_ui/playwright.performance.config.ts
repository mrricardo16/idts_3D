import { defineConfig, devices } from "@playwright/test";

export default defineConfig({
  testDir: "./tests/e2e",
  testMatch: "Poc3dTilesPerformanceDiagnosis.spec.ts",
  timeout: 60 * 60_000,
  expect: { timeout: 90_000 },
  workers: 1,
  outputDir: "./test-results/poc-3dt-performance-diagnosis",
  use: {
    trace: "off",
    screenshot: "off",
    video: "off",
  },
  projects: [
    {
      name: "chrome-performance",
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
      command: "npm run dev -- --port 5182",
      url: "http://127.0.0.1:5182/poc-3dtiles.html",
      timeout: 120_000,
      reuseExistingServer: false,
    },
    {
      command: "npm run preview -- --port 5183",
      url: "http://127.0.0.1:5183/poc-3dtiles.html",
      timeout: 120_000,
      reuseExistingServer: false,
    },
  ],
});
