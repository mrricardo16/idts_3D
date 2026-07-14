# DOC-3DT-02：全量任务文档统一修订报告

> 状态：Ready for user review。本文仅报告文档修订；不构成代码、数据库、依赖或 POC 执行授权。

## 1. 执行结论

DOC-3DT-02 已完成文档扫描、影响分析和统一修订。当前统一基线为：

- 3D Tiles 是厂房、楼层、墙体、道路、大型货架、固定设施和静态环境底座。
- GLB 是动态设备、业务交互、Object Tree、Movable Part、Motion Target、Edit / Monitor、worldZ、状态和告警。
- POC-3DT-01 不阻塞 MVP-09、MVP-10 或无关纯后端任务，但直接阻塞 MVP-10A。
- 正式 Scene Manifest 方向为 baseLayers + devices；POC 不落库；完整 CAD/IFC 转换、生产切片与完整 Tiles 资产管理不进入当前 MVP。

## 2. 扫描与实际读取

- 递归扫描文件数：36 份 Markdown（写入前的 mvp-tasks、api-contracts、architecture、reviews）。
- 实际内容读取：48 份 Markdown 加根目录 AGENTS.md 和 idts3D_ui/AGENTS.md，共 50 份文件。
- 未找到的指定文件：无。

已读取的递归目录文件包括：

- mvp-tasks：README、POC-3DT-01、MVP-00 至 MVP-16。
- api-contracts：README、asset-version、conversion-jobs、model-assets、motion-targets、movable-parts、object-tree-model-stats、scenes。
- architecture：ADR-001、main-delivery-workflow、project-architecture-baseline、project-architecture-debt-register、project-architecture-review-checklist、project-refactoring-roadmap、repository-text-and-artifact-policy。
- reviews：DOC-3DT-00、DOC-3DT-01。
- 核心规划：development-rules、idts-mvp-task-breakdown、domain-entity-dto-map、frontend-integration-plan、backend-implementation-plan、e2e-acceptance-plan、factory-scale-roadmap、idts-digital-twin-project-technical-plan、IDTS数字孪生系统完整实施计划、IDTS数字孪生系统建设方案_需求补充版、idts-requirement-supplement-business-simulation-wcs-cad-glb-v0.2、idts-demo-codex-performance-plan。

## 3. 影响矩阵

| 文档群 | 原有职责 | 冲突/缺口 | 本轮处理 |
|---|---|---|---|
| 任务索引与总纲 | GLB MVP 串行顺序与 POC 支线 | POC 曾被表述为不阻塞全部 MVP；无 MVP-10A | 增加双层门禁、MVP-10A、正式执行顺序 |
| MVP-09/10 | GLB 设备绑定与基础 Manifest | tilesets 仅占位，静态底座无边界 | 保持 GLB 范围，明确不伪装静态底座、正式方向延后至 MVP-10A |
| MVP-11～16 | 前端契约、加载、编辑、运行与验收 | 缺少混合场景依赖与验收 | 明确 MVP-10A 依赖、分层加载、只读底座、共存/回退/释放验收 |
| API/实体/前后端计划 | 当前 API 与实体映射 | 不能将占位或候选表视为实现 | 记录非实施设计边界与一对一同步要求 |
| 路线图/技术/性能计划 | 后期生产化方向 | 需区分近期 POC 与后续工具链 | 改为先验证后生产化，不虚构性能或现场数据 |
| 架构、POC、运维文档 | 缺少可审查设计与证据模板 | 坐标、生命周期、性能、回退、结果未成体系 | 新增设计草案、测试计划、结果模板、预算和回退方案 |

## 4. 修改文件

1. IDTS数字孪生系统完整实施计划.md
2. api-contracts/scenes.md
3. backend-implementation-plan.md
4. domain-entity-dto-map.md
5. e2e-acceptance-plan.md
6. factory-scale-roadmap.md
7. frontend-integration-plan.md
8. idts-demo-codex-performance-plan.md
9. idts-digital-twin-project-technical-plan.md
10. idts-mvp-task-breakdown.md
11. mvp-tasks/README.md
12. mvp-tasks/POC-3DT-01-threejs-3dtiles-renderer.md
13. mvp-tasks/MVP-09-scene-device-binding.md
14. mvp-tasks/MVP-10-scene-manifest.md
15. mvp-tasks/MVP-11-frontend-api-client-contract-types.md
16. mvp-tasks/MVP-12-frontend-manifest-object-tree.md
17. mvp-tasks/MVP-13-frontend-edit-mode-save.md
18. mvp-tasks/MVP-14-monitor-mode-runtime-animation.md
19. mvp-tasks/MVP-15-conversion-job-status-log.md
20. mvp-tasks/MVP-16-e2e-acceptance.md

## 5. 新增文件

1. architecture/ADR-001-3dtiles-glb-hybrid-architecture.md（本轮统一修订前已创建的已知文档，纳入本次基线）
2. architecture/coordinate-system-and-transform-spec.md
3. architecture/threejs-hybrid-scene-architecture.md
4. architecture/hybrid-scene-resource-lifecycle.md
5. design/scene-resource-manifest-design.md
6. poc/POC-3DT-01-test-plan.md
7. poc/POC-3DT-01-result-report-template.md
8. performance/3d-performance-budget.md
9. operations/3dtiles-fallback-and-rollback-plan.md
10. mvp-tasks/MVP-10A-3dtiles-glb-hybrid-scene.md
11. reviews/DOC-3DT-02-full-document-alignment-report.md

## 6. 已检查但未修改

- 受保护：reviews/DOC-3DT-00-document-gap-audit.md、reviews/DOC-3DT-01-audit-review-and-decision-gate.md。
- 既有任务：MVP-00 至 MVP-08。
- 既有契约：api-contracts/README、asset-version、conversion-jobs、model-assets、motion-targets、movable-parts、object-tree-model-stats。
- 既有架构治理：main-delivery-workflow、project-architecture-baseline、project-architecture-debt-register、project-architecture-review-checklist、project-refactoring-roadmap、repository-text-and-artifact-policy。
- 规划补充：development-rules、IDTS数字孪生系统建设方案_需求补充版、idts-requirement-supplement-business-simulation-wcs-cad-glb-v0.2。

## 7. 关键冲突及处理

- 旧的 POC 不阻塞全部 MVP 表述被新的运行规则取代：仅不阻塞 MVP-09/MVP-10 与无关纯后端工作，但阻塞 MVP-10A。
- 旧 tilesets 空数组被明确为兼容占位，不是 3D Tiles 已完成或正式静态底座契约。
- 静态底座不会使用 device_instance 或 device_model_binding 伪装表达。
- 后期生产化仍然不属于当前 MVP；近期必须做的是独立 POC 验证，而非生产切片。

DOC-3DT-00 和 DOC-3DT-01 中保留的旧表述是本轮修订前的审计证据，按用户指令未修改；它们不再作为当前执行规范。当前规范以任务索引、总纲、ADR、POC 卡和 MVP-10A 卡为准。

## 8. 仍待用户确认的 TBD

1. 现场原点、单位、轴向、高度基准、旋转约定、变换参数、标定点与坐标容差。
2. 阶段 B Tileset 的来源、许可、托管、再分发授权与现场资料可用性。
3. POC 硬件/浏览器/分辨率/网络基线和最终性能阈值。
4. baseLayers 的最终字段、版本兼容、旧 tilesets 兼容策略和正式 API 版本。
5. scene_resource / scene_layer 的最终表名、字段、关系、发布、回滚和 Migration 方案。
6. Conditional Go 的可接受条件、责任人、期限和批准记录格式。

## 9. 链接、关键词与范围检查

- UTF-8 严格解码：全部 idts3D_docs Markdown 通过，0 个无效文件。
- Markdown 相对链接：0 个缺失目标；任务索引包含 MVP-10A、POC 测试计划和结果模板可达路径。
- 旧的强制非阻塞措辞：0 命中。
- POC 不阻塞：命中为当前双层规则和两份受保护历史审计记录；已人工复核。
- 完整生产化能力：仅作为当前 MVP 不做事项出现。
- 后期生产化：技术计划中仅指后续生产化，审计文档中仅为历史证据；不表示 POC 可后置。
- tilesets：全部当前/历史占位或兼容说明均明确不是正式完成；正式方向为 baseLayers + devices。
- MVP-10A、baseLayers、devices、scene_resource、scene_layer 均已在当前规范和设计草案中出现。
- 范围：验证时 Git 变更与未跟踪文件均位于 idts3D_docs；没有业务代码、数据库、配置或模型文件变更。

## 10. Git 状态与审核结论

写入后 Git 状态由 20 个已跟踪文档修改和本报告列出的新增文档组成，另保留两份受保护未跟踪审计文档。没有已暂存文件、删除文件或非文档变更。

可以提交用户审核。未授权执行代码；未修改业务代码、未修改数据库、未安装依赖、未执行 POC、未构建、未测试、未执行 git add、未 commit、未 push。
