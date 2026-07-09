# 领域实体、DTO 与前端类型映射

本文档是数据库表、C# Entity、DbSet、DTO、API、TypeScript interface、前端消费位置的唯一映射基线。

## 1. 通用约定

| 项 | 规则 |
|---|---|
| 主键 | MVP 使用 `long id`，数据库列名 `id` |
| 时间字段 | `created_time`, `updated_time`, `published_time`, `archived_time` 使用 UTC |
| C# Entity | PascalCase，如 `ModelAsset` |
| DbSet | 复数 PascalCase，如 `ModelAssets` |
| DTO | 放在 `HZ.IDTS.DigitalTwin.Contracts` |
| TS interface | 放在 `idts3D_ui/src/types` |
| API Client | 放在 `idts3D_ui/src/api` |

## 2. 总览表

| 表名 | C# Entity | DbSet | 主键 | 创建 DTO | 更新 DTO | 查询 DTO | 响应 DTO | TypeScript interface | 主要读取接口 | 主要写入接口 | 前端消费位置 |
|---|---|---|---|---|---|---|---|---|---|---|---|
| `model_asset` | `ModelAsset` | `ModelAssets` | `id` | `UploadModelAssetRequest` | `UpdateModelAssetRequest` | `ModelAssetQueryRequest` | `ModelAssetResponse` | `ModelAssetDto` | upload, model manifest, scene manifest | upload, publish, rollback | `src/api/modelAssets.ts`, `TwinDemo.vue` |
| `asset_version` | `AssetVersion` | `AssetVersions` | `id` | `CreateAssetVersionRequest` | `ChangeVersionStatusRequest` | `AssetVersionQueryRequest` | `AssetVersionResponse` | `AssetVersionDto` | manifest, version status, scene manifest | upload, mark-ready, publish, archive, rollback | `src/api/modelAssets.ts`, `src/api/assetVersions.ts` |
| `model_asset_variant` | `ModelAssetVariant` | `ModelAssetVariants` | `id` | `CreateModelAssetVariantRequest` | `UpdateModelAssetVariantRequest` | `ModelAssetVariantQueryRequest` | `ModelAssetVariantResponse` | `ModelAssetVariantDto` | model manifest | upload, model stats update | `LODModelLoader.ts` via `ModelManifestResponse` |
| `model_conversion_job` | `ModelConversionJob` | `ModelConversionJobs` | `id` | `CreateConversionJobRequest` | `UpdateConversionJobStatusRequest` | `ConversionJobQueryRequest` | `ConversionJobResponse` | `ConversionJobDto` | conversion job query | upload, Worker status update | `src/api/conversionJobs.ts` |
| `model_object_index` | `ModelObjectIndex` | `ModelObjectIndexes` | `id` | `SaveObjectTreeRequest` | `UpdateObjectIndexRequest` | `ObjectTreeQueryRequest` | `ObjectTreeResponse` | `ModelObjectNodeDto` | object tree, publish guard, movable part validation | save object tree | `TwinDemo.vue`, `TwinScene.ts` |
| `asset_manifest` | `AssetManifest` | `AssetManifests` | `id` | `SaveAssetManifestRequest` | `UpdateAssetManifestRequest` | `ModelManifestQueryRequest` | `ModelManifestResponse` | `ModelManifestDto` | model manifest, model stats, publish guard | upload, save model stats, save manifest | `LODModelLoader.ts`, `TwinScene.ts` |
| `scene_node` | `SceneNode` | `SceneNodes` | `id` | `CreateSceneNodeRequest` | `UpdateSceneNodeRequest` | `SceneQueryRequest` | `SceneNodeResponse` | `SceneNodeDto` | scene manifest | seed scene | `src/api/scenes.ts`, `TwinScene.ts` |
| `device_instance` | `DeviceInstance` | `DeviceInstances` | `id` | `CreateDeviceInstanceRequest` | `UpdateDeviceInstanceRequest` | `DeviceInstanceQueryRequest` | `DeviceInstanceResponse` | `DeviceInstanceDto` | scene manifest | seed device | `TwinDemo.vue`, `TwinScene.ts` |
| `device_model_binding` | `DeviceModelBinding` | `DeviceModelBindings` | `id` | `CreateDeviceModelBindingRequest` | `UpdateDeviceModelBindingRequest` | `DeviceModelBindingQueryRequest` | `DeviceModelBindingResponse` | `DeviceModelBindingDto` | scene manifest, publish guard | seed binding, publish, rollback | `src/api/scenes.ts` |
| `movable_part_binding` | `MovablePartBinding` | `MovablePartBindings` | `id` | `CreateMovablePartRequest` | `UpdateMovablePartRequest` | `MovablePartQueryRequest` | `MovablePartResponse` | `MovablePartDto` | manifest, movable parts, publish guard | movable part CRUD | `TwinDemo.vue`, `TwinScene.ts` |
| `motion_target` | `MotionTarget` | `MotionTargets` | `id` | `CreateMotionTargetRequest` | `UpdateMotionTargetRequest` | `MotionTargetQueryRequest` | `MotionTargetResponse` | `MotionTargetDto` | manifest, motion targets, publish guard | motion target CRUD | `TwinDemo.vue`, `TwinScene.ts` |
| `operation_audit` | `OperationAudit` | `OperationAudits` | `id` | `CreateOperationAuditRequest` | 无 | `OperationAuditQueryRequest` | `OperationAuditResponse` | `OperationAuditDto` | audit query | upload, edit, publish, rollback, delete | 后台审计页面，MVP 可不做 UI |
| `tool_package` | `ToolPackage` | `ToolPackages` | `id` | `CreateToolPackageRequest` | `UpdateToolPackageRequest` | `ToolPackageQueryRequest` | `ToolPackageResponse` | `ToolPackageDto` | Worker 工具健康 | 工具注册 | MVP 文档规划，前端暂不消费 |
| `tool_health_check` | `ToolHealthCheck` | `ToolHealthChecks` | `id` | `CreateToolHealthCheckRequest` | 无 | `ToolHealthCheckQueryRequest` | `ToolHealthCheckResponse` | `ToolHealthCheckDto` | Worker 工具健康 | health check 记录 | MVP 文档规划，前端暂不消费 |

## 3. 字段设计

### 3.1 `model_asset`

| 字段 | 类型 | 必填 | 默认值 | 约束 / 索引 / 外键 | 枚举 |
|---|---|---|---|---|---|
| `id` | bigint | 是 | identity | PK |  |
| `asset_code` | varchar(100) | 是 |  | unique |  |
| `asset_name` | varchar(200) | 是 |  | index |  |
| `source_file_name` | varchar(255) | 是 |  |  |  |
| `source_file_hash` | varchar(128) | 是 |  | unique in MVP |  |
| `source_file_type` | varchar(50) | 是 | `glb` |  | `glb` |
| `asset_type` | varchar(50) | 是 | `device_glb` | index | `device_glb`, `static_glb` |
| `processing_status` | varchar(50) | 是 | `pending` | index | `pending`, `ready`, `failed` |
| `current_version_id` | bigint | 否 | null | FK -> `asset_version.id` |  |
| `created_time` | timestamptz | 是 | now |  |  |
| `updated_time` | timestamptz | 是 | now |  |  |

读取接口：upload 去重、model manifest、scene manifest。  
写入接口：upload、publish、rollback。  
前端消费：`ModelAssetDto`、`ModelManifestResponse`、`SceneManifestResponse`。

### 3.2 `asset_version`

| 字段 | 类型 | 必填 | 默认值 | 约束 / 索引 / 外键 | 枚举 |
|---|---|---|---|---|---|
| `id` | bigint | 是 | identity | PK |  |
| `model_asset_id` | bigint | 是 |  | FK -> `model_asset.id`, unique with `version_no` |  |
| `version_no` | int | 是 | 1 | unique with `model_asset_id` |  |
| `version_status` | varchar(50) | 是 | `Draft` | index | `Draft`, `Ready`, `Published`, `Archived`, `Failed`, `Invalid` |
| `created_time` | timestamptz | 是 | now |  |  |
| `published_time` | timestamptz | 否 | null |  |  |
| `archived_time` | timestamptz | 否 | null |  |  |

读取接口：manifest、object tree、model stats、movable parts、motion targets、scene manifest。  
写入接口：upload、mark-ready、publish、archive、rollback。

### 3.3 `model_asset_variant`

| 字段 | 类型 | 必填 | 默认值 | 约束 / 索引 / 外键 | 枚举 |
|---|---|---|---|---|---|
| `id` | bigint | 是 | identity | PK |  |
| `model_asset_id` | bigint | 是 |  | FK -> `model_asset.id` |  |
| `asset_version_id` | bigint | 是 |  | FK -> `asset_version.id`, unique with `variant_level` |  |
| `variant_level` | varchar(50) | 是 | `source` | index | `source`, `high`, `medium`, `low`, `proxy` |
| `file_url` | varchar(500) | 是 |  |  |  |
| `file_hash` | varchar(128) | 是 |  | index |  |
| `file_size` | bigint | 是 | 0 |  |  |
| `triangle_count` | bigint | 否 | null |  |  |
| `vertex_count` | bigint | 否 | null |  |  |
| `mesh_count` | int | 否 | null |  |  |
| `material_count` | int | 否 | null |  |  |
| `texture_count` | int | 否 | null |  |  |
| `created_time` | timestamptz | 是 | now |  |  |

读取接口：model manifest、model stats。  
写入接口：upload、后续 LOD / stats 更新。

### 3.4 `model_conversion_job`

| 字段 | 类型 | 必填 | 默认值 | 约束 / 索引 / 外键 | 枚举 |
|---|---|---|---|---|---|
| `id` | bigint | 是 | identity | PK |  |
| `model_asset_id` | bigint | 是 |  | FK -> `model_asset.id` |  |
| `asset_version_id` | bigint | 是 |  | FK -> `asset_version.id` |  |
| `job_type` | varchar(50) | 是 | `upload` | index | `upload`, `inspect`, `object_tree`, `model_stats`, `convert` |
| `status` | varchar(50) | 是 | `pending` | index | `pending`, `running`, `completed`, `failed`, `canceled` |
| `progress` | int | 是 | 0 |  |  |
| `message` | text | 否 | null |  |  |
| `input_file` | varchar(500) | 否 | null |  |  |
| `output_directory` | varchar(500) | 否 | null |  |  |
| `stdout_log_url` | varchar(500) | 否 | null |  |  |
| `stderr_log_url` | varchar(500) | 否 | null |  |  |
| `exit_code` | int | 否 | null |  |  |
| `elapsed_ms` | bigint | 否 | null |  |  |
| `retry_count` | int | 是 | 0 |  |  |
| `started_time` | timestamptz | 否 | null |  |  |
| `finished_time` | timestamptz | 否 | null |  |  |

读取接口：`GET /api/model-conversion-jobs/{jobId}`。  
写入接口：upload、Worker status update。

### 3.5 `model_object_index`

| 字段 | 类型 | 必填 | 默认值 | 约束 / 索引 / 外键 | 枚举 |
|---|---|---|---|---|---|
| `id` | bigint | 是 | identity | PK |  |
| `model_asset_id` | bigint | 是 |  | FK -> `model_asset.id` |  |
| `asset_version_id` | bigint | 是 |  | FK -> `asset_version.id`, index with `object_uuid`, index with `object_path` |  |
| `object_uuid` | varchar(100) | 是 |  | index |  |
| `object_name` | varchar(200) | 是 |  | index |  |
| `object_path` | varchar(1000) | 是 |  | index |  |
| `parent_uuid` | varchar(100) | 否 | null |  |  |
| `parent_path` | varchar(1000) | 否 | null |  |  |
| `object_type` | varchar(50) | 是 |  |  | `Group`, `Mesh`, `SkinnedMesh` 等 |
| `bounding_box_min_x/y/z` | decimal(18,6) | 否 | null |  |  |
| `bounding_box_max_x/y/z` | decimal(18,6) | 否 | null |  |  |
| `mesh_fingerprint` | varchar(200) | 否 | null | index |  |

读取接口：object tree、movable part validation、publish guard。  
写入接口：save object tree。

### 3.6 `asset_manifest`

| 字段 | 类型 | 必填 | 默认值 | 约束 / 索引 / 外键 | 枚举 |
|---|---|---|---|---|---|
| `id` | bigint | 是 | identity | PK |  |
| `model_asset_id` | bigint | 是 |  | FK -> `model_asset.id` |  |
| `asset_version_id` | bigint | 是 |  | FK -> `asset_version.id`, unique |  |
| `manifest_json` | jsonb / nvarchar(max) | 是 | `{}` |  |  |
| `model_stats_json` | jsonb / nvarchar(max) | 否 | null |  |  |
| `created_time` | timestamptz | 是 | now |  |  |
| `updated_time` | timestamptz | 是 | now |  |  |

读取接口：model manifest、model stats、publish guard。  
写入接口：upload 初始 manifest、save model stats、save manifest。

### 3.7 `scene_node`

| 字段 | 类型 | 必填 | 默认值 | 约束 / 索引 / 外键 | 枚举 |
|---|---|---|---|---|---|
| `id` | bigint | 是 | identity | PK |  |
| `parent_id` | bigint | 否 | null | FK -> `scene_node.id` |  |
| `node_code` | varchar(100) | 是 |  | unique |  |
| `node_name` | varchar(200) | 是 |  | index |  |
| `node_type` | varchar(50) | 是 | `scene` | index | `scene`, `area`, `floor`, `line` |
| `sort_no` | int | 是 | 0 |  |  |
| `enabled` | boolean | 是 | true | index |  |

读取接口：scene manifest。  
写入接口：MVP-09 seed scene。

### 3.8 `device_instance`

| 字段 | 类型 | 必填 | 默认值 | 约束 / 索引 / 外键 | 枚举 |
|---|---|---|---|---|---|
| `id` | bigint | 是 | identity | PK |  |
| `scene_node_id` | bigint | 是 |  | FK -> `scene_node.id` |  |
| `device_code` | varchar(100) | 是 |  | unique |  |
| `device_name` | varchar(200) | 是 |  | index |  |
| `position_x/y/z` | decimal(18,6) | 是 | 0 |  |  |
| `rotation_x/y/z` | decimal(18,6) | 是 | 0 |  |  |
| `scale_x/y/z` | decimal(18,6) | 是 | 1 |  |  |
| `enabled` | boolean | 是 | true | index |  |

读取接口：scene manifest。  
写入接口：MVP-09 seed device。

### 3.9 `device_model_binding`

| 字段 | 类型 | 必填 | 默认值 | 约束 / 索引 / 外键 | 枚举 |
|---|---|---|---|---|---|
| `id` | bigint | 是 | identity | PK |  |
| `device_instance_id` | bigint | 是 |  | FK -> `device_instance.id`, unique with `asset_version_id` |  |
| `model_asset_id` | bigint | 是 |  | FK -> `model_asset.id` |  |
| `asset_version_id` | bigint | 是 |  | FK -> `asset_version.id` |  |
| `binding_status` | varchar(50) | 是 | `active` | index | `active`, `archived`, `invalid` |
| `active_from` | timestamptz | 是 | now |  |  |
| `active_to` | timestamptz | 否 | null |  |  |

业务约束：同一 `device_instance_id` 同一时间只能存在一个 active binding，且 active binding 必须指向 Published asset version。  
读取接口：scene manifest、publish guard。  
写入接口：seed binding、publish、rollback。

### 3.10 `movable_part_binding`

| 字段 | 类型 | 必填 | 默认值 | 约束 / 索引 / 外键 | 枚举 |
|---|---|---|---|---|---|
| `id` | bigint | 是 | identity | PK |  |
| `model_asset_id` | bigint | 是 |  | FK -> `model_asset.id` |  |
| `asset_version_id` | bigint | 是 |  | FK -> `asset_version.id`, unique with `part_code` |  |
| `device_instance_id` | bigint | 否 | null | FK -> `device_instance.id` |  |
| `object_uuid` | varchar(100) | 否 | null | index |  |
| `object_name` | varchar(200) | 是 |  |  |  |
| `object_path` | varchar(1000) | 否 | null | index |  |
| `parent_uuid` | varchar(100) | 否 | null |  |  |
| `parent_path` | varchar(1000) | 否 | null |  |  |
| `business_name` | varchar(200) | 是 |  | index |  |
| `part_code` | varchar(100) | 是 |  | unique with `asset_version_id` |  |
| `motion_type` | varchar(50) | 是 | `linear` |  | `linear`, `rotate`, `path`, `joint` |
| `axis_mode` | varchar(50) | 是 | `world` |  | `world`, `local`, `custom` |
| `axis` | varchar(10) | 是 | `z` |  | `x`, `y`, `z` |
| `custom_axis_x/y/z` | decimal(18,6) | 否 | null |  |  |
| `min_value` | decimal(18,6) | 是 | 0 |  |  |
| `max_value` | decimal(18,6) | 是 | 0 |  |  |
| `home_value` | decimal(18,6) | 是 | 0 |  |  |
| `current_value` | decimal(18,6) | 是 | 0 |  |  |
| `default_speed` | decimal(18,6) | 是 | 1 |  |  |
| `binding_status` | varchar(50) | 是 | `active` | index | `active`, `archived`, `invalid` |
| `enabled` | boolean | 是 | true | index |  |

读取接口：model manifest、movable parts、publish guard。  
写入接口：movable part CRUD。

### 3.11 `motion_target`

| 字段 | 类型 | 必填 | 默认值 | 约束 / 索引 / 外键 | 枚举 |
|---|---|---|---|---|---|
| `id` | bigint | 是 | identity | PK |  |
| `movable_part_id` | bigint | 是 |  | FK -> `movable_part_binding.id`, unique with `target_code` |  |
| `target_code` | varchar(100) | 是 |  | unique with `movable_part_id` |  |
| `target_name` | varchar(200) | 是 |  |  |  |
| `target_value` | decimal(18,6) | 否 | null | index |  |
| `target_x/y/z` | decimal(18,6) | 否 | null |  |  |
| `sort_no` | int | 是 | 0 |  |  |
| `enabled` | boolean | 是 | true | index |  |

读取接口：model manifest、motion targets、publish guard。  
写入接口：motion target CRUD。

### 3.12 `operation_audit`

| 字段 | 类型 | 必填 | 默认值 | 约束 / 索引 / 外键 | 枚举 |
|---|---|---|---|---|---|
| `id` | bigint | 是 | identity | PK |  |
| `operation_type` | varchar(50) | 是 |  | index | `upload`, `edit`, `publish`, `rollback`, `delete` |
| `target_type` | varchar(50) | 是 |  | index | `model_asset`, `asset_version`, `movable_part`, `motion_target`, `scene` |
| `target_id` | bigint | 是 |  | index |  |
| `before_json` | jsonb / nvarchar(max) | 否 | null |  |  |
| `after_json` | jsonb / nvarchar(max) | 否 | null |  |  |
| `operator_id` | varchar(100) | 否 | null |  |  |
| `operator_name` | varchar(100) | 否 | null |  |  |
| `created_time` | timestamptz | 是 | now | index |  |

读取接口：后续审计查询。  
写入接口：upload、edit、publish、rollback、delete。

### 3.13 `tool_package`

| 字段 | 类型 | 必填 | 默认值 | 约束 / 索引 / 外键 | 枚举 |
|---|---|---|---|---|---|
| `id` | bigint | 是 | identity | PK |  |
| `tool_code` | varchar(100) | 是 |  | unique |  |
| `tool_name` | varchar(200) | 是 |  |  |  |
| `version` | varchar(100) | 是 |  | index |  |
| `install_path` | varchar(500) | 是 |  |  |  |
| `enabled` | boolean | 是 | true | index |  |
| `created_time` | timestamptz | 是 | now |  |  |
| `updated_time` | timestamptz | 是 | now |  |  |

读取接口：Worker 工具健康。  
写入接口：工具注册，MVP 可只建表不开放 UI。

### 3.14 `tool_health_check`

| 字段 | 类型 | 必填 | 默认值 | 约束 / 索引 / 外键 | 枚举 |
|---|---|---|---|---|---|
| `id` | bigint | 是 | identity | PK |  |
| `tool_package_id` | bigint | 是 |  | FK -> `tool_package.id`, index with `checked_time` |  |
| `status` | varchar(50) | 是 | `unknown` | index | `healthy`, `warning`, `error`, `unknown` |
| `message` | text | 否 | null |  |  |
| `checked_time` | timestamptz | 是 | now | index |  |

读取接口：Worker 工具健康。  
写入接口：health check 记录，MVP 可只建表不开放 UI。

## 4. 实现同步门禁

任何任务修改以上任一字段时，必须同步检查：

1. EF Entity 字段。
2. `DbContext` DbSet 和 Fluent API。
3. Migration。
4. Request DTO。
5. Response DTO。
6. API 契约文档。
7. TypeScript interface。
8. API Client 方法。
9. Vue / engine 消费位置。
10. 回归测试和 Swagger 示例。
