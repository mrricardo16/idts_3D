import { readFileSync } from "node:fs";
import { resolve } from "node:path";
import { describe, expect, it } from "vitest";
import {
  classifyPocTilesFailure,
  createPocTilesState,
  defaultPocTilesConfig,
  transitionPocTilesState,
} from "../../src/poc/pocTilesRuntime";
import { createPocStrict404Middleware } from "../../src/poc/pocStrict404Middleware";
import { pocWorldZTargets, setPocObjectWorldZ } from "../../src/poc/pocWorldZ";
import { createPocEvidencePayload, createPocRuntimeDiagnostics } from "../../src/poc/pocDiagnostics";
import { collectPocGlbNodeRecords } from "../../src/poc/pocGlbNodes";
import { createPocReadyGate } from "../../src/poc/pocLifecycle";
import { Group } from "three";

describe("POC 3D Tiles runtime state", () => {
  it("uses a replaceable repository-local default tileset URL", () => {
    expect(defaultPocTilesConfig.tilesetUrl).toBe("/poc-3dtiles/minimal/tileset.json");
    expect(defaultPocTilesConfig.tilesetUrl).not.toMatch(/^[A-Za-z]:|^https?:\/\//);
    expect(defaultPocTilesConfig.childMissingTilesetUrl).toBe(
      "/poc-3dtiles/child-missing/tileset.json",
    );
    expect(defaultPocTilesConfig.invalidJsonTilesetUrl).toBe(
      "/poc-3dtiles/invalid-json/tileset.json",
    );
    expect(defaultPocTilesConfig.missingTilesetUrl).toBe(
      "/__poc_3dt__/missing/tileset.json",
    );
  });

  it("returns a strict JSON HTTP 404 only for the POC missing-resource route", () => {
    const middleware = createPocStrict404Middleware();
    const response = {
      statusCode: 200,
      setHeader: (name: string, value: string) => headers.set(name, value),
      end: (body: string) => bodies.push(body),
    };
    const headers = new Map<string, string>();
    const bodies: string[] = [];
    let nextCalls = 0;

    middleware(
      { url: "/__poc_3dt__/missing/tileset.json" } as never,
      response as never,
      () => { nextCalls += 1; },
    );

    expect(response.statusCode).toBe(404);
    expect(headers.get("Content-Type")).toBe("application/json; charset=utf-8");
    expect(JSON.parse(bodies[0] ?? "{}")).toMatchObject({ error: "POC fixture not found" });
    expect(nextCalls).toBe(0);

    middleware(
      { url: "/poc-3dtiles/minimal/tileset.json" } as never,
      response as never,
      () => { nextCalls += 1; },
    );
    expect(nextCalls).toBe(1);
  });

  it("reports ready only after the tiles content model loads", () => {
    const loading = transitionPocTilesState(createPocTilesState(), {
      type: "start",
    });
    const rootLoaded = transitionPocTilesState(
      loading,
      { type: "rootLoaded" } as never,
    );
    expect(rootLoaded).toEqual({ phase: "loading" });

    const ready = transitionPocTilesState(
      loading,
      { type: "contentLoaded" } as never,
    );

    expect(ready).toEqual({ phase: "ready", error: undefined });
  });

  it("keeps the runtime disposed when an in-flight request resolves late", () => {
    const disposed = transitionPocTilesState(createPocTilesState(), {
      type: "dispose",
    });

    expect(transitionPocTilesState(disposed, { type: "contentLoaded" } as never)).toEqual(disposed);
  });

  it("exposes a readable error state for failed tileset loading", () => {
    const failed = transitionPocTilesState(createPocTilesState(), {
      type: "fail",
      message: "Tileset root request failed (HTTP 404).",
    });

    expect(failed).toEqual({
      phase: "failed",
      error: "Tileset root request failed (HTTP 404).",
    });
  });

  it("classifies strict HTTP 404 separately and permits a real ready recovery", () => {
    const failure = classifyPocTilesFailure(
      defaultPocTilesConfig.missingTilesetUrl,
      new Error("HTTP 404"),
    );
    const failed = transitionPocTilesState(createPocTilesState(), {
      type: "fail",
      message: failure.message,
      kind: failure.kind,
      httpStatus: failure.httpStatus,
    });
    const recovering = transitionPocTilesState(failed, { type: "start" });
    const ready = transitionPocTilesState(recovering, { type: "contentLoaded" });

    expect(failure).toMatchObject({ kind: "http", httpStatus: 404 });
    expect(failed).toMatchObject({ phase: "failed", errorKind: "http", httpStatus: 404 });
    expect(recovering).toEqual({ phase: "loading" });
    expect(ready).toEqual({ phase: "ready", error: undefined });
  });

  it("classifies invalid JSON and child-resource failures independently", () => {
    expect(
      classifyPocTilesFailure(defaultPocTilesConfig.invalidJsonTilesetUrl, new Error("Unexpected end")),
    ).toMatchObject({ kind: "parse" });
    expect(
      classifyPocTilesFailure("/__poc_3dt__/missing/child/missing-child.gltf", new Error("HTTP 404")),
    ).toMatchObject({ kind: "child", httpStatus: 404 });
  });

  it("keeps the repository-local glTF fixture as a factory that encloses the lifter", () => {
    const fixturePath = resolve(process.cwd(), "public/poc-3dtiles/minimal/minimal.gltf");
    const tilesetPath = resolve(process.cwd(), "public/poc-3dtiles/minimal/tileset.json");
    const fixture = JSON.parse(readFileSync(fixturePath, "utf8")) as {
      asset: { version: string };
      nodes: Array<{ name: string; mesh: number; translation: number[]; scale: number[] }>;
      materials: Array<{ name: string; doubleSided: boolean }>;
      meshes: Array<{ primitives: Array<{ indices: number; material: number }> }>;
      buffers: Array<{ byteLength: number; uri: string }>;
    };
    const tileset = JSON.parse(readFileSync(tilesetPath, "utf8")) as {
      asset: { gltfUpAxis: string };
      root: { boundingVolume: { box: number[] }; content: { uri: string } };
    };
    const bytes = Buffer.from(fixture.buffers[0].uri.split(",")[1], "base64");
    const dataView = new DataView(bytes.buffer, bytes.byteOffset, bytes.byteLength);
    const positions = Array.from({ length: 8 }, (_, index) => [
      dataView.getFloat32(index * 12, true),
      dataView.getFloat32(index * 12 + 4, true),
      dataView.getFloat32(index * 12 + 8, true),
    ]);
    const indices = Array.from({ length: 36 }, (_, index) => dataView.getUint16(96 + index * 2, true));
    const nodes = Object.fromEntries(fixture.nodes.map((node) => [node.name, node]));

    expect(fixture.asset.version).toBe("2.0");
    expect(fixture.buffers[0].byteLength).toBe(168);
    expect(bytes.byteLength).toBe(fixture.buffers[0].byteLength);
    expect(positions).toContainEqual([-0.5, -0.5, -0.5]);
    expect(positions).toContainEqual([0.5, 0.5, 0.5]);
    expect(indices).toHaveLength(36);
    expect(new Set(indices)).toEqual(new Set([0, 1, 2, 3, 4, 5, 6, 7]));
    expect(fixture.materials[0]).toMatchObject({
      name: "poc-factory-structure",
      doubleSided: false,
    });
    expect(fixture.materials[1]).toMatchObject({ name: "poc-factory-floor" });
    expect(nodes["poc-factory-floor"]).toMatchObject({
      mesh: 1,
      translation: [0, 0, -0.1],
      scale: [6.5, 7.5, 0.2],
    });
    expect(nodes["poc-factory-left-wall"]).toMatchObject({
      mesh: 0,
      translation: [-3.125, 0, 13],
      scale: [0.25, 7.5, 26],
    });
    expect(nodes["poc-factory-right-wall"]).toMatchObject({
      translation: [3.125, 0, 13],
      scale: [0.25, 7.5, 26],
    });
    expect(nodes["poc-factory-back-wall"]).toMatchObject({
      translation: [0, 3.625, 13],
      scale: [6.5, 0.25, 26],
    });
    expect(nodes["poc-factory-front-left-pier"]).toMatchObject({
      translation: [-2.5, -3.625, 13],
      scale: [1.5, 0.25, 26],
    });
    expect(nodes["poc-factory-front-right-pier"]).toMatchObject({
      translation: [2.5, -3.625, 13],
      scale: [1.5, 0.25, 26],
    });
    expect(nodes["poc-factory-front-lintel"]).toMatchObject({
      translation: [0, -3.625, 25],
      scale: [6.5, 0.25, 2],
    });
    expect(nodes["poc-factory-roof"]).toMatchObject({
      translation: [0, 0, 26.125],
      scale: [6.5, 7.5, 0.25],
    });
    expect(tileset.asset.gltfUpAxis).toBe("Z");
    expect(tileset.root.content.uri).toBe("minimal.gltf");
    expect(tileset.root.boundingVolume.box).toEqual([
      0, 0, 13.025,
      3.25, 0, 0,
      0, 3.75, 0,
      0, 0, 13.225,
    ]);
  });

  it("keeps separate POC-only fixtures for child-tile and JSON-parse failure", () => {
    const childFixturePath = resolve(process.cwd(), "public/poc-3dtiles/child-missing/tileset.json");
    const invalidJsonFixturePath = resolve(process.cwd(), "public/poc-3dtiles/invalid-json/tileset.json");
    const childFixture = JSON.parse(readFileSync(childFixturePath, "utf8")) as {
      asset: { version: string };
      root: {
        content: { uri: string };
        children: Array<{ content: { uri: string } }>;
      };
    };

    expect(childFixture.asset.version).toBe("1.0");
    expect(childFixture.root.content.uri).toBe("../minimal/minimal.gltf");
    expect(childFixture.root.children).toHaveLength(1);
    expect(childFixture.root.children[0]?.content.uri).toBe(
      "/__poc_3dt__/missing/child/missing-child.gltf",
    );
    expect(readFileSync(invalidJsonFixturePath, "utf8")).not.toSatisfy((value: string) => {
      try {
        JSON.parse(value);
        return true;
      } catch {
        return false;
      }
    });
  });

  it("keeps the synthetic POC lifter fixture explicit and moves only its platform on world Z", () => {
    const fixturePath = resolve(process.cwd(), "public/poc-3dtiles/poc-lifter/poc-lifter.gltf");
    const fixture = JSON.parse(readFileSync(fixturePath, "utf8")) as {
      asset: { generator: string };
      nodes: Array<{ name: string; children?: number[] }>;
    };
    const rootNode = fixture.nodes.find((node) => node.name === "lifter-root");
    const frameNode = fixture.nodes.find((node) => node.name === "lifter-frame");
    const platformNode = fixture.nodes.find((node) => node.name === "lifter-platform");
    const root = new Group();
    root.position.set(8, -4, 0);
    const platform = new Group();
    platform.name = "lifter-platform";
    root.add(platform);
    root.updateMatrixWorld(true);

    expect(fixture.asset.generator).toContain("POC-3DT-01 synthetic fixture");
    expect(rootNode?.children).toEqual(expect.arrayContaining([1, 2]));
    expect(frameNode).toBeDefined();
    expect(platformNode).toBeDefined();
    expect(pocWorldZTargets).toEqual([0, 6, 12]);

    for (const target of pocWorldZTargets) {
      expect(setPocObjectWorldZ(platform, target)).toBeCloseTo(target, 6);
    }
    expect(platform.name).toBe("lifter-platform");
  });

  it("exports actual runtime diagnostics without inventing unmeasured performance", () => {
    const diagnostics = createPocRuntimeDiagnostics({
      browser: "Chrome",
      userAgent: "unit-test-browser",
      webglAvailable: true,
      canvasCount: 1,
      rendererCount: 1,
      tilesetUrl: defaultPocTilesConfig.tilesetUrl,
      glbUrl: "/poc-3dtiles/poc-lifter/poc-lifter.gltf",
      selectedObject: "lifter-platform",
      worldZ: 6,
      lifecycle: [{ round: 1, enteredAt: "2026-07-21T00:00:00.000Z", released: true }],
    });
    const evidence = createPocEvidencePayload(diagnostics);

    expect(evidence).toMatchObject({
      browser: "Chrome",
      webglAvailable: true,
      canvasCount: 1,
      rendererCount: 1,
      selectedObject: "lifter-platform",
      worldZ: { low: null, middle: 6, high: null },
    });
    expect(evidence.performance).toMatchObject({ fps: null, drawCalls: null, memoryMb: null });
    expect(evidence.lifecycle).toHaveLength(1);
  });

  it("records loaded GLB nodes without inferring a movable platform", () => {
    const root = new Group();
    root.name = "real-lifter-root";
    root.position.set(1, 2, 3);
    const cadNode = new Group();
    cadNode.name = "NAUO_001";
    const meshNode = new Group();
    meshNode.name = "CAD_PART";
    cadNode.add(meshNode);
    root.add(cadNode);
    root.updateMatrixWorld(true);

    expect(collectPocGlbNodeRecords(root)).toEqual([
      expect.objectContaining({ name: "real-lifter-root", parent: null, childCount: 1, isMesh: false }),
      expect.objectContaining({ name: "NAUO_001", parent: "real-lifter-root", childCount: 1, isMesh: false }),
      expect.objectContaining({ name: "CAD_PART", parent: "NAUO_001", childCount: 0, isMesh: false }),
    ]);
  });

  it("does not advance a POC lifecycle round until content reaches ready", async () => {
    const gate = createPocReadyGate();
    const waiting = gate.waitForReady(1_000);
    gate.notify("loading");
    gate.notify("ready");

    await expect(waiting).resolves.toBe(true);
    gate.notify("ready");
    await expect(gate.waitForReady(1_000)).resolves.toBe(true);
  });
});
