import { BufferGeometry, Float32BufferAttribute, Group, Line, LineBasicMaterial, Mesh, MeshBasicMaterial, Object3D } from "three";
import { describe, expect, it } from "vitest";
import { collectModelObjectTree } from "../../src/engine/ModelStructure";

function createTriangleGeometry(): BufferGeometry {
  const geometry = new BufferGeometry();
  geometry.setAttribute("position", new Float32BufferAttribute([0, 0, 0, 2, 4, 6, 0, 0, 6], 3));
  return geometry;
}

describe("collectModelObjectTree", () => {
  it("collects supported nodes with hierarchy, transforms, and bounding boxes", () => {
    const root = new Group();
    root.name = "root";
    root.position.set(10, 0, 0);
    const container = new Object3D();
    container.name = "container";
    container.position.set(1, 2, 3);
    const mesh = new Mesh(createTriangleGeometry(), new MeshBasicMaterial());
    mesh.name = "load-platform";
    mesh.position.set(2, 0, 0);
    mesh.rotation.set(0, 0, 0);
    mesh.scale.set(2, 3, 4);
    root.add(container);
    container.add(mesh);
    root.updateMatrixWorld(true);

    const nodes = collectModelObjectTree(root);
    const meshNode = nodes.find((node) => node.uuid === mesh.uuid);

    expect(nodes).toHaveLength(3);
    expect(meshNode).toMatchObject({
      name: "load-platform",
      parentName: "container",
      parentUuid: container.uuid,
      depth: 1,
      childrenCount: 0,
      position: { x: 2, y: 0, z: 0 },
      worldPosition: { x: 13, y: 2, z: 3 },
      rotation: { x: 0, y: 0, z: 0 },
      scale: { x: 2, y: 3, z: 4 },
      boundingBox: {
        min: { x: 13, y: 2, z: 3 },
        max: { x: 17, y: 14, z: 27 },
        size: { x: 4, y: 12, z: 24 },
      },
    });
  });

  it("filters non-target Three.js object types", () => {
    const root = new Group();
    root.name = "root";
    const line = new Line(createTriangleGeometry(), new LineBasicMaterial());
    line.name = "guide-line";
    root.add(line);

    const nodes = collectModelObjectTree(root);

    expect(nodes.map((node) => node.name)).toEqual(["root"]);
    expect(nodes.some((node) => node.uuid === line.uuid)).toBe(false);
  });

  it("creates stable unnamed labels and serializes user data without a geometry box", () => {
    const root = new Group();
    root.name = "root";
    const unnamedMesh = new Mesh(new BufferGeometry(), new MeshBasicMaterial());
    const unnamedObject = new Object3D();
    unnamedObject.userData = {
      enabled: true,
      label: "operator",
      retries: 2,
      clearedAt: null,
      metadata: { source: "test" },
    };
    root.add(unnamedMesh, unnamedObject);

    const nodes = collectModelObjectTree(root);
    const objectNode = nodes.find((node) => node.uuid === unnamedObject.uuid);

    expect(nodes.find((node) => node.uuid === unnamedMesh.uuid)?.name).toBe("unnamed_mesh_001");
    expect(objectNode).toMatchObject({
      name: "unnamed_mesh_002",
      originalName: "",
      boundingBox: null,
      userData: {
        enabled: true,
        label: "operator",
        retries: 2,
        clearedAt: null,
        metadata: "[object Object]",
      },
    });
  });
});
