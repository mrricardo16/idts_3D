# MVP-07：可动部件配置 API

## 1. 任务目标

实现 movable part CRUD，把“设为可动部件”从前端临时状态升级为后端配置。

## 2. 前置条件

- MVP-05 object tree 可用。
- MVP-06 版本状态规则可用。
- 已读取 `api-contracts/movable-parts.md`。

## 3. 影响范围

- `idts3D_api/src/HZ.IDTS.DigitalTwin.Api/**`
- `idts3D_api/src/HZ.IDTS.DigitalTwin.Application/**`
- `idts3D_api/src/HZ.IDTS.DigitalTwin.Contracts/**`
- `idts3D_api/src/HZ.IDTS.DigitalTwin.Infrastructure/**`

## 4. 禁止修改范围

- 禁止实现 motion target。
- 禁止修改前端源码。
- 禁止允许 Published 版本直接修改。
- 禁止支持 local axis / rotate / path。

## 5. 后端变更

- Entity：`MovablePartBinding`, `ModelObjectIndex`, `AssetVersion`, `OperationAudit`。
- DbContext：无结构变更。
- Migration：无。
- Controller：新增 movable parts endpoints。
- Application Service：CRUD、校验、状态 guard。
- Infrastructure Repository / EF 查询：按 version 和 partId 查询。
- Request DTO：`CreateMovablePartRequest`, `UpdateMovablePartRequest`。
- Response DTO：`MovablePartResponse`, `MovablePartListResponse`。
- 校验规则：partCode 唯一、对象存在、min/home/max 合法、Published 不可修改。
- 错误码：`DUPLICATE_PART_CODE`, `VERSION_STATUS_INVALID`, `VALIDATION_FAILED`。

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
- 读取表：`asset_version`, `model_object_index`, `movable_part_binding`。
- 写入表：`movable_part_binding`, `operation_audit`。
- 约束：`asset_version_id + part_code` 唯一。
- 索引：`asset_version_id + object_uuid`, `asset_version_id + object_path`。

## 8. API 契约

完整契约见 `idts3D_docs/api-contracts/movable-parts.md`。

## 9. 前后端对应关系

| 后端 Entity | DTO | API | 前端 TypeScript interface | 前端调用文件 | Vue / engine 消费位置 |
|---|---|---|---|---|---|
| `MovablePartBinding` | `CreateMovablePartRequest`, `UpdateMovablePartRequest`, `MovablePartResponse` | movable part CRUD | MVP-11 `MovablePartDto` | MVP-11 `src/api/movableParts.ts` | MVP-13 edit 保存，MVP-14 monitor 只读 |

## 10. 执行步骤

1. 输出影响范围并等待确认。
2. 创建 movable part DTO。
3. 实现 GET list。
4. 实现 POST create。
5. 校验 objectUuid / objectPath 存在于 object tree。
6. 校验 partCode 唯一。
7. 校验 min/home/max。
8. 实现 PUT update。
9. 实现 DELETE disable 或删除策略。
10. 实现 Published guard。
11. 写入审计。
12. 运行 `dotnet build`。

## 11. 验收标准

- GET 返回配置列表。
- POST 可新增 movable part。
- 重复 partCode 返回 409。
- object 不存在返回 404 或 400。
- 范围错误返回 400。
- Published 版本修改返回 409。
- `dotnet build` 通过。
- 不需要 `npm run build`。

## 12. 回归测试

- GLB 加载：后续 MVP-12 验证。
- 对象树：Swagger 使用 object tree 校验对象存在。
- 对象点击：后续 MVP-13 验证。
- 查看子级 / 父级：后续 MVP-12 验证。
- 异常高亮：不执行。
- 异常 callout：不执行。
- WASD / 鼠标视角：不执行。
- monitor / edit guard：验证 Published 不能修改。
- localStorage fallback：不执行。
- worldZ 任务移动：后续 MVP-14 验证。
- 后端不可用时 fallback：后续 MVP-12 验证。
- 后端可用时优先走后端：后续 MVP-13 验证。

## 13. 风险点

- objectUuid 可能变化，必须保留 objectPath。
- 前端运行态选择对象和后端 object tree 不一致。
- Published 版本误改会影响 monitor。

## 14. 回滚策略

删除 movable part endpoints、DTO、Service；清理测试 `movable_part_binding` 数据。

## 15. Codex 执行提示词

```text
请执行 MVP-07：可动部件配置 API。
先读取 AGENTS.md、idts3D_docs/idts-mvp-task-breakdown.md、idts3D_docs/api-contracts/movable-parts.md、idts3D_docs/domain-entity-dto-map.md 和本任务卡。
先输出影响范围，等待我确认后再改。
只实现 movable part CRUD，不实现 motion target，不修改前端源码，不允许 Published 版本直接修改。
完成后运行 dotnet build，并用 Swagger 验证 200、400、404、409。
不要 commit，不要 push。
```
