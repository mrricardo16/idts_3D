# ADR-001：3D Tiles 静态底座与 GLB 动态设备混合架构

## 1. 状态

Accepted for design baseline

该状态仅表示本 ADR 已批准为后续文档设计基线；不代表已授权安装依赖、执行 POC、修改数据库、修改 API、修改 TypeScript 类型或修改正式前端主流程。

## 2. 决策日期

2026-07-14

## 3. 决策背景

当前 IDTS 3D 的 MVP 文档以 GLB 设备资产、对象树、可动部件、Motion Target、edit/monitor 和 worldZ 动画闭环为主。现有 Scene Manifest 面向设备，包含 `devices[]` 及设备 transform，并只预留空 `tilesets` 数组。

正式技术方案同时已经提出：厂区、建筑、楼层、地面、固定货架和固定结构适合使用 3D Tiles；提升机、堆垛机、AGV 和输送设备等动态业务对象继续使用独立 GLB，并在 Three.js 内共存。DOC-3DT-00 与 DOC-3DT-01 复核后，用户批准 D-01 至 D-06，以消除混合接入前的决策悬空状态。

## 4. 当前问题

1. 单纯 GLB 方案不天然适合大型静态厂区的空间层级流式加载。
2. 单纯 3D Tiles 方案不能替代当前 GLB 的对象树、可动部件、Motion Target、状态动画、拾取、高亮与告警业务边界。
3. 现有 POC 被定义为独立且不阻塞 MVP 主线；此前没有定义它与未来正式混合接入的关系。
4. Scene Manifest 的 `tilesets` 是扩展占位，尚无可实施的静态底座资源契约。
5. 数据模型可以表达 GLB 资产与设备绑定，但未冻结 Tileset/静态底座在场景内的正式领域表达。
6. 坐标、资源生命周期、性能预算、POC 测试计划与回退流程尚未冻结。

## 5. 决策目标

```text
大型静态厂区可流式加载
+
GLB 动态设备保留现有业务交互
+
两者在同一 Three.js 渲染环境中共存
```

本 ADR 的目标是冻结职责、分层方向、门禁和文档依赖，避免在 POC 或正式接入时把未批准的 API、数据库或实现细节当作既定事实。

## 6. 已批准决策

| 编号 | 已批准决定 | 本 ADR 的约束 |
|---|---|---|
| D-01 | POC 不阻塞 MVP-09、MVP-10 等纯后端或纯 GLB 基础任务；POC 直接阻塞未来 MVP-10A 正式混合场景接入。 | 不修改现有任务索引；任何依赖 MVP-10A 的后续任务按依赖关系间接受阻。 |
| D-02 | POC 不落库；正式接入优先使用 `model_asset` / `asset_version` 表达资源身份与版本，并研究 `scene_resource` 或 `scene_layer` 表达静态资源的场景放置关系。 | 不用 `device_instance` 表示静态厂区，不用 `device_model_binding` 绑定非设备静态场景；不冻结表名和字段。 |
| D-03 | 正式 Scene Manifest 的方向为 `baseLayers + devices`。 | 本次不定义 JSON、DTO、TypeScript 类型、兼容策略或 API 字段。 |
| D-04 | POC 使用独立页面和独立 tiles engine 模块，不修改 TwinDemo 主流程；正式接入在 TwinScene 内图层化管理。 | 正式场景至少区分 TilesLayer 与 DeviceLayer，并保持 renderer、camera、controls 和 animation loop 的统一所有权。 |
| D-05 | POC 分为公开许可明确的小型样例阶段和授权后的代表性中大型数据阶段。 | 记录来源、许可、规模、版本；未经授权的真实厂区数据不得进入公开仓库；不做 CAD/IFC/STEP 生产转换。 |
| D-06 | POC 使用 Go、Conditional Go、No-Go 三态。 | Codex 记录证据；用户/项目负责人拥有最终批准权；数值性能阈值由后续性能预算文档冻结。 |

## 7. 资源职责边界

### 3D Tiles

3D Tiles 负责静态环境与大体量空间资源，包括：

- 厂房、楼层、道路、墙体；
- 大型货架、固定设施和静态环境底座；
- 后续经批准的其他静态场景资源。

当前阶段，3D Tiles 不负责：

- 动态设备动画；
- Movable Part、Motion Target；
- 设备状态映射、设备告警；
- 复杂对象树编辑；
- 以 Tiles 内部节点承担当前 GLB 的业务控制。

### GLB

GLB 负责动态设备和业务交互对象，包括：

- 提升机、堆垛机、AGV、输送设备、机械臂；
- 托盘、货物及其他动态业务对象；
- 可动部件、状态动画、拾取、高亮和告警。

Object Tree、Movable Part、Motion Target、edit、monitor、worldZ 动画、状态映射与告警高亮继续服务于 GLB 设备层。

## 8. 前端场景架构方向

正式接入的方向为在同一个 `TwinScene` 渲染环境中进行图层化管理：

```text
TwinScene
├── TilesLayer
├── DeviceLayer
├── AnnotationLayer
└── HelperLayer
```

该图仅冻结分层方向，不冻结类名、方法、目录或实现细节。正式方案不得建立第二套长期并行的正式场景引擎，也不得由页面组件分别持有多套 renderer。

Renderer、Camera、Controls 与 Animation Loop 必须保持统一所有权。Tiles renderer 的生命周期和每帧更新由后续混合场景架构文档定义；本 ADR 不指定库调用方式或加载器实现。

POC 采用独立页面和独立 tiles engine 模块，只验证技术可行性，不构成正式生产入口。POC 代码是否可复用到正式架构，必须由 POC 结果评审决定。

## 9. Scene Manifest 与 Model Manifest 边界

正式方向：

- **Scene Manifest** 描述场景级资源，方向为 `baseLayers + devices`，并承载场景内资源的放置、变换与可见性语义。
- **Model Manifest** 描述单个模型资产，继续承载 GLB 版本、Object Tree、Model Stats，以及 Movable Part 与 Motion Target 的关联方向。

`baseLayers` 负责 3D Tiles 静态厂区底座和未来经批准的静态场景资源；`devices` 负责 GLB 动态设备、设备实例、设备模型绑定及其业务关联入口。

现有 `scenes.md` 的 `tilesets: []` 占位保持不变。本 ADR 不定义最终 JSON 字段、不修改 Route、DTO、TypeScript interface、API Client 或兼容策略。

## 10. 数据模型方向

POC 阶段不落库，也不新增或修改 Entity、DbContext、Migration 或表结构。

正式接入阶段的设计方向是：

- `model_asset` / `asset_version` 优先承担资源身份和版本能力；
- `scene_resource` 或 `scene_layer` 是表达静态底座资源及其在场景中放置关系的优先研究方向；
- `device_instance` 保持设备实例语义；
- `device_model_binding` 保持设备到 Published 模型版本的绑定语义。

`scene_resource`、`scene_layer` 的最终命名、字段、关系、版本策略和迁移方案必须由后续独立设计文档冻结。本 ADR 不预先锁死数据库结构。

## 11. POC 与正式接入门禁

```text
现有 GLB MVP 任务（包括 MVP-09、MVP-10）
  不受 POC 全局阻塞

POC-3DT-01
  记录技术可行性证据
  └─ Go / Conditional Go / No-Go 由用户或项目负责人批准

MVP-10A：3D Tiles + GLB 正式混合场景接入
  必须依赖已批准的 Go，或具有明确条件的 Conditional Go
```

POC 未通过前，禁止执行正式混合场景接入。该规则不改变当前 MVP 索引或依赖关系；索引更新由后续独立文档任务处理。

## 12. POC 三态判定原则

### Go

必须同时满足：

1. Mandatory 功能用例全部通过。
2. Tileset 与 GLB 能够在同一场景显示。
3. 相机、Controls 和主循环正常。
4. GLB 拾取和动画不被破坏。
5. 坐标、方向和比例能够依据明确转换规则稳定对齐。
6. 页面卸载后没有已确认的重复渲染循环或严重资源泄漏。
7. 代表性测试数据达到后续性能预算文档定义的门槛。
8. 没有许可、安全或数据合规阻塞问题。

### Conditional Go

适用于核心路线可行且 Mandatory 功能基本通过，但存在可修复问题的情况。每个条件项必须有明确责任人、期限、回退措施和问题记录；条件项不得涉及核心坐标不可用、主流程破坏、严重泄漏或许可违规。条件未关闭前不得视为正式 Go。

### No-Go

出现以下任一情况即为 No-Go：代表性 Tileset 无法稳定加载；GLB 与 Tileset 无法可靠共存；GLB 拾取、动画或现有主交互被破坏且没有可接受修复方案；坐标、比例或方向无法形成稳定转换；存在严重且可复现的资源泄漏或重复渲染循环；只能通过大规模推翻现有主场景架构才能工作；存在许可、安全或合规阻塞；关键风险没有责任人或回退方案。

Codex 负责执行获授权的任务并记录证据，不拥有最终三态批准权。用户/项目负责人负责审核证据、批准三态结果并决定是否允许进入 MVP-10A。

## 13. 方案对比

| 方案 | 优点 | 缺点 | 结论 |
|---|---|---|---|
| A. 全部使用 GLB | 延续现有设备交互、对象树和动画能力；实现路径连续。 | 大型静态厂区的空间层级、流式加载和维护成本不适合长期只依赖 GLB。 | 不作为长期厂区底座方案。 |
| B. 全部使用 3D Tiles | 适合静态大场景与空间层级流式加载。 | 不能替代当前动态设备的 Object Tree、Movable Part、Motion Target、业务动画、告警和拾取边界。 | 不作为当前动态设备业务方案。 |
| C. 3D Tiles 静态底座 + GLB 动态设备 | 匹配两类资源的变化频率与交互需求；保留现有 GLB 业务闭环；能在同一 Three.js 环境中演进。 | 需要坐标、资源生命周期、契约、性能和回退设计；必须先通过 POC。 | 选定。 |
| D. 以 CesiumJS 或其他独立地理场景引擎替换 Three.js | 可能提供不同的地理场景与大范围能力。 | 替换成本高，会扰动已验证的 GLB、拾取、动画、overlay 与相机控制能力；当前没有要求证明必须迁移。 | 当前不采用。 |

本 ADR 不宣称任何库或方案已经达到生产性能；性能和兼容性由后续 POC 与性能预算验证。

## 14. 最终选择

选择方案 C：在同一 Three.js 渲染环境内，使用 3D Tiles 承担静态厂区底座，使用 GLB 承担动态设备和业务交互。

该选择以 D-01 至 D-06 为边界：POC 独立验证且不全局阻塞 GLB MVP；只有经用户/项目负责人批准的 Go 或受控 Conditional Go，才允许开始 MVP-10A 的正式混合场景接入设计和实施。

## 15. 正向影响

- 为大型静态厂区资源建立与动态设备不同的职责边界。
- 保护现有 GLB 的业务对象树、动画、告警和 monitor/edit 语义。
- 将高风险的坐标、生命周期、性能、许可和回退问题前置到 POC 与文档门禁。
- 避免把静态厂区伪装成设备实例，降低后续数据模型混淆风险。
- 保持 Three.js 为主引擎，减少不必要的引擎替换风险。

## 16. 负向影响与代价

- 需要新增一组受控文档和独立任务，不能立即开始正式接入。
- 正式接入将涉及资源模型、Scene Manifest、前端图层和测试边界的协同设计。
- POC 需要管理测试数据许可、来源、版本和规模。
- 混合场景会增加坐标校准、资源释放、加载失败和回退治理成本。
- Conditional Go 会引入明确的条件项跟踪，不能将未关闭问题隐藏在正式结论中。

## 17. 风险

| 风险 | 控制原则 |
|---|---|
| 坐标、方向或比例不一致 | POC 前完成最小坐标规范；以可重复的转换与验收方法判断。 |
| GLB 交互回归 | POC 与正式接入均保留 GLB picking、动画、Object Tree 与 fallback 回归项。 |
| Tiles 生命周期造成内存/引用问题 | 后续生命周期文档明确所有权、取消、缓存、dispose 和动态内容注册。 |
| 静态资源语义混入设备模型 | 不用 `device_instance` / `device_model_binding` 表达静态厂区；独立评审资源层设计。 |
| API 或前端提前猜测字段 | 本 ADR 不定义最终字段；后续契约设计须同步 DTO、TS 与 API Client。 |
| 未授权测试数据进入仓库 | POC 记录来源和许可；未授权真实厂区数据不得提交。 |
| 将 POC 误当生产结论 | POC 三态由用户/项目负责人批准，性能阈值和正式回退另行冻结。 |

## 18. 回退原则

TilesLayer 加载失败不得必然导致 GLB 设备层崩溃。正式接入必须具备关闭 TilesLayer 或回退纯 GLB 模式的能力。

本 ADR 只冻结回退原则；加载失败处理、用户提示、配置开关、版本回滚和运维流程由后续 `3dtiles-fallback-and-rollback-plan.md` 设计。

## 19. 明确不做

本 ADR 不执行或授权：

- 依赖安装、3D Tiles Loader、正式 TilesLayer、POC 页面或真实厂区 Tileset；
- CAD、IFC、STEP 到 3D Tiles 的生产转换；
- Scene Manifest、API、DTO、TypeScript 类型、API Client 或数据库修改；
- Entity、DbContext、Migration、Worker、Controller、Service 或路由修改；
- TwinDemo、TwinScene、正式页面或正式渲染主流程修改；
- 业务代码、数据库、API、DTO、TypeScript 类型或生产入口实现；
- 构建、测试、执行 POC、`git add`、commit 或 push。

## 20. 尚未冻结事项

以下事项明确保留给后续文档和用户确认：

1. 场景原点、单位、轴方向、Y-up/Z-up、矩阵顺序、误差容限和标定方法。
2. `scene_resource` / `scene_layer` 的最终名称、表结构、关系、发布与回滚策略。
3. `baseLayers` 与 `devices` 的最终 JSON、DTO、TypeScript 类型、API Route 和兼容策略。
4. Tiles renderer 的库版本、加载器实现、缓存、取消、动态内容注册和销毁细节。
5. POC 的具体数据、许可证、浏览器/硬件环境、测试步骤、数值性能阈值和结果格式。
6. 正式接入的完整资源生命周期、fallback/rollback、可观测性和端到端验收。
7. POC 代码对正式 `TwinScene` 分层架构的可复用性。

## 21. 后续必需文档

以下文档必须在对应阶段创建和审校，但本次不创建：

1. `coordinate-system-and-transform-spec.md`
2. `threejs-hybrid-scene-architecture.md`
3. POC-3DT-01 修订任务卡
4. `POC-3DT-01-test-plan.md`
5. `POC-3DT-01-result-report-template.md`
6. `scene-resource-manifest-design.md`
7. `hybrid-scene-resource-lifecycle.md`
8. `3d-performance-budget.md`
9. `3dtiles-fallback-and-rollback-plan.md`
10. `MVP-10A-3dtiles-glb-hybrid-scene.md`

任务索引已同步 POC 门禁、MVP-10A 总任务和 MVP-10A-01～05 子任务。默认顺序是文档审核、POC、POC 结果审核、用户批准、MVP-09/10、MVP-10A 子任务、MVP-11～16；正式架构、资源清单、生命周期、性能和回退文档是该审核链的一部分，不构成编码授权。

## 22. 实施授权边界

本 ADR 是文档设计基线，不是编码实施授权。

在后续文档完成并由用户明确授权前，不得根据本 ADR 修改任何数据库、API、DTO、TypeScript 类型、前端主流程、后端工程、POC 代码或任务索引。任何跨端契约变化必须由独立任务同步记录后端 DTO、前端 TypeScript interface、API Client、验证和回滚影响。

## 23. 参考文档

以下参考文档在本次读取中均存在：

- `idts3D_docs/reviews/DOC-3DT-00-document-gap-audit.md`
- `idts3D_docs/reviews/DOC-3DT-01-audit-review-and-decision-gate.md`
- `idts3D_docs/mvp-tasks/README.md`
- `idts3D_docs/idts-mvp-task-breakdown.md`
- `idts3D_docs/mvp-tasks/POC-3DT-01-threejs-3dtiles-renderer.md`
- `idts3D_docs/mvp-tasks/MVP-09-scene-device-binding.md`
- `idts3D_docs/mvp-tasks/MVP-10-scene-manifest.md`
- `idts3D_docs/mvp-tasks/MVP-11-frontend-api-client-contract-types.md`
- `idts3D_docs/mvp-tasks/MVP-12-frontend-manifest-object-tree.md`
- `idts3D_docs/mvp-tasks/MVP-16-e2e-acceptance.md`
- `idts3D_docs/api-contracts/scenes.md`
- `idts3D_docs/domain-entity-dto-map.md`
- `idts3D_docs/frontend-integration-plan.md`
- `idts3D_docs/e2e-acceptance-plan.md`
- `idts3D_docs/factory-scale-roadmap.md`
- `idts3D_docs/idts-digital-twin-project-technical-plan.md`
- `idts3D_docs/IDTS数字孪生系统完整实施计划.md`
- `idts3D_docs/idts-demo-codex-performance-plan.md`
- `idts3D_ui/.agents/skills/digital-twin-ui-design/references/vue-three-architecture.md`
- `idts3D_ui/.agents/skills/digital-twin-ui-design/references/scene-interaction-status.md`
- `idts3D_ui/.agents/skills/digital-twin-ui-design/references/acceptance-review.md`

未找到的引用文件：无。
