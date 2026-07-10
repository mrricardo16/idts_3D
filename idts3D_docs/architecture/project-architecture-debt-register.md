# 项目架构债务登记册

> 状态值：Open、Planned、Closed。除明确 Closed 的条目外，均未因 ARCH-01 自动修复。

## 登记字段

每项按“状态 / 优先级 / 证据 / 当前影响与扩张风险 / 触发条件 / 治理任务 / 功能-API-数据库-Migration 影响 / 验证 / 回滚”记录。治理任务默认不改变已有行为。

## 后端

| ID | 登记 |
|---|---|
| BE-DEBT-001 | Open / P2；`Domain/Entities/DigitalTwinEntities.cs`；Entity 集中，继续扩张将降低可维护性；新增第二个独立实体组时触发；ARCH-05；否/否/否/否；编译与依赖检查；还原该次仅文件拆分。 |
| BE-DEBT-002 | Open / P2；`Infrastructure/Persistence/Configurations/DigitalTwinEntityConfigurations.cs`；EF 配置集中，新增配置会扩大耦合；新增第二个独立配置组时触发；ARCH-06；否/否/否/否；migration snapshot 与集成验证；还原仅配置拆分。 |
| BE-DEBT-003 | Open / P2；`Application/ModelAssets/IModelAssetRepository.cs`；抽象覆盖多个资产用例；继续添加无关用例会模糊能力；新增第三种独立能力时触发；ARCH-07；否/否/否/否；接口消费者与测试；还原接口拆分。 |
| BE-DEBT-004 | Open / P2；`Infrastructure/ModelAssets/ModelAssetRepository.cs`；实现职责较多，状态、索引与存储查询会继续耦合；新增无关聚合时触发；ARCH-07；否/否/否/否；回归/事务测试；还原实现拆分。 |
| BE-DEBT-005 | Open / P3；`Api/Extensions/ServiceCollectionExtensions.cs`；Application DI 位于 Api，组合根职责边界需明确；新增模块注册时触发；ARCH-08；否/否/否/否；启动与 DI 验证；还原注册迁移。 |
| BE-DEBT-006 | Open / P1；无测试项目；状态和事务缺少自动回归；下一次状态流转或事务变更前触发；ARCH-03；否/否/否/否；新增 Application/Integration Test；删除新测试基础设施。 |
| BE-DEBT-007 | Closed / P1；`idts3D_api/README.md` 曾停留 MVP-01；进度误导；ARCH-01 已同步；ARCH-13 持续防漂移；否/否/否/否；README 与代码/任务卡核对；还原文档提交。 |
| BE-DEBT-008 | Open / P2；`Api/Controllers/ModelAssetsController.cs`；端点继续堆积会使 Controller 失焦；新增独立领域端点时触发；ARCH-07；否/否/否/否；路由/响应回归；还原控制器拆分。 |
| BE-DEBT-009 | Open / P1；跟踪的 Development appsettings；连接串与路径策略未定；进入多人/部署环境前触发；ARCH-04；否/否/否/否；secret scan 与启动配置验证；还原配置治理变更。 |

## 前端

| ID | 登记 |
|---|---|
| FE-DEBT-001 | Open / P2；`TwinDemo.vue` 与 engine；边界需持续约束；新增页面能力时触发；ARCH-09；否/否/否/否；页面/场景人工回归；还原边界重构。 |
| FE-DEBT-002 | Open / P2；`src/engine/**`；加载、LOD、统计、交互需防止横向耦合；新增第三类场景加载能力时触发；ARCH-09；否/否/否/否；模型加载与选择回归；还原模块拆分。 |
| FE-DEBT-003 | Open / P1；不存在 `src/api`；正式 API Client 未建立；MVP-11 前触发；ARCH-10；否/是/否/否；API Client 测试与 fallback 回归；移除独立 Client 变更。 |
| FE-DEBT-004 | Open / P1；无 API TypeScript 类型；契约可能漂移；MVP-11 前触发；ARCH-10/11；否/是/否/否；契约比对；还原类型变更。 |
| FE-DEBT-005 | Open / P2；`TwinDemo.vue`；页面膨胀风险；新增独立面板/工作流时触发；ARCH-09；否/否/否/否；页面表现回归；还原组件拆分。 |
| FE-DEBT-006 | Open / P3；`src/utils` 未来目录；通用工具可能吞入业务逻辑；新增领域 utils 时触发；ARCH-09；否/否/否/否；依赖/行为检查；还原移动。 |
| FE-DEBT-007 | Open / P1；无前端测试；关键交互无自动保护；引入 API Client 或状态流转前触发；ARCH-03；否/否/否/否；test/lint；删除测试基础设施。 |
| FE-DEBT-008 | Open / P1；静态资源与无环境 API 地址；环境/接口地址治理未建立；MVP-11 前触发；ARCH-04/10；否/是/否/否；开发/部署配置验证；还原配置抽象。 |

## 跨端

| ID | 登记 |
|---|---|
| CROSS-DEBT-001 | Open / P1；契约 Markdown 与无 TS API 类型；无自动同步；MVP-11 前触发；ARCH-11；否/是/否/否；契约检查；还原检查器。 |
| CROSS-DEBT-002 | Open / P2；ErrorCode 与前端未同步；错误处理可能分叉；正式 Client 前触发；ARCH-11；否/是/否/否；错误码映射测试；还原映射。 |
| CROSS-DEBT-003 | Open / P2；mode/version 状态语义跨端未落地；monitor/edit 可能偏离；MVP-12 前触发；ARCH-11；否/是/否/否；状态矩阵回归；还原同步规则。 |
| CROSS-DEBT-004 | Open / P1；`e2e-acceptance-plan.md` 仅规划；联调未自动化；MVP-16 前触发；ARCH-03/11；否/是/是/否；端到端验收；移除自动化脚本。 |
| CROSS-DEBT-005 | Open / P2；README 与最近代码提交曾不同步；进度易漂移；每个 MVP 完成时触发；ARCH-13；否/否/否/否；文档核对；还原文档修改。 |
| CROSS-DEBT-006 | Open / P2；Markdown 与 DOCX 并存；规范源曾不明确；修改规范文档时触发；ARCH-13；否/否/否/否；链接与规范源检查；还原声明。 |

## 仓库级

| ID | 登记 |
|---|---|
| REPO-DEBT-001 | Closed / P1；ARCH-02 已创建根级 `.editorconfig`；新文本具备 UTF-8、换行、缩进和基础空白规则；ARCH-02；否/否/否/否；配置 UTF-8 检查与 `git diff --check`；删除配置可回退。 |
| REPO-DEBT-002 | Closed / P1；ARCH-02 已创建根级 `.gitattributes`；文本换行与二进制边界已机械化；ARCH-02；否/否/否/否；`git check-attr` 验证代表性文本和二进制文件；删除配置可回退。 |
| REPO-DEBT-003 | Open / P1；无 `.github` CI；main 无自动门禁；ARCH-03；否/否/否/否；CI 运行；删除 workflow。 |
| REPO-DEBT-004 | Planned / P1；ARCH-02 已声明 debug/reports 与产物边界，但 `idts3D_ui/public/models/**/*.glb` 同时命中 `public/models/lifter.glb` 和 `public/models/lifter/lifter.high.glb`；正式 GLB 与临时/转换 GLB 不能仅靠现有目录规则区分，忽略规则可能误伤正式资产。本次禁止提交或取消忽略二进制模型，故未修复；ARCH-02A；否/否/否/否；核验来源、授权、大小、部署依赖、LFS/外部制品与精确忽略例外；仅回退 ARCH-02A 的资产治理决定。 |
| REPO-DEBT-005 | Closed / P1；根级项目 Skill 缺失；ARCH-01 创建 `.agents/skills/idts3d-architecture-governance`；无影响；Skill 发现与路径检查；还原 ARCH-01 提交。 |
| REPO-DEBT-006 | Closed / P1；AGENTS 曾永久默认禁止 commit/push；ARCH-01 改为确认后 main 直交付；无影响；规则搜索；还原 ARCH-01 提交。 |
| REPO-DEBT-007 | Open / P2；ARCH-02 只读检查发现 8 个混合换行文本、10 个 UTF-8 BOM、67 个含行尾空白的文本和 1 个缺少末尾换行的文本；历史文件一致性不满足新基线，但无严格 UTF-8 解码失败；独立历史文本规范化任务（ARCH-14，待排期）；否/否/否/否；逐文件审计、最小 diff 与功能回归；按文件回退。 |
