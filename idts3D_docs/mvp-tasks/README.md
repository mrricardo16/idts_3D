# IDTS 数字孪生 MVP 任务卡索引

> 当前执行基线：文档审核后优先执行 POC-3DT-01；POC 结果经用户批准后才可进入 MVP-10A。本文档不授权任何代码、数据库、依赖、POC、构建或测试。

## 1. 状态与唯一门禁

- MVP-00～MVP-08 保持原编号、既有交付和 GLB 基线。
- POC-3DT-01 是正式 3D Tiles 与 GLB 混合接入的前置技术验证；它不阻塞 MVP-09、MVP-10 或无关纯后端工作。
- 单人默认优先顺序是文档审核、POC、POC 结果审核、MVP-09、MVP-10、MVP-10A-01～05、MVP-11～MVP-16。
- MVP-09、MVP-10 可以与 POC 并行，但不再要求必须先于 POC。
- MVP-10A、MVP-10A-01～05 均为 Blocked，只有 POC 完成、结果报告完成、用户批准 Go 或获批 Conditional Go、相关文档审核通过后才可解除。
- MVP-11～MVP-16 的正式混合场景验收依赖对应的 MVP-10A 子任务完成；不以纯 GLB 路径宣称混合场景完成。

## 2. 资源与职责

| 资源 | 职责 |
|---|---|
| 3D Tiles / baseLayers | 厂房、楼层、墙体、道路、大型货架、固定设施和静态厂区底座的流式加载 |
| GLB / devices | 动态设备、业务交互、Object Tree、Movable Part、Motion Target、Edit / Monitor、worldZ、状态、告警、拾取与高亮 |

静态底座不得用 device_instance 或 device_model_binding 伪装表达；Tiles 内部节点不进入设备 Object Tree。

## 3. 任务列表

| 顺序 | 任务 | 状态或依赖 | 文档 |
|---:|---|---|---|
| 0～8 | MVP-00～MVP-08 | 保持既有范围 | [既有任务卡](./) |
| POC | POC-3DT-01 | Ready for user execution approval | [POC 卡](./POC-3DT-01-threejs-3dtiles-renderer.md) |
| 9 | MVP-09 | 可与 POC 并行；GLB 设备绑定 | [MVP-09](./MVP-09-scene-device-binding.md) |
| 10 | MVP-10 | 可与 POC 并行；当前 devices 基线 | [MVP-10](./MVP-10-scene-manifest.md) |
| 10A | MVP-10A 总卡 | Blocked；由 POC 门禁控制 | [MVP-10A](./MVP-10A-3dtiles-glb-hybrid-scene.md) |
| 10A-01 | 契约与数据冻结 | Blocked | [任务卡](./MVP-10A-01-contract-data-freeze.md) |
| 10A-02 | TwinScene 图层骨架 | Blocked | [任务卡](./MVP-10A-02-twinscene-layer-skeleton.md) |
| 10A-03 | Tiles 与坐标接入 | Blocked | [任务卡](./MVP-10A-03-tiles-coordinate-integration.md) |
| 10A-04 | 混合 Manifest 加载 | Blocked | [任务卡](./MVP-10A-04-hybrid-manifest-loading.md) |
| 10A-05 | 回退、性能与生命周期 | Blocked | [任务卡](./MVP-10A-05-fallback-performance-lifecycle.md) |
| 11～16 | 前端类型、混合加载、编辑、运行、日志、验收 | 按新依赖执行 | [MVP-11](./MVP-11-frontend-api-client-contract-types.md) 至 [MVP-16](./MVP-16-e2e-acceptance.md) |

POC 材料：[测试计划](../poc/POC-3DT-01-test-plan.md)、[结果模板](../poc/POC-3DT-01-result-report-template.md)、[性能预算](../performance/3d-performance-budget.md)、[回退方案](../operations/3dtiles-fallback-and-rollback-plan.md)。

## 4. 当前默认执行顺序

~~~text
文档审核
→ POC-3DT-01
→ POC 结果审核
→ 用户批准 Go 或获批 Conditional Go
→ MVP-09 / MVP-10
→ MVP-10A-01 → 10A-02 → 10A-03 → 10A-04 → 10A-05
→ MVP-11 → MVP-12 → MVP-13 → MVP-14 → MVP-15 → MVP-16
~~~

## 5. 执行授权

POC 文档状态为 Ready for user execution approval；这不是 Executing、Completed 或生产授权。每张实现任务卡仍须在实施时单独取得用户授权，并按项目规则输出影响范围、验证与回滚方案。
