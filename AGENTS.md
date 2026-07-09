# AGENTS.md

本文件是 `07_src_3DModesys` 仓库的全局 Codex 执行规则。后续任何任务都必须先读取本文件，再读取对应任务卡。

## 1. 文件编码

1. 修改代码或文档时必须保持 UTF-8。
2. 不要使用 ANSI / GBK 重写文件。
3. 不要把已有中文注释、中文文案、中文日志改成乱码。
4. 如果发现文件疑似非 UTF-8 编码，必须先提示用户确认，不要直接重写整个文件。

## 2. 分阶段开发规则

### 2.1 文档设计阶段

当前任务属于文档设计阶段，只允许修改项目文档、任务卡、规则文件和 README。

文档设计阶段禁止：

- 写 C# 业务代码。
- 写 Vue / TypeScript 业务代码。
- 创建真实 `idts3D_api` .NET solution 或 project。
- 执行 `dotnet new`。
- 执行 `dotnet ef migrations add` 或 `dotnet ef database update`。
- 执行 `npm install`。
- 修改 `idts3D_ui/src/**`。
- 修改 `idts3D_ui/public/models/lifter.glb`。
- commit。
- push。

### 2.2 MVP 开发阶段

只有用户明确要求执行某个 MVP 任务卡时，才进入 MVP 开发阶段。

MVP 开发阶段允许在对应任务卡范围内：

- 创建或修改 `idts3D_api` 后端工程。
- 写 C# / ASP.NET Core / EF Core 代码。
- 写 Vue 3 / TypeScript / Three.js 代码。
- 执行 migration。
- 运行 `dotnet build`、`npm run build`、Swagger 验证和页面验证。

但仍必须遵守：

1. 每次只执行一个 MVP 任务。
2. 执行前必须输出影响范围并等待用户确认。
3. 不允许跨任务扩展。
4. 不允许一次性实现多个 MVP。
5. 不允许修改与当前任务无关的文件。
6. 不允许 commit / push，除非用户明确要求。

## 3. 必读顺序

后续执行任何 MVP 任务前，必须按顺序读取：

1. `AGENTS.md`
2. `idts3D_ui/AGENTS.md`
3. `idts3D_docs/development-rules.md`
4. `idts3D_docs/idts-mvp-task-breakdown.md`
5. 当前 MVP 任务卡
6. 当前任务引用的 `idts3D_docs/api-contracts/*.md`
7. 当前任务涉及的前端、后端、数据库映射文档

如果任一被任务卡引用的文件不存在，必须在输出中明确说明，不能假装已读取。

## 4. 前后端一体化约束

1. 数据库实体、C# Entity、DbSet、DTO、API 契约、TypeScript interface、前端 API Client 必须一一对应。
2. 后端接口字段不得由前端猜测。
3. 前端不得散落直接 `fetch` 调用，必须通过 `src/api` API Client 封装。
4. `monitor` 模式只读取 Published 配置。
5. `edit` 模式只编辑 Draft / Ready 配置，不得直接影响 monitor 当前 Published 配置。
6. 后端不可用时允许 fallback 到本地模型 / localStorage / 静态 JSON，但 UI 必须能区分 fallback 与正式后端数据。

## 5. 输出规则

执行任务前必须输出：

- 已读取文件列表。
- 未找到但被引用的文件列表。
- 影响范围。
- 禁止修改范围。
- 风险点。

执行完成后必须输出：

- 修改文件列表。
- 新增文件列表。
- 删除文件列表。
- 是否修改业务代码。
- 是否修改 `idts3D_ui/src/**`。
- 是否创建真实 `idts3D_api` 工程。
- 是否新增依赖。
- 验证命令和结果。
- `git status` 摘要。
- `git diff --stat` 摘要。
