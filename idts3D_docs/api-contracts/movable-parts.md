# API 契约：movable part

## 1. GET /api/model-assets/{assetId}/versions/{versionId}/movable-parts

| 项 | 值 |
|---|---|
| Method | `GET` |
| Route | `/api/model-assets/{assetId}/versions/{versionId}/movable-parts` |
| Query 参数 | `enabled?: boolean` |
| Path 参数 | `assetId: long`, `versionId: long` |
| Request Body | 无 |
| monitor 模式 | 允许读取 Published |
| edit 模式 | 允许 |
| 前端 TS interface | `MovablePartListItem`, `MovablePartListResponse` |
| 后端 DTO | `GetMovablePartsRequest`, `MovablePartListResponse` |
| 后端实体 | `MovablePartBinding`, `AssetVersion` |
| 读取表 | `movable_part_binding`, `asset_version`, `model_object_index` |
| 写入表 | 无 |

Response Body：

```json
{
  "success": true,
  "code": "OK",
  "message": "",
  "data": {
    "items": [
      {
        "partId": 10,
        "assetId": 1,
        "versionId": 1,
        "objectUuid": "uuid-1",
        "objectName": "lifter-platform",
        "objectPath": "/root/lifter-platform",
        "businessName": "载货台",
        "partCode": "LIFTER_PLATFORM",
        "motionType": "linear",
        "axisMode": "world",
        "axis": "z",
        "minValue": 0,
        "maxValue": 12,
        "homeValue": 0,
        "currentValue": 0,
        "defaultSpeed": 1,
        "bindingStatus": "active",
        "enabled": true
      }
    ]
  }
}
```

400 示例：`enabled` 参数不是 boolean。  
404 示例：asset/version 不存在。  
409 示例：monitor 读取非 Published 版本。

## 2. POST /api/model-assets/{assetId}/versions/{versionId}/movable-parts

| 项 | 值 |
|---|---|
| Method | `POST` |
| Route | `/api/model-assets/{assetId}/versions/{versionId}/movable-parts` |
| Query 参数 | 无 |
| Path 参数 | `assetId: long`, `versionId: long` |
| monitor 模式 | 不允许 |
| edit 模式 | 允许 |
| 前端 TS interface | `CreateMovablePartRequest`, `MovablePartResponse` |
| 后端 DTO | `CreateMovablePartRequest`, `MovablePartResponse` |
| 后端实体 | `MovablePartBinding`, `ModelObjectIndex`, `AssetVersion` |
| 读取表 | `asset_version`, `model_object_index`, `movable_part_binding` |
| 写入表 | `movable_part_binding`, `operation_audit` |

Request Body：

```json
{
  "objectUuid": "uuid-1",
  "objectName": "lifter-platform",
  "objectPath": "/root/lifter-platform",
  "parentUuid": "uuid-root",
  "parentPath": "/root",
  "businessName": "载货台",
  "partCode": "LIFTER_PLATFORM",
  "motionType": "linear",
  "axisMode": "world",
  "axis": "z",
  "customAxisX": null,
  "customAxisY": null,
  "customAxisZ": null,
  "minValue": 0,
  "maxValue": 12,
  "homeValue": 0,
  "currentValue": 0,
  "defaultSpeed": 1,
  "enabled": true
}
```

字段类型和枚举：

| 字段 | 类型 | 必填 | 说明 |
|---|---|---|---|
| `objectUuid` | string? | 否 | 与 `objectPath` 至少填一个 |
| `objectPath` | string? | 否 | 与 `objectUuid` 至少填一个 |
| `partCode` | string | 是 | 同 asset version 唯一 |
| `businessName` | string | 是 | 业务名称 |
| `motionType` | enum | 是 | MVP 只实现 `linear` |
| `axisMode` | enum | 是 | MVP 只实现 `world` |
| `axis` | enum | 是 | MVP 只实现 `z` |
| `minValue` / `maxValue` / `homeValue` | number | 是 | 必须满足 `min <= home <= max` |
| `defaultSpeed` | number | 是 | 大于 0 |
| `enabled` | boolean | 是 | 是否启用 |

成功响应返回完整 `MovablePartResponse`。

400 示例：

```json
{
  "success": false,
  "code": "VALIDATION_FAILED",
  "message": "homeValue 必须在 minValue 和 maxValue 之间。",
  "errors": [{ "field": "homeValue", "message": "0 <= homeValue <= 12。" }]
}
```

404 示例：

```json
{
  "success": false,
  "code": "NOT_FOUND",
  "message": "object tree 中找不到该对象。",
  "errors": [{ "field": "objectUuid", "message": "uuid-1 不存在。" }]
}
```

409 示例：

```json
{
  "success": false,
  "code": "DUPLICATE_PART_CODE",
  "message": "partCode 已存在。",
  "errors": [{ "field": "partCode", "message": "LIFTER_PLATFORM 已存在。" }]
}
```

## 3. PUT /api/model-assets/{assetId}/versions/{versionId}/movable-parts/{partId}

| 项 | 值 |
|---|---|
| Method | `PUT` |
| Route | `/api/model-assets/{assetId}/versions/{versionId}/movable-parts/{partId}` |
| Query 参数 | 无 |
| Path 参数 | `assetId: long`, `versionId: long`, `partId: long` |
| Request Body JSON | 同 `CreateMovablePartRequest`，字段可按更新 DTO 要求全量提交 |
| monitor 模式 | 不允许 |
| edit 模式 | 允许 |
| 前端 TS interface | `UpdateMovablePartRequest`, `MovablePartResponse` |
| 后端 DTO | `UpdateMovablePartRequest`, `MovablePartResponse` |
| 后端实体 | `MovablePartBinding` |
| 读取表 | `asset_version`, `movable_part_binding`, `model_object_index` |
| 写入表 | `movable_part_binding`, `operation_audit` |

400 / 404 / 409 示例同新增接口，409 还包括 Published 版本禁止修改。

## 4. DELETE /api/model-assets/{assetId}/versions/{versionId}/movable-parts/{partId}

| 项 | 值 |
|---|---|
| Method | `DELETE` |
| Route | `/api/model-assets/{assetId}/versions/{versionId}/movable-parts/{partId}` |
| Query 参数 | 无 |
| Path 参数 | `assetId: long`, `versionId: long`, `partId: long` |
| Request Body | 无 |
| monitor 模式 | 不允许 |
| edit 模式 | 允许 |
| 前端 TS interface | `DeleteMovablePartResponse` |
| 后端 DTO | `DeleteMovablePartRequest`, `DeleteMovablePartResponse` |
| 后端实体 | `MovablePartBinding`, `MotionTarget` |
| 读取表 | `asset_version`, `movable_part_binding`, `motion_target` |
| 写入表 | `movable_part_binding`, `motion_target`, `operation_audit` |

成功响应：

```json
{
  "success": true,
  "code": "OK",
  "message": "",
  "data": {
    "partId": 10,
    "deleted": true
  }
}
```

400 示例：partId 无效。  
404 示例：part 不存在。  
409 示例：Published 版本不能物理删除，只允许新草稿版本禁用。
