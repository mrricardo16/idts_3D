# Three.js 混合场景架构

> 状态：**Draft for review**。仅定义目标设计，不授权实现。

## 1. 目标结构

~~~text
TwinScene
├── TilesLayer
├── DeviceLayer
├── AnnotationLayer
└── HelperLayer
    ├── Renderer
    ├── Camera
    ├── Controls
    ├── Animation Loop
    ├── Raycaster
    ├── Resource Manager
    └── Coordinate Transformer
~~~

TwinScene 是正式生产入口的统一拥有者。Renderer、Camera、Controls 和 Animation Loop 只能各有一个实例，所有层共享其上下文，不允许页面分别创建相互竞争的渲染循环。

## 2. 图层职责

| 模块 | 职责 | 明确不负责 |
|---|---|---|
| TilesLayer | 读取 baseLayers、加载/更新/卸载 3D Tiles 静态底座、报告失败 | 设备业务编辑、Movable Part、Motion Target、设备 Object Tree |
| DeviceLayer | 读取 devices、加载 GLB、对象注册、拾取、状态/动画 | 表达厂区静态底座 |
| AnnotationLayer | 告警、标签、callout、状态说明 | 私自管理模型资源 |
| HelperLayer | 网格、标定点、调试辅助 | 作为生产业务数据来源 |
| Resource Manager | 资源引用、缓存、取消、释放和诊断 | 绕开图层直接改变业务状态 |
| Coordinate Transformer | 资产到场景的显式转换与标定 | 猜测现场最终坐标 |

## 3. 交互与更新

- Raycaster 对候选对象分层过滤，GLB 业务对象优先进入设备交互；Tiles 选择策略须在 MVP-10A 另行确认。
- Animation Loop 先推进 TilesLayer 更新，再推进 DeviceLayer 动画、Annotation 和渲染；实际调用顺序实现前须验证。
- Object Tree、Movable Part、Motion Target、Edit / Monitor、worldZ、状态和告警继续面向 GLB。
- TilesLayer 加载失败时将错误交给页面状态与回退策略，DeviceLayer 不应被连带销毁。

## 4. POC 与正式接入

POC 只可使用独立页面与独立 tiles engine 来验证概念，不改 TwinDemo 或生产 TwinScene。MVP-10A 在 POC 获批准后才能把本设计落入正式入口。

