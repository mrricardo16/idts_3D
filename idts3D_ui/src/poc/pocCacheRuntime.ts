export type PocCacheMode = "baseline" | "versioned-cache";

export interface PocCacheManifest {
  asset: "lifter";
  sha256: string;
  versionedUrl: string;
  contentLength: number;
  cachePolicy: "immutable";
}

export interface PocCachePageState {
  mode: PocCacheMode;
  manifestUrl: string;
  glbUrl: string;
  sha256: string | null;
  cacheControl: string;
  actualTransferBytes: number | null;
  cacheHitType: string;
}

export const pocCacheManifestUrl = "/__poc_cache__/manifest.json";
export const pocBaselineGlbUrl = "/models/lifter.glb";

export function resolvePocCacheMode(value: string | null): PocCacheMode {
  return value === "versioned-cache" ? "versioned-cache" : "baseline";
}

export function createPocCachePageState(
  mode: PocCacheMode,
  manifest?: PocCacheManifest,
): PocCachePageState {
  const versioned = mode === "versioned-cache" && manifest;
  return {
    mode,
    manifestUrl: pocCacheManifestUrl,
    glbUrl: versioned ? manifest.versionedUrl : pocBaselineGlbUrl,
    sha256: versioned ? manifest.sha256 : null,
    cacheControl: versioned ? "public, max-age=31536000, immutable" : "no-cache（原始基线）",
    actualTransferBytes: null,
    cacheHitType: "由 CDP 缓存测试取证",
  };
}

export async function loadPocCacheManifest(
  fetcher: typeof fetch = fetch,
): Promise<PocCacheManifest> {
  const response = await fetcher(pocCacheManifestUrl, { cache: "no-cache" });
  if (!response.ok) {
    throw new Error(`POC cache manifest request failed (HTTP ${response.status}).`);
  }

  const manifest = await response.json() as PocCacheManifest;
  if (
    manifest.asset !== "lifter"
    || !/^[a-f0-9]{64}$/.test(manifest.sha256)
    || !manifest.versionedUrl.includes(manifest.sha256)
    || !Number.isSafeInteger(manifest.contentLength)
    || manifest.contentLength <= 0
    || manifest.cachePolicy !== "immutable"
  ) {
    throw new Error("POC cache manifest response is invalid.");
  }

  return manifest;
}
