# DOC-3DT-00：IDTS 3D Tiles 文档缺口审计

> 文档性质：仅文档审计；不改变既有设计、契约、代码或任务顺序。  
> 审计基线：仓库 `main`，2026-07-14；本报告只依据本仓库已读取的 Markdown 与 UI 架构参考。  
> 审计结论用语：**已定义**表示已有可执行规则；**部分定义**表示有方向但不足以实施；**未定义**表示本次读取范围未找到可执行规则。

## 1. 审计目标

确认现有文档能否在不破坏 GLB 设备业务闭环的前提下，支持“3D Tiles 承担大型静态厂区底座、GLB 承担动态设备和业务交互、二者共存于同一 Three.js 场景”的后续决策。

本报告只识别事实、冲突、缺口、推荐补全文档和待确认决策；不替用户选择数据模型、API 形状或实现方案。

## 2. 审计范围

已读取：

- 项目规则：`AGENTS.md`、`idts3D_ui/AGENTS.md`、`development-rules.md`。
- 任务总纲与索引：`idts-mvp-task-breakdown.md`、`mvp-tasks/README.md`、`POC-3DT-01-threejs-3dtiles-renderer.md`、MVP-09 至 MVP-14、MVP-16。
- 契约与映射：`api-contracts/scenes.md`、`domain-entity-dto-map.md`、`frontend-integration-plan.md`、`backend-implementation-plan.md`、`e2e-acceptance-plan.md`。
- 规划与参考：`factory-scale-roadmap.md`、`idts-digital-twin-project-technical-plan.md`、`IDTS数字孪生系统完整实施计划.md`、`idts-demo-codex-performance-plan.md` 及三个 UI 设计参考。

未找到的引用文件：无。审计未读取运行时代码，也未运行 POC、构建或测试。

## 3. 当前项目方向

当前总纲已经明确以下稳定方向：

1. 提升机、堆垛机、AGV、输送线关键段等动态业务设备使用独立 GLB；Object Tree、Movable Part、Motion Target、worldZ 动画、状态映射、告警高亮以及 edit/monitor 语义继续服务于 GLB。
2. 厂区、建筑、楼层、地面、固定货架和固定结构适合由 3D Tiles 或静态低精资产承载；3D Tiles 不承担设备级控制或动态业务动画。
3. Three.js 仍是唯一主三维引擎；技术方案建议在该场景内集成 `3DTilesRendererJS` 或同类 renderer。
4. 现有 MVP 主线仍是 GLB 资产、scene/device/binding、Scene Manifest、前端 API Client、edit/monitor 闭环；3D Tiles 尚未进入该闭环。
5. 新方向要求：正式混合场景接入前，必须先完成文档补全、审校、用户确认和 POC 门禁。该门禁尚未写入现有任务索引或任务卡，因而目前只是本次审计的输入约束。

## 4. 当前文档清单

| 文档 | 当前状态 | 与本审计的直接关系 |
|---|---|---|
| `mvp-tasks/README.md` | 已定义 | MVP-00~16 串行；POC 独立且不阻塞主线。 |
| `idts-mvp-task-breakdown.md` | 已定义 | GLB MVP 闭环、MVP-10 禁止生产化 3D Tiles。 |
| `mvp-tasks/POC-3DT-01-threejs-3dtiles-renderer.md` | 部分定义 | 覆盖最小 tileset、GLB 共显、坐标轴/比例、picking、dispose。 |
| `factory-scale-roadmap.md` | 部分定义 | 规定在 GLB/LOD/资源释放等稳定后再评估 3D Tiles。 |
| `idts-digital-twin-project-technical-plan.md` | 部分定义 | 已有职责边界、早期验证和后期生产化方向，但不是可执行混合契约。 |
| `IDTS数字孪生系统完整实施计划.md` | 部分定义 | 将 3D Tiles 置于后续阶段，给出宏观验收/风险。 |
| `api-contracts/scenes.md` | 部分定义 | 有设备 GLB 的 `devices` 和空 `tilesets` 占位，未定义 tileset 条目。 |
| `domain-entity-dto-map.md` | 部分定义 | 设备/资产/绑定模型完备；无静态场景资源或底座层实体语义。 |
| `frontend-integration-plan.md` | 部分定义 | 规定 GLB/API/fallback 链路；未定义混合层加载及所有权。 |
| `idts-demo-codex-performance-plan.md` | 部分定义 | 有 GLB 性能指标采集、资源释放和长期 Tiles 预研，但无 Tiles POC 基线。 |
| UI 架构与验收参考 | 参考性已定义 | 给出一个 renderer/loop、tile 生命周期、分层 picking、释放和失败状态的通用原则。 |

## 5. 当前文档职责矩阵

| 文档 | 当前职责 | 当前状态 | 主要问题 | 是否需要修改 |
|---|---|---|---|---|
| `mvp-tasks/README.md` | MVP 与 POC 的执行顺序 | 部分定义 | POC 被定义为不阻塞主线，未表达混合场景的 POC 门禁。 | 是 |
| `idts-mvp-task-breakdown.md` | MVP GLB 闭环和门禁 | 部分定义 | `tilesets` 仅为未来扩展，不含混合资源模型。 | 是 |
| `POC-3DT-01...md` | 最小技术验证 | 部分定义 | 缺少可量化性能、失败、重复进出、结果记录及 Go/No-Go 标准。 | 是 |
| `api-contracts/scenes.md` | Scene Manifest 契约 | 部分定义 | `tilesets: []` 没有类型、版本、URL、transform、可见性或错误语义。 | 是 |
| `domain-entity-dto-map.md` | GLB 资产与设备数据映射 | 部分定义 | `device_instance` 不宜天然代表静态厂区；没有 base layer/resource binding 语义。 | 是 |
| `frontend-integration-plan.md` | GLB 前端接入/fallback | 部分定义 | 未规定 TilesLayer、资源管理、选择层与单一动画循环。 | 是 |
| `e2e-acceptance-plan.md` | GLB E2E 闭环 | 部分定义 | 不覆盖 tiles 加载、共显、失败降级、释放或性能结果。 | 是 |
| `factory-scale-roadmap.md` | 后期扩展研究 | 部分定义 | “后续评估”与现在要建立 POC 门禁的关系未冻结。 | 是 |
| `idts-demo-codex-performance-plan.md` | GLB 性能优化路线 | 部分定义 | 指标和观测存在，但没有 POC 硬件/浏览器/资源规模/阈值。 | 是 |
| `idts-digital-twin-project-technical-plan.md` | 正式项目方向 | 部分定义 | 已列模块和方向，缺少可落地的场景资源契约及坐标规范。 | 是 |

## 6. 文档之间的冲突

| 冲突编号 | 文档 A | 文档 B | 冲突内容 | 建议处理方式 |
|---|---|---|---|---|
| C-3DT-01 | `mvp-tasks/README.md` | 本次新方向 | 前者规定 POC 独立且不得阻塞 MVP 主线；新方向要求 POC 成为正式混合场景接入的前置门禁。 | 保留“POC 不阻塞既有 GLB MVP”并新增“POC 通过才可开始混合接入”的双层门禁。 |
| C-3DT-02 | `factory-scale-roadmap.md` | `idts-digital-twin-project-technical-plan.md` | 前者建议 GLB 性能阶段 0~11 稳定后再评估；后者建议 MVP 期间并行做最小验证。 | 用户确认 POC 的触发条件：仅可与 GLB 主线并行，且不得提前变更主场景。 |
| C-3DT-03 | `api-contracts/scenes.md` | 技术方案第 9、10 节 | 契约仅有空 `tilesets`，技术方案已描述静态底座层、TilesetLayer 和统一坐标对齐。 | 在架构决策完成后补充资源清单契约；在此之前保持 `tilesets` 为空，禁止前端猜测字段。 |
| C-3DT-04 | `idts-mvp-task-breakdown.md` / MVP-10 | 技术方案第 22、23 节 | 前者禁止 MVP-10 做生产化 3D Tiles，后者要求早期验证和后期生产化。 | 将 POC、正式混合接入、生产化明确拆为三个独立任务阶段。 |
| C-3DT-05 | `e2e-acceptance-plan.md` | UI Tiles 参考 | E2E 只验 GLB 闭环；参考要求 Tiles 流式状态、动态内容生命周期和错误分层。 | POC 另建测试计划与结果模板；正式接入后再扩展 E2E，不能将参考原则当作已验收事实。 |

## 7. 缺失文档清单

| 缺失文档 | 目标 | POC 前必需 | 正式接入前必需 | 优先级 |
|---|---|---:|---:|---|
| `architecture/ADR-001-3dtiles-glb-hybrid-architecture.md` | 冻结分层、职责和 POC 门禁决策 | 是 | 是 | P0 |
| `architecture/coordinate-system-and-transform-spec.md` | 冻结场景原点、单位、轴、矩阵与验收方法 | 是 | 是 | P0 |
| `poc/POC-3DT-01-test-plan.md` | 定义 POC 数据、步骤、失败场景和指标 | 是 | 是 | P0 |
| `poc/POC-3DT-01-result-report-template.md` | 固定证据、环境、结果、结论和残余风险 | 是 | 是 | P0 |
| `architecture/threejs-hybrid-scene-architecture.md` | 定义 renderer/camera/controls/loop/图层/选择/销毁所有权 | 否 | 是 | P1 |
| `design/scene-resource-manifest-design.md` | 定义 base layer、资源、设备与 model manifest 的边界 | 否 | 是 | P1 |
| `architecture/hybrid-scene-resource-lifecycle.md` | 定义加载、取消、释放、缓存、切换和失败回退 | 否 | 是 | P1 |
| `performance/3d-performance-budget.md` | 定义环境、数据规模、首屏、交互、FPS、内存和网络预算 | 是 | 是 | P1 |
| `operations/3dtiles-fallback-and-rollback-plan.md` | 定义 Tiles 失败时保留 GLB 的降级与发布回滚 | 否 | 是 | P2 |
| `mvp-tasks/MVP-10A-3dtiles-glb-hybrid-scene.md` | 在决策已冻结后限定正式混合接入范围 | 否 | 是 | P1 |

## 8. 需要修改的现有文档

| 文件 | 当前问题 | 建议修改 | POC 前必需 | 正式接入前必需 |
|---|---|---|---:|---:|
| `mvp-tasks/README.md` | POC 仅为非阻塞支线 | 增加不阻塞 GLB 主线、但阻止混合接入的门禁关系。 | 是 | 是 |
| `idts-mvp-task-breakdown.md` | 未给出混合接入任务位与依赖 | 在 ADR 后调整后续索引；保留 MVP-10 的现有边界。 | 否 | 是 |
| `POC-3DT-01...md` | 验收不完整 | 引用 POC 测试计划、记录模板与 Go/No-Go 规则。 | 是 | 是 |
| `api-contracts/scenes.md` | 只有空 `tilesets` | 在资源设计冻结后定义兼容演进方式、字段和 monitor/edit 读取规则。 | 否 | 是 |
| `domain-entity-dto-map.md` | 无静态资源语义 | 依据用户确认决定复用或新增资源模型；同步 Entity/DTO/TS/API 影响。 | 否 | 是 |
| `frontend-integration-plan.md` | 没有混合引擎边界 | 链接混合场景架构、资源生命周期、失败和纯 GLB 回退。 | 否 | 是 |
| `e2e-acceptance-plan.md` | 无混合验收 | 仅在正式接入任务后增加端到端验收项。 | 否 | 是 |
| `factory-scale-roadmap.md` | POC 时机表述偏后 | 明确它是生产化评估，不否定受控的最小 POC。 | 是 | 是 |
| `idts-demo-codex-performance-plan.md` | 无 Tiles POC 性能基线 | 交叉引用新的性能预算，避免把 GLB 指标直接套用。 | 否 | 是 |

## 9. 数据库设计影响

现有 `model_asset`、`asset_version`、`model_asset_variant`、`asset_manifest`、`scene_node`、`device_instance`、`device_model_binding` 足以表达当前 GLB 设备资产、版本和设备 transform；`model_asset.asset_type` 也已有 `device_glb`、`static_glb` 枚举方向。

但它们尚不能明确表达“静态厂区底座资源”的资源类型、tileset 根 URL、空间范围、版本/发布状态、场景层级、资源与场景绑定、全局 transform 或回滚粒度。尤其不应在未确认语义前直接复用 `device_instance` 表示静态厂区，因为其名称、约束和 active binding 都面向设备。

结论：**涉及数据库设计，但本次不修改数据库。**MVP 可暂不改表并维持 `tilesets: []`；正式混合接入前必须由用户在 ADR 中确认以下之一：复用并扩展资产模型、增加 Scene Resource/Base Layer/Resource Binding，或采用其他已说明的模型。随后才可同步 Entity、DbContext、migration、DTO、API 和 TypeScript。

## 10. Scene Manifest 与 API 契约影响

现有 `GET /api/scenes/{sceneId}/manifest` 面向 GLB 设备：`devices[]` 含设备的 `manifestUrl` 与 position/rotation/scale；`tilesets` 固定返回空数组。现有 Model Manifest 面向模型版本与 GLB levels；二者职责尚未覆盖静态底座资源。

正式混合接入前需要决定：

- 是否使用 `baseLayers`、`resources`、`devices`、`nodes` 的分层结构，或保留兼容的 `tilesets` 扩展；
- 3D Tiles URL、版本和发布状态的归属；GLB URL 仍应由 model manifest 提供；
- 每一类资源的 transform、可见性、空间范围、失败策略和 mode 读取语义；
- 旧响应的兼容策略以及后端 DTO、前端 TypeScript 和 API Client 的同步边界。

结论：**涉及 Scene Manifest 与 API 契约，但本次不改变契约。**在上述决定冻结前，不能新增或猜测任何字段。

## 11. 前端 Three.js 架构影响

现有规划与 UI 参考共同指向：同一场景应有一个清晰的 renderer、camera、controls 和 animation loop 所有者；Tiles renderer 由 scene/tile service 层持有；每帧在统一循环中更新 controls、camera、tiles renderer 和场景系统后再 render。GLB picking 应使用设备层 hitbox 或 GLB mesh，而不是依赖 tileset 内部节点。

正式接入前应定义 `TilesLayer`、`DeviceLayer`、`AnnotationLayer`、`HelperLayer`、`InteractionManager`、`ResourceManager` 和 `CoordinateTransformer` 的职责、输入输出和所有权，而不是把它们直接作为实现要求。还必须规定：Tiles 动态加载/卸载时对象注册表如何更新、Raycaster 候选层如何限制、场景切换如何取消请求、tiles 加载失败如何保留 GLB 和纯 GLB 回退、卸载时谁释放资源。

结论：**涉及前端架构，但本次不修改 `idts3D_ui/src/**`。**

## 12. 坐标系统文档缺口

当前文档只零散提到 GLB manifest 的 `upAxis: Z`、示例 transform、设备 position/rotation/scale，以及必须统一 3D Tiles、GLB、WCS 坐标；未找到项目级、可验收的坐标规范。

以下均为未定义：场景原点、长度单位、Three.js/GLB/3D Tiles 轴定义、Y-up/Z-up 转换、平移/旋转/缩放的承载层、旋转单位和顺序、矩阵乘法顺序、高度基准、设备定位公式、误差容限、标定数据和验证方法。

这是 POC 前 P0 缺口：没有它无法判断“看起来对齐”是否可复现，也不能安全设计资源 transform。

## 13. 资源生命周期文档缺口

性能计划包含 GLB `ResourceDisposer` 方向，UI 参考也要求在 unmount 时取消动画帧、移除监听、清理 renderer/tile 自定义资源和对象注册；但仓库没有混合场景资源生命周期规范。

未定义内容包括：资源创建与所有者、加载取消、Tiles 流式内容的注册/反注册、缓存范围、场景/图层切换顺序、geometry/material/texture 与 tiles renderer 的释放责任、失败重试、重复进入退出验证，以及避免双重 dispose 的规则。

正式接入前应补充专门的生命周期文档；POC 至少应记录页面反复进入/退出与 dispose 后的可观察结果。

## 14. 性能与验收文档缺口

现有性能计划定义了 FPS、draw calls、triangles、geometry、texture 等观测方向；正式技术方案给出单设备、10~30 台设备以及首屏可交互时间的建议预算，完整实施计划另有大场景 `>=25fps` 的长期指标。它们都不能直接构成最小 Tiles POC 的可重复验收基线。

POC 缺少：测试机器/GPU/浏览器/分辨率、tileset 与 GLB 样本规模、冷/热缓存条件、首屏/可操作时间、相机漫游路径、稳定/最低 FPS、帧时间、内存或可观察资源指标、网络错误模拟、告警与 callout 开启后的表现、结果记录格式和阈值裁决。

因此 POC 结果只能作为可行性证据，不应被表述为生产性能达标；性能预算文档应在 POC 前确定最小阈值，并在正式接入前扩展为发布门禁。

## 15. POC 前置门禁建议

建议仅在以下条件全部满足后执行 POC：

1. 用户审阅并确认本审计的冲突处理方向。
2. ADR 明确 POC 不阻塞既有 GLB MVP，但 POC 结论控制正式混合接入。
3. 坐标规范冻结到足以验证原点、单位、轴、transform 与容差。
4. POC 测试计划和结果模板已审校，包含最小 tileset + GLB 共显、相机/controls、GLB picking、worldZ 动画、网络失败、Tiles 失败、重复进入退出、dispose 和最小性能证据。
5. 测试数据来源、可用许可、URL 托管方式及不入库策略已确认。
6. 明确 Go、Conditional Go、No-Go 的判定者和后续动作。

## 16. 正式接入门禁建议

建议仅在以下条件全部满足后开始正式混合场景任务：

1. POC 有可复核结果并达到用户确认的 Go 或 Conditional Go 条件。
2. ADR、坐标规范、混合场景架构、Scene Resource Manifest 设计、资源生命周期、性能预算和 fallback/rollback 计划已批准。
3. 数据库与 API 决策已明确，并已列出 Entity/DTO/API/TypeScript/API Client 的同步任务；不得在实现时临时发明字段。
4. MVP-10A（或经用户确认的等价任务）有明确范围、兼容性、迁移、测试和回滚边界。
5. 纯 GLB 模式及当前 monitor/edit 语义的回归验收项已写入正式接入任务。

## 17. 推荐文档补全顺序

```text
DOC-3DT-00 文档缺口审计
→ 用户审校审计结果
→ ADR：3D Tiles / GLB 混合架构决策
→ 坐标系统与 transform 规范
→ 混合 Three.js 场景架构
→ POC 测试计划
→ POC 结果报告模板
→ POC 执行与结果审校
→ Scene Resource Manifest 设计、资源生命周期、性能预算、fallback/rollback
→ 文档一致性审校与用户确认
→ MVP-10A 正式混合场景接入任务卡
```

该顺序是建议，不自动执行；POC 前只需完成其前置文档，正式接入前需完成全部适用文档。

## 18. 待用户确认的决策项

| 决策编号 | 待确认问题 | 可选方案 | 影响范围 | 推荐方案 |
|---|---|---|---|---|
| D-3DT-01 | POC 与 MVP 主线的关系 | A. 继续完全非阻塞；B. 不阻塞 GLB 主线但作为混合接入门禁；C. 阻塞所有 MVP。 | 任务索引、路线图、门禁 | B |
| D-3DT-02 | 静态厂区资源数据模型 | A. 扩展资产模型；B. 新增 Scene Resource/Base Layer/Binding；C. 仅配置文件。 | DB、契约、发布回滚 | 先通过 ADR 比较 A/B；不推荐在正式场景长期采用 C。 |
| D-3DT-03 | Scene Manifest 演进 | A. 扩展 `tilesets`；B. 新增 `baseLayers/resources`；C. 新版本路由。 | API 兼容、DTO、TS、前端 | 先定资源设计和兼容策略后再选。 |
| D-3DT-04 | 坐标权威来源 | A. CAD/BIM；B. 现场测量；C. 项目局部坐标加显式转换。 | 对齐、WCS、资产流水线 | C 作为运行时规范，并记录 A/B 的输入来源。 |
| D-3DT-05 | POC 判定标准 | A. 仅功能可行；B. 功能+最小性能；C. 接近生产预算。 | POC 计划、排期 | B |
| D-3DT-06 | Tiles 失败策略 | A. 仅提示失败；B. 保留 GLB、隐藏底座并提示降级；C. 整页失败。 | UX、验收、运维 | B |

## 19. 风险清单

| 风险 | 当前证据 | 后果 | 控制建议 |
|---|---|---|---|
| POC 门禁含义不清 | POC 当前不阻塞 MVP，新方向要求前置门禁 | 任务顺序争议或提前接入 | 冻结 D-3DT-01。 |
| 坐标未规范 | 只有零散 transform 与“统一坐标”要求 | Tiles/GLB/WCS 对齐不可复现 | POC 前完成坐标规范。 |
| 静态资源语义混淆 | 数据模型以设备 GLB 为中心 | 误用 device/binding，后续迁移成本高 | ADR 先确定资源模型。 |
| 契约被前端猜测 | `tilesets` 无条目定义 | API、DTO、TS 漂移 | 正式接入前完成 resource manifest 设计。 |
| Tile 生命周期遗漏 | UI 参考提示内容可动态卸载 | 残留对象引用、内存增长、picking 错误 | 制定生命周期并写入 POC/E2E。 |
| POC 结果不可比较 | 无环境和阈值基线 | “通过”无法支撑投入决策 | 建立性能预算和结果模板。 |
| GLB 能力回归 | 现有 GLB 主线有 object tree、动画、fallback | 混合接入破坏业务演示 | 保留纯 GLB 回退及完整回归清单。 |

## 20. 审计结论

仓库已经具备清晰的高层职责方向：3D Tiles 用于静态厂区底座，GLB 用于动态设备及其业务交互，且两者应在一个 Three.js 场景中共存。现有文档也已识别早期验证、坐标对齐、picking、资源释放和后期生产化的风险。

但该方向尚未形成可安全执行的混合场景文档基线。关键缺口是 POC 门禁与 MVP 的关系、全局坐标规范、静态资源的数据/Scene Manifest 模型、前端图层与生命周期所有权、可重复的 POC 性能与结果判定。当前不得直接把空 `tilesets`、现有设备绑定或 UI 参考原则当作正式设计实施。

推荐先由用户确认第 18 节的不可逆架构决策，再按第 17 节补全文档并执行受控 POC；POC 通过前不创建正式 TilesLayer、正式 3D Tiles Loader、Scene Manifest 契约或数据库迁移。
