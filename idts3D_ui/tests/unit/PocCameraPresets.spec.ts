import { describe, expect, it } from "vitest";
import { getPocCameraPreset } from "../../src/poc/pocCameraPresets";
import { pocLifecycleRoundTimeoutMs } from "../../src/poc/pocLifecycle";

describe("POC deterministic camera presets", () => {
  it("returns a stable exterior preset that frames the factory entrance", () => {
    expect(getPocCameraPreset("factory-exterior")).toEqual({
      position: [16, -20, 14],
      target: [0, 0, 12],
    });
  });

  it("uses a 90-second lifecycle readiness ceiling for slow local browser starts", () => {
    expect(pocLifecycleRoundTimeoutMs).toBe(90_000);
  });
});
