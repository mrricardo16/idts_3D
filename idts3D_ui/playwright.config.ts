import { defineConfig, devices } from "@playwright/test";

export default defineConfig({
  testDir: "./tests/e2e",
  timeout: 20 * 60_000,
  expect: { timeout: 90_000 },
  outputDir: "./test-results/poc-3dt",
  use: {
    baseURL: "http://127.0.0.1:5180",
    trace: "off",
    screenshot: "only-on-failure",
  },
  projects: [
    {
      name: "chrome",
      use: { ...devices["Desktop Chrome"], channel: "chrome" },
    },
    {
      name: "edge",
      use: { ...devices["Desktop Edge"], channel: "msedge" },
    },
  ],
  webServer: {
    command: "npm run dev -- --port 5180",
    url: "http://127.0.0.1:5180/poc-3dtiles.html",
    timeout: 120_000,
    reuseExistingServer: false,
  },
});
