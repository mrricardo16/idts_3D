# MVP-10：Scene Manifest

## 1. 任务目标

实现 `GET /api/scenes/{sceneId}/manifest`，让前端按场景加载设备和 model manifest。

## 2. 前置条件

- MVP-09 已完成。
- 存在 active device model binding。
- active binding 指向 Published asset version。
- 已读取 `api-contracts/scenes.md`。

## 3. 影响范围

- `idts3D_api/src/HZ.IDTS.DigitalTwin.Api/**`
- `idts3D_api/src/HZ.IDTS.DigitalTwin.Application/**`
- `idts3D_api/src/HZ.IDTS.DigitalTwin.Contracts/**`
- `idts3D_api/src/HZ.IDTS.DigitalTwin.Infrastructure/**`

## 4. 禁止修改范围

- 禁止修改前端源码。
- 禁止做 3D Tiles 生产化。
- 禁止返回非 Published 版本给 monitor。
- 禁止实现完整场景编辑后台。

## 5. 后端变更

- Entity：`SceneNode`, `DeviceInstance`, `DeviceModelBinding`, `ModelAsset`, `AssetVersion`, `AssetManifest`。
- DbContext：无结构变更。
- Migration：无。
- Controller：`ScenesController.GetManifest`。
- Application Service：`SceneManifestService`。
- Infrastructure Repository / EF 查询：查询 scene、device、active binding。
- Request DTO：`GetSceneManifestRequest`。
- Response DTO：`SceneManifestResponse`, `SceneDeviceManifestResponse`。
- 校验规则：monitor 只返回 active + Published。
- 错误码：`NOT_FOUND`, `VERSION_STATUS_INVALID`。

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
- 读取表：`scene_node`, `device_instance`, `device_model_binding`, `model_asset`, `asset_version`, `asset_manifest`。
- 写入表：无。
- 约束：只读取 active binding。
- 索引：使用 scene、device、binding 索引。

## 8. API 契约

完整契约见 `idts3D_docs/api-contracts/scenes.md`。

## 9. 前后端对应关系

| 后端 Entity | DTO | API | 前端 TypeScript interface | 前端调用文件 | Vue / engine 消费位置 |
|---|---|---|---|---|---|
| `SceneNode`, `DeviceInstance`, `DeviceModelBinding` | `GetSceneManifestRequest`, `SceneManifestResponse` | `GET /api/scenes/{sceneId}/manifest` | MVP-11 `SceneManifestResponse` | MVP-11 `src/api/scenes.ts` | MVP-12 `TwinDemo.vue`, `TwinScene.ts` |

## 10. 执行步骤

1. 输出影响范围并等待确认。
2. 创建 scene manifest DTO。
3. 查询 scene node。
4. 查询 enabled device instances。
5. 查询 active device model bindings。
6. 校验绑定版本 Published。
7. 生成 model manifest URL。
8. `tilesets` 返回空数组。
9. Swagger 验证 200 / 404 / 409。
10. 运行 `dotnet build`。

## 11. 验收标准

- GET scene manifest 返回设备列表。
- 只返回 active binding。
- 只返回 Published asset version。
- manifestUrl 由后端生成。
- `tilesets` MVP 返回空数组。
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
- monitor / edit guard：Swagger 验证 monitor 只拿 Published。
- localStorage fallback：不执行。
- worldZ 任务移动：不执行。
- 后端不可用时 fallback：后续 MVP-12 验证。
- 后端可用时优先走后端：后续 MVP-12 验证。

## 13. 风险点

- 场景 manifest 和 model manifest 字段重复。
- 无效绑定处理策略需要固定为 409 或跳过并 warning。
- 前端后续如果直接请求 model manifest 会绕过 scene 设备 transform。

## 14. 回滚策略

删除 scene manifest Controller、Service、DTO，不改数据库结构和 seed 数据。

## 15. Codex 执行提示词

```text
请执行 MVP-10：Scene Manifest。
先读取 AGENTS.md、idts3D_docs/idts-mvp-task-breakdown.md、idts3D_docs/api-contracts/scenes.md、idts3D_docs/domain-entity-dto-map.md 和本任务卡。
先输出影响范围，等待我确认后再改。
只实现 GET /api/scenes/{sceneId}/manifest，不修改前端源码，不做 3D Tiles 生产化。
完成后运行 dotnet build，并用 Swagger 验证 200、404、409。
不要 commit，不要 push。
```
