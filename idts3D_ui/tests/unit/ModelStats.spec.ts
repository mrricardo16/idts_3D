import { BufferGeometry, Float32BufferAttribute, Mesh, MeshBasicMaterial, Object3D, Texture } from "three";
import type { WebGLRenderer } from "three";
import { describe, expect, it } from "vitest";
import { collectModelPerformanceStats, collectModelStructureStats, createModelPerformanceStats } from "../../src/engine/ModelStats";

function createIndexedGeometry(): BufferGeometry {
  const geometry = new BufferGeometry();
  geometry.setAttribute("position", new Float32BufferAttribute([0, 0, 0, 2, 0, 0, 2, 2, 0, 0, 2, 0], 3));
  geometry.setIndex([0, 1, 2, 0, 2, 3]);
  return geometry;
}

function createNonIndexedGeometry(): BufferGeometry {
  const geometry = new BufferGeometry();
  geometry.setAttribute("position", new Float32BufferAttribute([0, 0, 0, 0, 1, 0, 0, 0, 1, 1, 0, 0, 1, 1, 0, 1, 0, 1], 3));
  return geometry;
}

function createRenderer(): WebGLRenderer {
  return {
    info: {
      render: { calls: 7, triangles: 18 },
      memory: { geometries: 3, textures: 2 },
    },
  } as unknown as WebGLRenderer;
}

describe("ModelStats", () => {
  it("counts indexed and non-indexed mesh geometry", () => {
    const root = new Object3D();
    root.name = "root";
    root.add(
      new Mesh(createIndexedGeometry(), new MeshBasicMaterial()),
      new Mesh(createNonIndexedGeometry(), new MeshBasicMaterial()),
    );

    const stats = collectModelStructureStats(root);

    expect(stats.meshCount).toBe(2);
    expect(stats.triangleCount).toBe(4);
    expect(stats.vertexCount).toBe(10);
    expect(stats.boxSize).toEqual({ x: 2, y: 2, z: 1 });
  });

  it("deduplicates shared materials and textures", () => {
    const root = new Object3D();
    const texture = new Texture();
    const sharedMaterial = new MeshBasicMaterial({ map: texture });
    const secondMaterial = new MeshBasicMaterial({ map: texture });
    root.add(
      new Mesh(createIndexedGeometry(), sharedMaterial),
      new Mesh(createNonIndexedGeometry(), [sharedMaterial, secondMaterial]),
    );

    const stats = collectModelStructureStats(root);

    expect(stats.materialCount).toBe(2);
    expect(stats.textureCount).toBe(1);
  });

  it("keeps the first one hundred named nodes in traversal order", () => {
    const root = new Object3D();
    root.name = "root";
    for (let index = 1; index <= 101; index += 1) {
      const child = new Object3D();
      child.name = `node-${index}`;
      root.add(child);
    }

    const stats = collectModelStructureStats(root);

    expect(stats.first100NodeNames).toHaveLength(100);
    expect(stats.first100NodeNames.slice(0, 3)).toEqual(["root", "node-1", "node-2"]);
    expect(stats.first100NodeNames.at(-1)).toBe("node-99");
  });

  it("maps renderer and runtime metrics without constructing a WebGL renderer", () => {
    const stats = createModelPerformanceStats(
      {
        meshCount: 2,
        materialCount: 3,
        textureCount: 4,
        vertexCount: 5,
        triangleCount: 6,
        boxSize: { x: 1, y: 2, z: 3 },
        first100NodeNames: ["root"],
      },
      createRenderer(),
      "high",
      "/models/lifter.high.glb",
      60,
      { sceneMode: "area", deviceCount: 8, modelInstanceCount: 12 },
    );

    expect(stats).toMatchObject({
      sceneMode: "area",
      deviceCount: 8,
      modelInstanceCount: 12,
      currentLevel: "high",
      currentUrl: "/models/lifter.high.glb",
      fps: 60,
      rendererRenderCalls: 7,
      rendererRenderTriangles: 18,
      rendererMemoryGeometries: 3,
      rendererMemoryTextures: 2,
    });
  });

  it("combines collected structure and supplied renderer metrics", () => {
    const root = new Object3D();
    root.add(new Mesh(createIndexedGeometry(), new MeshBasicMaterial()));

    const stats = collectModelPerformanceStats(root, createRenderer(), "source", "/models/lifter.glb", 30);

    expect(stats).toMatchObject({
      meshCount: 1,
      triangleCount: 2,
      currentLevel: "source",
      currentUrl: "/models/lifter.glb",
      fps: 30,
      rendererRenderCalls: 7,
    });
  });
});
