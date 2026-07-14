# IDTS 数字孪生 MVP 任务卡索引

> 本目录保存可由 Codex 按单任务执行的 MVP 任务卡。本文档中的 3D Tiles / GLB 混合场景方向是文档基线；在用户完成审核和明确授权前，不授权任何正式编码。

## 1. 当前状态与门禁

- 已完成或正在执行的 MVP-00～MVP-08 保持原编号和既有 GLB 主线，不重写、不改号。
- POC-3DT-01 是正式混合场景接入前必须完成的技术验证，但不阻塞 MVP-09、MVP-10 或无关纯后端任务。
- MVP-10A 是新增的正式混合场景接入任务，状态为 **Blocked**；它直接受 POC-3DT-01 结果门禁约束。
- MVP-11～MVP-16 的正式混合场景验收依赖 MVP-10A；在 MVP-10A 完成前，不得以纯 GLB 最终架构宣称这些任务已完成混合场景交付。
- 文档门禁：先完成本轮文档审核，再执行 POC；POC 结果、用户 Go 或获批 Conditional Go、相关设计审核通过后，才可解除 MVP-10A。

详细规则见 development-rules.md、ADR-001 和 MVP-10A 任务卡。

POC 材料入口：[测试计划](../poc/POC-3DT-01-test-plan.md)、[结果报告模板](../poc/POC-3DT-01-result-report-template.md)、[性能预算](../performance/3d-performance-budget.md) 和 [回退方案](../operations/3dtiles-fallback-and-rollback-plan.md)。

## 2. 任务卡列表

| 顺序 | 任务编号 | 任务名称 | 状态 / 依赖 | 文档 |
|---:|---|---|---|---|
| 0～8 | MVP-00～MVP-08 | 既有文档、后端与 GLB 基线 | 保持原范围和编号 | [MVP-00 至 MVP-08](./) |
| 9 | MVP-09 | 场景 / 设备实例 / 设备模型绑定 | GLB 设备职责；不表达静态底座 | [MVP-09](./MVP-09-scene-device-binding.md) |
| 10 | MVP-10 | Scene Manifest 基础交付 | 当前 tilesets 仅占位 | [MVP-10](./MVP-10-scene-manifest.md) |
| POC | POC-3DT-01 | 3D Tiles + GLB 独立验证 | Ready for review；不阻塞 MVP-09/10 | [POC-3DT-01](./POC-3DT-01-threejs-3dtiles-renderer.md) |
| 10A | MVP-10A | 3D Tiles + GLB 正式混合场景 | **Blocked**；受 POC 结果与用户批准门禁约束 | [MVP-10A](./MVP-10A-3dtiles-glb-hybrid-scene.md) |
| 11 | MVP-11 | 前端 API Client 与契约类型 | 正式混合类型以 MVP-10A 后的设计为准 | [MVP-11](./MVP-11-frontend-api-client-contract-types.md) |
| 12 | MVP-12 | 前端 Manifest / Object Tree | 正式混合加载依赖 MVP-10A | [MVP-12](./MVP-12-frontend-manifest-object-tree.md) |
| 13 | MVP-13 | Edit 保存 Movable Part / Motion Target | Tiles 底座只读 | [MVP-13](./MVP-13-frontend-edit-mode-save.md) |
| 14 | MVP-14 | Monitor / worldZ 动画 | TilesLayer 共存与失败回退 | [MVP-14](./MVP-14-monitor-mode-runtime-animation.md) |
| 15 | MVP-15 | GLB 转换任务状态与基础日志 | 不含 Tiles 生产切片 | [MVP-15](./MVP-15-conversion-job-status-log.md) |
| 16 | MVP-16 | 端到端联调与验收 | 正式混合场景闭环 | [MVP-16](./MVP-16-e2e-acceptance.md) |

既有任务卡：[MVP-00](./MVP-00-development-rules-doc-baseline.md)、[MVP-01](./MVP-01-backend-solution-skeleton.md)、[MVP-02](./MVP-02-database-core-schema.md)、[MVP-03](./MVP-03-glb-upload-file-storage.md)、[MVP-04](./MVP-04-model-manifest-api.md)、[MVP-05](./MVP-05-object-tree-model-stats.md)、[MVP-06](./MVP-06-asset-version-publish-baseline.md)、[MVP-07](./MVP-07-movable-part-api.md)、[MVP-08](./MVP-08-motion-target-api.md)。

## 3. 依赖与执行入口

建议执行顺序为：

~~~text
MVP-08
→ 文档基线审核
→ POC-3DT-01 文档准备
→ MVP-09
→ MVP-10
→ 执行 POC-3DT-01
→ POC 结果审核
→ MVP-10A
→ MVP-11～MVP-16
~~~

其中：

- POC 可在 MVP-09 / MVP-10 之后执行；它不冻结无关后端工作。
- POC 为 Go，或 Conditional Go 已被用户明确批准，且结果报告和设计审核完成，才可启动 MVP-10A。
- baseLayers + devices 是 MVP-10A 的正式 Scene Manifest 演进方向；MVP-10 的 tilesets: [] 不是正式静态底座完成证明。

## 4. 职责边界

| 资源 | 正式职责 | 不承担的职责 |
|---|---|---|
| 3D Tiles / baseLayers | 厂房、楼层、墙体、道路、大型货架、固定设施及静态环境底座的流式加载 | 动态设备动画、Movable Part、Motion Target、设备状态/告警、设备级业务编辑、完整业务 Object Tree |
| GLB / devices | 提升机、堆垛机、AGV、输送设备、机械臂、托盘、货物及动态业务交互 | 伪装承载大型静态厂区底座 |

Object Tree、Movable Part、Motion Target、Edit / Monitor、worldZ、拾取、高亮、状态与告警继续服务 GLB 动态设备。

## 5. 统一任务卡结构

所有实现任务卡必须包含目标、前置条件、影响和禁止范围、后端/前端/数据库/API 契约、对应关系、执行步骤、验收、回归、风险、回滚和 Codex 提示词。任何正式编码仍须按项目规则先输出影响范围并取得该次实施授权。

## 6. 本轮文档后的下一入口

本轮仅完成文档统一修订，下一步是用户审核本轮文档及 POC 准备材料；不是直接实现 POC 或 MVP-10A。
