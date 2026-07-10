# IDTS 3D 数字孪生系统

本仓库是 IDTS 3D 数字孪生系统的前后端一体化开发基线，包含 Vue 3 + TypeScript + Three.js 前端、.NET 8 后端与 Worker、以及可由 Codex 逐任务执行的 MVP 文档。

## 目录结构

- `idts3D_ui`：Vue3 + TypeScript + Three.js 前端工程，由原 `idts3ddemo` 技术验证 Demo 迁移而来。
- `idts3D_api`：.NET 8 后端 API、Application、Contracts、Domain、Infrastructure 与 Worker 六项目 Solution；当前已完成 MVP-01 至 MVP-06 的后端基线。
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

## 当前进度

- 已实现：后端 MVP-01 至 MVP-06，包括 PostgreSQL EF Core、初始 Migration、GLB 上传、本地文件存储、manifest、object-tree、model-stats 和资产版本生命周期。
- 部分实现：Worker 仅为空骨架；前端仍是静态数字孪生 Demo，使用静态 manifest、mock 和 localStorage。
- 尚未实现：正式前端 API Client、前后端业务联调、Scene/Movable Part/Motion Target API、Worker 转换流水线、自动化测试和 CI。

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
