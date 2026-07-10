# AGENTS.md

本文件是 `idts3D_api` 后端工程的 Codex 执行规则。根目录 `AGENTS.md` 仍然优先适用于所有任务；本文件只补充后端边界。

## 1. 后端定位

- 当前目录是 IDTS 3D MVP 的 ASP.NET Core Web API / .NET 8 / EF Core 8 后端与 Worker 工程目录。
- 后端工程按任务卡逐步创建和演进。
- 后端不属于旧 `HZ.IDTS.API`，不允许修改旧项目代码。

## 2. 必读文件

执行后端任务前必须读取：

- `../AGENTS.md`
- `../idts3D_docs/development-rules.md`
- `../idts3D_docs/idts-mvp-task-breakdown.md`
- `../idts3D_docs/mvp-tasks/README.md`
- 当前 MVP 任务卡
- 当前任务涉及的 API 契约文档
- 当前任务涉及的实体 / DTO / 数据库映射文档

## 3. 技术边界

- 固定采用 C# / ASP.NET Core Web API / .NET 8 / EF Core 8。
- DTO、统一响应、错误码和接口枚举放在 Contracts 层。
- Entity 和领域枚举放在 Domain 层。
- Controller 只负责 HTTP 入参、权限边界、调用 Application Service、返回统一响应。
- 复杂业务逻辑不得堆在 Controller。
- Infrastructure 负责 EF Core、文件存储、Provider 切换、Repository 或 EF 查询。
- Worker 在 MVP 阶段只做 conversion job 基础日志和状态处理，不提前实现完整 CAD 转换流水线。

## 4. 后端开发规则

- 不允许 Controller 直接返回 Entity。
- 不允许前端字段倒逼后端随意改 DTO。
- 不允许绕过 API 契约自行发明 Route、DTO、Entity、枚举、表名。
- 新增字段时必须同步 Entity、DTO、API 契约、TypeScript interface 和 API Client。
- PostgreSQL 优先，SQL Server 备选；Provider 边界必须清晰。
- migration 只在任务卡明确要求时执行。
- 后端任务不得主动修改 `idts3D_ui`，除非任务明确标记为联调任务。

## 5. 验证规则

后端修改完成后，根据项目实际情况执行：

- `dotnet restore`
- `dotnet build`
- `dotnet test`

如果项目尚未创建，则说明无法执行对应命令，不要假装验证通过。

## 6. 架构治理补充

- 所有后端任务必须先读取 `../.agents/skills/idts3d-architecture-governance/SKILL.md`。
- Controller 不得访问 DbContext；Application 不得依赖 Infrastructure；Domain 不得依赖 Application、Infrastructure 或 Api。
- Infrastructure 不得决定业务状态矩阵。Contracts 不得返回 Entity。
- Repository 不得无边界扩张为多业务能力集合；新增能力先判断是否需要独立 Service / Repository。
- 事务、锁和审计必须属于明确用例；不得持续把新能力堆入已有大型 Repository。
- 非功能重构不得改变 API、数据库、状态、事务或错误码。
- 发现架构债务必须登记，不在业务任务中顺手修复；完成后必须执行结构复检。
