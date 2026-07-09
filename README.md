# IDTS 3D 数字孪生系统

本仓库是 IDTS 3D 数字孪生系统的前后端一体化开发基线，包含 Vue 3 + TypeScript + Three.js 前端、预留 .NET 8 后端目录，以及可由 Codex 逐任务执行的 MVP 文档。

## 目录结构

- `idts3D_ui`：Vue3 + TypeScript + Three.js 前端工程，由原 `idts3ddemo` 技术验证 Demo 迁移而来。
- `idts3D_api`：.NET 8 后端 API 与 Worker 工程目录；文档设计阶段只保留 README，MVP-01 才创建真实 solution。
- `idts3D_docs`：项目方案、需求补充、MVP 总纲、API 契约、实体 DTO 映射、前后端集成计划、任务卡。

## 必读文档

- `AGENTS.md`：全仓库 Codex 执行规则。
- `idts3D_docs/development-rules.md`：文档设计阶段与 MVP 开发阶段规则。
- `idts3D_docs/idts-mvp-task-breakdown.md`：MVP 总纲。
- `idts3D_docs/api-contracts/README.md`：统一 API 契约入口。
- `idts3D_docs/domain-entity-dto-map.md`：数据库实体、DTO、TypeScript 类型映射。
- `idts3D_docs/mvp-tasks/README.md`：MVP-00 到 MVP-16 任务卡索引。

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
- 默认禁止 commit / push，除非用户明确要求。

## 版本控制边界

- Git 管理项目源码、配置、脚本和文档。
- `idts3D_ui/node_modules`、`idts3D_ui/dist`、缓存、日志和临时文件不纳入 Git。
- `idts3D_ui/public/models/lifter.glb` 是大模型运行文件，默认不作为普通 Git 对象提交；如需纳入版本控制，建议使用 Git LFS。
