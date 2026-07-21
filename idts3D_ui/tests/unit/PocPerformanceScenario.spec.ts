import { describe, expect, it } from "vitest";
import {
  resolvePocPerformanceScenario,
  type PocPerformanceScenario,
} from "../../src/poc/pocPerformanceScenario";

describe("resolvePocPerformanceScenario", () => {
  it("keeps normal POC loading as the hybrid default", () => {
    expect(resolvePocPerformanceScenario(null)).toEqual({
      name: "hybrid",
      loadTiles: true,
      glb: "real",
      diagnosticsPublishIntervalMs: 1_000,
    });
  });

  it.each([
    ["factory-only", { loadTiles: true, glb: "none" }],
    ["glb-only", { loadTiles: false, glb: "real" }],
    ["hybrid", { loadTiles: true, glb: "real" }],
    ["synthetic", { loadTiles: true, glb: "synthetic" }],
    ["hybrid-throttled", { loadTiles: true, glb: "real", diagnosticsPublishIntervalMs: 10_000 }],
  ] as Array<[PocPerformanceScenario, Record<string, unknown>]>)("maps %s to the isolated measurement scene", (value, expected) => {
    expect(resolvePocPerformanceScenario(value)).toMatchObject(expected);
  });

  it("rejects unknown query values instead of changing normal POC behavior", () => {
    expect(resolvePocPerformanceScenario("not-a-scenario")).toBeNull();
  });
});
