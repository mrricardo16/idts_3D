import { mkdir, readFile, writeFile } from "node:fs/promises";
import path from "node:path";
import { fileURLToPath } from "node:url";

function assertFiniteVector(value, name) {
  if (!Array.isArray(value) || value.length !== 3 || !value.every(Number.isFinite)) {
    throw new Error(`${name} must be a three-element finite number array.`);
  }
}

function toBox(bounds) {
  assertFiniteVector(bounds?.min, "bounds.min");
  assertFiniteVector(bounds?.max, "bounds.max");

  const center = bounds.min.map((minimum, index) => (minimum + bounds.max[index]) / 2);
  const halfLength = bounds.min.map((minimum, index) => (bounds.max[index] - minimum) / 2);

  if (halfLength.some((value) => value < 0)) {
    throw new Error("bounds.min must not exceed bounds.max.");
  }

  return [
    center[0], center[1], center[2],
    halfLength[0], 0, 0,
    0, halfLength[1], 0,
    0, 0, halfLength[2],
  ];
}

function assertRelativeGlbUri(uri) {
  if (
    typeof uri !== "string" ||
    !uri.endsWith(".glb") ||
    uri.startsWith("/") ||
    uri.includes("\\") ||
    uri.includes(":") ||
    uri.split("/").some((segment) => segment === "." || segment === "..")
  ) {
    throw new Error("Tile content must use a relative GLB URI.");
  }
}

function toTile(tile) {
  if (!tile || typeof tile.id !== "string") {
    throw new Error("Each metadata tile must have an id.");
  }

  assertRelativeGlbUri(tile.contentUri);

  if (!Number.isFinite(tile.geometricError) || tile.geometricError < 0) {
    throw new Error(`Tile '${tile.id}' must have a non-negative geometricError.`);
  }

  return {
    boundingVolume: { box: toBox(tile.bounds) },
    geometricError: tile.geometricError,
    content: { uri: tile.contentUri },
  };
}

export function createTileset(metadata) {
  if (!metadata || metadata.schemaVersion !== 1 || !Array.isArray(metadata.tiles)) {
    throw new Error("tile-metadata.json must use schemaVersion 1 and contain tiles.");
  }

  const root = metadata.tiles.find((tile) => tile.id === "root");
  if (!root) {
    throw new Error("tile-metadata.json must contain a root tile.");
  }

  const rootTile = toTile(root);
  const childTiles = metadata.tiles
    .filter((tile) => tile.id !== "root")
    .sort((left, right) => left.id.localeCompare(right.id))
    .map(toTile);

  return {
    asset: { version: "1.1" },
    geometricError: root.geometricError,
    root: {
      ...rootTile,
      refine: "ADD",
      children: childTiles,
    },
  };
}

export async function writeTileset(metadataPath, outputPath) {
  const metadata = JSON.parse(await readFile(metadataPath, "utf8"));
  const tileset = createTileset(metadata);

  await mkdir(path.dirname(outputPath), { recursive: true });
  await writeFile(outputPath, `${JSON.stringify(tileset, null, 2)}\n`, "utf8");
}

const isDirectExecution = process.argv[1] && path.resolve(process.argv[1]) === fileURLToPath(import.meta.url);

if (isDirectExecution) {
  const [metadataPath, outputPath] = process.argv.slice(2);
  if (!metadataPath || !outputPath) {
    throw new Error("Usage: node generate-tileset.mjs <tile-metadata.json> <tileset.json>");
  }

  await writeTileset(metadataPath, outputPath);
}
