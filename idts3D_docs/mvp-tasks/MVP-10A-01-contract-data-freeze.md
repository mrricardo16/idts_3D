# MVP-10A-01：混合契约与数据冻结

> 状态：**Blocked**。这是设计冻结任务；不实施业务代码、Migration 或运行时契约。

## 1. 任务状态

Blocked。计划完整化不解除 POC 或本卡实现门禁。

## 2. 任务目标

在获批 POC 后，以可审核的决定记录冻结正式 Scene Manifest 的 `baseLayers + devices` 方向、资源身份/版本、数据来源、兼容与回滚边界，消除后续前后端猜字段。

## 3. 背景与问题

现有 `/api/scenes/{sceneId}/manifest` 只返回 `devices`，`tilesets: []` 为兼容占位；`scene_resource`/`scene_layer`、字段、数据库关系、版本策略和静态资源放置关系均尚未实施或冻结。设计草案不能直接当作 API、Entity 或表结构事实。

## 4. 前置条件

POC-3DT-01 结果报告完整、用户批准 Go 或获批 Conditional Go，且 ADR、坐标、Manifest、性能与回退文档已审核。

## 5. 解锁条件

用户单独授权本卡，并提供 POC 中确认的库/样本/坐标/兼容约束；缺任一输入时保持 Blocked。

## 6. 输入

- 获批 POC 结果与许可证/测试数据证据；
- `api-contracts/scenes.md` 当前响应与 `scene-resource-manifest-design.md` 草案；
- `model_asset`、`asset_version`、`scene_node`、`device_instance`、`device_model_binding` 的现有映射；
- 用户或项目负责人对 API 版本、兼容窗口和回滚责任的审核意见。

## 7. 输出 / 交付物

1. 审核后的最终 JSON 示例与字段数据字典；2. 后端 DTO 草案、TypeScript interface 草案和逐字段映射表；3. `schemaVersion`、旧 `tilesets`、错误响应、资源身份/版本、静态资源放置关系和 API 版本策略的决定记录；4. `model_asset`/`asset_version` 使用结论；5. `scene_resource` 或 `scene_layer` 是否需要及 Migration 是否需要的结论；6. 兼容/回滚方案、用户审核清单和全部 TBD 归属。

## 8. 允许修改范围

仅限与冻结决定直接相关的 `idts3D_docs/**` 任务卡、API 契约、实体映射和前后端计划。允许列出候选路径并明确“实施前确认”。

## 9. 禁止修改范围

禁止 `idts3D_ui/src/**`、`idts3D_api/**`、数据库、Migration、配置、依赖、POC 样本和真实响应；禁止把草案字段写成已实现字段。

## 10. 现有文件

- `idts3D_docs/api-contracts/scenes.md`：当前 `devices` 与空 `tilesets` 占位；
- `idts3D_docs/design/scene-resource-manifest-design.md`：非实施草案；
- `idts3D_docs/domain-entity-dto-map.md`：现有 GLB 实体映射；
- `idts3D_docs/architecture/coordinate-system-and-transform-spec.md`：坐标 TBD 边界。

## 11. 计划新增文件

不预设新增代码文件。若冻结结论要求实现，候选为 Contracts DTO、前端 `src/types`、`src/api` 与后端 Application/Infrastructure 路径；这些均由 MVP-10A-04 在实施前确认，不得在本卡创建。

## 12. 前端影响

本卡不修改前端。输出必须标明：TypeScript/API Client/TwinScene 为“推迟至 10A-04”，并给出拟冻结字段与消费责任；页面不得定义临时 Tiles 类型。

## 13. 后端影响

本卡不修改后端。当前未检出 `SceneManifestResponse`、Scene Manifest Application Service 或 Controller；它们只能作为 10A-01 冻结后由 10A-04 计划新增的候选，最终名称不得在本卡前假定。现有 Model Manifest 链路不是 Scene Manifest 的同义替代。

## 14. 数据库影响

不建表、不改表、不建 Migration。必须作出可审核结论：静态资源仅复用现有资源身份能力，或需要独立场景放置关系；未能决定时记录 TBD、所需输入、阻塞的 10A-04 和用户决定人。

## 15. API / DTO / TypeScript 契约

冻结范围至少包括 `schemaVersion`、`baseLayers`、`devices`、资源 ID/版本、URL、变换、状态、错误响应和旧 `tilesets` 兼容。字段名、类型、可空性、枚举与错误码必须以批准记录为准；本卡前所有未知值写 TBD，不能由页面或库默认值推断。

## 16. 前后端一对一映射

交付映射表须包含：数据来源 → Entity/配置 → DTO → API 字段 → TypeScript → API Client → TwinScene 消费位置。`devices` 保持既有 GLB 路径；`baseLayers` 不映射到 device 实体或 Object Tree。

## 17. 执行步骤

1. 核验 POC 批准和当前契约；2. 列出所有冻结决策与未知项；3. 由用户审核 JSON、字段字典和兼容策略；4. 记录数据库/Migration 结论；5. 建立跨端映射；6. 回写引用文档并进行链接/一致性检查；7. 未获决定的项登记为 TBD 后停止，不提前实现。

## 18. 数据准备

只使用 POC 已批准的样本元数据、许可证和版本信息；不导入 Tiles、现场数据或数据库记录。

## 19. 构建命令

无。该设计冻结任务不得运行构建、测试、POC 或 Migration。

## 20. 自动化测试

无代码自动化测试。执行 Markdown 相对链接、UTF-8 严格解码、章节/映射完整性和 `git diff --check`。

## 21. 手工验证

逐项审查 JSON、字典、DTO/TS 草案、映射、旧客户端行为、错误响应、数据来源、Migration 结论和每个 TBD 的归属。

## 22. 验收标准

不存在前端猜字段；静态底座不伪装成设备；每个跨端字段一一对应；所有阻塞 10A-02～05 的决定已冻结，或有明确 TBD 责任、所需输入和阻塞范围；用户审核清单可签核。

## 23. 回归测试

确认现有 `devices` 响应及 `tilesets: []` 兼容基线未被本卡改变；POC、GLB Object Tree、worldZ、Edit/Monitor 与现有 API 均未执行或改变。

## 24. 失败停止条件

POC 未获批准、字段来源无法证明、兼容策略无用户决定、需新增依赖/表但未授权、或映射存在未解释断点时停止，维持 Blocked。

## 25. 风险

将草案当事实、过早锁定表名、误用 device 实体、旧客户端破坏及版本回滚不可逆。

## 26. 回滚方案

撤销未实施的冻结草案及其引用，恢复当前 `devices` 契约与空 `tilesets` 占位；不涉及数据库、二进制或生产入口回滚。

## 27. 证据与报告

保存审核后的 JSON、字段字典、映射表、决定日志、TBD 清单、链接/编码检查结果和用户确认记录至本卡实施报告；不得伪造实测数据。

## 28. 完成定义

用户可依据全部交付物授权 10A-02；本卡完成不代表 10A-02～05、MVP-11～16 或正式接口已获授权。

## 29. 下一任务入口

MVP-10A-02，仅在本卡冻结结果已审核且用户单独授权后进入。

## 30. Codex 执行提示词

```text
请执行 MVP-10A-01。只冻结经过 POC 和用户审核支持的混合 Scene Manifest 决策；输出 JSON、字段字典、DTO/TS 草案、跨端映射、数据库/Migration 结论、兼容/回滚和 TBD 归属。不得实现代码、Migration 或猜字段；未决项必须停止并请求决定。
```

## 31. 冻结产物、固定路径与模板

本卡完成时必须实际修改或新增下列文档，不能只在任务输出中口头说明：

| 路径 | 必须写入内容 |
|---|---|
| `idts3D_docs/design/scene-resource-manifest-design.md` | 审核后的 JSON、字段字典、`baseLayers`/`devices` 边界和未冻结项 |
| `idts3D_docs/api-contracts/scenes.md` | 当前契约不变的声明、计划演进边界、冻结后的 API 版本/错误/兼容规则 |
| `idts3D_docs/domain-entity-dto-map.md` | 资源来源、现有 Entity 与计划新增 Scene Manifest 链路的状态映射 |
| `idts3D_docs/architecture/ADR-001-3dtiles-glb-hybrid-architecture.md` | 复用 Model Manifest 或新增 Scene Manifest 链路的获批架构决定及替代方案 |
| `idts3D_docs/reviews/MVP-10A-01-contract-freeze-report.md` | 下列冻结报告模板的完整审核记录 |

冻结报告模板必须包含：

```text
1. 冻结结论                 2. schemaVersion
3. Scene Manifest JSON      4. 字段数据字典
5. 后端 DTO                 6. TypeScript 类型
7. API 端点                 8. Model Manifest 与 Scene Manifest 关系
9. 数据来源                 10. 新表 / 复用表结论
11. Migration 结论          12. 旧 tilesets 兼容
13. 错误码                  14. 回滚
15. 未冻结项                16. 用户批准人和日期
17. 是否解锁 MVP-10A-02    18. 是否解锁 MVP-10A-04
```

没有该报告或用户批准时，10A-02、10A-03、10A-04 均不得实施；任何未冻结项必须列出所需输入、责任任务和阻塞范围。
