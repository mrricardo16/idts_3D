import type { PocTilesPhase } from "./pocTilesRuntime";

export const pocLifecycleRoundTimeoutMs = 90_000;

export interface PocReadyGate {
  notify: (phase: PocTilesPhase) => void;
  reset: () => void;
  waitForReady: (timeoutMs: number) => Promise<boolean>;
}

export function createPocReadyGate(): PocReadyGate {
  let resolveReady: ((ready: boolean) => void) | undefined;
  let timeoutHandle: ReturnType<typeof window.setTimeout> | undefined;
  let wasReady = false;

  const settle = (ready: boolean): void => {
    if (!resolveReady) {
      return;
    }
    if (timeoutHandle !== undefined) {
      window.clearTimeout(timeoutHandle);
      timeoutHandle = undefined;
    }
    const resolve = resolveReady;
    resolveReady = undefined;
    resolve(ready);
  };

  return {
    notify: (phase) => {
      if (phase === "ready") {
        wasReady = true;
        settle(true);
      }
    },
    reset: () => {
      wasReady = false;
      settle(false);
    },
    waitForReady: (timeoutMs) => {
      if (wasReady) {
        return Promise.resolve(true);
      }
      return new Promise((resolve) => {
        resolveReady = resolve;
        timeoutHandle = window.setTimeout(() => settle(false), timeoutMs);
      });
    },
  };
}
