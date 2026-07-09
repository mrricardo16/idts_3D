# idts3D_api

本目录预留给 IDTS 3D 数字孪生系统后端工程。文档设计阶段只允许保留本说明文件，不创建真实 .NET solution / project。

后续 .NET solution 和项目命名应使用：

- `HZ.IDTS.DigitalTwin.sln`
- `HZ.IDTS.DigitalTwin.Api`
- `HZ.IDTS.DigitalTwin.Application`
- `HZ.IDTS.DigitalTwin.Domain`
- `HZ.IDTS.DigitalTwin.Infrastructure`
- `HZ.IDTS.DigitalTwin.Contracts`
- `HZ.IDTS.DigitalTwin.Worker`

当前已进入 MVP-01 后端解决方案骨架阶段，可创建 .NET solution 和 project，但不实现业务 API。

## MVP-01 后端骨架启动说明

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

MVP-01 不包含 Entity、DbContext、Migration、业务 API、GLB 上传、Manifest、Object Tree、Movable Part、Motion Target 或前端联调。

## 分阶段规则

- 文档设计阶段：禁止创建真实后端工程，禁止写 C# 代码，禁止执行 migration。
- MVP-01 后端解决方案骨架阶段：允许按 `idts3D_docs/mvp-tasks/MVP-01-backend-solution-skeleton.md` 创建 solution 和项目骨架。
- MVP-02 之后：允许按任务卡逐步创建 Entity、DbContext、Migration、Controller、Application Service、Infrastructure Repository、DTO 和 Worker 基础日志。
- 任何阶段默认禁止 commit / push，除非用户明确要求。

后端详细规划见：

- `idts3D_docs/backend-implementation-plan.md`
- `idts3D_docs/domain-entity-dto-map.md`
- `idts3D_docs/api-contracts/README.md`
