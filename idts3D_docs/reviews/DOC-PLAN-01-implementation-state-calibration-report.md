# DOC-PLAN-01：实现状态与测试事实校准报告

> 任务类型：文档事实校准。
> 校准日期：2026-07-15。
> 范围：仅修改任务书授权的状态文档并新增本报告；未修改业务代码、数据库、Migration、配置、模型或 DOC-PLAN-00。

## 1. 校准结论

README、后端 README、架构基线、MVP-08、任务索引与总纲已按同一事实基线校准：MVP-00～MVP-07 已完成；MVP-08 为 **Partially Completed（Implementation Complete / Verification Incomplete）**。

MVP-08 的代码、初始 Migration、Application/API 自动测试和本地 build/test 均有本次证据；真实 PostgreSQL、Swagger 实例、事务、行锁和审计落库验证仍未执行。CI 配置存在，但当前基线最近 CI 失败于 repository-policy，不能写为“CI 通过”。因此本次关闭了文档间的事实矛盾，但**不能进入下一实现任务或授权 POC**。

## 2. 仓库基线

| 项目 | 事实 |
|---|---|
| 分支 | `main` |
| 基线 commit | `5a9a2c5339e11bd3c77072ce276a8a4940c09739` |
| 最近提交 | `5a9a2c5 Revise 3D tiles task docs and POC authorization flow` |
| MVP-08 实现提交 | `b0d277f335e76d5b0437747ed25c2282f88e0810` (`feat: add motion target configuration api`) |
| SDK 声明 | `idts3D_api/global.json` 指定 8.0.100 |

## 3. 工作区状态

开始前 `git status --short` 仅显示获准保留且未跟踪的 `idts3D_docs/reviews/DOC-PLAN-00-plan-taskbook-health-audit.md`。无已暂存、删除、业务代码修改、merge、rebase 或 cherry-pick 状态。DOC-PLAN-00 未被修改。

## 4. MVP-00～MVP-08 状态矩阵

| 任务 | 状态 | 代码证据 | 文档证据 | 构建/测试证据 | 说明 |
|---|---|---|---|---|---|
| MVP-00 | Completed | 根规则、开发规则与任务卡存在 | MVP-00 卡 | 文档任务；不适用后端构建 | 文档基线已建立 |
| MVP-01 | Completed | 六项目 Solution 与各项目文件存在 | MVP-01 卡、提交历史 | 本次 solution build 通过 | 后端骨架已存在 |
| MVP-02 | Completed | `DigitalTwinDbContext`、配置、初始 Migration 存在 | MVP-02 卡、提交历史 | 本次 solution build 通过；真实数据库未重验 | PostgreSQL schema 已落入初始 Migration |
| MVP-03 | Completed | 上传 Controller/Service/Repository/本地存储存在 | MVP-03 卡、提交历史 | Application/API 测试项目存在且本次整体通过 | GLB 上传与受控静态文件能力已实现 |
| MVP-04 | Completed | Model Manifest Controller/Service/Repository/DTO 存在 | MVP-04 卡、提交历史 | 对应 Application/API 测试随整体通过 | Manifest 查询已实现 |
| MVP-05 | Completed | Object Tree/Model Stats Service/DTO/Controller 入口存在 | MVP-05 卡、提交历史 | 对应 Application/API 测试随整体通过 | 对象树与统计 API 已实现 |
| MVP-06 | Completed | AssetVersionLifecycle Service/DTO/Controller 入口存在 | MVP-06 卡、提交历史 | 对应 Application/API 测试随整体通过 | 资产版本生命周期已实现 |
| MVP-07 | Completed | MovableParts Controller/Service/Repository/DTO 存在 | MVP-07 卡、提交历史 | 对应 Application/API 测试随整体通过 | Movable Part CRUD 已实现 |
| MVP-08 | Partially Completed | Motion Target 全链路与审计写入存在 | MVP-08 卡、提交 `b0d277f` | 本地 build 通过；84 项整体测试通过；真实 PostgreSQL/Swagger 未执行 | 实现与自动验证完成，真实环境验证未完成 |

## 5. MVP-08 实现核验

| 能力 | 是否存在 | 文件/符号 | 是否测试 | 结论 |
|---|---:|---|---:|---|
| Motion Target Entity | 是 | `Domain/Entities/DigitalTwinEntities.cs` 的 `MotionTarget` | 间接 | 已实现 |
| DbSet / Mapping | 是 | `DigitalTwinDbContext.MotionTargets`、`MotionTargetConfiguration` | 间接 | 已实现 |
| Migration | 是 | `20260709080033_InitDigitalTwinSchema` | 否，真实 DB 未执行 | 已定义，未做真实 DB 验证 |
| Create API | 是 | `MotionTargetsController.Create` | 是 | API Integration 覆盖路由/绑定 |
| Update API | 是 | `MotionTargetsController.Update` | 是 | API Integration 覆盖路由/绑定 |
| Delete API | 是 | `MotionTargetsController.Delete` | 是 | API Integration 覆盖路由/绑定 |
| Query API | 是 | `MotionTargetsController.Get` | 是 | API Integration 覆盖路由/绑定 |
| 重复校验 | 是 | Service + Repository + 唯一索引 | 是 | Application Test 覆盖重复 code 冲突 |
| 范围校验 | 是 | `MotionTargetService` 以 movable part min/max 校验 | 是 | Application Test 覆盖越界 |
| 版本状态 guard | 是 | Draft/Ready 写入、Published monitor 读取逻辑 | 是 | Application Test 覆盖 Published 写保护 |
| OperationAudit | 是 | `MotionTargetRepository.AddAudit` | 否，真实 DB 未验证 | 代码存在，落库证据缺失 |
| 单元测试 | 是 | `MotionTargetServiceTests` | 是 | 属于本次 54 个 Application Tests |
| 集成测试 | 是 | `MotionTargetApiTests` | 是 | 属于本次 22 个 API Integration Tests，使用 TestServer/Fake |
| Swagger 验证 | 未执行 | Swagger 配置/启动入口未作为本次验收启动 | 否 | 无 Swagger 运行证据 |

## 6. 本地构建结果

| 项目 | 结果 |
|---|---|
| 命令 | `dotnet --info`；`dotnet build idts3D_api/HZ.IDTS.DigitalTwin.sln` |
| 时间 | 2026-07-15（本任务执行期间） |
| Exit Code | 0 |
| Warning / Error | 0 / 0 |
| 实际 SDK | 8.0.100 |
| global.json 一致性 | `idts3D_api/global.json` 声明 8.0.100；版本一致。命令从仓库根执行时 `dotnet --info` 不会向下发现该文件，构建结果不应据此宣称 global.json 被自动选中。 |

## 7. 本地测试结果

| 项目 | 结果 |
|---|---|
| 测试项目 | Application Tests、Architecture Tests、API Integration Tests，均列入 Solution |
| 命令 | `dotnet test idts3D_api/HZ.IDTS.DigitalTwin.sln --no-build` |
| Exit Code | 0 |
| Application Tests | 54 passed，0 failed，0 skipped |
| Architecture Tests | 8 passed，0 failed，0 skipped |
| API Integration Tests | 22 passed，0 failed，0 skipped |
| MVP-08 覆盖 | 有 Service 与 TestServer/API 路由覆盖；无真实 PostgreSQL、Swagger 或浏览器覆盖 |

测试输出在本机 PowerShell 控制台显示为乱码属于控制台编码呈现；退出码和测试统计由 .NET Test 输出确认，不据此判断源码编码异常。

## 8. CI 配置与运行状态

| 项目 | 事实 |
|---|---|
| Workflow | `.github/workflows/ci.yml` 存在 |
| 触发 | `push` 到 `main`、针对 `main` 的 `pull_request`、`workflow_dispatch` |
| 后端 | restore、Release build、Application/Architecture/API Integration Tests |
| 前端 | `npm ci`、lint、type-check、unit test、build |
| 最近运行 | `29313188014`，对应当前基线 commit，结论为 failure |
| 失败位置 | `repository-policy`；`DOC-3DT-03-semantic-consistency-fix-report.md:58` 检出末尾多余空行 |
| 后续作业 | `backend-quality` 与 `frontend-quality` 未启动 |
| MVP-08 实现提交 CI | `29299043634`，结论为 success；不能替代当前基线 CI 结果 |

## 9. 文档冲突及处理

| 原冲突 | 处理 |
|---|---|
| README/后端 README 称只完成 MVP-01～07、没有 Movable Part/Motion Target/测试/CI | 改为可区分的实现、验证、CI 状态；加入本次证据与限制 |
| 架构基线称 Motion Target 仍属未实现、缺失表中列 Movable Part/Motion Target API | 改为 MVP-08 实现完成但真实验证不完整；只保留实际缺失项 |
| MVP-08 称 Completed / Ready after CI | 改为 Partially Completed，并明确本地测试、当前 CI 失败与 `MVP-08-VERIFY` |
| 索引/总纲缺少当前完成与 CI 状态 | 仅增加事实校准，不改变 POC/MVP-10A 规划门禁 |

## 10. 修改文件

1. `README.md`
2. `idts3D_api/README.md`
3. `idts3D_docs/architecture/project-architecture-baseline.md`
4. `idts3D_docs/mvp-tasks/MVP-08-motion-target-api.md`
5. `idts3D_docs/mvp-tasks/README.md`
6. `idts3D_docs/idts-mvp-task-breakdown.md`
7. `idts3D_docs/reviews/DOC-PLAN-01-implementation-state-calibration-report.md`（新增）

## 11. 未修改文件

- `idts3D_docs/reviews/DOC-PLAN-00-plan-taskbook-health-audit.md`
- 所有业务代码、数据库、Migration、配置、模型、3D Tiles 设计和 CI workflow。

## 12. 仍未确认事项

1. 真实 PostgreSQL 的 MVP-08 Draft CRUD、唯一约束、事务、行锁、OperationAudit 与 Published 写保护验证。
2. 实际启动 API 后的 Swagger 验证。
3. 当前基线 CI 在 repository-policy 修复后是否能够完整通过；当前无此证据。
4. Scene/Device API、正式前端 API Client/联调、Worker 转换流水线、浏览器 E2E、真实 WebGL 和生产部署验证。

## 13. 当前下一入口

**没有实现任务入口。** 应先由独立获授权任务处理当前 CI 的 repository-policy 失败并重新获得当前基线 CI 证据。此后仍须按任务卡单独授权；POC-3DT-01 仍需用户单独授权和许可明确的数据，MVP-10A 继续 Blocked。

## 14. 是否可以进入下一实现任务

**否。** 本地 build/test 通过不等于当前 CI 通过，且当前基线 CI 的后端与前端质量作业未启动；MVP-08 的真实 PostgreSQL/Swagger 验证也尚未完成。DOC-PLAN-01 不解除 POC 或 MVP-10A 门禁。

## 15. 结论

DOC-PLAN-00 的 P0“文档事实互相矛盾”已校准为一套可追溯状态：实现、自动验证、本地构建、CI 配置、最近 CI 和未验证事项分别陈述。当前状态不是“全部通过”：MVP-08 应保持 Partially Completed，当前基线 CI 为失败，下一实现任务与 POC 均不应启动。
