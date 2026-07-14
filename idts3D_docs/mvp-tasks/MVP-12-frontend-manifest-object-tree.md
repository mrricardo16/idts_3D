# MVP-12：前端接后端 Manifest / Object-tree

## 1. 任务目标

让前端优先从后端读取 scene manifest、model manifest 和 object tree，同时保留当前本地 fallback。

## 2. 前置条件

- MVP-10 已完成。
- MVP-11 API Client 与类型已完成。
- 当前 GLB / fallback 页面可正常运行。

## 3. 影响范围

- `idts3D_ui/src/views/TwinDemo.vue`
- `idts3D_ui/src/engine/TwinScene.ts`
- `idts3D_ui/src/engine/LODModelLoader.ts`
- `idts3D_ui/src/api/**`
- `idts3D_ui/src/types/**`

## 4. 禁止修改范围

- 禁止保存 edit 配置。
- 禁止修改 worldZ 动画策略。
- 禁止移除本地 fallback。
- 禁止修改 `public/models/lifter.glb`。
- 禁止实现 motion target 驱动动画。

## 5. 后端变更

- Entity：无。
- DbContext：无。
- Migration：无。
- Controller：无。
- Application Service：无。
- Infrastructure Repository / EF 查询：无。
- Request DTO：无。
- Response DTO：无。
- 校验规则：无。
- 错误码：无。

## 6. 前端变更

- TypeScript 类型：使用 MVP-11 类型。
- API Client：调用 `getSceneManifest`, `getModelManifest`, `getObjectTree`。
- Vue 页面：增加后端优先加载状态和 fallback 提示。
- Engine 层：让 `TwinScene` 可消费后端 scene/model manifest 和 object tree。
- fallback：后端 404 / 409 / 网络错误回到本地静态配置、localStorage、GLB、几何体。
- 状态字段：增加数据来源状态，例如 `backend`, `localStorage`, `static`, `fallback`。
- UI 提示：显示后端加载失败和 fallback 来源。

## 7. 数据库变更

- 新增表：无。
- 修改表：无。
- 读取表：通过 API 间接读取 `scene_node`, `device_instance`, `device_model_binding`, `model_asset`, `asset_version`, `model_object_index`。
- 写入表：无。
- 约束：无。
- 索引：无。

## 8. API 契约

引用 `api-contracts/scenes.md`、`api-contracts/model-assets.md`、`api-contracts/object-tree-model-stats.md`。

## 9. 前后端对应关系

| 后端 Entity | DTO | API | 前端 TypeScript interface | 前端调用文件 | Vue / engine 消费位置 |
|---|---|---|---|---|---|
| `SceneNode`, `DeviceInstance`, `DeviceModelBinding` | `SceneManifestResponse` | `GET /api/scenes/{sceneId}/manifest` | `SceneManifestResponse` | `src/api/scenes.ts` | `TwinDemo.vue`, `TwinScene.ts` |
| `ModelAsset`, `AssetVersion`, `AssetManifest` | `ModelManifestResponse` | `GET /api/model-assets/{assetId}/manifest` | `ModelManifestResponse` | `src/api/modelAssets.ts` | `LODModelLoader.ts` |
| `ModelObjectIndex` | `ObjectTreeResponse` | `GET /api/model-assets/{assetId}/object-tree` | `ObjectTreeResponse` | `src/api/objectTree.ts` | 对象树 UI |

## 10. 执行步骤

1. 输出影响范围并等待确认。
2. 扫描当前 `TwinDemo.vue` 加载入口。
3. 扫描 `TwinScene` 模型安装和对象树刷新逻辑。
4. 在页面启动时调用 scene manifest。
5. 根据 scene manifest 加载 model manifest。
6. 将 model manifest levels 传入 `LODModelLoader`。
7. 调用 object tree 接口。
8. 后端 object tree 可用时优先展示。
9. 后端失败时保留现有 fallback。
10. 验证对象点击和对象树。
11. 运行 `npm run build`。

## 11. 验收标准

- 后端可用时优先请求 scene manifest。
- 后端可用时 GLB URL 来自 model manifest。
- 后端 object tree 可显示。
- 后端 404 / 409 / 网络错误时 fallback。
- 当前 GLB 加载、对象点击、对象树、异常、worldZ 不破坏。
- `npm run build` 通过。
- 不需要 `dotnet build`，除非本任务同时发现后端契约错误并经用户确认修复。

## 12. 回归测试

- GLB 加载：后端可用和不可用各测一次。
- 对象树：后端 object tree 和 runtime tree fallback 各测一次。
- 对象点击：点击模型和对象树节点。
- 查看子级 / 父级：验证按钮行为。
- 异常高亮：启用异常模拟。
- 异常 callout：点击异常对象。
- WASD / 鼠标视角：验证移动和旋转。
- monitor / edit guard：确认模式切换仍有效。
- localStorage fallback：保留本地配置。
- worldZ 任务移动：使用现有 F1/F2/F3/F4。
- 后端不可用时 fallback：关闭后端验证。
- 后端可用时优先走后端：网络面板确认。

## 13. 风险点

- 后端 objectUuid 与运行时 uuid 不一致。
- 加载顺序改变可能破坏当前 fallback。
- 后端 transform 与前端本地 transform 重复叠加。

## 14. 回滚策略

撤回本任务修改，恢复前端只走本地 manifest / localStorage / GLB fallback 的加载方式，保留 MVP-11 API Client。

## 15. Codex 执行提示词

```text
请执行 MVP-12：前端接后端 Manifest / Object-tree。
先读取 AGENTS.md、idts3D_docs/idts-mvp-task-breakdown.md、idts3D_docs/frontend-integration-plan.md、api-contracts/scenes.md、api-contracts/model-assets.md、api-contracts/object-tree-model-stats.md 和本任务卡。
先扫描当前 TwinDemo.vue、TwinScene.ts、LODModelLoader.ts，输出影响范围，等待我确认后再改。
只接 scene/model manifest 和 object tree，不保存 edit 配置，不改 worldZ 策略，不移除 fallback。
完成后运行 npm run build，并回归 GLB、对象树、点击、异常、WASD、fallback。
不要 commit，不要 push。
```
# DOC-3DT-02 对齐说明

MVP-12 的正式混合场景加载依赖 MVP-10A。加载链路必须先按 Scene Manifest 区分 baseLayers 与 devices，再分层加载 3D Tiles 底座和 GLB 设备；不得把 Tiles 节点树强行加入现有设备 Object Tree。Object Tree 继续主要服务 GLB 设备，底座加载失败时必须保留可识别的 GLB fallback。
