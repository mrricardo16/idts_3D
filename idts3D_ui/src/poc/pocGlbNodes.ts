import { Mesh, Vector3, type Object3D } from "three";

export interface PocGlbNodeRecord {
  name: string;
  type: string;
  parent: string | null;
  childCount: number;
  worldPosition: { x: number; y: number; z: number };
  isMesh: boolean;
  isExplicitMovablePart: boolean;
}

export function collectPocGlbNodeRecords(root: Object3D): PocGlbNodeRecord[] {
  root.updateMatrixWorld(true);
  const records: PocGlbNodeRecord[] = [];
  root.traverse((object) => {
    const worldPosition = object.getWorldPosition(new Vector3());
    records.push({
      name: object.name || "(unnamed)",
      type: object.type,
      parent: object === root ? null : object.parent?.name || "(unnamed)",
      childCount: object.children.length,
      worldPosition: {
        x: worldPosition.x,
        y: worldPosition.y,
        z: worldPosition.z,
      },
      isMesh: object instanceof Mesh,
      isExplicitMovablePart: object.name === "lifter-platform",
    });
  });
  return records;
}
