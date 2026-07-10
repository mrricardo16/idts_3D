---
name: idts3d-architecture-governance
description: Govern all idts_3D repository tasks, including architecture, documentation, backend, frontend, contracts, integration, verification, and direct delivery to main.
---

# idts_3D 项目架构治理

## 适用范围与触发

本 Skill 覆盖根目录治理、后端、前端、API 契约、数据库、Worker、文件存储、文档、测试、Git 交付和架构债务。修改源码、配置、数据库、API、DTO、前端类型、目录结构、Worker、文件存储、任务卡、架构文档，进行非功能重构或提交/推送 `main` 时，必须先读取本 Skill。

## 任务开始门禁

1. 判定任务类型和允许/禁止修改范围；只执行一个独立任务。
2. 确认只有一个写入型 Codex 选项卡，当前分支为 `main`，`git status --short` 为空。
3. 执行 `git fetch origin`、`git rev-list --left-right --count main...origin/main`、`git pull --ff-only origin main`；不是 `0/0` 或无法 fast-forward 时停止。
4. 输出受影响层、API/数据库/前端/Worker/事务影响、测试计划、可能新增债务及是否应拆为独立 ARCH 任务；未经用户确认不得写入。

## 当前真实基线

| 状态 | 内容 |
|---|---|
| 已实现 | .NET 8 六项目 Solution、PostgreSQL EF Core、初始 Migration、GLB 上传、本地文件存储、`/assets`、Model Manifest、Object Tree、Model Stats、Asset Version 生命周期。 |
| 部分实现 | Worker 为空骨架；scene/device/motion 已有表结构但无业务 API；前端是静态数字孪生 Demo，使用静态 manifest、mock 和 localStorage。 |
| 未实现 | 正式前端 API Client、前后端业务联调、Scene/Movable Part/Motion Target API、Worker 转换流水线、自动化测试、CI。 |

不得将目标架构描述为已实现状态。

## 根目录与文档治理

- 源码、文档、生成文件、本地配置必须分离；不得提交 `bin`、`obj`、`dist`、日志、数据库导出、上传/转换临时文件。
- 新建文本文件必须遵循根级 `.editorconfig`；文本与二进制 Git 属性以根级 `.gitattributes` 为准。
- 业务任务不得执行 `git add --renormalize .`，也不得顺手批量转换全仓库换行、BOM 或文件末尾换行。
- 新增构建产物、本地输出或自动生成报告前必须检查 `.gitignore`；正式模型资产不得仅因扩展名被误忽略，目录与大文件策略未明确时须登记独立治理任务。
- `debug` 与 `reports` 仅可在人工筛选后入库；历史编码或换行问题登记为独立治理任务，不在无关任务中批量修复。
- 不无边界新增根目录；新增目录必须有明确职责。
- README 必须反映真实进度。
- Markdown 是治理规则和 API 契约的规范源；DOCX 只作参考，不覆盖 Markdown。

## 后端依赖与分层

允许方向：`Api -> Application`、`Api -> Contracts`、`Api -> Infrastructure`、`Application -> Domain`、`Application -> Contracts`、`Infrastructure -> Application/Domain/Contracts`、`Worker -> Application/Contracts/Infrastructure`。

禁止：`Domain -> Application/Infrastructure/Api`，`Application -> Infrastructure/Api`，`Contracts -> Infrastructure/Api`。

- Controller 只处理路由、绑定、调用 Service 和返回统一响应；不得访问 DbContext、管理事务、承载复杂规则或返回 Entity。
- Application 编排用例、状态和门禁；不得引用 Infrastructure、使用 Provider SQL 或实现具体文件系统。
- Repository 必须服务清晰能力/聚合；新增能力先判断是否需要独立 Service/Repository。发现多职责只登记债务，不在业务任务中大拆。
- Domain 保持纯净，不含 HTTP、EF、文件存储或 UI。
- Contracts 与契约一致，统一 ErrorCode，不返回 Entity。
- Infrastructure 承担 EF Core、PostgreSQL、文件存储和 Provider 实现；不得决定业务状态矩阵。

## 前端与跨端规则

- Vue 页面不得承载完整 Three.js 引擎，不得散落 fetch/axios、复制 DTO 或混合 API、业务状态、engine 和复杂 UI。
- 场景生命周期、模型加载、交互、拾取、LOD、统计与 Vue UI 必须通过稳定边界协作；页面不得任意操作 Scene、Renderer、Object3D。
- 正式联调后，所有 API 请求进入 `src/api` 或明确 service 层；页面和 engine 不得分别请求同一接口。
- TypeScript 类型、错误码、状态枚举、`edit`/`monitor`/`Published` 语义必须与 Contracts 和 API 契约同步。
- 页面局部、跨组件、设备实时与 Three.js 引擎状态必须区分；utils 不得成为领域逻辑收容器。
- API 契约 Markdown 是设计规范源。修改任一端时必须列出另一端影响，不得假设自动兼容。

## 数据、Worker、文件与事务

- Schema 变更必须有明确任务，并同步 Entity、EF Configuration、Migration 和数据库；`database update` 需明确授权。
- Worker 只处理后台任务，与 API 共用 Application 能力；幂等、重试、日志、失败恢复必须显式设计。
- 数据库只保存公开 URL 或受控相对路径；不得返回物理路径或硬编码个人机器路径。文件与数据库提交必须有补偿策略。
- 状态更新必须属于明确事务边界；锁放在 Infrastructure；审计与业务更新保持一致；Controller 不管理事务。
- 使用 ApiResponse 和 ErrorCode；不回传异常堆栈，按语义返回 400/404/409，而非模糊 500。

## 拆分信号、测试与非功能保护

评估拆分的信号包括：一个类/Repository/Controller 承担两个以上独立能力，一个文件聚集无关 Entity/EF Configuration，一个 Vue 页面混合 API、状态、Three.js 与复杂 UI，或新需求为已有大类增加第三种职责。当前任务只做必要内容并登记债务，创建独立 ARCH 任务，不顺手大拆。

测试缺失是已登记债务。状态流转优先 Application Test，事务优先 Integration Test，前端 API Client/状态/关键交互优先测试；没有自动测试时必须列出人工回归。

非功能重构必须独立任务、独立选项卡、独立 commit；不得改变 API 路由/JSON、数据库/Migration、状态/事务、错误码、文件路径、DI 生命周期、页面或 Three.js 表现，并需验证和回滚方法。

## 债务登记

债务必须记录编号、范围、状态、优先级、证据、当前影响、扩张风险、触发条件、独立任务、行为保护、验证和回滚。旧债务不得在无关业务任务中修复。

## main 直接交付

1. 写入任务从最新 `origin/main` 开始，只在 `main` 工作，不创建任务分支。
2. 实施只改允许范围；发现债务只登记。
3. 执行任务要求的 build/test/lint/API/页面验证，以及 `git diff --check`、`git diff --stat`、`git status --short` 和生成物/本地配置检查。
4. 任一适用验证失败、未执行或无法给出明确失败原因时，必须停止，不得 commit 或 push。
5. 提交前再次 `git fetch origin` 并检查 `main...origin/main`；远端变化时只允许 `git pull --ff-only origin main`，同步后重新验证。
6. 一个任务一个 commit，`git push origin main`；禁止 force push。
7. 推送后确认 `main` 与 `origin/main` 为 `0/0` 且工作区干净。

只读分析阶段禁止 commit/push。未经用户确认的写入阶段禁止 commit/push。用户确认写入任务后，完成验证必须 commit 并 push `origin/main`。

## 架构预检输出

输出：当前分支、main 同步状态、工作区状态、任务类型、允许/禁止范围、受影响层、API/数据库/前端/Worker/事务影响、测试计划、新增债务和是否需要独立 ARCH 任务。

## 架构复检输出

输出：实际新增/修改文件、分层/依赖/API/数据库/前端/Worker/事务/审计变化、测试与 build/lint 结果、新增债务、git diff、commit SHA、push 结果和最终工作区状态。
