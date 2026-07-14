# MVP-10A：3D Tiles + GLB 正式混合场景接入

> 状态：Blocked。总卡仅编排已拆分子任务，不授权实施。

## 1. 解锁条件

POC-3DT-01 已完成；结果报告完整；用户批准 Go 或获批 Conditional Go；ADR、坐标、架构、生命周期、Manifest、性能与回退文档审核通过。

## 2. 子任务链

1. [MVP-10A-01：契约与数据冻结](./MVP-10A-01-contract-data-freeze.md)
2. [MVP-10A-02：TwinScene 图层骨架](./MVP-10A-02-twinscene-layer-skeleton.md)
3. [MVP-10A-03：Tiles 与坐标接入](./MVP-10A-03-tiles-coordinate-integration.md)
4. [MVP-10A-04：混合 Manifest 加载](./MVP-10A-04-hybrid-manifest-loading.md)
5. [MVP-10A-05：回退、性能与生命周期](./MVP-10A-05-fallback-performance-lifecycle.md)

每张子卡必须独立授权、验收、回归和回滚。不得跳过顺序；MVP-11～16 的正式混合验收按子卡依赖执行。

## 3. 总体边界

正式方向是 baseLayers + devices。TilesLayer 只读承载静态底座；DeviceLayer 承载 GLB 动态设备、Object Tree、Movable Part、Motion Target、Edit / Monitor、worldZ、状态和告警。不得用 device_instance 或 device_model_binding 伪装静态底座；不得混入 CAD/IFC 转换、生产切片或完整 Tiles 资产平台。

## 4. 总体验收与回退

完成链路须证明静态底座与 GLB 同场、坐标可复现、GLB 交互和动画正常、Tiles 失败时 GLB-only fallback 可用、请求取消和资源释放可验证。回退优先关闭 TilesLayer 并恢复经验证的纯 GLB 路径。

## 5. Codex 提示词

请执行 MVP-10A 总卡编排核对。确认全部解锁条件和指定子任务前置均已满足；只执行当前获授权的一张子卡，不扩大到其他子卡、CAD/IFC 转换或生产切片。
