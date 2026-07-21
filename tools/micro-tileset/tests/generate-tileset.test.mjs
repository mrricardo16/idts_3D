import assert from "node:assert/strict";
import test from "node:test";

import { createTileset } from "../scripts/generate-tileset.mjs";

const metadata = {
  schemaVersion: 1,
  tiles: [
    {
      id: "root",
      contentUri: "root.glb",
      bounds: { min: [-40, 0, -30], max: [40, 15, 30] },
      geometricError: 80,
    },
    {
      id: "area-a",
      contentUri: "area-a.glb",
      bounds: { min: [-38, 0, 2], max: [-2, 8, 28] },
      geometricError: 0,
    },
    {
      id: "area-b",
      contentUri: "area-b.glb",
      bounds: { min: [2, 0, 2], max: [38, 8, 28] },
      geometricError: 0,
    },
  ],
};

test("creates a 3D Tiles 1.1 tree with measured box volumes and GLB content", () => {
  const tileset = createTileset(metadata);

  assert.deepEqual(tileset.asset, { version: "1.1" });
  assert.equal(tileset.root.content.uri, "root.glb");
  assert.deepEqual(tileset.root.boundingVolume.box, [0, 7.5, 0, 40, 0, 0, 0, 7.5, 0, 0, 0, 30]);
  assert.equal(tileset.root.children.length, 2);
  assert.equal(tileset.root.children[0].content.uri, "area-a.glb");
  assert.equal(tileset.root.children[0].geometricError, 0);
});

test("rejects non-relative and non-GLB content URIs", () => {
  const invalid = structuredClone(metadata);
  invalid.tiles[1].contentUri = "https://example.test/area-a.b3dm";

  assert.throws(() => createTileset(invalid), /relative GLB URI/);
});
