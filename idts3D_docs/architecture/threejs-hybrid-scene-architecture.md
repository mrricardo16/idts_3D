# Three.js 混合场景架构

> 状态：Draft for review。仅定义设计，不授权实现。

## 1. 目标结构

~~~text
TwinScene
├── Renderer
├── Camera
├── Controls
├── AnimationLoop
├── ResourceManager
├── CoordinateTransformer
├── InteractionManager
├── TilesLayer
├── DeviceLayer
├── AnnotationLayer
└── HelperLayer
~~~

Renderer、Camera、Controls、AnimationLoop、ResourceManager、CoordinateTransformer 与 InteractionManager 均由 TwinScene 直接拥有；它们不是 HelperLayer 的子职责。

## 2. 职责

TilesLayer 消费 baseLayers，加载、更新、隐藏、卸载静态 3D Tiles 底座并报告错误。DeviceLayer 消费 devices，加载 GLB、维护 Object Tree、拾取、状态、动画和业务交互。AnnotationLayer 管理标签、告警、callout。HelperLayer 只提供网格、标定点和调试辅助。

InteractionManager 分层筛选候选对象，GLB 业务对象遵从现有注册和拾取规则；Tiles 节点策略须在正式子任务中确认，不能进入设备 Object Tree。

## 3. POC 与正式接入

POC 使用独立页面，但复用项目现有 Renderer、Controls、Loader、交互、worldZ、生命周期和 fallback 形态。MVP-10A-02 建立图层骨架，MVP-10A-03 接入 Tiles 和坐标，MVP-10A-04 接入正式 Manifest，MVP-10A-05 验收回退、性能和生命周期。
