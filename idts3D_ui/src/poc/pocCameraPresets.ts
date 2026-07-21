export type PocCameraPresetName = "factory-exterior" | "factory-high-door" | "factory-interior";

export interface PocCameraPreset {
  position: [number, number, number];
  target: [number, number, number];
}

const pocCameraPresets: Record<PocCameraPresetName, PocCameraPreset> = {
  "factory-exterior": {
    position: [16, -20, 14],
    target: [0, 0, 12],
  },
  "factory-high-door": {
    position: [0, -8, 12],
    target: [0, 0, 12],
  },
  "factory-interior": {
    position: [0, -1.5, 12],
    target: [0, 1.5, 12],
  },
};

export function getPocCameraPreset(name: PocCameraPresetName): PocCameraPreset {
  const preset = pocCameraPresets[name];
  return {
    position: [...preset.position],
    target: [...preset.target],
  };
}
