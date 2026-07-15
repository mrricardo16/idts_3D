# IDTS 3D 数字孪生系统

本仓库是 IDTS 3D 数字孪生系统的前后端一体化开发基线，包含 Vue 3 + TypeScript + Three.js 前端、.NET 8 后端与 Worker、以及可由 Codex 逐任务执行的 MVP 文档。

## 目录结构

- `idts3D_ui`：Vue3 + TypeScript + Three.js 前端工程，由原 `idts3ddemo` 技术验证 Demo 迁移而来。
- `idts3D_api`：.NET 8 后端 API、Application、Contracts、Domain、Infrastructure 与 Worker 六项目 Solution；实现状态以本文件“当前实现状态”和 DOC-PLAN-01 校准报告为准。
- `idts3D_docs`：项目方案、需求补充、MVP 总纲、API 契约、实体 DTO 映射、前后端集成计划、任务卡。

## 必读文档

- `AGENTS.md`：全仓库 Codex 执行规则。
- `idts3D_docs/development-rules.md`：文档设计阶段与 MVP 开发阶段规则。
- `idts3D_docs/idts-mvp-task-breakdown.md`：MVP 总纲。
- `idts3D_docs/api-contracts/README.md`：统一 API 契约入口。
- `idts3D_docs/domain-entity-dto-map.md`：数据库实体、DTO、TypeScript 类型映射。
- `idts3D_docs/mvp-tasks/README.md`：MVP-00 到 MVP-16 任务卡索引。
- `idts3D_docs/architecture/project-architecture-baseline.md`：当前真实架构基线。
- `idts3D_docs/architecture/project-architecture-debt-register.md`：架构债务登记册。
- `idts3D_docs/architecture/project-refactoring-roadmap.md`：独立治理任务路线图。
- `idts3D_docs/architecture/project-architecture-review-checklist.md`：任务架构复检清单。
- `idts3D_docs/architecture/main-delivery-workflow.md`：main 直接交付流程。

## 当前实现状态

> 事实校准日期：2026-07-15；基线为 `main` 的 `5a9a2c5339e11bd3c77072ce276a8a4940c09739`。状态与证据详见 `idts3D_docs/reviews/DOC-PLAN-01-implementation-state-calibration-report.md`。本节不将代码存在、自动化测试、本地构建和 CI 运行互相替代。

- 当前分支：`main`；本次校准开始时工作区仅有未跟踪的 DOC-PLAN-00 审计报告。
- 已完成：MVP-00～MVP-07 的文档/后端基线。后端已具备 PostgreSQL EF Core、初始 Migration、GLB 上传与本地存储、Model Manifest、Object Tree/Model Stats、资产版本生命周期和 Movable Part CRUD。
- 部分完成：MVP-08 的 Motion Target CRUD 已实现，且本地 Application、Architecture 与 API Integration 测试已于 2026-07-15 通过；真实 PostgreSQL/Swagger 手工验收、事务与行锁验证仍未完成，因此不标为完全验证完成。
- 规划中或未开始：Scene/Device 业务 API、正式前端 API Client、前后端业务联调、Worker 转换流水线、浏览器 E2E、真实 WebGL 和 PostgreSQL 基础设施集成验证。
- 本地构建：`dotnet build idts3D_api/HZ.IDTS.DigitalTwin.sln` 于 2026-07-15 退出码 0、0 warning、0 error；实际 SDK 为 8.0.100，与 `idts3D_api/global.json` 声明的版本一致。
- 本地测试：`dotnet test idts3D_api/HZ.IDTS.DigitalTwin.sln --no-build` 于 2026-07-15 退出码 0；Application 54、Architecture 8、API Integration 22，共 84 项通过。该结果不覆盖真实 PostgreSQL、真实文件存储、浏览器 E2E 或真实 WebGL。
- CI：`.github/workflows/ci.yml` 存在，包含后端 build/三类测试和前端 lint/type-check/unit/build。当前基线最近 CI 运行 `29313188014` 为失败：`repository-policy` 检出 `DOC-3DT-03-semantic-consistency-fix-report.md` 末尾多余空行，后端与前端质量作业未启动；不得写为“CI 通过”。
- 当前下一入口：暂无实现任务入口。须先在独立获授权任务中处理并复核当前 CI 失败；POC-3DT-01 仍需用户单独授权，MVP-10A 继续 Blocked。

## 前端运行

```bash
cd idts3D_ui
npm install
npm run dev
npm run build
```

## 开发阶段规则

- 文档设计阶段禁止写业务代码、创建真实后端工程、执行 migration、修改 `idts3D_ui/src/**`。
- MVP 开发阶段必须由用户明确指定单个任务卡，并且先输出影响范围、等待确认后再修改。
- 只读阶段禁止 commit / push；用户确认写入任务后，按项目级 Skill 的流程验证、commit 并 push `origin/main`，始终禁止 force push。

## 版本控制边界

- Git 管理项目源码、配置、脚本和文档。
- `idts3D_ui/node_modules`、`idts3D_ui/dist`、缓存、日志和临时文件不纳入 Git。
- `idts3D_ui/public/models/lifter.glb` 是大模型运行文件，默认不作为普通 Git 对象提交；如需纳入版本控制，建议使用 Git LFS。
