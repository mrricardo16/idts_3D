# API 契约：asset version 状态流转

本文件覆盖 `mark-ready`、`publish`、`archive`、`rollback`。这些接口都不允许 monitor 模式调用，只允许 edit 模式或后台管理调用。

## 1. POST /api/model-assets/{assetId}/versions/{versionId}/mark-ready

| 项 | 值 |
|---|---|
| Method | `POST` |
| Route | `/api/model-assets/{assetId}/versions/{versionId}/mark-ready` |
| Query 参数 | 无 |
| Path 参数 | `assetId: long`, `versionId: long` |
| Request Body JSON | `{ "remark": "object tree 和 model stats 已确认" }` |
| monitor 模式 | 不允许 |
| edit 模式 | 允许 |
| 前端 TS interface | `ChangeVersionStatusRequest`, `AssetVersionResponse` |
| 后端 DTO | `ChangeVersionStatusRequest`, `AssetVersionResponse` |
| 后端实体 | `AssetVersion`, `AssetManifest`, `ModelObjectIndex` |
| 读取表 | `model_asset`, `asset_version`, `asset_manifest`, `model_object_index`, `movable_part_binding`, `motion_target` |
| 写入表 | `asset_version`, `operation_audit` |

成功响应：

```json
{
  "success": true,
  "code": "OK",
  "message": "",
  "data": {
    "assetId": 1,
    "versionId": 1,
    "versionStatus": "Ready",
    "changedTime": "2026-07-09T10:00:00Z"
  }
}
```

400 示例：

```json
{
  "success": false,
  "code": "OBJECT_TREE_REQUIRED",
  "message": "标记 Ready 前必须保存 object tree。",
  "errors": []
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
  "message": "只有 Draft 版本可以标记为 Ready。",
  "errors": [{ "field": "versionStatus", "message": "当前状态为 Published。" }]
}
```

字段类型：`remark` 为可选 string，最长 500；`versionStatus` 枚举见 `README.md`。

## 2. POST /api/model-assets/{assetId}/versions/{versionId}/publish

| 项 | 值 |
|---|---|
| Method | `POST` |
| Route | `/api/model-assets/{assetId}/versions/{versionId}/publish` |
| Query 参数 | 无 |
| Path 参数 | `assetId: long`, `versionId: long` |
| Request Body JSON | `{ "remark": "发布提升机模型 v1" }` |
| monitor 模式 | 不允许 |
| edit 模式 | 允许 |
| 前端 TS interface | `ChangeVersionStatusRequest`, `AssetVersionResponse` |
| 后端 DTO | `ChangeVersionStatusRequest`, `AssetVersionResponse` |
| 后端实体 | `ModelAsset`, `AssetVersion`, `DeviceModelBinding` |
| 读取表 | `model_asset`, `asset_version`, `asset_manifest`, `model_object_index`, `movable_part_binding`, `motion_target`, `device_model_binding` |
| 写入表 | `asset_version`, `model_asset`, `device_model_binding`, `operation_audit` |

成功响应 `versionStatus` 为 `Published`。发布必须归档同一 model asset 的旧 Published 版本，并更新 `model_asset.current_version_id`。

400 示例：缺少 manifest、object tree、model stats 或 enabled movable part 校验失败。  
404 示例：asset/version 不存在。  
409 示例：当前版本不是 Ready，或 Failed / Invalid 禁止发布。

## 3. POST /api/model-assets/{assetId}/versions/{versionId}/archive

| 项 | 值 |
|---|---|
| Method | `POST` |
| Route | `/api/model-assets/{assetId}/versions/{versionId}/archive` |
| Query 参数 | 无 |
| Path 参数 | `assetId: long`, `versionId: long` |
| Request Body JSON | `{ "remark": "归档旧版本" }` |
| monitor 模式 | 不允许 |
| edit 模式 | 允许 |
| 前端 TS interface | `ChangeVersionStatusRequest`, `AssetVersionResponse` |
| 后端 DTO | `ChangeVersionStatusRequest`, `AssetVersionResponse` |
| 后端实体 | `AssetVersion`, `DeviceModelBinding` |
| 读取表 | `asset_version`, `device_model_binding` |
| 写入表 | `asset_version`, `device_model_binding`, `operation_audit` |

成功响应 `versionStatus` 为 `Archived`。如果归档 Published 版本，需要同步归档 active device model binding。

400 示例：请求缺少必要 remark 且系统要求填写审计说明。  
404 示例：版本不存在。  
409 示例：版本状态不允许归档。

## 4. POST /api/model-assets/{assetId}/versions/{versionId}/rollback

| 项 | 值 |
|---|---|
| Method | `POST` |
| Route | `/api/model-assets/{assetId}/versions/{versionId}/rollback` |
| Query 参数 | 无 |
| Path 参数 | `assetId: long`, `versionId: long` |
| Request Body JSON | `{ "remark": "回滚到上一稳定版本" }` |
| monitor 模式 | 不允许 |
| edit 模式 | 允许 |
| 前端 TS interface | `ChangeVersionStatusRequest`, `AssetVersionResponse` |
| 后端 DTO | `ChangeVersionStatusRequest`, `AssetVersionResponse` |
| 后端实体 | `ModelAsset`, `AssetVersion`, `DeviceModelBinding` |
| 读取表 | `model_asset`, `asset_version`, `device_model_binding` |
| 写入表 | `asset_version`, `model_asset`, `device_model_binding`, `operation_audit` |

成功响应 `versionStatus` 为 `Published`。回滚后同一 model asset 只能有一个 Published，同一 device instance 只能有一个 active binding。

400 示例：目标版本缺少 manifest。  
404 示例：目标版本不存在。  
409 示例：目标版本为 Failed / Invalid 或存在 active binding 冲突。
