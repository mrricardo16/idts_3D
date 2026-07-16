# DOC-PLAN-09：MVP-10A 计划任务书 P0 / P1 修复报告

## 1. 修复结论

DOC-PLAN-08 的 P0 与四项 P1 已在任务书层面修复。修订后不再将未检出的 Scene Manifest 链路称为现有实现，冻结产物、实际 npm scripts、测试/手工/性能入口、故障回退矩阵和三态放行规则均已写入对应任务卡。结论：**Ready for user review**，仍不构成 POC 或实现授权。

## 2. 仓库基线

`main` / `e0b4180b0833f6fdad336bdcfb597870430a8b41`。

## 3. 工作区排除项

- `.gitignore`：只读核验为 `tools/micro-tileset` 的输出、Python 缓存和 Blender 备份忽略规则；未修改、暂存或提交。
- `tools/`：未读取、扫描、修改、暂存或提交。
- DOC-PLAN-08 报告：未修改。

## 4. P0 修复

MVP-10A-04 增加当前事实表：当前存在 `ModelAssetsController.GetManifest`、`IModelManifestService`、`ModelManifestService`、`ModelManifestResponse`；未检出 `ScenesController`、`SceneManifestService`、`SceneManifestResponse` 和前端 `src/api/scenes.ts`。这些 Scene Manifest 项均改为“MVP-10A-01 冻结后、MVP-10A-04 计划新增”，不再写成现有文件。

## 5. 10A-01 修复

固定冻结交付写入 `scene-resource-manifest-design.md`、`api-contracts/scenes.md`、`domain-entity-dto-map.md`、ADR-001，并新增实施期冻结报告 `reviews/MVP-10A-01-contract-freeze-report.md`。任务卡给出包含 JSON、字典、DTO、TS、端点、数据/Migration、兼容、错误、回滚、批准和解锁结论的 18 项模板。

## 6. 10A-02 修复

从实际 `package.json` 固定 `npm ci`、`npm run lint`、`npm run type-check`、`npm run test:unit`、`npm run build`。增加默认 TwinDemo 手工入口、十次生命周期回归、通过条件、失败停止规则，以及候选 TwinScene Vitest 测试路径。ResourceManager 明确为计划新增，非现有文件。

## 7. 10A-03 修复

固定实际 npm 验证入口；补充 CoordinateTransformer、TilesLayer 生命周期、InteractionManager 隔离的候选测试路径；指定默认 TwinDemo 手工入口、POC 冻结样本、三点/worldZ/错误注入和证据方式。明确 10A-03 只记录接入基线，完整性能/生命周期放行归 10A-05。

## 8. 10A-05 修复

新增十类故障的注入、检测、用户状态、即时处理、回退、恢复与结论矩阵，并新增功能、GLB、坐标、性能、生命周期、fallback、API/数据库和证据完整度的 Pass/Conditional Pass/Fail 判定表及 MVP-11～16 解锁规则。

## 9. 实际后端链路

| 能力 | 实际路径 / 符号 | 状态 |
|---|---|---|
| Model Manifest API | `idts3D_api/src/HZ.IDTS.DigitalTwin.Api/Controllers/ModelAssetsController.cs` / `ModelAssetsController.GetManifest` | 现有 |
| Model Manifest Application | `...Application/ModelAssets/IModelManifestService.cs`、`ModelManifestService.cs` | 现有 |
| Model Manifest DTO | `...Contracts/ModelAssets/ModelManifestResponse.cs` | 现有 |
| Scene Manifest API/Service/DTO | 未检出 | 10A-01 冻结后由 10A-04 计划新增 |

## 10. 现有 / 计划新增文件表

| 范围 | 现有 | 计划新增 / 冻结后确定 |
|---|---|---|
| GLB Manifest | ModelAssetsController、ModelManifestService、ModelManifestResponse | 不适用 |
| Scene Manifest 后端 | SceneNode/DeviceInstance/DeviceModelBinding Entity | Controller/Service/DTO/查询，名称由 10A-01 冻结 |
| 前端 | TwinDemo、TwinScene、现有 engine 模块 | scene API Client、Scene Manifest TS、TilesLayer/CoordinateTransformer/ResourceManager 及测试候选 |

## 11. 同步修改

- `backend-implementation-plan.md`：移除将 Scene Manifest Controller/Service/Query 写成现有实现的表述。
- `domain-entity-dto-map.md`：将 scene DTO/API Client 由现有事实改为 10A-01 冻结/10A-04 计划项。
- `api-contracts/scenes.md`：标注为计划规范，说明当前代码未检出 Scene Manifest 实现。
- `frontend-integration-plan.md`：将 `src/api/scenes.ts` 等明确为 MVP-11/10A-04 计划新增。
- `e2e-acceptance-plan.md` 未包含上述虚构符号，未修改。

## 12. 剩余 TBD

库/版本/许可样本由 POC 决定；坐标/容差由 POC 与现场资料决定；Scene Manifest 策略、字段、表、Migration、API 版本和旧 `tilesets` 兼容由 10A-01 冻结；真实性能/生命周期结果由 10A-05 产生。这些 TBD 均有责任任务和停止条件。

## 13. 用户待确认

批准 POC 后，用户或项目负责人须批准 10A-01 冻结报告中的正式实现策略（扩展 Model Manifest、独立 Scene Manifest 或有真实证据的既有链路）、字段、数据来源和解锁结论。

## 14. UTF-8

本次变更 Markdown 已以 UTF-8 严格解码，0 失败。

## 15. 相对链接

本次变更 Markdown 相对链接检查，0 失败。

## 16. git diff --check

`git diff --check` 已通过。

## 17. 是否具备再次审查条件

是。待 UTF-8、相对链接、diff 范围与 `git diff --check` 通过后，可重新执行 DOC-PLAN-08 的实质性审查；DOC-PLAN-09 不自动给出 Approved。
