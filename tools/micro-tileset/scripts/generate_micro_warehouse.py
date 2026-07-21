"""Generate the POC-3DT-01 micro warehouse tiles in Blender background mode."""

import argparse
import json
import math
import os
import sys
from datetime import datetime, timezone

import bpy
from mathutils import Vector


AREA_CENTERS = {
    "area-a": (-20.0, 15.0),
    "area-b": (20.0, 15.0),
    "area-c": (-20.0, -15.0),
    "area-d": (20.0, -15.0),
}


def parse_arguments():
    arguments = sys.argv[sys.argv.index("--") + 1 :] if "--" in sys.argv else []
    parser = argparse.ArgumentParser()
    parser.add_argument("--output", required=True, help="Directory for GLB files and tile-metadata.json.")
    return parser.parse_args(arguments)


def reset_scene():
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)

    for collection in list(bpy.data.collections):
        bpy.data.collections.remove(collection)

    scene = bpy.context.scene
    scene.unit_settings.system = "METRIC"
    scene.unit_settings.length_unit = "METERS"
    scene.unit_settings.scale_length = 1.0


def create_collection(name):
    collection = bpy.data.collections.new(name)
    bpy.context.scene.collection.children.link(collection)
    return collection


def create_material(name, color):
    material = bpy.data.materials.new(name)
    material.diffuse_color = (*color, 1.0)
    return material


def create_box(collection, name, location, dimensions, material):
    bpy.ops.mesh.primitive_cube_add(size=2.0, location=location)
    obj = bpy.context.object
    obj.name = name
    obj.scale = tuple(value / 2.0 for value in dimensions)

    for linked_collection in list(obj.users_collection):
        linked_collection.objects.unlink(obj)
    collection.objects.link(obj)
    obj.data.materials.append(material)
    return obj


def create_root_tile(collection, materials):
    create_box(collection, "root-ground", (0.0, 0.0, -0.15), (80.0, 60.0, 0.3), materials["concrete"])
    create_box(collection, "root-wall-north", (0.0, 29.75, 7.5), (80.0, 0.5, 15.0), materials["wall"])
    create_box(collection, "root-wall-south", (0.0, -29.75, 7.5), (80.0, 0.5, 15.0), materials["wall"])
    create_box(collection, "root-wall-east", (39.75, 0.0, 7.5), (0.5, 60.0, 15.0), materials["wall"])
    create_box(collection, "root-wall-west", (-39.75, 0.0, 7.5), (0.5, 60.0, 15.0), materials["wall"])

    for x in (-36.0, -12.0, 12.0, 36.0):
        for y in (-24.0, 0.0, 24.0):
            create_box(collection, f"root-column-{x:g}-{y:g}", (x, y, 7.5), (0.8, 0.8, 15.0), materials["steel"])

    for area_id, (x, y) in AREA_CENTERS.items():
        create_box(collection, f"root-zone-{area_id}", (x, y, 0.03), (36.0, 26.0, 0.06), materials["zone"])


def create_rack(collection, prefix, center_x, center_y, materials):
    height = 7.2
    half_width = 0.55
    half_length = 6.0
    post_size = (0.14, 0.14, height)

    for x_offset in (-half_width, half_width):
        for y_offset in (-half_length, half_length):
            create_box(
                collection,
                f"{prefix}-post-{x_offset:g}-{y_offset:g}",
                (center_x + x_offset, center_y + y_offset, height / 2.0),
                post_size,
                materials["rack"],
            )

    for level in (1.2, 3.1, 5.0, 6.9):
        create_box(
            collection,
            f"{prefix}-shelf-{level:g}",
            (center_x, center_y, level),
            (1.2, 12.3, 0.16),
            materials["rack"],
        )


def create_area_tile(collection, area_id, center, materials):
    center_x, center_y = center
    for index, (x_offset, y_offset) in enumerate(((-10.0, -8.0), (-10.0, 8.0), (10.0, -8.0), (10.0, 8.0)), start=1):
        create_rack(collection, f"{area_id}-rack-{index}", center_x + x_offset, center_y + y_offset, materials)


def mesh_objects(collection):
    return [obj for obj in collection.objects if obj.type == "MESH"]


def blender_bounds(objects):
    points = [obj.matrix_world @ Vector(corner) for obj in objects for corner in obj.bound_box]
    minimum = [min(point[index] for point in points) for index in range(3)]
    maximum = [max(point[index] for point in points) for index in range(3)]
    return {"min": minimum, "max": maximum}


def blender_to_gltf(vector):
    return [vector[0], vector[2], -vector[1]]


def gltf_bounds(native_bounds):
    corners = [
        (x, y, z)
        for x in (native_bounds["min"][0], native_bounds["max"][0])
        for y in (native_bounds["min"][1], native_bounds["max"][1])
        for z in (native_bounds["min"][2], native_bounds["max"][2])
    ]
    transformed = [blender_to_gltf(corner) for corner in corners]
    return {
        "min": [min(point[index] for point in transformed) for index in range(3)],
        "max": [max(point[index] for point in transformed) for index in range(3)],
    }


def export_glb(objects, output_path):
    bpy.ops.object.select_all(action="DESELECT")
    for obj in objects:
        obj.select_set(True)
    bpy.context.view_layer.objects.active = objects[0]
    bpy.ops.export_scene.gltf(filepath=output_path, export_format="GLB", use_selection=True, export_yup=True)


def tile_metadata(tile_id, collection, output_directory):
    objects = mesh_objects(collection)
    native_bounds = blender_bounds(objects)
    converted_bounds = gltf_bounds(native_bounds)
    extents = [converted_bounds["max"][index] - converted_bounds["min"][index] for index in range(3)]
    geometric_error = math.ceil(max(extents)) if tile_id == "root" else 0

    export_glb(objects, os.path.join(output_directory, f"{tile_id}.glb"))

    return {
        "id": tile_id,
        "contentUri": f"{tile_id}.glb",
        "blenderWorldBounds": native_bounds,
        "bounds": converted_bounds,
        "geometricError": geometric_error,
        "stats": {
            "nodeCount": len(objects),
            "meshCount": len({obj.data.name for obj in objects}),
            "triangleCount": sum(sum(max(0, len(polygon.vertices) - 2) for polygon in obj.data.polygons) for obj in objects),
        },
    }


def main():
    args = parse_arguments()
    output_directory = os.path.abspath(args.output)
    os.makedirs(output_directory, exist_ok=True)

    reset_scene()
    materials = {
        "concrete": create_material("concrete", (0.34, 0.38, 0.42)),
        "wall": create_material("wall", (0.72, 0.75, 0.78)),
        "steel": create_material("steel", (0.18, 0.23, 0.28)),
        "zone": create_material("zone", (0.14, 0.26, 0.34)),
        "rack": create_material("rack", (0.84, 0.38, 0.08)),
    }

    collections = {"root": create_collection("tile-root")}
    create_root_tile(collections["root"], materials)
    for area_id, center in AREA_CENTERS.items():
        collections[area_id] = create_collection(f"tile-{area_id}")
        create_area_tile(collections[area_id], area_id, center, materials)

    bpy.context.view_layer.update()
    tile_order = ("root", "area-a", "area-b", "area-c", "area-d")
    metadata = {
        "schemaVersion": 1,
        "generatedAtUtc": datetime.now(timezone.utc).replace(microsecond=0).isoformat(),
        "coordinateSystem": {
            "unit": "meter",
            "blender": {
                "origin": "warehouse floor center (0, 0, 0)",
                "axes": "X=east-west, Y=north-south, Z=up",
            },
            "gltf": {
                "origin": "warehouse floor center (0, 0, 0)",
                "axes": "X=east-west, Y=up, Z=south",
                "fromBlender": "[x, z, -y]",
            },
        },
        "tiles": [tile_metadata(tile_id, collections[tile_id], output_directory) for tile_id in tile_order],
    }

    metadata_path = os.path.join(output_directory, "tile-metadata.json")
    with open(metadata_path, "w", encoding="utf-8", newline="\n") as metadata_file:
        json.dump(metadata, metadata_file, ensure_ascii=False, indent=2)
        metadata_file.write("\n")


if __name__ == "__main__":
    main()
