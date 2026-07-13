# IDTS 数字孪生 MVP 任务卡索引

> 本目录保存后续可由 Codex 串行执行的 MVP 任务卡。文档设计阶段只维护任务卡；MVP 开发阶段必须由用户明确指定单个任务后才能执行。

## 1. 阶段规则

| 阶段 | 规则 |
|---|---|
| 文档设计阶段 | 禁止写业务代码，禁止创建真实后端工程，禁止 migration，禁止修改 `idts3D_ui/src/**` |
| MVP 开发阶段 | 允许严格按当前任务卡创建后端、写 C#、写 Vue/TS、执行 migration |
| 所有阶段 | 每个任务先输出影响范围，等待确认后再改；禁止跨任务扩展；禁止一次性实现多个 MVP；禁止 commit / push，除非用户明确要求 |

详细规则见 `../development-rules.md`。

## 2. 任务卡列表

| 顺序 | 任务编号 | 任务名称 | 文档 |
|---:|---|---|---|
| 0 | MVP-00 | 开发规则切换与文档基线 | [MVP-00-development-rules-doc-baseline.md](./MVP-00-development-rules-doc-baseline.md) |
| 1 | MVP-01 | 后端解决方案骨架 | [MVP-01-backend-solution-skeleton.md](./MVP-01-backend-solution-skeleton.md) |
| 2 | MVP-02 | 数据库核心实体与 Migration | [MVP-02-database-core-schema.md](./MVP-02-database-core-schema.md) |
| 3 | MVP-03 | 文件存储与 GLB 上传 | [MVP-03-glb-upload-file-storage.md](./MVP-03-glb-upload-file-storage.md) |
| 4 | MVP-04 | Model Manifest 查询接口 | [MVP-04-model-manifest-api.md](./MVP-04-model-manifest-api.md) |
| 5 | MVP-05 | Object-tree / Model-stats | [MVP-05-object-tree-model-stats.md](./MVP-05-object-tree-model-stats.md) |
| 6 | MVP-06 | 资产版本状态与发布基线 | [MVP-06-asset-version-publish-baseline.md](./MVP-06-asset-version-publish-baseline.md) |
| 7 | MVP-07 | 可动部件配置 API | [MVP-07-movable-part-api.md](./MVP-07-movable-part-api.md) |
| 8 | MVP-08 | Motion Target API | [MVP-08-motion-target-api.md](./MVP-08-motion-target-api.md) |
| 9 | MVP-09 | 场景 / 设备实例 / 设备模型绑定 | [MVP-09-scene-device-binding.md](./MVP-09-scene-device-binding.md) |
| 10 | MVP-10 | Scene Manifest | [MVP-10-scene-manifest.md](./MVP-10-scene-manifest.md) |
| 11 | MVP-11 | 前端 API Client 与契约类型 | [MVP-11-frontend-api-client-contract-types.md](./MVP-11-frontend-api-client-contract-types.md) |
| 12 | MVP-12 | 前端接后端 Manifest / Object-tree | [MVP-12-frontend-manifest-object-tree.md](./MVP-12-frontend-manifest-object-tree.md) |
| 13 | MVP-13 | 前端 Edit 模式保存可动部件与目标点位 | [MVP-13-frontend-edit-mode-save.md](./MVP-13-frontend-edit-mode-save.md) |
| 14 | MVP-14 | Monitor 模式只读配置并驱动 worldZ 动画 | [MVP-14-monitor-mode-runtime-animation.md](./MVP-14-monitor-mode-runtime-animation.md) |
| 15 | MVP-15 | 转换任务状态与基础日志 | [MVP-15-conversion-job-status-log.md](./MVP-15-conversion-job-status-log.md) |
| 16 | MVP-16 | 端到端联调与验收 | [MVP-16-e2e-acceptance.md](./MVP-16-e2e-acceptance.md) |
| POC | POC-3DT-01 | Three.js + 3DTilesRendererJS 最小验证 | [POC-3DT-01-threejs-3dtiles-renderer.md](./POC-3DT-01-threejs-3dtiles-renderer.md) |

## 3. 必须串行任务

当前已完成至 MVP-07；下一项串行后端任务为 MVP-08 Motion Target API。

MVP 主线必须按 MVP-00 到 MVP-16 顺序执行。每次只执行一个任务。

关键依赖：

- MVP-02 依赖 MVP-01。
- MVP-03 依赖 MVP-02。
- MVP-04 依赖 MVP-03。
- MVP-05 依赖 MVP-02 和 MVP-03。
- MVP-06 依赖 MVP-04 和 MVP-05。
- MVP-07 依赖 MVP-05 和 MVP-06。
- MVP-08 依赖 MVP-07。
- MVP-09 依赖 MVP-06。
- MVP-10 依赖 MVP-09。
- MVP-11 依赖后端契约稳定。
- MVP-12 依赖 MVP-10 和 MVP-11。
- MVP-13 依赖 MVP-07、MVP-08、MVP-12。
- MVP-14 依赖 MVP-13。
- MVP-15 依赖 MVP-03。
- MVP-16 依赖 MVP-01 到 MVP-15。

## 4. 后端任务

- MVP-01 后端解决方案骨架。
- MVP-02 数据库核心实体与 Migration。
- MVP-03 文件存储与 GLB 上传。
- MVP-04 Model Manifest 查询接口。
- MVP-05 Object-tree / Model-stats。
- MVP-06 资产版本状态与发布基线。
- MVP-07 可动部件配置 API。
- MVP-08 Motion Target API。
- MVP-09 场景 / 设备实例 / 设备模型绑定。
- MVP-10 Scene Manifest。
- MVP-15 转换任务状态与基础日志。

## 5. 前端任务

- MVP-11 前端 API Client 与契约类型。
- MVP-12 前端接后端 Manifest / Object-tree。
- MVP-13 前端 Edit 模式保存可动部件与目标点位。
- MVP-14 Monitor 模式只读配置并驱动 worldZ 动画。

## 6. 联调任务

- MVP-16 端到端联调与验收。

## 7. 技术验证任务

- POC-3DT-01 可独立验证 Three.js + 3D Tiles，不并入 MVP 主线。
- POC 不得阻塞 MVP-00 到 MVP-16。
- POC 不得修改 MVP 主流程，除非用户明确要求合并验证结果。

## 8. 统一任务卡结构

所有 MVP 任务卡必须包含：

1. 任务目标
2. 前置条件
3. 影响范围
4. 禁止修改范围
5. 后端变更
6. 前端变更
7. 数据库变更
8. API 契约
9. 前后端对应关系
10. 执行步骤
11. 验收标准
12. 回归测试
13. 风险点
14. 回滚策略
15. Codex 执行提示词

## 9. 后续执行入口

当前文档基线完成后，后续真正开发应从：

```text
MVP-01：后端解决方案骨架
```

开始，不要跳过 MVP 顺序。
