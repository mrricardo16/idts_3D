import { createServer, type Server } from "node:http";
import { mkdtemp, rm, writeFile } from "node:fs/promises";
import { tmpdir } from "node:os";
import { join } from "node:path";
import { afterEach, describe, expect, it } from "vitest";
import {
  createPocCacheMiddleware,
  createPocVersionedAsset,
  type PocCacheMiddleware,
} from "../../pocCacheMiddleware";
import {
  createPocCachePageState,
  resolvePocCacheMode,
} from "../../src/poc/pocCacheRuntime";

const servers: Server[] = [];
const temporaryDirectories: string[] = [];

afterEach(async () => {
  await Promise.all(servers.splice(0).map((server) => new Promise<void>((resolve, reject) => {
    server.close((error) => error ? reject(error) : resolve());
  })));
  await Promise.all(temporaryDirectories.splice(0).map((directory) => rm(directory, { recursive: true, force: true })));
});

async function createTemporaryAsset(name: string, bytes: number[]): Promise<string> {
  const directory = await mkdtemp(join(tmpdir(), "idts3d-poc-cache-"));
  temporaryDirectories.push(directory);
  const path = join(directory, name);
  await writeFile(path, Buffer.from(bytes));
  return path;
}

async function startMiddlewareServer(middleware: PocCacheMiddleware): Promise<string> {
  const server = createServer((request, response) => {
    middleware(request, response, () => {
      response.statusCode = 404;
      response.setHeader("Content-Type", "application/json; charset=utf-8");
      response.end(JSON.stringify({ error: "not handled" }));
    });
  });
  servers.push(server);
  await new Promise<void>((resolve) => server.listen(0, "127.0.0.1", resolve));
  const address = server.address();
  if (!address || typeof address === "string") {
    throw new Error("Expected a TCP middleware test server address.");
  }
  return `http://127.0.0.1:${address.port}`;
}

describe("POC versioned cache runtime", () => {
  it("uses only an explicit query mode and keeps baseline as the default", () => {
    expect(resolvePocCacheMode(null)).toBe("baseline");
    expect(resolvePocCacheMode("baseline")).toBe("baseline");
    expect(resolvePocCacheMode("versioned-cache")).toBe("versioned-cache");
    expect(resolvePocCacheMode("unexpected")).toBe("baseline");
    expect(createPocCachePageState("versioned-cache")).toMatchObject({
      mode: "versioned-cache",
      manifestUrl: "/__poc_cache__/manifest.json",
      actualTransferBytes: null,
      cacheHitType: "由 CDP 缓存测试取证",
    });
  });

  it("derives a deterministic content-hash URL without copying the asset", async () => {
    const sourcePath = await createTemporaryAsset("lifter.glb", [1, 2, 3, 4, 5, 6]);
    const asset = await createPocVersionedAsset({
      asset: "lifter",
      sourcePath,
      contentType: "model/gltf-binary",
    });

    expect(asset.contentLength).toBe(6);
    expect(asset.sha256).toMatch(/^[a-f0-9]{64}$/);
    expect(asset.shortHash).toHaveLength(16);
    expect(asset.versionedUrl).toBe(`/__poc_cache__/models/lifter/${asset.sha256}/lifter.glb`);
    expect(asset.manifest).toEqual({
      asset: "lifter",
      sha256: asset.sha256,
      versionedUrl: asset.versionedUrl,
      contentLength: 6,
      cachePolicy: "immutable",
    });
  });

  it("serves only the matching hash with immutable headers and byte ranges", async () => {
    const sourcePath = await createTemporaryAsset("lifter.glb", [0, 1, 2, 3, 4, 5]);
    const asset = await createPocVersionedAsset({
      asset: "lifter",
      sourcePath,
      contentType: "model/gltf-binary",
    });
    const baseUrl = await startMiddlewareServer(createPocCacheMiddleware({ assets: [asset] }));

    const manifest = await fetch(`${baseUrl}/__poc_cache__/manifest.json`);
    expect(manifest.status).toBe(200);
    expect(manifest.headers.get("cache-control")).toBe("no-cache");
    expect(await manifest.json()).toEqual(asset.manifest);

    const full = await fetch(`${baseUrl}${asset.versionedUrl}`);
    expect(full.status).toBe(200);
    expect(full.headers.get("cache-control")).toBe("public, max-age=31536000, immutable");
    expect(full.headers.get("content-type")).toContain("model/gltf-binary");
    expect(full.headers.get("content-length")).toBe("6");
    expect(full.headers.get("etag")).toBe(`"${asset.sha256}"`);
    expect(full.headers.get("accept-ranges")).toBe("bytes");
    expect([...new Uint8Array(await full.arrayBuffer())]).toEqual([0, 1, 2, 3, 4, 5]);

    const partial = await fetch(`${baseUrl}${asset.versionedUrl}`, { headers: { Range: "bytes=2-4" } });
    expect(partial.status).toBe(206);
    expect(partial.headers.get("content-range")).toBe("bytes 2-4/6");
    expect(partial.headers.get("content-length")).toBe("3");
    expect([...new Uint8Array(await partial.arrayBuffer())]).toEqual([2, 3, 4]);

    const invalidRange = await fetch(`${baseUrl}${asset.versionedUrl}`, { headers: { Range: "bytes=9-10" } });
    expect(invalidRange.status).toBe(416);
    expect(invalidRange.headers.get("content-range")).toBe("bytes */6");

    const wrongHash = await fetch(`${baseUrl}/__poc_cache__/models/lifter/${"0".repeat(64)}/lifter.glb`);
    expect(wrongHash.status).toBe(404);
    expect(wrongHash.headers.get("content-type")).toContain("application/json");
  });

  it("keeps different test fixture content on separate versioned URLs", async () => {
    const v1Path = await createTemporaryAsset("v1.gltf", [1, 1, 1]);
    const v2Path = await createTemporaryAsset("v2.gltf", [2, 2, 2]);
    const v1 = await createPocVersionedAsset({ asset: "cache-fixture", sourcePath: v1Path, contentType: "model/gltf+json" });
    const v2 = await createPocVersionedAsset({ asset: "cache-fixture", sourcePath: v2Path, contentType: "model/gltf+json" });

    expect(v1.sha256).not.toBe(v2.sha256);
    expect(v1.versionedUrl).not.toBe(v2.versionedUrl);
  });

  it("leaves the original model route outside the POC cache middleware", async () => {
    const sourcePath = await createTemporaryAsset("lifter.glb", [1, 2, 3]);
    const asset = await createPocVersionedAsset({ asset: "lifter", sourcePath, contentType: "model/gltf-binary" });
    const baseUrl = await startMiddlewareServer(createPocCacheMiddleware({ assets: [asset] }));

    const baseline = await fetch(`${baseUrl}/models/lifter.glb`);
    expect(baseline.status).toBe(404);
    expect(await baseline.json()).toEqual({ error: "not handled" });
  });
});
