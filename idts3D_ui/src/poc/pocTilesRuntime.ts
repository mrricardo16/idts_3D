export type PocTilesPhase = "idle" | "loading" | "ready" | "failed" | "disposed";

export interface PocTilesState {
  phase: PocTilesPhase;
  error?: string;
  errorKind?: "http" | "parse" | "child";
  httpStatus?: number;
}

export type PocTilesEvent =
  | { type: "start" }
  | { type: "rootLoaded" }
  | { type: "contentLoaded" }
  | {
      type: "fail";
      message: string;
      kind?: PocTilesState["errorKind"];
      httpStatus?: number;
    }
  | { type: "dispose" };

export const defaultPocTilesConfig = {
  tilesetUrl: "/poc-3dtiles/minimal/tileset.json",
  missingTilesetUrl: "/__poc_3dt__/missing/tileset.json",
  childMissingTilesetUrl: "/poc-3dtiles/child-missing/tileset.json",
  invalidJsonTilesetUrl: "/poc-3dtiles/invalid-json/tileset.json",
} as const;

export function createPocTilesState(): PocTilesState {
  return { phase: "idle" };
}

export function transitionPocTilesState(
  current: PocTilesState,
  event: PocTilesEvent,
): PocTilesState {
  if (current.phase === "disposed") {
    return current;
  }

  switch (event.type) {
    case "start":
    case "rootLoaded":
      return { phase: "loading" };
    case "contentLoaded":
      return { phase: "ready" };
    case "fail":
      return {
        phase: "failed",
        error: event.message,
        errorKind: event.kind,
        httpStatus: event.httpStatus,
      };
    case "dispose":
      return { phase: "disposed" };
  }
}

export function classifyPocTilesFailure(
  failedUrl: string,
  error: Error,
): {
  message: string;
  kind: NonNullable<PocTilesState["errorKind"]>;
  httpStatus?: number;
} {
  const isStrictMissing = failedUrl.startsWith("/__poc_3dt__/missing/");
  const isChildFailure = /missing-child\.gltf$/.test(failedUrl) || failedUrl.includes("/missing/child/");
  const httpStatus = isStrictMissing || isChildFailure ? 404 : undefined;
  const errorKind = isChildFailure ? "child" : isStrictMissing ? "http" : "parse";
  const prefix = httpStatus ? `HTTP ${httpStatus}` : "Tileset 解析失败";

  return {
    message: `${prefix}：${error.message}（${failedUrl}）`,
    kind: errorKind,
    httpStatus,
  };
}
