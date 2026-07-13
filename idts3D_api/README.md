# idts3D_api

本目录承载 IDTS 3D 数字孪生系统后端工程。当前已建立 .NET 8 六项目 Solution，并完成 MVP-01 至 MVP-07 的后端能力基线；MVP-07 提供独立的 movable part CRUD API，写入仅允许 Draft、Ready 版本。

后续 .NET solution 和项目命名应使用：

- `HZ.IDTS.DigitalTwin.sln`
- `HZ.IDTS.DigitalTwin.Api`
- `HZ.IDTS.DigitalTwin.Application`
- `HZ.IDTS.DigitalTwin.Domain`
- `HZ.IDTS.DigitalTwin.Infrastructure`
- `HZ.IDTS.DigitalTwin.Contracts`
- `HZ.IDTS.DigitalTwin.Worker`

当前已实现的业务能力包括 PostgreSQL EF Core、初始 Migration、GLB 上传、本地文件存储、Model Manifest、Object Tree、Model Stats 和 Asset Version 生命周期。Worker 仍为空骨架；Scene、Movable Part、Motion Target、转换流水线和前端联调尚未完成。

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

当前尚未包含 Scene、Movable Part、Motion Target 业务 API、Worker 转换流水线、正式前端 API Client 或前后端联调。自动化测试和 CI 尚未建立。

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
