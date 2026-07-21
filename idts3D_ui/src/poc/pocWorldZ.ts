import { Vector3, type Object3D } from "three";

export const pocWorldZTargets = [0, 6, 12] as const;

export function setPocObjectWorldZ(object: Object3D, worldZ: number): number {
  const parent = object.parent;
  if (!parent) {
    object.position.z = worldZ;
    object.updateMatrixWorld(true);
    return object.getWorldPosition(new Vector3()).z;
  }

  const worldPosition = object.getWorldPosition(new Vector3());
  worldPosition.z = worldZ;
  parent.worldToLocal(worldPosition);
  object.position.copy(worldPosition);
  object.updateMatrixWorld(true);
  return object.getWorldPosition(new Vector3()).z;
}
