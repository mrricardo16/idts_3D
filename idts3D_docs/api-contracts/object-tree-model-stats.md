# API 契约：object tree 与 model stats

## 1. PUT /api/model-assets/{assetId}/versions/{versionId}/object-tree

| 项 | 值 |
|---|---|
| Method | `PUT` |
| Route | `/api/model-assets/{assetId}/versions/{versionId}/object-tree` |
| Query 参数 | 无 |
| Path 参数 | `assetId: long`, `versionId: long` |
| monitor 模式 | 不允许 |
| edit 模式 | 允许 |
| 前端 TS interface | `SaveObjectTreeRequest`, `ObjectTreeResponse` |
| 后端 DTO | `SaveObjectTreeRequest`, `ObjectTreeResponse` |
| 后端实体 | `ModelAsset`, `AssetVersion`, `ModelObjectIndex` |
| 读取表 | `model_asset`, `asset_version` |
| 写入表 | `model_object_index`, `asset_manifest`, `operation_audit` |

Request Body：

```json
{
  "nodes": [
    {
      "objectUuid": "uuid-1",
      "objectName": "lifter-platform",
      "objectPath": "/root/lifter-platform",
      "parentUuid": "uuid-root",
      "parentPath": "/root",
      "objectType": "Mesh",
      "boundingBox": {
        "min": { "x": -1, "y": -1, "z": 0 },
        "max": { "x": 1, "y": 1, "z": 1 }
      },
      "meshFingerprint": "optional"
    }
  ]
}
```

Response Body：

```json
{
  "success": true,
  "code": "OK",
  "message": "",
  "data": {
    "assetId": 1,
    "versionId": 1,
    "nodeCount": 1,
    "savedTime": "2026-07-09T10:00:00Z"
  }
}
```

400 示例：

```json
{
  "success": false,
  "code": "VALIDATION_FAILED",
  "message": "object tree 节点不能为空。",
  "errors": [{ "field": "nodes", "message": "至少需要一个节点。" }]
}
```

404 示例：

```json
{
  "success": false,
  "code": "VERSION_NOT_FOUND",
  "message": "asset version 不存在。",
  "errors": []
}
```

409 示例：

```json
{
  "success": false,
  "code": "VERSION_STATUS_INVALID",
  "message": "Published 版本不能直接覆盖 object tree。",
  "errors": []
}
```

字段类型和枚举：

| 字段 | 类型 | 必填 | 说明 |
|---|---|---|---|
| `objectUuid` | string | 是 | Three.js / GLTF 对象 uuid |
| `objectName` | string | 是 | 对象名 |
| `objectPath` | string | 是 | 稳定路径 |
| `parentUuid` | string? | 否 | 父节点 uuid |
| `parentPath` | string? | 否 | 父节点路径 |
| `objectType` | string | 是 | `Group` / `Mesh` / `SkinnedMesh` 等 |
| `boundingBox` | object? | 否 | 包围盒 |
| `meshFingerprint` | string? | 否 | 重绑定辅助指纹 |

## 2. GET /api/model-assets/{assetId}/object-tree

| 项 | 值 |
|---|---|
| Method | `GET` |
| Route | `/api/model-assets/{assetId}/object-tree` |
| Query 参数 | `versionId?: long`, `mode?: monitor\|edit` |
| Path 参数 | `assetId: long` |
| Request Body | 无 |
| monitor 模式 | 允许，但默认读取 Published |
| edit 模式 | 允许 |
| 前端 TS interface | `ObjectTreeQuery`, `ObjectTreeResponse` |
| 后端 DTO | `GetObjectTreeRequest`, `ObjectTreeResponse` |
| 后端实体 | `ModelAsset`, `AssetVersion`, `ModelObjectIndex` |
| 读取表 | `model_asset`, `asset_version`, `model_object_index` |
| 写入表 | 无 |

Response Body：

```json
{
  "success": true,
  "code": "OK",
  "message": "",
  "data": {
    "assetId": 1,
    "versionId": 1,
    "nodes": []
  }
}
```

400 / 404 / 409 示例同保存接口，409 用于 monitor 读取非 Published。

## 3. PUT /api/model-assets/{assetId}/versions/{versionId}/model-stats

| 项 | 值 |
|---|---|
| Method | `PUT` |
| Route | `/api/model-assets/{assetId}/versions/{versionId}/model-stats` |
| Query 参数 | 无 |
| Path 参数 | `assetId: long`, `versionId: long` |
| monitor 模式 | 不允许 |
| edit 模式 | 允许 |
| 前端 TS interface | `SaveModelStatsRequest`, `ModelStatsResponse` |
| 后端 DTO | `SaveModelStatsRequest`, `ModelStatsResponse` |
| 后端实体 | `AssetManifest`, `ModelAssetVariant` |
| 读取表 | `model_asset`, `asset_version` |
| 写入表 | `asset_manifest`, `model_asset_variant`, `operation_audit` |

Request Body：

```json
{
  "fileSizeMb": 25.6,
  "meshCount": 120,
  "materialCount": 12,
  "textureCount": 8,
  "vertexCount": 100000,
  "triangleCount": 180000,
  "drawCallEstimate": 120,
  "maxTextureSize": 2048,
  "hasMovableCandidates": true,
  "hasDuplicatedNames": false,
  "hasInvalidMaterials": false,
  "isOverBudget": false,
  "budgetMessages": []
}
```

Response Body：

```json
{
  "success": true,
  "code": "OK",
  "message": "",
  "data": {
    "assetId": 1,
    "versionId": 1,
    "isOverBudget": false,
    "savedTime": "2026-07-09T10:00:00Z"
  }
}
```

400 示例：`triangleCount` 小于 0 或字段缺失。  
404 示例：asset/version 不存在。  
409 示例：Published 版本不能直接覆盖 stats。

## 4. GET /api/model-assets/{assetId}/versions/{versionId}/model-stats

| 项 | 值 |
|---|---|
| Method | `GET` |
| Route | `/api/model-assets/{assetId}/versions/{versionId}/model-stats` |
| Query 参数 | 无 |
| Path 参数 | `assetId: long`, `versionId: long` |
| Request Body | 无 |
| monitor 模式 | 允许读取 Published |
| edit 模式 | 允许 |
| 前端 TS interface | `ModelStatsResponse` |
| 后端 DTO | `GetModelStatsRequest`, `ModelStatsResponse` |
| 后端实体 | `AssetManifest`, `ModelAssetVariant` |
| 读取表 | `asset_manifest`, `model_asset_variant` |
| 写入表 | 无 |

Response Body 与保存接口一致，`data` 返回完整 model stats。400 / 404 / 409 示例同上。
