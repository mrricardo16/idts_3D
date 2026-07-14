# DOC-3DT-01：审计报告复核与关键决策冻结

> 文档性质：DOC-3DT-00 的证据复核与待用户批准的决策门禁。  
> 前置基线：DOC-3DT-00 已存在；本报告不修改其结论、既有设计、任务索引、契约或代码。  
> 状态约定：**已确认**仅表示可在仓库文档中找到证据；不表示已实现或已获用户批准。**需要用户决策**不构成既定架构决定。

## 1. 复核目标

复核 DOC-3DT-00 的关键结论是否被现有仓库文档支持，区分确认事实、部分确认、表述过度和待用户决策事项，并冻结下一阶段“可以开始编写哪类文档”的前置顺序。

本报告不创建 ADR、坐标规范、混合场景架构、POC 测试计划、结果模板或 MVP-10A。

## 2. 复核范围

复核对象为 `idts3D_docs/reviews/DOC-3DT-00-document-gap-audit.md`，并重新读取以下现有来源：项目规则、MVP/POC 索引和任务卡、Scene Manifest 契约、实体 DTO 映射、前端和 E2E 计划、厂区路线图、正式技术方案、完整实施计划、性能计划和三份 UI 架构参考。

本次未读取运行时代码，未运行 POC、构建或测试；因此所有“已确认”均是文档事实，不是运行时验证结论。

## 3. 复核方法

1. 对 DOC-3DT-00 的每一项重要结论回溯到至少一个仓库文档。
2. 将“现有文档已规定”与“本次新方向要求”分开；后者不能被写成仓库内部既有事实。
3. 将 UI 参考视为通用设计依据，不视为项目已经实现或已经批准的架构。
4. 对没有可执行字段、状态、职责、验收或边界的内容，判定为“有占位但不可实施”或“部分确认”，而非简单称为“完全没有”。

## 4. DOC-3DT-00 结论逐项核验

| 编号 | DOC-3DT-00 结论 | 依据文件 | 证据位置 | 核验状态 | 复核说明 |
|---|---|---|---|---|---|
| R-01 | GLB 承担动态设备及业务交互，3D Tiles 承担静态大场景。 | `idts-digital-twin-project-technical-plan.md` | §2、§9 | 已确认 | 技术方案明确设备 GLB、静态厂区 Tiles，且 Tiles 不承担设备级控制。该事实是规划方向，不是已实现状态。 |
| R-02 | POC“非阻塞”与正式混合接入前置门禁存在冲突。 | `mvp-tasks/README.md`、`idts-mvp-task-breakdown.md`、DOC-3DT-00 输入方向 | README §7；总纲 §9 | 需要用户决策 | 现有仓库只明确 POC 不阻塞 MVP-00~16；“POC 阻塞正式混合接入”来自本轮方向，尚未写入现有文档。因此是门禁缺口，不是两个既有仓库文档的直接矛盾。 |
| R-03 | Scene Manifest 只有弱定义/空 `tilesets`，不能作为静态底座契约。 | `api-contracts/scenes.md`、`MVP-10-scene-manifest.md` | 契约响应示例和字段表；MVP-10 §10、§11 | 已确认 | `tilesets` 已出现，但仅为“空数组、保留扩展”；无条目字段、DTO 字段、TS 类型、数据来源或发布规则。 |
| R-04 | 数据模型以设备 GLB 为中心，静态底座资源语义未冻结。 | `domain-entity-dto-map.md`、MVP-09、MVP-10 | 映射总览与 §3.1/§3.7~§3.9；MVP-09 §5~§9 | 部分确认 | `model_asset.asset_type` 含 `static_glb`，表明存在静态 GLB 方向；但 source 类型固定为 `glb`，且没有 tileset/base layer/resource binding 语义。不能表述为“完全没有静态资源能力”。 |
| R-05 | 项目缺少可实施的坐标规范。 | 性能计划、POC-3DT-01、正式技术方案、完整实施计划 | 性能计划 manifest 示例；POC §10；技术方案 §9、§22；实施计划 §18 | 已确认 | 有 `upAxis: Z`、transform 示例、坐标风险和“统一坐标”要求；未定义原点、单位、矩阵顺序、容差、标定和验证方法。 |
| R-06 | 项目缺少混合资源生命周期规范。 | 性能计划、三个 UI 参考、POC-3DT-01 | 性能计划阶段 6；UI 架构 §15~§17；交互参考 §23；验收参考 §17~§19 | 部分确认 | 已有 GLB disposer 方向和 Tiles 生命周期通用原则；未形成项目级的资源所有权、取消、缓存、切换和错误恢复规范。 |
| R-07 | 缺少可重复的 Tiles POC 性能与 Go/Conditional Go/No-Go 门禁。 | 性能计划、正式技术方案、完整实施计划、POC-3DT-01 | 性能观测阶段；技术方案 §25；实施计划 §16；POC §10~§11 | 部分确认 | 已有指标方向和长期预算；POC 未定义测试机器、浏览器、样本规模、阈值、结果模板或三态判定。 |
| R-08 | 正式接入前需补齐 ADR、坐标、架构、资源清单、生命周期等文档。 | DOC-3DT-00 | §7、§15~§18 | 需要用户决策 | 这是合理的补全文档建议，不是仓库已批准的文档清单或文件命名决定。 |

## 5. 证据索引

| 证据编号 | 文件 | 章节或关键内容 | 支持的结论 |
|---|---|---|---|
| E-01 | `mvp-tasks/README.md` | §3：MVP-00~16 串行；§7：POC 独立且不阻塞 MVP。 | R-02、DOC-3DT-00 的顺序分析。 |
| E-02 | `idts-mvp-task-breakdown.md` | §1、§9、§11：GLB 闭环、POC 不进主链、MVP-10 不做生产化 Tiles。 | R-02、R-03。 |
| E-03 | `POC-3DT-01-threejs-3dtiles-renderer.md` | §1、§10~§13：最小 tileset、GLB 共显、轴/比例、Raycaster、dispose 和风险。 | R-05、R-06、R-07。 |
| E-04 | `api-contracts/scenes.md` | `devices[]`、`position/rotation/scale`、`tilesets: []` 与字段表。 | R-03。 |
| E-05 | `MVP-09-scene-device-binding.md`、`MVP-10-scene-manifest.md` | device/binding 的 Published 约束；MVP-10 返回空 `tilesets`。 | R-03、R-04。 |
| E-06 | `domain-entity-dto-map.md` | 总览表；`model_asset.asset_type`；`scene_node`、`device_instance`、`device_model_binding` 字段和约束。 | R-04。 |
| E-07 | `frontend-integration-plan.md`、MVP-11~14 | GLB/API/fallback、Scene Manifest 消费、worldZ 与 mode 语义。 | R-01、R-03、R-06。 |
| E-08 | `idts-digital-twin-project-technical-plan.md` | §2、§9、§10、§22、§23、§25。 | R-01、R-05、R-06、R-07。 |
| E-09 | `factory-scale-roadmap.md`、性能计划 | 3D Tiles 后期评估顺序；性能观测、资源释放、长期预研。 | R-02、R-06、R-07。 |
| E-10 | `IDTS数字孪生系统完整实施计划.md` | §13、§16、§18、§20：阶段 P6、坐标风险、长期性能指标。 | R-01、R-05、R-07。 |
| E-11 | 三份 UI 参考 | renderer/loop 所有权、动态 tile 生命周期、按层 picking、dispose、错误状态。 | R-06、R-07。 |

## 6. 已确认事实

1. 当前 MVP 文档把 POC-3DT-01 定义为独立技术验证，不进入并且不得阻塞 MVP-00~16。
2. 当前 Scene Manifest 已有 `devices[]`、设备 transform 和 `tilesets: []`，但 `tilesets` 仅是扩展占位。
3. 现有 Scene Manifest 读取数据源和 DTO 都是 scene/device/model-asset 体系，未声明 tileset 条目来源。
4. 现有任务卡保留 GLB 的 Object Tree、Movable Part、Motion Target、edit/monitor、worldZ 和 fallback 边界。
5. 正式技术方案把 GLB 动态设备和 Tiles 静态底座作为推荐职责划分，并把早期验证和后期生产化分阶段。
6. 现有文档只提供零散坐标与资源释放信息，未提供项目级可验收规范。

## 7. 部分确认事项

1. `model_asset` 可保存静态 GLB 方向，但尚无证据表明它已能正确表达 Tileset、空间层级或场景底座发布。
2. 性能文档已有 FPS、draw call、模型统计、长期场景帧率等信息；它们不足以直接成为 POC 的可重复门禁。
3. UI 参考已描述 Tiles renderer 生命周期、动态内容和一个 render loop；它们是参考原则，尚未转化为本项目的正式架构规范。
4. POC 任务卡覆盖坐标轴、比例、共显、Raycaster 与 dispose，但不覆盖请求取消、场景切换、错误降级、重复进出及三态裁决。

## 8. 缺少依据或表述过度事项

1. “当前文档之间存在 POC 阻塞冲突”表述过度：现有仓库文档只写了 POC 非阻塞；阻塞正式混合接入是本次待冻结的新规则。
2. “Scene Manifest 完全没有 Tiles”不成立：已有 `tilesets` 占位。准确表述应为“有占位但不可实施”。
3. “当前完全没有静态资源语义”表述过度：`static_glb` 已存在；准确缺口是没有 Tiles/base-layer/resource-binding 的可执行语义。
4. “生命周期完全未定义”表述过度：GLB disposer 方向和 UI 参考存在；准确缺口是没有项目级混合生命周期契约。
5. 任意具体资源表、Scene Manifest 字段、类名或 API 版本都缺少已批准依据，不能由本报告冻结为实现事实。

## 9. 审计报告遗漏项

1. **POC 依赖与许可证**：公开或真实 Tileset 的来源、再分发许可、URL 托管方式和是否入库尚未列为 POC 门禁。
2. **内容安全与网络边界**：tileset 及其子资源的跨域、认证、缓存和失败可观测性没有归属。
3. **空间参考类型**：项目是否使用局部工程坐标、地理坐标或两者转换，尚未作为决策项明确。
4. **范围分层**：应区分“最小 POC”“正式混合接入”“Tiles 生产化”；不能用一个 POC 结论替代后两者的契约、发布和运维设计。
5. **实现状态警示**：治理基线明确前端 API Client、业务联调和 scene/device/motion API 仍未实现或仅部分实现，后续文档不得将其当作完成事实。

## 10. POC 与 MVP 顺序复核

当前事实是：POC 不阻塞 MVP-00~16，MVP-09/10 是 GLB 设备和 Scene Manifest 任务，MVP-11~14 是 GLB/API/edit/monitor 闭环。现有文档没有指定任何“混合场景正式接入”任务。

因此两种规则可以同时成立，但必须以不同对象表述：

- POC **不应阻塞**纯 GLB 后端任务、MVP-09、MVP-10、MVP-11~MVP-14，避免无关 CRUD 与 GLB 闭环被 Tiles 风险冻结。
- POC **应阻塞**未来新增的正式混合接入任务（暂称 MVP-10A），因为该任务会改变场景资源、前端图层、坐标和契约边界。
- POC 是否阻塞 MVP-11~14：当前建议为否；这些任务的既定范围不要求 Tiles。若将来 POC 发现必须重构当前 API/引擎才能共存，应登记独立架构决策，而非倒推修改既有 MVP 顺序。

该建议尚待用户批准，不能修改现有索引。

## 11. Scene Manifest 缺口复核

当前 `GET /api/scenes/{sceneId}/manifest` 有：`sceneId`、`sceneCode`、`sceneName`、`devices[]`，每个设备有 `manifestUrl` 与 position/rotation/scale；另有 `tilesets: []`。MVP-10 明确该数组为空。

当前缺少：

- tileset 条目字段、后端 DTO 字段、TypeScript interface 和 API Client 消费定义；
- URL、版本、Published/active 规则、数据表或其他数据来源；
- 静态底座的 transform、可见性、空间范围、失败与降级语义；
- 与 model manifest 的职责边界和响应兼容策略。

复核结论：不是“完全没有”，而是**存在预留占位但尚不可实施**。POC 无须修改此契约；正式混合接入前必须冻结其演进方案。

## 12. 数据模型影响复核

| 对象 | 当前可表达内容 | 当前限制 | 复核结论 |
|---|---|---|---|
| `model_asset` | GLB 资产及 `device_glb`/`static_glb` 分类方向。 | source 类型为 `glb`；无 tileset 格式、空间范围、场景层或资源绑定定义。 | 可作为正式方案候选复用基础，但不能据此认定已支持 Tiles。 |
| `model_asset_variant` | `source/high/medium/low/proxy` GLB 变体、文件 URL 和版本。 | 没有 `tileset.json` 及其层级资源语义。 | 是否扩展为 Tiles 派生资产需要用户决策。 |
| `scene_node` | 场景/区域/楼层/线的树结构。 | 无资源引用、transform 或 base-layer 规则。 | 可表达空间组织，不能单独成为静态底座资源定义。 |
| `device_instance` | 设备编码、位置、旋转、缩放、enabled。 | 名称、约束和后续 binding 均面向设备。 | 不宜默认复用为厂区底座。 |
| `device_model_binding` | 设备到 Published asset version 的 active binding。 | 绑定语义和唯一约束面向 device instance。 | 不应默认绑定厂区底座。 |

POC 阶段不需要数据库支持；可使用受控测试资源和 POC 内配置。正式接入可以先冻结 Scene Manifest 设计而暂缓 migration，但不得把“暂缓改表”误作“无需确定领域语义”。

## 13. 前端架构影响复核

现有文档与 UI 参考支持以下最小事实：Vue 不应同时持有传输、业务、renderer、模型加载、raycasting 和面板；renderer、camera、controls、animation loop 与 3D Tiles renderer 应有明确所有者；tile 内容会动态加载/卸载；picking 应限制候选对象；页面卸载必须 dispose。

当前未冻结 `TwinScene` 是否增加图层、是否引入 `HybridScene`、Tiles renderer 更新位置、GLB 与 Tiles 的选择优先级、对象注册、请求取消与缓存责任。因此 POC 最安全的架构边界是独立页面；正式方案应在用户批准后才确定。

## 14. 坐标规范缺口复核

已找到的文档内容只有：GLB manifest 示例 `upAxis: Z`、rotation/position/scale 示例；POC 验证 Z-up/Y-up 与比例；技术方案要求统一场景坐标并让 GLB 与 tileset 对齐；实施计划把坐标不一致列为风险。

未找到的正式定义：场景原点、单位、轴正方向、Y-up/Z-up 转换、海拔/高度基准、旋转单位和顺序、缩放归属、矩阵乘法顺序、GLB/Tileset 到场景的转换公式、设备标定、允许误差、测量与验收方法。

复核结论：坐标规范是 POC 前最小必需文档；不得把代码或 manifest 示例的默认值视作正式规范。

## 15. 资源生命周期缺口复核

现有 GLB 方向：性能计划提出 `ResourceDisposer`；UI 参考要求取消 animation frame、移除监听、释放 geometry/material/texture、避免双重 dispose。现有 Tiles 方向：UI 参考要求 scene/tile service 持有 renderer、在统一循环 update、动态内容注册/反注册、清理定制资源和陈旧引用。

未定义的项目级内容：初始化、加载、更新、隐藏、卸载、销毁、场景切换、请求取消、缓存上限、失败重试、GLB/Tiles 资源的所有权和错误恢复。复核结论为**部分确认**：有原则和局部方向，但无可执行混合生命周期规范。

## 16. 性能与 POC 门禁缺口复核

已存在：FPS、draw calls、triangles、geometries/textures 等观测；单设备/小区域/首屏建议预算；长期 16 台堆垛机加 120 台 AGV 的 `>=25fps` 指标；POC 的共显、Raycaster、dispose 验收点。

POC 未定义：测试机器/GPU、浏览器版本、分辨率、Tileset/GLB 数据规模、冷暖缓存条件、首屏和可操作时间、最低/稳定 FPS、内存与网络请求采样、坐标误差阈值、重复进入退出次数、失败注入、Go/Conditional Go/No-Go 判定者和阈值。

复核结论：现有资料可作为 POC 性能预算的输入，不能直接作为 POC 通过门禁。

## 17. 关键决策方案矩阵

| 决策编号 | 方案 | 优点 | 缺点 | 影响范围 | 推荐级别 |
|---|---|---|---|---|---|
| D-01 | A. 阻塞全部后续 MVP | 风险最保守。 | 无关 GLB 任务被 Tiles 风险阻塞，违背当前 POC 规则。 | 全部 MVP 排期。 | 不推荐 |
| D-01 | B. 不阻塞 MVP-09/10，但阻塞 MVP-11 后 | 提前控制前端返工。 | 与 MVP-11~14 的既定 GLB 范围不匹配，扩大阻塞。 | 前端 MVP 排期。 | 不推荐 |
| D-01 | C. 仅阻塞未来 MVP-10A 正式混合接入 | 保持 GLB 主线，且在不可逆接入前获得 POC 证据。 | 需要新增明确任务门禁。 | 未来混合接入。 | 推荐 |
| D-01 | D. 完全独立，无任何门禁 | 排期最简单。 | 无法避免未验证的混合方案进入正式实现。 | 正式接入风险。 | 不推荐 |
| D-02 | A. 复用 `model_asset + scene_node` | 表数量少。 | 资源绑定、图层、transform 和生命周期语义容易混入既有对象。 | 领域模型、契约。 | 可研究 |
| D-02 | B. 复用 `model_asset`，新增 `scene_resource/scene_layer` | 保留资产版本能力，同时明确静态资源/图层关系。 | 正式接入需 schema、DTO、契约和迁移协同。 | DB、API、前端。 | 正式接入推荐 |
| D-02 | C. 新建 `tileset_asset` 体系 | Tiles 语义最直观。 | 重复资产/版本/发布能力，治理成本高。 | DB、Worker、运维。 | 暂不推荐 |
| D-02 | D. POC 阶段不落库，正式接入前再冻结 | POC 范围最小，不提前固化错误语义。 | 不能替代正式领域设计。 | 仅 POC。 | POC 推荐 |
| D-03 | A. `baseLayers + devices` | 静态底座与动态设备职责直观，最符合当前方向。 | 需要兼容现有 `tilesets` 占位。 | Scene Manifest、DTO、TS。 | 正式接入推荐 |
| D-03 | B. `resources` 统一列表，以 `resourceType` 区分 | 扩展性强。 | 读取方和交互规则更复杂，易混淆静态/动态职责。 | 契约、前端。 | 可研究 |
| D-03 | C. `nodes` 统一场景树 | 与空间层级一致。 | 资源、设备、交互和加载策略可能过度耦合。 | 契约、场景模型。 | 暂不推荐 |
| D-03 | D. 仅增加 `tilesets` | 改动最小。 | 容易继续形成弱契约，无法表达更广泛静态资源。 | 契约兼容。 | 仅短期兼容 |
| D-04 | A. `TwinScene` 内增加 TilesLayer 和 DeviceLayer | 保留现有入口，符合单 renderer/loop 原则。 | 需先冻结职责与生命周期，实施需谨慎回归。 | 前端 engine。 | 正式接入推荐 |
| D-04 | B. 新 `HybridScene` 取代 `TwinScene` | 概念独立。 | 高重构和回归风险，现阶段缺乏必要证据。 | 前端主流程。 | 不推荐 |
| D-04 | C. POC 独立页面，正式方案后定 | 不污染 GLB 主页面，验证成本低。 | 不能证明正式接入代码可零成本复用。 | POC 前端。 | POC 推荐 |
| D-04 | D. 页面分别管理 GLB/Tiles | 快速原型。 | 违反分层和生命周期所有权原则。 | 页面、engine。 | 不推荐 |
| D-05 | A. 公开小型 Tileset | 易获得、风险低。 | 代表性不足。 | POC 数据。 | 基础必需 |
| D-05 | B. 公开中型样例 + 当前 GLB | 能验证共显。 | 仍可能不代表厂区规模。 | POC 数据。 | 可选 |
| D-05 | C. 真实厂区 Tileset + 当前 GLB | 最有代表性。 | 许可、数据、性能和坐标风险高。 | 数据治理、POC。 | 条件使用 |
| D-05 | D. 小型验证后再做代表性大样本 | 先验证集成，再收集规模证据。 | 需要两次记录与门禁。 | POC 排期。 | 推荐 |
| D-06 | Go | 核心共显、坐标、GLB picking/worldZ、dispose 与最小性能全部通过已批准阈值。 | 需先批准阈值与样本。 | POC 结论。 | 推荐定义 |
| D-06 | Conditional Go | 核心正确性通过，存在已登记且有缓解/回退措施的非阻断限制。 | 必须限制正式接入范围并跟踪条件。 | 后续设计。 | 推荐定义 |
| D-06 | No-Go | 核心共显、坐标可校准性、GLB 交互或生命周期任一失败，或无法得到可重复证据。 | 延后混合接入。 | 正式接入门禁。 | 推荐定义 |

## 18. 推荐方案

以下均为推荐，尚未获批准：

1. D-01：采用 C。POC 不阻塞纯 GLB 后端任务、MVP-09、MVP-10 和 MVP-11~14；仅作为未来 MVP-10A 的进入门禁。
2. D-02：采用 D 作为 POC 数据策略；若进入正式接入，优先研究 B，避免把厂区底座伪装为设备实例。
3. D-03：正式契约优先采用 A，明确 `baseLayers` 与 `devices`；是否保留 `tilesets` 作为兼容字段须在 ADR 中决定。
4. D-04：POC 采用 C；正式接入优先采用 A，且必须保留一个明确的 renderer、camera、controls 和 animation loop 所有者。
5. D-05：采用 D；先小样本验证，再在获得许可和坐标资料后做代表性样本验证。
6. D-06：建立三态判定，但所有数值阈值、样本规模和判定责任人必须由用户在 POC 测试计划前批准。

## 19. 待用户批准项

| 决策编号 | 需要批准的内容 | 推荐方案 | 不批准的影响 |
|---|---|---|---|
| D-01 | POC 是否只阻塞未来正式混合接入。 | 仅阻塞 MVP-10A。 | 不能冻结 POC 与现有 MVP 的关系。 |
| D-02 | POC 是否不落库，以及正式接入的数据模型方向。 | POC 不落库；正式优先研究 scene resource/layer。 | 不得设计表、迁移或资源绑定。 |
| D-03 | Scene Manifest 的正式演进方向。 | `baseLayers + devices`。 | 不得新增字段或 API/TS 类型。 |
| D-04 | POC 与正式前端架构边界。 | POC 独立页；正式在 `TwinScene` 内分层。 | 不得创建正式 TilesLayer。 |
| D-05 | POC 数据分阶段与许可边界。 | 小样本后代表性样本。 | 不得选择或引入真实厂区数据。 |
| D-06 | POC 的三态门槛和阈值批准方式。 | Go/Conditional Go/No-Go。 | POC 结果不能作为正式接入依据。 |

## 20. 下一阶段文档编写顺序

| 顺序 | 文档任务 | 前置决策 | 目标 | 是否允许同时执行 |
|---:|---|---|---|---:|
| 1 | ADR：3D Tiles/GLB 混合架构决策 | D-01~D-04 批准 | 冻结职责、阶段、门禁和未决边界。 | 否 |
| 2 | 坐标系统与 transform 最小规范 | ADR、D-04/D-05 | 定义 POC 可复现的坐标、轴、单位和容差。 | 否 |
| 3 | POC 测试计划 | ADR、坐标规范、D-05/D-06 | 固定数据、环境、步骤、阈值和失败注入。 | 否 |
| 4 | POC 结果报告模板 | POC 测试计划 | 固定证据、三态结论和残余风险。 | 是，与步骤 3 收尾并行 |
| 5 | 受控执行 POC | 测试计划与模板均批准 | 获取可复核技术证据。 | 否 |
| 6 | POC 结果审校 | POC 记录 | 决定 Go/Conditional Go/No-Go。 | 否 |
| 7 | 混合场景架构、资源清单、生命周期、性能预算、fallback/rollback | Go 或受控 Conditional Go | 为正式接入冻结跨层契约。 | 否 |
| 8 | MVP-10A 任务卡与既有文档一致性审校 | 上述正式文档已批准 | 限定正式实现范围、迁移、回归和回滚。 | 否 |

## 21. 禁止提前执行事项

在第 19 节的决策未获批准前，禁止：

- 创建 ADR、坐标规范、混合场景架构、POC 计划、结果模板或 MVP-10A；
- 修改 POC-3DT-01、MVP 顺序、任务索引、Scene Manifest、数据库设计、API、DTO 或 TypeScript 类型；
- 创建正式 TilesLayer、Tiles Loader、POC 页面或真实厂区 Tileset；
- 运行 POC、构建、测试，安装依赖，commit 或 push；
- 以本报告的推荐方案替代用户批准。

## 22. 复核结论

DOC-3DT-00 的核心方向大体成立，但需要更精确的边界：POC 阻塞关系是待新增的治理规则，不是现有文档的直接冲突；Scene Manifest 与静态资源不是完全缺失，而是已有空占位或 GLB 静态方向但没有可实施的 Tiles 语义；资源生命周期和性能也有原则与长期指标，但没有项目级 POC 门禁。

当前可以安全冻结的是“先批准 D-01~D-06，再按第 20 节依序编写文档”。在用户批准前，本报告不授权任何架构、契约、数据库或前端实现变更。
