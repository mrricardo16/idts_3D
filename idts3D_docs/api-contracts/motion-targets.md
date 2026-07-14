# API 契约：motion target

## 1. GET /api/movable-parts/{partId}/motion-targets

| 项 | 值 |
|---|---|
| Method | `GET` |
| Route | `/api/movable-parts/{partId}/motion-targets` |
| Query 参数 | `enabled?: boolean`, `mode?: string` |
| Path 参数 | `partId: long` |
| Request Body | 无 |
| monitor 模式 | 仅允许读取 Published 对应 part |
| edit 模式 | 允许读取 Draft、Ready、Published 对应 part |
| 前端 TS interface | `MotionTargetListItem`, `MotionTargetListResponse` |
| 后端 DTO | `GetMotionTargetsRequest`, `MotionTargetListResponse` |
| 后端实体 | `MotionTarget`, `MovablePartBinding`, `AssetVersion` |
| 读取表 | `motion_target`, `movable_part_binding`, `asset_version` |
| 写入表 | 无 |

Response Body：

```json
{
  "success": true,
  "code": "OK",
  "message": "",
  "data": {
    "partId": 10,
    "items": [
      {
        "targetId": 101,
        "targetCode": "F1",
        "targetName": "F1",
        "targetValue": 0,
        "targetX": null,
        "targetY": null,
        "targetZ": 0,
        "sortNo": 1,
        "enabled": true
      }
    ]
  }
}
```

400 示例：enabled 参数无效。  
404 示例：movable part 不存在。  
409 示例：monitor 读取非 Published 配置。

读取规则：`mode` 缺失、`null` 或空白时为 `monitor`；`monitor` 只允许 Published，`edit` 只允许 Draft、Ready、Published，其他值返回 `400 VALIDATION_FAILED`。列表按 `sortNo`、`targetCode`、`targetId` 稳定排序；无数据时返回 `200` 和 `items: []`。

## 2. POST /api/movable-parts/{partId}/motion-targets

| 项 | 值 |
|---|---|
| Method | `POST` |
| Route | `/api/movable-parts/{partId}/motion-targets` |
| Query 参数 | 无 |
| Path 参数 | `partId: long` |
| monitor 模式 | 不允许 |
| edit 模式 | 允许 |
| 前端 TS interface | `CreateMotionTargetRequest`, `MotionTargetResponse` |
| 后端 DTO | `CreateMotionTargetRequest`, `MotionTargetResponse` |
| 后端实体 | `MotionTarget`, `MovablePartBinding` |
| 读取表 | `movable_part_binding`, `asset_version`, `motion_target` |
| 写入表 | `motion_target`, `operation_audit` |

Request Body：

```json
{
  "targetCode": "F1",
  "targetName": "F1",
  "targetValue": 0,
  "targetX": null,
  "targetY": null,
  "targetZ": 0,
  "sortNo": 1,
  "enabled": true
}
```

字段类型和枚举：

| 字段 | 类型 | 必填 | 说明 |
|---|---|---|---|
| `targetCode` | string | 是 | 同 movable part 下唯一 |
| `targetName` | string | 是 | 显示名称 |
| `targetValue` | number? | MVP 是 | linear 模式唯一权威目标值，必须在 movable part 的 minValue/maxValue 内 |
| `targetX/Y` | number? | MVP 是 null | MVP-08 不支持三维路径点 |
| `targetZ` | number? | 否 | world-Z 展示冗余值；省略时服务端保存为 targetValue，提供时必须等于 targetValue |
| `sortNo` | int? | 是 | 必须显式提供且大于等于 0；允许重复 |
| `enabled` | boolean | 是 | 是否启用 |

成功响应返回完整 `MotionTargetResponse`。

服务端会 Trim `targetCode`、`targetName`，并将 `targetCode` 转为大写；同一 movable part 下不区分大小写。`enabled=false` 仍须通过全部字段与范围校验。Create、Update、Delete 仅允许 Draft、Ready；Published、Archived、Failed、Invalid 返回 `409 VERSION_STATUS_INVALID`。

400 示例：

```json
{
  "success": false,
  "code": "VALIDATION_FAILED",
  "message": "targetValue 超出 movable part 运动范围。",
  "errors": [{ "field": "targetValue", "message": "必须在 0 到 12 之间。" }]
}
```

404 示例：

```json
{
  "success": false,
  "code": "MOVABLE_PART_NOT_FOUND",
  "message": "movable part 不存在。",
  "errors": []
}
```

409 示例：

```json
{
  "success": false,
  "code": "DUPLICATE_TARGET_CODE",
  "message": "targetCode 已存在。",
  "errors": [{ "field": "targetCode", "message": "F1 已存在。" }]
}
```

## 3. PUT /api/movable-parts/{partId}/motion-targets/{targetId}

| 项 | 值 |
|---|---|
| Method | `PUT` |
| Route | `/api/movable-parts/{partId}/motion-targets/{targetId}` |
| Query 参数 | 无 |
| Path 参数 | `partId: long`, `targetId: long` |
| Request Body JSON | 同 `CreateMotionTargetRequest` |
| monitor 模式 | 不允许 |
| edit 模式 | 允许 |
| 前端 TS interface | `UpdateMotionTargetRequest`, `MotionTargetResponse` |
| 后端 DTO | `UpdateMotionTargetRequest`, `MotionTargetResponse` |
| 后端实体 | `MotionTarget` |
| 读取表 | `movable_part_binding`, `asset_version`, `motion_target` |
| 写入表 | `motion_target`, `operation_audit` |

400 / 404 / 409 示例同新增接口，409 还包括 Published 版本禁止修改。

## 4. DELETE /api/movable-parts/{partId}/motion-targets/{targetId}

| 项 | 值 |
|---|---|
| Method | `DELETE` |
| Route | `/api/movable-parts/{partId}/motion-targets/{targetId}` |
| Query 参数 | 无 |
| Path 参数 | `partId: long`, `targetId: long` |
| Request Body | 无 |
| monitor 模式 | 不允许 |
| edit 模式 | 允许 |
| 前端 TS interface | `DeleteMotionTargetResponse` |
| 后端 DTO | `DeleteMotionTargetRequest`, `DeleteMotionTargetResponse` |
| 后端实体 | `MotionTarget` |
| 读取表 | `motion_target`, `movable_part_binding`, `asset_version` |
| 写入表 | `motion_target`, `operation_audit` |

成功响应：

```json
{
  "success": true,
  "code": "OK",
  "message": "",
  "data": {
    "targetId": 101,
    "deleted": true
  }
}
```

400 示例：targetId 无效。  
404 示例：target 不存在。  
409 示例：Published 版本不能直接物理删除。
