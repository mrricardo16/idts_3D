# 坐标系统与变换规范

> 状态：**Draft for review**。本规范不授权编码，不声明任何现场参数已确认。

## 1. 目的与适用范围

定义 POC-3DT-01 和后续 MVP-10A 的 GLB 与 3D Tiles 共用场景坐标约定、变换责任和可复现验证方法。它适用于静态底座与 GLB 动态设备同场展示，不把 Tiles 节点作为设备业务对象。

## 2. 当前可冻结的规则

| 项目 | 规则 |
|---|---|
| 场景坐标 | 由 TwinScene 作为统一世界空间，所有图层必须转换至该空间 |
| 单位 | 逻辑单位优先采用米；实际资产单位须经 POC 校验 |
| GLB | 设备实例变换由 devices 条目管理，Movable Part / Motion Target 仍在 GLB 内部业务坐标工作 |
| Tileset | 静态底座变换由 baseLayers 条目管理，不得复用设备绑定 |
| 变换顺序 | 使用先缩放、再旋转、后平移的局部到世界组合；具体矩阵约定实施前须与 Three.js API 复核 |
| 标定 | 至少使用三个可重复识别标定点，记录源坐标、目标坐标、偏差和观测方式 |

## 3. 待确认参数

下列全部为 TBD - 待 POC 或现场资料确认，不得从现有 GLB 示例、浏览器默认值或 tileset 元数据推断为最终现场事实：

- 场景原点及原点语义。
- 轴正方向、Y-up / Z-up 的资产输入与转换。
- 旋转单位、欧拉顺序或四元数来源。
- 高度基准、海拔/局部高程和地理参考关系。
- GLB、Tileset 与现场资料的绝对缩放。
- 坐标误差容差和最终验收阈值。

## 4. POC 验证方法

1. 记录浏览器、数据版本、GLB 与 Tileset 来源和 transform 配置。
2. 阶段 A 用小型许可明确 Tileset 检查单位、轴向、原点、比例和三个标定点。
3. 阶段 B 用获授权代表性样本复核同一方法，并记录偏差、原因和是否可通过显式变换校准。
4. 同时验证 GLB 拾取、worldZ 动画、Controls 和 Camera，不以视觉大致重合代替测量。
5. 在结果报告中记录误差、截图/录屏/日志位置和责任人；未达成项只能导致 Conditional Go 或 No-Go。

## 5. 变换记录格式

~~~text
assetKind: glb | tileset
assetId/version: TBD
sourceAxis: TBD
sceneAxis: TBD
translation: [TBD, TBD, TBD]
rotation: [TBD, TBD, TBD] or quaternion TBD
scale: [TBD, TBD, TBD]
heightReference: TBD
controlPoints: source / scene / error / evidence
~~~

## 6. 实施边界

本规范不创建数据库字段、DTO、TypeScript 类型或转换代码。正式字段仅可在 Scene Resource Manifest 设计审核和 MVP-10A 解锁后确定。

