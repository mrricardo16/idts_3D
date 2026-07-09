# API 契约：model asset 与 model manifest

## 1. POST /api/model-assets/upload

| 项 | 值 |
|---|---|
| Method | `POST` |
| Route | `/api/model-assets/upload` |
| Query 参数 | 无 |
| Path 参数 | 无 |
| Content-Type | `multipart/form-data` |
| monitor 模式 | 不允许 |
| edit 模式 | 允许 |
| 前端 TS interface | `UploadModelAssetRequest`, `UploadModelAssetResponse` |
| 后端 DTO | `UploadModelAssetRequest`, `UploadModelAssetResponse` |
| 后端实体 | `ModelAsset`, `AssetVersion`, `ModelAssetVariant`, `ModelConversionJob` |
| 读取表 | `model_asset` |
| 写入表 | `model_asset`, `asset_version`, `model_asset_variant`, `model_conversion_job`, `operation_audit` |

Request Body：

```json
{
  "file": "binary glb file",
  "assetCode": "LIFTER-001",
  "assetName": "提升机模型",
  "assetType": "device_glb",
  "sourceFileType": "glb"
}
```

字段类型：

| 字段 | 类型 | 必填 | 说明 |
|---|---|---|---|
| `file` | binary | 是 | `.glb` 文件 |
| `assetCode` | string | 是 | model asset 业务编码，同表唯一 |
| `assetName` | string | 是 | model asset 名称 |
| `assetType` | enum | 是 | `device_glb` / `static_glb` |
| `sourceFileType` | enum | 是 | MVP 只允许 `glb` |

Response Body：

```json
{
  "success": true,
  "code": "OK",
  "message": "",
  "data": {
    "assetId": 1,
    "versionId": 1,
    "jobId": 1,
    "assetCode": "LIFTER-001",
    "versionStatus": "Draft",
    "processingStatus": "pending",
    "sourceFileHash": "sha256",
    "sourceFileUrl": "/assets/models/1/1/source.glb"
  }
}
```

400 示例：

```json
{
  "success": false,
  "code": "FILE_TYPE_NOT_ALLOWED",
  "message": "MVP 阶段只允许上传 GLB 文件。",
  "errors": [{ "field": "file", "message": "文件扩展名必须是 .glb。" }]
}
```

404 示例：

```json
{
  "success": false,
  "code": "NOT_FOUND",
  "message": "上传接口不存在或路由未注册。",
  "errors": []
}
```

409 示例：

```json
{
  "success": false,
  "code": "CONFLICT",
  "message": "assetCode 已存在。",
  "errors": [{ "field": "assetCode", "message": "LIFTER-001 已存在。" }]
}
```

## 2. GET /api/model-assets/{assetId}/manifest

| 项 | 值 |
|---|---|
| Method | `GET` |
| Route | `/api/model-assets/{assetId}/manifest` |
| Query 参数 | `versionId?: long`, `mode?: monitor\|edit` |
| Path 参数 | `assetId: long` |
| Request Body | 无 |
| monitor 模式 | 允许，但只能读取 Published |
| edit 模式 | 允许读取 Draft / Ready / Published |
| 前端 TS interface | `ModelManifestQuery`, `ModelManifestResponse` |
| 后端 DTO | `GetModelManifestRequest`, `ModelManifestResponse` |
| 后端实体 | `ModelAsset`, `AssetVersion`, `ModelAssetVariant`, `AssetManifest`, `MovablePartBinding`, `MotionTarget` |
| 读取表 | `model_asset`, `asset_version`, `model_asset_variant`, `asset_manifest`, `movable_part_binding`, `motion_target` |
| 写入表 | 无 |

Response Body：

```json
{
  "success": true,
  "code": "OK",
  "message": "",
  "data": {
    "assetId": 1,
    "versionId": 3,
    "assetCode": "LIFTER-001",
    "assetName": "提升机模型",
    "versionStatus": "Published",
    "levels": {
      "source": "/assets/models/1/3/source.glb",
      "high": null,
      "medium": null,
      "low": null,
      "proxy": null
    },
    "transform": {
      "position": { "x": 0, "y": 0, "z": 0 },
      "rotationDeg": { "x": 0, "y": 0, "z": 0 },
      "scale": { "x": 1, "y": 1, "z": 1 }
    },
    "movableParts": [
      {
        "partId": 10,
        "partCode": "LIFTER_PLATFORM",
        "businessName": "载货台",
        "motionType": "linear",
        "axisMode": "world",
        "axis": "z",
        "targets": [
          { "targetId": 101, "targetCode": "F1", "targetName": "F1", "targetValue": 0 }
        ]
      }
    ]
  }
}
```

字段类型：

| 字段 | 类型 | 说明 |
|---|---|---|
| `assetId` / `versionId` | long | 数据库主键 |
| `versionStatus` | enum | `Draft` / `Ready` / `Published` / `Archived` / `Failed` / `Invalid` |
| `levels.*` | string? | 后端生成的静态文件 URL |
| `transform.position` | object | 模型 root 平移 |
| `transform.rotationDeg` | object | 模型 root 旋转角度 |
| `transform.scale` | object | 模型 root 缩放 |
| `movableParts` | array | 已启用 movable part 摘要 |

400 示例：

```json
{
  "success": false,
  "code": "VALIDATION_FAILED",
  "message": "mode 参数无效。",
  "errors": [{ "field": "mode", "message": "只允许 monitor 或 edit。" }]
}
```

404 示例：

```json
{
  "success": false,
  "code": "ASSET_NOT_FOUND",
  "message": "model asset 不存在。",
  "errors": [{ "field": "assetId", "message": "未找到 assetId=1。" }]
}
```

409 示例：

```json
{
  "success": false,
  "code": "VERSION_STATUS_INVALID",
  "message": "monitor 模式只能读取 Published 版本。",
  "errors": [{ "field": "versionStatus", "message": "当前版本为 Draft。" }]
}
```
