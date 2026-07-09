# API 契约总览

本文档定义 IDTS 3D MVP 后端 API 的统一规则。每个具体接口以本目录专题文档为准，任务卡只能引用，不得重新发明字段。

## 1. 统一响应结构

成功响应：

```json
{
  "success": true,
  "code": "OK",
  "message": "",
  "data": {}
}
```

错误响应：

```json
{
  "success": false,
  "code": "VERSION_STATUS_INVALID",
  "message": "monitor 模式只能读取 Published 版本。",
  "errors": []
}
```

`errors` 元素结构：

```json
{
  "field": "versionId",
  "message": "版本不存在。"
}
```

## 2. 固定枚举

| 枚举 | 值 |
|---|---|
| `AssetType` | `device_glb`, `static_glb` |
| `SourceFileType` | `glb` |
| `VariantLevel` | `source`, `high`, `medium`, `low`, `proxy` |
| `VersionStatus` | `Draft`, `Ready`, `Published`, `Archived`, `Failed`, `Invalid` |
| `RunMode` | `monitor`, `edit` |
| `MotionType` | `linear`, `rotate`, `path`, `joint` |
| `AxisMode` | `world`, `local`, `custom` |
| `Axis` | `x`, `y`, `z` |
| `BindingStatus` | `active`, `archived`, `invalid` |
| `ConversionJobType` | `upload`, `inspect`, `object_tree`, `model_stats`, `convert` |
| `ConversionJobStatus` | `pending`, `running`, `completed`, `failed`, `canceled` |

MVP 动画只实现 `MotionType=linear`、`AxisMode=world`、`Axis=z`。

## 3. 统一错误码

| code | HTTP | 使用场景 |
|---|---:|---|
| `VALIDATION_FAILED` | 400 | 请求字段缺失、格式错误、范围非法 |
| `FILE_TYPE_NOT_ALLOWED` | 400 | 上传非 GLB |
| `OBJECT_TREE_REQUIRED` | 400 | 发布或绑定前缺少 object tree |
| `MODEL_STATS_REQUIRED` | 400 | 发布前缺少 model stats |
| `NOT_FOUND` | 404 | 资源不存在 |
| `ASSET_NOT_FOUND` | 404 | model asset 不存在 |
| `VERSION_NOT_FOUND` | 404 | asset version 不存在 |
| `MOVABLE_PART_NOT_FOUND` | 404 | movable part 不存在 |
| `CONFLICT` | 409 | 唯一约束冲突 |
| `VERSION_STATUS_INVALID` | 409 | 版本状态不允许当前操作 |
| `DUPLICATE_PART_CODE` | 409 | partCode 重复 |
| `DUPLICATE_TARGET_CODE` | 409 | targetCode 重复 |

## 4. 契约文档索引

| 文档 | 覆盖接口 |
|---|---|
| `model-assets.md` | GLB 上传、model manifest |
| `object-tree-model-stats.md` | object tree、model stats |
| `asset-version.md` | mark-ready、publish、archive、rollback |
| `movable-parts.md` | movable part CRUD |
| `motion-targets.md` | motion target CRUD |
| `scenes.md` | scene manifest |
| `conversion-jobs.md` | conversion job 查询 |

## 5. 前后端对应要求

每个接口实现时必须同步：

| 层 | 位置 |
|---|---|
| C# Request / Response DTO | `idts3D_api/src/HZ.IDTS.DigitalTwin.Contracts` |
| Controller | `idts3D_api/src/HZ.IDTS.DigitalTwin.Api/Controllers` |
| Application Service | `idts3D_api/src/HZ.IDTS.DigitalTwin.Application` |
| Entity | `idts3D_api/src/HZ.IDTS.DigitalTwin.Domain/Entities` |
| EF 查询 / Repository | `idts3D_api/src/HZ.IDTS.DigitalTwin.Infrastructure` |
| TypeScript interface | `idts3D_ui/src/types` |
| API Client | `idts3D_ui/src/api` |
| 页面 / engine 消费 | `TwinDemo.vue`、`TwinScene.ts`、`LODModelLoader.ts` |

任何字段新增、删除或重命名，都必须在同一个 MVP 任务内同步更新以上位置。
