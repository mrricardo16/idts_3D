# API 契约：conversion job

## GET /api/model-conversion-jobs/{jobId}

| 项 | 值 |
|---|---|
| Method | `GET` |
| Route | `/api/model-conversion-jobs/{jobId}` |
| Query 参数 | 无 |
| Path 参数 | `jobId: long` |
| Request Body | 无 |
| monitor 模式 | 允许只读 |
| edit 模式 | 允许只读 |
| 前端 TS interface | `ConversionJobResponse` |
| 后端 DTO | `GetConversionJobRequest`, `ConversionJobResponse` |
| 后端实体 | `ModelConversionJob`, `ModelAsset`, `AssetVersion` |
| 读取表 | `model_conversion_job`, `model_asset`, `asset_version` |
| 写入表 | 无 |

Response Body：

```json
{
  "success": true,
  "code": "OK",
  "message": "",
  "data": {
    "jobId": 1,
    "assetId": 1,
    "versionId": 1,
    "jobType": "upload",
    "status": "completed",
    "progress": 100,
    "message": "GLB 上传完成。",
    "exitCode": 0,
    "startedTime": "2026-07-09T10:00:00Z",
    "finishedTime": "2026-07-09T10:00:05Z",
    "logUrl": "/assets/jobs/1/job.log",
    "stdoutLogUrl": "/assets/jobs/1/stdout.log",
    "stderrLogUrl": "/assets/jobs/1/stderr.log"
  }
}
```

字段类型和枚举：

| 字段 | 类型 | 说明 |
|---|---|---|
| `jobId` / `assetId` / `versionId` | long | 数据库主键 |
| `jobType` | enum | `upload` / `inspect` / `object_tree` / `model_stats` / `convert` |
| `status` | enum | `pending` / `running` / `completed` / `failed` / `canceled` |
| `progress` | int | 0 到 100 |
| `message` | string | 状态说明 |
| `exitCode` | int? | Worker 或工具退出码 |
| `logUrl` / `stdoutLogUrl` / `stderrLogUrl` | string? | 后端生成的日志 URL |

400 示例：

```json
{
  "success": false,
  "code": "VALIDATION_FAILED",
  "message": "jobId 无效。",
  "errors": [{ "field": "jobId", "message": "必须大于 0。" }]
}
```

404 示例：

```json
{
  "success": false,
  "code": "NOT_FOUND",
  "message": "conversion job 不存在。",
  "errors": [{ "field": "jobId", "message": "未找到 jobId=1。" }]
}
```

409 示例：

```json
{
  "success": false,
  "code": "CONFLICT",
  "message": "conversion job 状态不一致。",
  "errors": [{ "field": "status", "message": "finishedTime 存在但状态仍为 running。" }]
}
```

MVP-15 只查询和记录基础日志，不实现完整 CAD 转换、STEP / IFC 转换、3D Tiles 切片或完整 Worker 队列系统。
