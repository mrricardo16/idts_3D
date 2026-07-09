# MVP-05：Object-tree / Model-stats

## 1. 任务目标

实现 object tree 与 model stats 的保存和查询能力，为发布门禁、对象树 UI 和可动部件绑定提供数据基础。

## 2. 前置条件

- MVP-02 已完成。
- MVP-03 已有 asset/version 数据。
- 已读取 `api-contracts/object-tree-model-stats.md`。

## 3. 影响范围

- `idts3D_api/src/HZ.IDTS.DigitalTwin.Api/**`
- `idts3D_api/src/HZ.IDTS.DigitalTwin.Application/**`
- `idts3D_api/src/HZ.IDTS.DigitalTwin.Contracts/**`
- `idts3D_api/src/HZ.IDTS.DigitalTwin.Infrastructure/**`

## 4. 禁止修改范围

- 禁止要求 Worker 自动解析 GLB。
- 禁止修改前端源码。
- 禁止实现发布状态流转。
- 禁止实现 movable part。

## 5. 后端变更

- Entity：`ModelObjectIndex`, `AssetManifest`, `ModelAsset`, `AssetVersion`。
- DbContext：无结构变更。
- Migration：无，缺字段则停止并回报。
- Controller：新增 object tree / model stats endpoints。
- Application Service：保存、查询、覆盖策略。
- Infrastructure Repository / EF 查询：批量写入 object tree，更新 stats json。
- Request DTO：`SaveObjectTreeRequest`, `SaveModelStatsRequest`。
- Response DTO：`ObjectTreeResponse`, `ModelStatsResponse`。
- 校验规则：nodes 非空、version 存在、Published 不可直接覆盖。
- 错误码：`VALIDATION_FAILED`, `VERSION_NOT_FOUND`, `VERSION_STATUS_INVALID`。

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
- 读取表：`model_asset`, `asset_version`, `model_object_index`, `asset_manifest`。
- 写入表：`model_object_index`, `asset_manifest`, `operation_audit`。
- 约束：object tree 以 `asset_version_id` 为边界。
- 索引：`asset_version_id + object_uuid`, `asset_version_id + object_path`。

## 8. API 契约

完整契约见 `idts3D_docs/api-contracts/object-tree-model-stats.md`。

## 9. 前后端对应关系

| 后端 Entity | DTO | API | 前端 TypeScript interface | 前端调用文件 | Vue / engine 消费位置 |
|---|---|---|---|---|---|
| `ModelObjectIndex` | `SaveObjectTreeRequest`, `ObjectTreeResponse` | `PUT/GET object-tree` | MVP-11 `ModelObjectNodeDto` | MVP-11 `src/api/objectTree.ts` | MVP-12 对象树 UI |
| `AssetManifest` | `SaveModelStatsRequest`, `ModelStatsResponse` | `PUT/GET model-stats` | MVP-11 `ModelStatsDto` | MVP-11 `src/api/objectTree.ts` | 性能面板 / 发布门禁 |

## 10. 执行步骤

1. 输出影响范围并等待确认。
2. 创建 object tree DTO。
3. 创建 model stats DTO。
4. 实现保存 object tree：先清同版本旧节点，再批量写入。
5. 实现查询 object tree。
6. 实现保存 model stats 到 `asset_manifest.model_stats_json`。
7. 实现查询 model stats。
8. 实现 Published 版本覆盖 guard。
9. Swagger 验证 200 / 400 / 404 / 409。
10. 运行 `dotnet build`。

## 11. 验收标准

- 能保存 object tree。
- 能查询 object tree。
- 能保存 model stats。
- 能查询 model stats。
- Published 版本直接覆盖返回 409。
- `dotnet build` 通过。
- 不需要 `npm run build`。

## 12. 回归测试

- GLB 加载：后续 MVP-12 验证。
- 对象树：Swagger 验证数据可保存/查询，页面后续验证。
- 对象点击：后续 MVP-12 验证。
- 查看子级 / 父级：后续 MVP-12 验证。
- 异常高亮：不执行。
- 异常 callout：不执行。
- WASD / 鼠标视角：不执行。
- monitor / edit guard：验证 Published 不可覆盖。
- localStorage fallback：不执行。
- worldZ 任务移动：不执行。
- 后端不可用时 fallback：后续 MVP-12 验证。
- 后端可用时优先走后端：后续 MVP-12 验证。

## 13. 风险点

- 大型 object tree 批量写入性能。
- object uuid 可能不稳定，需要保留 object path。
- model stats JSON 与发布门禁字段不一致。

## 14. 回滚策略

删除新增 endpoints、DTO、Service；删除测试写入的 object tree 和 stats 数据。

## 15. Codex 执行提示词

```text
请执行 MVP-05：Object-tree / Model-stats。
先读取 AGENTS.md、idts3D_docs/idts-mvp-task-breakdown.md、idts3D_docs/api-contracts/object-tree-model-stats.md、idts3D_docs/domain-entity-dto-map.md 和本任务卡。
先输出影响范围，等待我确认后再改。
只实现 object-tree 和 model-stats 保存/查询，不修改前端源码，不实现发布或可动部件。
完成后运行 dotnet build，并用 Swagger 验证接口。
不要 commit，不要 push。
```
