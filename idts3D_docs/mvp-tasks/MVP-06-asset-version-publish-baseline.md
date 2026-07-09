# MVP-06：资产版本状态与发布基线

## 1. 任务目标

实现 Draft / Ready / Published / Archived / Failed / Invalid 状态流转，以及 mark-ready、publish、archive、rollback 接口。

## 2. 前置条件

- MVP-04 和 MVP-05 已完成。
- manifest、object tree、model stats 可用。
- 已读取 `api-contracts/asset-version.md`。

## 3. 影响范围

- `idts3D_api/src/HZ.IDTS.DigitalTwin.Api/**`
- `idts3D_api/src/HZ.IDTS.DigitalTwin.Application/**`
- `idts3D_api/src/HZ.IDTS.DigitalTwin.Contracts/**`
- `idts3D_api/src/HZ.IDTS.DigitalTwin.Infrastructure/**`

## 4. 禁止修改范围

- 禁止修改前端源码。
- 禁止实现场景管理页面。
- 禁止跳过发布前校验。
- 禁止让 Failed / Invalid 发布。

## 5. 后端变更

- Entity：`ModelAsset`, `AssetVersion`, `AssetManifest`, `ModelObjectIndex`, `MovablePartBinding`, `MotionTarget`, `DeviceModelBinding`, `OperationAudit`。
- DbContext：无结构变更。
- Migration：无。
- Controller：新增版本状态接口。
- Application Service：状态流转和发布门禁。
- Infrastructure Repository / EF 查询：查询版本、归档旧 Published、更新 currentVersion。
- Request DTO：`ChangeVersionStatusRequest`。
- Response DTO：`AssetVersionResponse`。
- 校验规则：Ready 前检查 manifest/object tree/model stats；Published 前检查绑定和状态。
- 错误码：`OBJECT_TREE_REQUIRED`, `MODEL_STATS_REQUIRED`, `VERSION_STATUS_INVALID`。

## 6. 前端变更

- TypeScript 类型：无。
- API Client：无。
- Vue 页面：无。
- Engine 层：无。
- fallback：无。
- 状态字段：无。
- UI 提示：无。

## 7. 数据库变更

- 新增表：无。
- 修改表：无。
- 读取表：`model_asset`, `asset_version`, `asset_manifest`, `model_object_index`, `movable_part_binding`, `motion_target`, `device_model_binding`。
- 写入表：`asset_version`, `model_asset`, `device_model_binding`, `operation_audit`。
- 约束：同一 model asset 只能有一个 Published。
- 索引：使用版本状态索引。

## 8. API 契约

完整契约见 `idts3D_docs/api-contracts/asset-version.md`。

## 9. 前后端对应关系

| 后端 Entity | DTO | API | 前端 TypeScript interface | 前端调用文件 | Vue / engine 消费位置 |
|---|---|---|---|---|---|
| `AssetVersion`, `ModelAsset` | `ChangeVersionStatusRequest`, `AssetVersionResponse` | mark-ready / publish / archive / rollback | MVP-11 `AssetVersionDto` | MVP-11 `src/api/assetVersions.ts` | 后续管理 UI，monitor 读取 Published |

## 10. 执行步骤

1. 输出影响范围并等待确认。
2. 定义状态流转矩阵。
3. 创建状态变更 DTO。
4. 实现 mark-ready 校验。
5. 实现 publish 校验。
6. 归档旧 Published。
7. 更新 `model_asset.current_version_id`。
8. 实现 archive。
9. 实现 rollback。
10. 写入 `operation_audit`。
11. Swagger 验证 200 / 400 / 404 / 409。
12. 运行 `dotnet build`。

## 11. 验收标准

- Draft 可 mark-ready。
- Ready 可 publish。
- Failed / Invalid 不能 publish。
- 同一 model asset 只有一个 Published。
- 旧 Published 自动 Archived。
- rollback 后仍只有一个 Published。
- `dotnet build` 通过。
- 不需要 `npm run build`。

## 12. 回归测试

- GLB 加载：后续 MVP-12 验证。
- 对象树：发布前校验 object tree 存在。
- 对象点击：不执行。
- 查看子级 / 父级：不执行。
- 异常高亮：不执行。
- 异常 callout：不执行。
- WASD / 鼠标视角：不执行。
- monitor / edit guard：验证 monitor 只能读取 Published 的前置状态。
- localStorage fallback：不执行。
- worldZ 任务移动：不执行。
- 后端不可用时 fallback：后续 MVP-12 验证。
- 后端可用时优先走后端：后续 MVP-12 验证。

## 13. 风险点

- 发布和回滚需要事务。
- active binding 约束在 MVP-09 前可能只能预留服务入口。
- 发布门禁字段缺失会导致误发布。

## 14. 回滚策略

删除状态接口、Service、DTO；将测试版本状态恢复为 Draft 或清理测试数据。

## 15. Codex 执行提示词

```text
请执行 MVP-06：资产版本状态与发布基线。
先读取 AGENTS.md、idts3D_docs/idts-mvp-task-breakdown.md、idts3D_docs/api-contracts/asset-version.md、idts3D_docs/domain-entity-dto-map.md 和本任务卡。
先输出影响范围，等待我确认后再改。
只实现版本状态流转，不修改前端源码，不实现场景或可动部件 UI。
完成后运行 dotnet build，并用 Swagger 验证 mark-ready、publish、archive、rollback。
不要 commit，不要 push。
```
