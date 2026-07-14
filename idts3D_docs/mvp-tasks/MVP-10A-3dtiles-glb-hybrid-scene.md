# MVP-10A：3D Tiles + GLB 正式混合场景接入

> 状态：**Blocked**。本任务卡不授权实施。

设计输入：[ADR-001](../architecture/ADR-001-3dtiles-glb-hybrid-architecture.md)、[坐标规范](../architecture/coordinate-system-and-transform-spec.md)、[混合场景架构](../architecture/threejs-hybrid-scene-architecture.md)、[资源生命周期](../architecture/hybrid-scene-resource-lifecycle.md)、[Manifest 设计](../design/scene-resource-manifest-design.md)、[性能预算](../performance/3d-performance-budget.md)、[回退方案](../operations/3dtiles-fallback-and-rollback-plan.md)。

## 1. 目标

在正式 TwinScene 中以统一 Renderer、Camera、Controls 和 Animation Loop 接入 TilesLayer、DeviceLayer、AnnotationLayer、HelperLayer，实现 baseLayers 静态底座与 devices GLB 动态设备的受控混合场景。

## 2. 解锁前置条件

必须全部满足：

1. POC-3DT-01 已完成。
2. POC 结果报告已完成且证据可复核。
3. 用户批准 Go，或批准指定条件已满足的 Conditional Go。
4. ADR、坐标、混合架构、生命周期、Manifest、性能和回退文档审核通过。

## 3. 影响范围与禁止范围

| 范围 | 预期影响 |
|---|---|
| 前端 | TwinScene 图层、资源管理、坐标转换、回退与前端契约消费 |
| 后端 / API | 仅按审核后的 Scene Resource Manifest 需要同步调整 |
| 数据库 | 仅经审核后评估 model_asset / asset_version 与 scene_resource 或 scene_layer |

禁止：以 device_instance / device_model_binding 表示静态底座；将 Tiles 节点塞入设备 Object Tree；修改 Tiles 节点业务属性；把 CAD/IFC 转换、生产切片或完整 Tiles 资产平台混入本任务；在没有用户授权时使用真实客户数据。

## 4. API、数据与前端原则

- 正式 Manifest 采用 baseLayers + devices；当前 tilesets 占位不能直接扩展为实施依据。
- 后端 DTO、API 契约、TypeScript 类型和 API Client 必须一一对应，不能由前端猜测。
- TilesLayer 是静态只读层；GLB DeviceLayer 保留拾取、Object Tree、Movable Part、Motion Target、Edit / Monitor、worldZ、状态和告警。
- POC 无数据库影响；正式数据库和 Migration 是否需要变更必须在解锁后另行输出影响分析并取得授权。

## 5. 执行步骤（解锁后）

1. 复核 POC 证据、批准记录和所有设计草案。
2. 输出跨端、数据库、契约、回退与兼容影响，等待实施授权。
3. 冻结 Manifest、坐标和生命周期实施契约。
4. 以单一上下文实现图层化加载和错误隔离。
5. 接入 GLB 设备交互与静态底座的分层回退。
6. 同步实现必要的后端、数据库、API、TypeScript 和 Client 变更。
7. 执行回归、性能、生命周期与回退验证，形成证据。

## 6. 验收与回归

- baseLayers 3D Tiles 与 devices GLB 可同场加载且坐标、方向、比例满足审核标准。
- GLB Object Tree、拾取、高亮、Movable Part、Motion Target、Edit / Monitor 和 worldZ 不回归。
- TilesLayer 存在时 GLB 动画、告警、高亮正常；Tiles 失败时 GLB 可回退。
- 场景切换、页面退出、请求取消和 WebGL 资源释放可验证。
- API、DTO、TypeScript、数据库映射和回退路径有一致性证据。

## 7. 风险、回滚与 Codex 提示词

风险包括坐标偏差、动态 tile 生命周期、契约漂移、性能退化、静态资源误入设备模型和回退失效。回滚应优先关闭 TilesLayer 并恢复纯 GLB 路径；数据或依赖回滚只能按获批方案执行。

~~~text
请执行 MVP-10A。先确认本卡已解除 Blocked，并读取 POC 结果、批准记录、ADR、坐标、架构、生命周期、Manifest、性能和回退文档。
先输出完整跨端影响与实施计划，等待用户对实施授权。不得把 device_instance 或 device_model_binding 用作静态底座，不得混入 CAD/IFC 转换或生产切片。
~~~
