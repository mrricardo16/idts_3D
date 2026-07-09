# AGENTS.md

本文件是 `idts_3D` monorepo 的全局 Codex 执行规则。

## 1. 仓库定位

- 本仓库是 IDTS 3D 数字孪生 MVP monorepo。
- `idts3D_api` 是 ASP.NET Core Web API / .NET 8 / EF Core 8 后端与 Worker 工程目录。
- `idts3D_ui` 是 Vue 3 + Vite + TypeScript + Three.js 前端工程。
- `idts3D_docs` 是架构方案、开发规则、API 契约、实体映射、MVP 任务卡来源。
- Codex 桌面端主工作目录应使用仓库根目录，不应把 `idts3D_api` 或 `idts3D_ui` 单独作为主工作区。

## 2. 文件编码

- 修改代码或文档时必须保持 UTF-8。
- 不要使用 ANSI / GBK 重写文件。
- 不要把已有中文注释、中文文案、中文日志改成乱码。
- 如果发现文件疑似非 UTF-8 编码，必须先提示用户确认，不要直接重写整个文件。

## 3. 任务类型

本仓库允许按用户明确指定的任务进行开发，不再默认判定为“禁止开发”。

任务分为：

1. 文档任务：
   - 只修改 README、AGENTS.md、`idts3D_docs/**/*.md`。
   - 不修改业务代码。

2. 后端任务：
   - 主要修改 `idts3D_api`。
   - 必须读取当前任务卡和相关 API 契约。
   - 可以在任务卡范围内创建或修改 .NET 8 后端工程、C# 代码、EF、EF Core 代码、Worker 代码、migration。

3. 前端任务：
   - 主要修改 `idts3D_ui`。
   - 必须读取当前任务卡和相关 API 契约。
   - 可以在任务卡范围内修改 Vue 3、TypeScript、Three.js、API Client、types、页面和 engine 代码。

4. 契约任务：
   - 主要修改 `idts3D_docs`。
   - 如果接口契约变化，必须同步说明后端 DTO、前端 TypeScript interface、API Client 的影响。

5. 联调任务：
   - 允许同时修改 `idts3D_api`、`idts3D_ui`、`idts3D_docs`。
   - 仅在用户明确标记为“联调任务”或当前任务卡要求前后端同时修改时允许。

## 4. 必读顺序

执行任何 MVP 开发任务前，必须按顺序读取：

1. `AGENTS.md`
2. `idts3D_docs/development-rules.md`
3. `idts3D_docs/idts-mvp-task-breakdown.md`
4. `idts3D_docs/mvp-tasks/README.md`
5. 当前 MVP 任务卡
6. 当前任务引用的 `idts3D_docs/api-contracts/*.md`
7. 当前任务涉及的实体映射、前端集成、后端实现文档

如果是前端任务，还必须读取：

- `idts3D_ui/AGENTS.md`

如果是后端任务，还必须读取：

- `idts3D_api/AGENTS.md`

如果任一被引用文件不存在，必须明确报告，不得假装已读取。

## 5. 前后端一体化约束

- 数据库实体、C# Entity、DbSet、DTO、API 契约、TypeScript interface、前端 API Client 必须一一对应。
- 后端接口字段不得由前端猜测。
- 前端不得散落直接 `fetch` 调用，必须通过 `src/api` API Client 封装。
- `monitor` 模式只读取 Published 配置。
- `edit` 模式只编辑 Draft / Ready 配置，不得直接影响 monitor 当前 Published 配置。
- 后端不可用时允许 fallback 到本地模型 / localStorage / 静态 JSON，但 UI 必须能区分 fallback 与正式后端数据。
- 不允许绕过 docs 自行发明 API 路由、DTO 字段、响应格式、枚举值、数据库表或目录结构。

## 6. 执行边界

- 每次只执行一个明确任务。
- 不允许跨任务扩展。
- 不允许一次性实现多个 MVP。
- 不允许修改与当前任务无关的文件。
- 不允许大范围无关重构。
- 本项目为独立的 IDTS 3D 数字孪生工程。
- 不允许 commit / push，除非用户明确要求。
- 如果任务需要新增依赖，必须先说明原因、影响范围和替代方案，等待用户确认。

## 7. 执行前输出

修改文件前必须输出：

- 已读取文件列表。
- 未找到但被引用的文件列表。
- 当前任务类型。
- 影响范围。
- 禁止修改范围。
- 计划执行命令。
- 风险点。
- 是否需要等待用户确认。

## 8. 执行完成输出

任务完成后必须输出：

- 修改文件列表。
- 新增文件列表。
- 删除文件列表。
- 是否修改业务代码。
- 是否修改 `idts3D_ui/src/**`。
- 是否创建或修改 `idts3D_api` 工程。
- 是否新增依赖。
- 验证命令和结果。
- `git status` 摘要。
- `git diff --stat` 摘要。
