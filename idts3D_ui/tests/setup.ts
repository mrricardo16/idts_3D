import { enableAutoUnmount } from "@vue/test-utils";
import { afterEach, vi } from "vitest";

enableAutoUnmount(afterEach);

afterEach(() => {
  document.body.replaceChildren();
  localStorage.clear();
  sessionStorage.clear();
  vi.restoreAllMocks();
  vi.useRealTimers();
});
