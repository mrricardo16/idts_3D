# IDTS 数字孪生 MVP 开发总纲

> 本总纲是当前唯一执行基线。它只定义任务边界和依赖，不授权业务代码、数据库、依赖安装、POC、构建、测试或 Git 写操作。

> 实现状态校准：MVP-00～MVP-07 已完成；MVP-08 为 Partially Completed（实现和本地自动验证完成，真实 PostgreSQL/Swagger 验证未完成）。当前基线最近 CI 未通过 repository-policy，因此在该失败被独立处理并复核前，不进入下一实现任务；这不改变 POC/MVP-10A 的既有门禁。

## 1. MVP 目标与范围

MVP 继续完成 GLB 动态设备的资产、Manifest、Object Tree、Movable Part、Motion Target、Edit / Monitor 与 worldZ 闭环。3D Tiles 负责大型静态厂区底座，GLB 负责动态设备和业务交互。

当前 MVP 不包含完整 CAD/IFC 自动转换、生产切片平台、完整 3D Tiles 资产管理、未授权真实数据或最终现场性能承诺。POC 不落库；正式数据库结构、Scene Resource 字段、DTO、TypeScript 与 API 仍需在 MVP-10A-01 审核冻结。

## 2. 唯一执行顺序

~~~text
文档审核
→ POC-3DT-01
→ POC 结果审核
→ 用户批准 Go 或获批 Conditional Go
→ MVP-09 / MVP-10
→ MVP-10A-01
→ MVP-10A-02
→ MVP-10A-03
→ MVP-10A-04
→ MVP-10A-05
→ MVP-11～MVP-16
~~~

MVP-09、MVP-10 可以与 POC 并行；单人默认优先执行 POC。POC 不阻塞这两个任务或无关纯后端工作，但直接阻塞全部 MVP-10A 子任务。MVP-11～MVP-16 的正式混合场景完成依赖其所需的 MVP-10A 子任务。

## 3. 阶段门禁

| 阶段 | 进入条件 | 退出/验收条件 |
|---|---|---|
| 文档审核 | 本总纲、ADR、坐标、架构、生命周期、Manifest、性能、回退与任务卡可审查 | 用户确认可进行 POC 准备 |
| POC-3DT-01 | 单独执行授权、许可明确的阶段 A 数据 | 测试计划和结果模板有可复现证据 |
| POC 结果审核 | Go / Conditional Go / No-Go 建议已记录 | 用户或项目负责人批准结论 |
| MVP-09/10 | 既有后端前置完成 | GLB 设备绑定和当前 devices Manifest 基线可验收 |
| MVP-10A-01～05 | POC 门禁全部解除 | 每张子卡独立验收、回归、回滚材料完备 |
| MVP-11～16 | 所需 10A 子任务完成 | 混合场景、后端配置、失败回退与资源释放闭环验收 |

## 4. 禁止跨阶段事项

- POC 不修改 TwinDemo 主流程、正式 TwinScene、API、DTO、TypeScript 正式类型、数据库或 Migration。
- MVP-09 不使用设备实例或设备模型绑定表达静态厂区底座。
- MVP-10 只维持当前 devices 基线；tilesets 空占位不是正式静态底座契约。
- MVP-11 只建立当前 GLB/API Client 基线，不提前定义 baseLayers、Tiles 或 Scene Resource 类型。
- MVP-12 不在 MVP-10A-04 前实现正式混合 Manifest 加载，不把 Tiles 内部节点加入设备 Object Tree。
- MVP-13 只编辑 GLB 动态设备；TilesLayer 只读。
- MVP-14 只驱动 GLB；Tiles 存在或失败都不得破坏 GLB Monitor。
- MVP-15 不承担 CAD/IFC 到 3D Tiles 的生产切片。
- MVP-16 不把 POC 证据、tilesets 空占位或单纯构建成功作为正式混合场景验收。

## 5. 端到端联调路径

~~~text
审核后的 baseLayers + devices 契约
→ 加载静态 3D Tiles 底座
→ 加载 GLB 动态设备
→ 校验三个标定点、位置、方向、比例
→ GLB Object Tree、拾取、高亮仍可用
→ Edit 保存 GLB Movable Part / Motion Target
→ 发布并在 Monitor 只读加载
→ Tiles 存在时 worldZ、告警、高亮正常
→ Tiles 失败时 GLB-only fallback
→ 场景切换、请求取消、页面退出后资源释放
~~~

## 6. 契约与数据规则

正式方向为 baseLayers + devices。baseLayers 表达场景级静态底座，devices 表达 GLB 动态设备。model_asset / asset_version 是资源身份与版本候选；scene_resource 或 scene_layer 是静态底座放置关系候选。最终表名、字段、兼容策略和 API 版本必须由 MVP-10A-01 冻结，不得猜测实施。

## 7. Codex 执行规则

每次只执行一个明确任务。实施前读取当前任务卡和引用契约，输出影响范围、禁止范围、风险、验证与回滚，再等待用户对该实施任务的授权。任何任务发现跨端契约、数据库或架构债务时，只登记，不在无关任务中顺手修复。
