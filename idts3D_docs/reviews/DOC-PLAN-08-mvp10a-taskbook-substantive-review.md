# DOC-PLAN-08：MVP-10A 计划任务书实质性审查

## 1. 审查结论

**Not approved**。任务链的目标、依赖、边界与状态大体一致，但存在一项会让实施者从错误的后端基线开始的 P0：MVP-10A-04 将场景 Manifest Controller/Service/DTO 作为现有文件/能力引用，实际源代码中未检出该链路。另有多张卡未给出可直接执行的构建/测试命令或足够具体的故障矩阵。

## 2. 仓库基线

- 分支：`main`
- HEAD：`e0b4180b0833f6fdad336bdcfb597870430a8b41`（`docs: complete MVP-10A task books`）
- DOC-PLAN-07 已提交并推送；本审查直接审阅该提交的文档内容。

## 3. 审查范围

已读取项目/前端规则、开发规则、DOC-PLAN-07 报告、MVP-10A 总卡和 01～05、索引/总纲/实施计划、前后端/实体/API/E2E 同步文档、POC/架构/坐标/Manifest/性能/回退资料、MVP-09～16，并只读核对 `idts3D_ui/src/**`、`idts3D_api/src/**`、`idts3D_api/tests/**`。

## 4. 工作区范围

允许排除项：`.gitignore` 与 `tools/`。`.gitignore` 的未暂存差异仅忽略 `tools/micro-tileset` 的生成 tiles、Python 缓存和 Blender 备份，符合本任务允许条件；未读取或操作 `tools/`。本报告是唯一 DOC-PLAN-08 新增文件。

## 5. .gitignore 排除项核验

差异只包含：`tools/micro-tileset/output/`、`__pycache__/`、`*.blend1`、`*.blend2`、`*.blend@`。未修改、暂存或将其计入问题清单。

## 6. 总卡审查

评分：**4 / 5**。总目标、静态 Tiles/动态 GLB 责任、POC 门禁、01～05 严格依赖、输入输出、不变量、失败停线、GLB-only 总体回退、逐卡授权和排除范围均明确。缺口是下游卡的具体可执行性不足，因而总卡不能单独提升结论。

## 7. MVP-10A-01 审查

评分：**3 / 5**。正确禁止代码/Migration，要求 JSON、字典、DTO/TS 草案、映射和用户审核，并把未决数据库结论归属到本卡。缺口：最终 JSON、字段字典、决定记录及实施报告没有指定承载文档/模板或最小字段表；“所有阻塞决定已冻结”无法仅靠当前卡判定，10A-04 仍会面对未具名的候选 Entity/DTO/API Client 路径。

## 8. MVP-10A-02 审查

评分：**3 / 5**。真实入口 `src/views/TwinDemo.vue`、`src/engine/TwinScene.ts` 及 Renderer/Controls/Interaction/LOD/ResourceDisposer 路径均存在，且单一 Renderer/Camera/Controls/Loop 与“不加载真实 Tiles”边界清楚。缺口：目标中的 ResourceManager 没有现有或计划新增的具体模块路径；构建/测试仅写“按 package.json/lockfile”，没有可直接运行命令、测试文件或通过条件。

## 9. MVP-10A-03 审查

评分：**3 / 5**。没有在 POC 前固定库，禁止正式 Manifest/数据库，包含坐标、三点、分层拾取、Object Tree 隔离、worldZ 回归、许可与关闭 TilesLayer 回滚。缺口：计划新增测试/受控样本路径仍是候选，自动化与性能前置验证没有命令、测试入口、采样脚本或通过判据到任务卡的可执行映射。

## 10. MVP-10A-04 审查

评分：**2 / 5**。跨端范围、分层加载、部分失败和兼容/回滚意图正确，且有字段映射表形式。**P0**：第 6、10 节把“当前 `SceneManifestService/Controller`、现有 Scene Manifest DTO”列为输入/现有文件；代码核对没有发现 `ScenesController`、`SceneManifestService`、`SceneManifestResponse` 或其测试。当前后端实际检出的 Manifest 链路是 `ModelAssetsController`、`IModelManifestService`、`ModelManifestService`。此外映射表的 `BaseLayer DTO`、`BaseLayer interface`、`scenes client` 是未冻结的泛称，无法告诉实施者真实文件和字段。

## 11. MVP-10A-05 审查

评分：**3 / 5**。输入、性能预算、生命周期资源种类、原始证据、GLB-only 回滚与下游门禁均已列出。缺口：要求的十类故障没有逐项回退矩阵；“逐一验证并记录”没有为每种故障给出检测、用户状态、即时处理、回退、证据和恢复条件。最终没有明确 Pass / Conditional Pass / Fail 的判定表和与 MVP-11～16 的逐项放行规则。

## 12. 跨文档一致性

索引、总纲、总卡、POC 状态和 10A Blocked 状态一致；明确保持 `tilesets: []` 为占位，未把纯 GLB 当作最终混合闭环；前端/后端/实体/API/E2E 文档都表达 `baseLayers + devices`、Tiles 隔离与 GLB fallback。CI 表述已对齐为任务书给定的“通过”，同时保留 MVP-08 的真实 PostgreSQL/Swagger 缺口。历史审计报告未被修改。唯一实质冲突是文档声称的现有 Scene Manifest 后端实现与代码现实不符。

## 13. 代码路径真实性

已确认存在：`TwinDemo.vue`、`TwinScene.ts`、`RendererManager.ts`、`ControlsManager.ts`、`InteractionManager.ts`、`ResourceDisposer.ts`、`LODModelLoader.ts`、`AnimationManager.ts`；以及 `SceneNode`、`DeviceInstance`、`DeviceModelBinding` 和其 EF Configuration。未检出：`ScenesController`、`SceneManifestService`、`SceneManifestResponse`、其专属测试。Model Manifest 实现存在，但不能替代 Scene Manifest。

## 14. 评分表

| 卡片 | 评分 | 可独立授权性 |
|---|---:|---|
| MVP-10A 总卡 | 4 / 5 | 基本可用 |
| MVP-10A-01 | 3 / 5 | 需补充冻结产物模板/路径 |
| MVP-10A-02 | 3 / 5 | 需补充 ResourceManager 与命令/测试入口 |
| MVP-10A-03 | 3 / 5 | 需补充测试/样本/性能执行入口 |
| MVP-10A-04 | 2 / 5 | P0，不能授权 |
| MVP-10A-05 | 3 / 5 | 需逐项故障矩阵与放行判据 |

## 15. P0

1. **10A-04 的后端路径基线失实。** 文档将不存在的 Scene Manifest Controller/Service/DTO 作为现有输入，实施者无法判断是要创建新链路、扩展 Model Manifest，还是等待 MVP-10；这会直接导致跨端契约、职责分层和文件影响猜测。

## 16. P1

1. 10A-01 未规定最终 JSON、字段字典、决定记录和审核清单的具体承载模板/路径。
2. 10A-02 未落实 ResourceManager 的现有/计划路径，构建/测试没有具体命令和通过条件。
3. 10A-03 未落实测试、受控样本和性能采样的具体入口与命令。
4. 10A-05 缺少十类故障逐项回退矩阵，以及 Pass/Conditional Pass/Fail 放行判据。

## 17. P2

1. 多张卡的“构建命令”“自动化测试”“回归测试”使用相近泛化表述，阅读导航完整但不能直接复制执行。

## 18. TBD 与用户待确认事项

POC 的库/版本/样本/许可、坐标与容差、静态资源实体/表及 Migration、schemaVersion/旧 `tilesets` 兼容窗口、真实性能环境均应继续保持 TBD，并分别由 POC、10A-01、10A-04、10A-05 处理。问题不在这些 TBD 的存在，而在 10A-04 未基于真实后端路径说明它们如何落地。

## 19. 是否批准

**否，Not approved。** 10A-01～05 中多张评分低于 4，且存在 P0；不应提交“任务书已达到可独立授权实施”的批准结论。

## 20. 下一步

仅描述，不执行：创建独立的 DOC-PLAN 修订任务，先以代码现实校正 10A-04 的现有/新增后端文件及跨端映射，再补足 10A-01/02/03/05 的具体产物、命令、测试入口、回退矩阵和放行规则；完成后重新执行实质审查。
