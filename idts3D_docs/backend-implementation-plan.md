# 后端实现计划

本文档定义 `idts3D_api` 的后端工程规划。文档设计阶段不创建真实工程；MVP-01 开始按任务卡执行。

## 1. 目录结构

```text
idts3D_api/
  global.json
  HZ.IDTS.DigitalTwin.sln
  src/
    HZ.IDTS.DigitalTwin.Api/
    HZ.IDTS.DigitalTwin.Application/
    HZ.IDTS.DigitalTwin.Contracts/
    HZ.IDTS.DigitalTwin.Domain/
    HZ.IDTS.DigitalTwin.Infrastructure/
    HZ.IDTS.DigitalTwin.Worker/
  tests/
    HZ.IDTS.DigitalTwin.Tests/
```

## 2. Solution 名称

固定为：

```text
HZ.IDTS.DigitalTwin.sln
```

## 3. 项目分层

| 项目 | 职责 |
|---|---|
| `HZ.IDTS.DigitalTwin.Api` | Controller、Swagger、统一异常中间件、统一响应输出 |
| `HZ.IDTS.DigitalTwin.Application` | 应用服务、用例编排、事务边界、校验 |
| `HZ.IDTS.DigitalTwin.Contracts` | Request DTO、Response DTO、枚举契约、错误码、`ApiResponse<T>` |
| `HZ.IDTS.DigitalTwin.Domain` | Entity、领域枚举、领域规则常量 |
| `HZ.IDTS.DigitalTwin.Infrastructure` | EF Core、DbContext、Migration、文件存储、Repository / Query |
| `HZ.IDTS.DigitalTwin.Worker` | MVP 基础任务日志和 job 状态更新 |

## 4. 项目引用方向

允许：

```text
Api -> Application
Api -> Contracts
Application -> Contracts
Application -> Domain
Infrastructure -> Application
Infrastructure -> Domain
Worker -> Application
Worker -> Contracts
Worker -> Infrastructure
```

禁止：

- `Domain` 引用任何其他项目。
- `Contracts` 引用 `Infrastructure`。
- `Application` 直接引用 `Api`。
- 循环引用。

## 5. 命名规则

| 类型 | 命名 |
|---|---|
| DbContext | `DigitalTwinDbContext` |
| Entity | `ModelAsset`, `AssetVersion`, `MovablePartBinding` |
| DbSet | `ModelAssets`, `AssetVersions`, `MovablePartBindings` |
| Controller | `ModelAssetsController`, `AssetVersionsController`, `ScenesController` |
| Application Service | `ModelAssetService`, `AssetVersionService`, `SceneManifestService` |
| Repository / Query | `ModelAssetRepository`, `SceneManifestQuery` |
| Request DTO | `CreateMovablePartRequest`, `GetModelManifestRequest` |
| Response DTO | `MovablePartResponse`, `ModelManifestResponse` |
| Error Code | `VERSION_STATUS_INVALID`, `DUPLICATE_PART_CODE` |

## 6. EF Core 配置方式

推荐：

```text
Infrastructure/
  Persistence/
    DigitalTwinDbContext.cs
    Configurations/
      ModelAssetConfiguration.cs
      AssetVersionConfiguration.cs
      ...
    Migrations/
```

规则：

1. Entity 保持干净，不把 Fluent API 堆进 Entity。
2. 表名使用 snake_case。
3. 字段名使用 snake_case。
4. 枚举 MVP 存 string，便于 Swagger 和前端理解。
5. JSON 字段 PostgreSQL 使用 `jsonb`，SQL Server 使用 `nvarchar(max)`，Provider 差异放在 Infrastructure。

## 7. Migration 策略

MVP-02 创建初始 migration：

```text
InitDigitalTwinSchema
```

后续新增字段必须：

1. 先更新 `domain-entity-dto-map.md`。
2. 再更新 Entity / Configuration。
3. 再创建 migration。
4. 再更新 API 契约和 TS interface。

禁止在非数据库任务中偷偷新增 migration，除非任务卡明确写入。

## 8. PostgreSQL / SQL Server 切换边界

配置键：

```json
{
  "Database": {
    "Provider": "PostgreSql",
    "ConnectionString": ""
  }
}
```

边界：

- Provider 选择只在 Infrastructure 启动注册中处理。
- Application 不关心数据库类型。
- JSON、大小写、索引差异由 EF Configuration 或 migration 处理。
- MVP 默认 PostgreSQL；SQL Server 作为后续验证项，不阻塞 MVP 主线。

## 9. 文件存储

配置：

```json
{
  "FileStorage": {
    "RootPath": "D:/idts3d-assets",
    "PublicBaseUrl": "/assets",
    "AllowedExtensions": [".glb"],
    "MaxFileSizeMb": 500
  }
}
```

落盘建议：

```text
{RootPath}/models/{assetId}/{versionId}/source.glb
{RootPath}/jobs/{jobId}/job.log
```

静态文件 URL 只能由后端生成，前端不能拼路径。

## 10. Serilog

MVP-01 配置控制台和文件日志。

要求：

- API 和 Worker 使用同一日志格式。
- conversion job 写入 `jobId`, `assetId`, `versionId`。
- 错误响应不泄露服务器物理路径。

## 11. Swagger

要求：

- 启用 XML 注释或 endpoint description。
- multipart upload 可在 Swagger 测试。
- 统一响应结构在 Swagger 可见。
- 400 / 404 / 409 响应示例与 `api-contracts` 一致。

## 12. 统一异常处理中间件

异常映射：

| 异常 | HTTP | code |
|---|---:|---|
| 参数校验失败 | 400 | `VALIDATION_FAILED` |
| 资源不存在 | 404 | `NOT_FOUND` |
| 版本状态非法 | 409 | `VERSION_STATUS_INVALID` |
| 唯一约束冲突 | 409 | `CONFLICT` |
| 未处理异常 | 500 | `INTERNAL_ERROR` |

中间件输出必须符合 `ApiResponse<T>` 错误结构。

## 13. 统一响应结构

C# 结构建议：

```csharp
public sealed record ApiResponse<T>(
    bool Success,
    string Code,
    string Message,
    T? Data,
    IReadOnlyList<ApiErrorItem> Errors);
```

Controller 不直接返回裸 DTO。

## 14. Worker 边界

MVP Worker 只做：

- conversion job 状态记录。
- 基础日志写入。
- upload / inspect 占位状态流转。

MVP Worker 不做：

- 完整 CAD 转换。
- STEP / IFC / DWG / RVT / SolidWorks / CATIA 自动转换。
- 3D Tiles 切片。
- 完整队列、重试、调度中心。

完整转换流水线必须后续独立任务规划。
