# POC-3DT-01：3D Tiles + GLB 项目内最小验证

> 状态：Ready for user execution approval。未获单独授权时不得创建页面、安装依赖、执行 POC、构建或测试。

## 1. 目标与门禁

验证当前 IDTS 工程可以在独立 POC 页面中让 3D Tiles 静态底座与当前 GLB 动态设备同场展示，并保留 GLB 拾取、Object Tree、worldZ、Camera、Controls、Raycaster、错误回退和资源释放。POC 不阻塞 MVP-09/MVP-10，但其经批准结果直接解锁 MVP-10A。

## 2. 工程形态一致性

独立页面必须尽量复用当前工程的 Three.js 版本、Renderer 关键配置、Controls 类型和主要参数、GLB Loader 或等价正式路径、GLB 拾取和对象注册规则、worldZ 入口或封装、Vue 挂载/卸载生命周期、错误展示与 fallback 约定。

禁止另写一套长期脱离当前项目的 Demo 引擎。结果报告必须分别列出复用模块与临时代码。

## 3. 数据、范围与禁止项

阶段 A：公开、许可明确的小型 Tileset 加当前 GLB/fallback。阶段 B：获授权代表性中大型 Tileset 加当前提升机 GLB。POC 不落库，不修改 TwinDemo、正式 TwinScene、API、DTO、TypeScript 正式类型、模型文件或数据库；不做 CAD/IFC 转换和生产切片。

## 4. 必测内容

- 同场展示、三个标定点、轴向/比例、GLB 拾取和 Object Tree 不回归。
- Tiles 存在时 GLB worldZ、告警和高亮正常。
- Tiles 根 URL、子瓦片请求和解析失败时 GLB fallback 可用。
- 场景切换、请求取消、连续进入/退出 10 次、动画循环/监听清理和 WebGL 资源释放。
- 按测试计划记录纯 GLB 对照、冷/暖缓存、性能、内存和网络证据。

## 5. 结论与回退

结论仅为 Codex 建议的 Go、Conditional Go 或 No-Go；最终批准人为用户或项目负责人。回滚仅删除经批准的 POC 页面/模块和受控测试配置，不得修改生产入口。

## 6. Codex 提示词

请执行 POC-3DT-01。先读取当前工程入口、加载、拾取、worldZ、生命周期和 fallback 代码，以及 POC 测试计划和性能预算。复用现有工程形态创建独立 POC 页面，先输出影响范围并等待授权；不得修改正式入口、后端、数据库或契约。
