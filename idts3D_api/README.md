# idts3D_api

本目录承载 IDTS 3D 数字孪生系统后端工程。当前已建立 .NET 8 六项目 Solution。MVP-01～MVP-07 的后端基线已实现；MVP-08 的 Motion Target CRUD 已实现但仍有真实 PostgreSQL/Swagger 验证缺口。完整事实与证据见 `../idts3D_docs/reviews/DOC-PLAN-01-implementation-state-calibration-report.md`。

后续 .NET solution 和项目命名应使用：

- `HZ.IDTS.DigitalTwin.sln`
- `HZ.IDTS.DigitalTwin.Api`
- `HZ.IDTS.DigitalTwin.Application`
- `HZ.IDTS.DigitalTwin.Domain`
- `HZ.IDTS.DigitalTwin.Infrastructure`
- `HZ.IDTS.DigitalTwin.Contracts`
- `HZ.IDTS.DigitalTwin.Worker`

## 当前后端事实

- Solution：`HZ.IDTS.DigitalTwin.sln` 存在，包含 Api、Application、Contracts、Domain、Infrastructure、Worker 和 3 个测试项目。
- 数据库：Provider 为 PostgreSQL；初始 Migration `20260709080033_InitDigitalTwinSchema` 已包含 `motion_target`、`operation_audit` 等表。此次未执行数据库更新或真实数据库验证。
- 已实现 API：Model Asset 上传/Manifest、Object Tree/Model Stats、资产版本生命周期、Movable Part CRUD、Motion Target CRUD。Motion Target 的路由为 `/api/movable-parts/{partId}/motion-targets`，包含 GET/POST/PUT/DELETE、Draft/Ready 写入 guard、Published monitor 读取 guard、范围与唯一性校验以及 OperationAudit 写入。
- 测试项目：Application Tests、Architecture Tests、API Integration Tests 均存在。2026-07-15 本地 `dotnet test ... --no-build` 共 84 项通过，其中 Motion Target 有 Service 与 TestServer/API 路由覆盖；这些测试不连接真实 PostgreSQL 或真实文件存储。
- 本地构建：2026-07-15 `dotnet build .\HZ.IDTS.DigitalTwin.sln` 退出码 0、0 warning、0 error。实际 SDK 为 8.0.100，与 `global.json` 一致。
- CI：工作流存在，但当前基线最近运行失败于 repository-policy；后端质量作业未启动，不能把本地结果描述为当前 CI 已通过。
- 尚未实现或未验证：Scene/Device 业务 API、Worker 转换流水线、正式前端 API Client/联调、真实 PostgreSQL Migration/事务/行锁验证、Swagger 实例验证、浏览器 E2E、真实 WebGL 验证。

## 当前后端启动说明

本目录使用本机实际存在的 .NET SDK `8.0.100`，并通过 `global.json` 锁定。

构建后端骨架：

```powershell
dotnet build .\HZ.IDTS.DigitalTwin.sln
```

启动 API：

```powershell
dotnet run --project .\src\HZ.IDTS.DigitalTwin.Api\HZ.IDTS.DigitalTwin.Api.csproj
```

启动后可访问健康检查：

```text
GET /api/health
```

开发环境下可打开 Swagger：

```text
/swagger
```

启动 Worker 空骨架：

```powershell
dotnet run --project .\src\HZ.IDTS.DigitalTwin.Worker\HZ.IDTS.DigitalTwin.Worker.csproj
```

本地启动方式仅说明运行入口，不构成 Swagger、数据库连接或业务 API 的本次验收。当前 CI 失败未处理前，不得把该启动说明解释为部署或 CI 通过证据。

## 分阶段规则

- 后续后端能力必须严格按当前任务卡实施，不得把规划中的 API 写成已完成能力。
- 只读阶段禁止 commit / push；用户确认写入任务后，按项目级 Skill 验证、commit 并 push `origin/main`，禁止 force push。

后端详细规划见：

- `idts3D_docs/backend-implementation-plan.md`
- `idts3D_docs/domain-entity-dto-map.md`
- `idts3D_docs/api-contracts/README.md`
- `../idts3D_docs/architecture/project-architecture-baseline.md`
- `../idts3D_docs/architecture/project-architecture-debt-register.md`
- `../idts3D_docs/architecture/main-delivery-workflow.md`
