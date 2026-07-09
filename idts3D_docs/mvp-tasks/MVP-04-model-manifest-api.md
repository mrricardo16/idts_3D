# MVP-04：Model Manifest 查询接口

## 1. 任务目标

实现 `GET /api/model-assets/{assetId}/manifest`，让前端后续从后端读取 model manifest，而不是拼本地路径。

## 2. 前置条件

- MVP-03 已完成。
- 至少存在一个 `model_asset`、`asset_version`、`model_asset_variant`。
- 已读取 `api-contracts/model-assets.md`。

## 3. 影响范围

- `idts3D_api/src/HZ.IDTS.DigitalTwin.Api/**`
- `idts3D_api/src/HZ.IDTS.DigitalTwin.Application/**`
- `idts3D_api/src/HZ.IDTS.DigitalTwin.Contracts/**`
- `idts3D_api/src/HZ.IDTS.DigitalTwin.Infrastructure/**`

## 4. 禁止修改范围

- 禁止修改前端源码。
- 禁止实现 object tree 保存。
- 禁止实现版本发布。
- 禁止让 monitor 读取 Draft / Failed / Invalid。

## 5. 后端变更

- Entity：读取 `ModelAsset`, `AssetVersion`, `ModelAssetVariant`, `AssetManifest`, `MovablePartBinding`, `MotionTarget`。
- DbContext：无结构变更。
- Migration：无。
- Controller：`ModelAssetsController.GetManifest`。
- Application Service：`ModelManifestService`。
- Infrastructure Repository / EF 查询：按 assetId/versionId 查询。
- Request DTO：`GetModelManifestRequest`。
- Response DTO：`ModelManifestResponse`。
- 校验规则：monitor 只允许 Published，edit 允许 Draft / Ready / Published。
- 错误码：`ASSET_NOT_FOUND`, `VERSION_NOT_FOUND`, `VERSION_STATUS_INVALID`。

## 6. 前端变更

- TypeScript 类型：无，本任务只后端。
- API Client：无。
- Vue 页面：无。
- Engine 层：无。
- fallback：文档要求保留，代码不改。
- 状态字段：无。
- UI 提示：无。

## 7. 数据库变更

- 新增表：无。
- 修改表：无。
- 读取表：`model_asset`, `asset_version`, `model_asset_variant`, `asset_manifest`, `movable_part_binding`, `motion_target`。
- 写入表：无。
- 约束：使用 version status 规则。
- 索引：使用 MVP-02 已建索引。

## 8. API 契约

完整契约见 `idts3D_docs/api-contracts/model-assets.md#2-get-apimodel-assetsassetidmanifest`。

## 9. 前后端对应关系

| 后端 Entity | DTO | API | 前端 TypeScript interface | 前端调用文件 | Vue / engine 消费位置 |
|---|---|---|---|---|---|
| `ModelAsset`, `AssetVersion`, `ModelAssetVariant`, `AssetManifest` | `GetModelManifestRequest`, `ModelManifestResponse` | `GET /api/model-assets/{assetId}/manifest` | MVP-11 `ModelManifestResponse` | MVP-11 `src/api/modelAssets.ts` | MVP-12 `LODModelLoader.ts`, `TwinScene.ts` |

## 10. 执行步骤

1. 输出影响范围并等待确认。
2. 创建 manifest DTO。
3. 实现查询服务。
4. 后端生成静态文件 URL。
5. 实现 monitor / edit 状态校验。
6. 实现 404 和 409。
7. 在 Swagger 验证 Published 可读。
8. 在 Swagger 验证 Draft monitor 返回 409。
9. 运行 `dotnet build`。

## 11. 验收标准

- Published 版本 monitor 可读。
- Draft 版本 monitor 返回 409。
- edit 可读 Draft / Ready。
- asset/version 不存在返回 404。
- 文件 URL 由后端生成。
- `dotnet build` 通过。
- 不需要 `npm run build`。

## 12. 回归测试

- GLB 加载：后续 MVP-12 验证。
- 对象树：不执行。
- 对象点击：不执行。
- 查看子级 / 父级：不执行。
- 异常高亮：不执行。
- 异常 callout：不执行。
- WASD / 鼠标视角：不执行。
- monitor / edit guard：Swagger 验证 monitor 只能读 Published。
- localStorage fallback：不执行。
- worldZ 任务移动：不执行。
- 后端不可用时 fallback：后续 MVP-12 验证。
- 后端可用时优先走后端：后续 MVP-12 验证。

## 13. 风险点

- 前端若继续拼 URL，会绕过后端契约。
- 状态校验漏掉 Failed / Invalid 会污染 monitor。
- manifest 字段与 TS interface 不一致会影响 MVP-11。

## 14. 回滚策略

删除 manifest Controller、Service、DTO 和相关测试，不改数据库结构。

## 15. Codex 执行提示词

```text
请执行 MVP-04：Model Manifest 查询接口。
先读取 AGENTS.md、idts3D_docs/idts-mvp-task-breakdown.md、idts3D_docs/api-contracts/model-assets.md、idts3D_docs/domain-entity-dto-map.md 和本任务卡。
先输出影响范围，等待我确认后再改。
只实现 GET /api/model-assets/{assetId}/manifest，不修改前端源码，不实现 object-tree、发布或可动部件接口。
完成后运行 dotnet build，并用 Swagger 验证 200、404、409。
不要 commit，不要 push。
```
