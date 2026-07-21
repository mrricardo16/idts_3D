export type PocPerformanceScenario =
  | "factory-only"
  | "glb-only"
  | "hybrid"
  | "synthetic"
  | "hybrid-throttled";

export interface PocPerformanceScenarioConfig {
  name: PocPerformanceScenario;
  loadTiles: boolean;
  glb: "none" | "real" | "synthetic";
  diagnosticsPublishIntervalMs: number;
}

const scenarioConfigs: Record<PocPerformanceScenario, PocPerformanceScenarioConfig> = {
  "factory-only": {
    name: "factory-only",
    loadTiles: true,
    glb: "none",
    diagnosticsPublishIntervalMs: 1_000,
  },
  "glb-only": {
    name: "glb-only",
    loadTiles: false,
    glb: "real",
    diagnosticsPublishIntervalMs: 1_000,
  },
  hybrid: {
    name: "hybrid",
    loadTiles: true,
    glb: "real",
    diagnosticsPublishIntervalMs: 1_000,
  },
  synthetic: {
    name: "synthetic",
    loadTiles: true,
    glb: "synthetic",
    diagnosticsPublishIntervalMs: 1_000,
  },
  "hybrid-throttled": {
    name: "hybrid-throttled",
    loadTiles: true,
    glb: "real",
    diagnosticsPublishIntervalMs: 10_000,
  },
};

export function resolvePocPerformanceScenario(
  value: string | null,
): PocPerformanceScenarioConfig | null {
  if (!value) {
    return { ...scenarioConfigs.hybrid };
  }

  const config = scenarioConfigs[value as PocPerformanceScenario];
  return config ? { ...config } : null;
}
