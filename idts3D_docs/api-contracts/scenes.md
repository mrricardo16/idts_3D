# API 契约：scene manifest

## DOC-3DT-02 契约边界

本文件当前的 API 契约不因本轮文档而变更。现有 tilesets 空数组只是兼容占位，不能作为正式 3D Tiles 静态底座契约或已实现能力。

正式演进方向为 baseLayers + devices：baseLayers 表达 3D Tiles 等场景级静态底座资源，devices 表达 GLB 动态设备及其模型绑定。该方向的 JSON、DTO、TypeScript、版本兼容和数据库候选仅见 scene-resource-manifest-design.md 的设计草案；在 MVP-10A 解除 Blocked 并经过用户审核前，不得修改当前接口、DTO、TypeScript 类型或数据库。


## GET /api/scenes/{sceneId}/manifest

| 项 | 值 |
|---|---|
| Method | `GET` |
| Route | `/api/scenes/{sceneId}/manifest` |
| Query 参数 | `mode?: monitor\|edit` |
| Path 参数 | `sceneId: long` |
| Request Body | 无 |
| monitor 模式 | 允许，只返回 active binding + Published asset version |
| edit 模式 | 允许，可按任务卡扩展读取 Draft / Ready，但 MVP 默认仍返回 Published |
| 前端 TS interface | `SceneManifestQuery`, `SceneManifestResponse` |
| 后端 DTO | `GetSceneManifestRequest`, `SceneManifestResponse` |
| 后端实体 | `SceneNode`, `DeviceInstance`, `DeviceModelBinding`, `ModelAsset`, `AssetVersion` |
| 读取表 | `scene_node`, `device_instance`, `device_model_binding`, `model_asset`, `asset_version`, `asset_manifest` |
| 写入表 | 无 |

Response Body：

```json
{
  "success": true,
  "code": "OK",
  "message": "",
  "data": {
    "sceneId": 1,
    "sceneCode": "DEFAULT_SCENE",
    "sceneName": "默认场景",
    "devices": [
      {
        "deviceId": 1,
        "deviceCode": "LIFTER-001",
        "deviceName": "提升机 001",
        "modelAssetId": 1,
        "assetVersionId": 1,
        "manifestUrl": "/api/model-assets/1/manifest?versionId=1&mode=monitor",
        "position": { "x": 0, "y": 0, "z": 0 },
        "rotation": { "x": 0, "y": 0, "z": 0 },
        "scale": { "x": 1, "y": 1, "z": 1 }
      }
    ],
    "tilesets": []
  }
}
```

字段类型：

| 字段 | 类型 | 说明 |
|---|---|---|
| `sceneId` | long | scene_node 主键 |
| `sceneCode` | string | 场景编码 |
| `devices` | array | 当前场景设备 |
| `manifestUrl` | string | 后端生成的 model manifest URL |
| `position` / `rotation` / `scale` | object | 设备在场景中的 transform |
| `tilesets` | array | MVP 返回空数组，保留 3D Tiles 扩展 |

枚举值：`mode` 只允许 `monitor` / `edit`。

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
  "code": "NOT_FOUND",
  "message": "scene 不存在。",
  "errors": [{ "field": "sceneId", "message": "未找到 sceneId=1。" }]
}
```

409 示例：

```json
{
  "success": false,
  "code": "VERSION_STATUS_INVALID",
  "message": "scene 中存在非 Published active 绑定。",
  "errors": [{ "field": "deviceCode", "message": "LIFTER-001 绑定的版本不是 Published。" }]
}
```
