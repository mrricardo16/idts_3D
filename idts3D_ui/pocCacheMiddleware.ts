import { createHash } from "node:crypto";
import { createReadStream } from "node:fs";
import { stat } from "node:fs/promises";
import type { IncomingMessage, ServerResponse } from "node:http";
import { basename } from "node:path";
import type { PocCacheManifest } from "./src/poc/pocCacheRuntime";

const pocCachePrefix = "/__poc_cache__/";
const immutableCacheControl = "public, max-age=31536000, immutable";

export interface PocVersionedAssetOptions {
  asset: "lifter";
  sourcePath: string;
  contentType: string;
}

export interface PocVersionedAsset extends PocVersionedAssetOptions {
  sha256: string;
  shortHash: string;
  contentLength: number;
  lastModified: Date;
  versionedUrl: string;
  manifest: PocCacheManifest;
}

export interface PocCacheMiddlewareOptions {
  assets: PocVersionedAsset[];
  testVersionManifests?: Record<string, PocCacheManifest>;
}

export type PocCacheMiddleware = (
  request: IncomingMessage,
  response: ServerResponse,
  next: () => void,
) => void;

async function hashFile(sourcePath: string): Promise<string> {
  return new Promise((resolve, reject) => {
    const hash = createHash("sha256");
    const stream = createReadStream(sourcePath);
    stream.on("data", (chunk: Buffer) => hash.update(chunk));
    stream.on("error", reject);
    stream.on("end", () => resolve(hash.digest("hex")));
  });
}

export async function createPocVersionedAsset(
  options: PocVersionedAssetOptions,
): Promise<PocVersionedAsset> {
  const sourceStat = await stat(options.sourcePath);
  if (!sourceStat.isFile()) {
    throw new Error(`POC cache source is not a file: ${options.sourcePath}`);
  }

  const sha256 = await hashFile(options.sourcePath);
  const versionedUrl = `/__poc_cache__/models/${options.asset}/${sha256}/${basename(options.sourcePath)}`;
  return {
    ...options,
    sha256,
    shortHash: sha256.slice(0, 16),
    contentLength: sourceStat.size,
    lastModified: sourceStat.mtime,
    versionedUrl,
    manifest: {
      asset: options.asset,
      sha256,
      versionedUrl,
      contentLength: sourceStat.size,
      cachePolicy: "immutable",
    },
  };
}

function writeJson(response: ServerResponse, status: number, body: Record<string, unknown>): void {
  response.statusCode = status;
  response.setHeader("Content-Type", "application/json; charset=utf-8");
  response.setHeader("Cache-Control", "no-store");
  response.end(JSON.stringify(body));
}

function parseRange(value: string | undefined, totalLength: number): { start: number; end: number } | "invalid" | undefined {
  if (!value) {
    return undefined;
  }

  const match = /^bytes=(\d*)-(\d*)$/.exec(value);
  if (!match || (!match[1] && !match[2])) {
    return "invalid";
  }

  if (!match[1]) {
    const suffixLength = Number(match[2]);
    if (!Number.isSafeInteger(suffixLength) || suffixLength <= 0) {
      return "invalid";
    }
    return { start: Math.max(0, totalLength - suffixLength), end: totalLength - 1 };
  }

  const start = Number(match[1]);
  const end = match[2] ? Number(match[2]) : totalLength - 1;
  if (
    !Number.isSafeInteger(start)
    || !Number.isSafeInteger(end)
    || start < 0
    || start >= totalLength
    || end < start
  ) {
    return "invalid";
  }

  return { start, end: Math.min(end, totalLength - 1) };
}

function writeAssetHeaders(response: ServerResponse, asset: PocVersionedAsset, length: number): void {
  response.setHeader("Content-Type", asset.contentType);
  response.setHeader("Cache-Control", immutableCacheControl);
  response.setHeader("Content-Length", String(length));
  response.setHeader("ETag", `"${asset.sha256}"`);
  response.setHeader("Last-Modified", asset.lastModified.toUTCString());
  response.setHeader("Accept-Ranges", "bytes");
}

function streamAsset(
  response: ServerResponse,
  asset: PocVersionedAsset,
  range: { start: number; end: number } | undefined,
): void {
  const start = range?.start ?? 0;
  const end = range?.end ?? asset.contentLength - 1;
  const length = end - start + 1;
  response.statusCode = range ? 206 : 200;
  writeAssetHeaders(response, asset, length);
  if (range) {
    response.setHeader("Content-Range", `bytes ${start}-${end}/${asset.contentLength}`);
  }

  const stream = createReadStream(asset.sourcePath, { start, end });
  stream.on("error", (error) => {
    if (!response.headersSent) {
      writeJson(response, 500, { error: "POC cache asset stream failed", message: String(error) });
      return;
    }
    response.destroy(error);
  });
  stream.pipe(response);
}

export function createPocCacheMiddleware(options: PocCacheMiddlewareOptions): PocCacheMiddleware {
  const assetsByUrl = new Map(options.assets.map((asset) => [asset.versionedUrl, asset]));
  const lifterManifest = options.assets.find((asset) => asset.asset === "lifter")?.manifest;

  return (request, response, next) => {
    const pathname = new URL(request.url ?? "/", "http://poc-cache.local").pathname;
    if (!pathname.startsWith(pocCachePrefix)) {
      next();
      return;
    }

    if (pathname === "/__poc_cache__/manifest.json") {
      if (!lifterManifest) {
        writeJson(response, 404, { error: "POC cache manifest is unavailable" });
        return;
      }
      response.statusCode = 200;
      response.setHeader("Content-Type", "application/json; charset=utf-8");
      response.setHeader("Cache-Control", "no-cache");
      response.setHeader("ETag", `"${lifterManifest.sha256}"`);
      response.end(JSON.stringify(lifterManifest));
      return;
    }

    const testManifestMatch = /^\/__poc_cache__\/test-version\/([^/]+)\/manifest\.json$/.exec(pathname);
    if (testManifestMatch) {
      const testManifest = options.testVersionManifests?.[testManifestMatch[1] ?? ""];
      if (!testManifest) {
        writeJson(response, 404, { error: "POC test-version manifest is unavailable" });
        return;
      }
      response.statusCode = 200;
      response.setHeader("Content-Type", "application/json; charset=utf-8");
      response.setHeader("Cache-Control", "no-cache");
      response.setHeader("ETag", `"${testManifest.sha256}"`);
      response.end(JSON.stringify(testManifest));
      return;
    }

    const asset = assetsByUrl.get(pathname);
    if (!asset) {
      writeJson(response, 404, { error: "POC versioned cache asset not found", path: pathname });
      return;
    }

    const rangeHeader = Array.isArray(request.headers.range)
      ? request.headers.range[0]
      : request.headers.range;
    const range = parseRange(rangeHeader, asset.contentLength);
    if (range === "invalid") {
      response.statusCode = 416;
      response.setHeader("Content-Range", `bytes */${asset.contentLength}`);
      response.setHeader("Accept-Ranges", "bytes");
      response.end();
      return;
    }

    streamAsset(response, asset, range);
  };
}
