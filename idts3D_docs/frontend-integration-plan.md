# 前端集成计划

本文档基于当前 `idts3D_ui` 实际代码编写，不描述尚不存在的能力为已完成能力。

## 1. 当前前端事实

| 项 | 当前事实 |
|---|---|
| 技术栈 | Vue 3 + Vite + TypeScript + Three.js |
| build 命令 | `vue-tsc -b && vite build` |
| 页面入口 | `idts3D_ui/src/views/TwinDemo.vue` |
| 三维核心 | `idts3D_ui/src/engine/TwinScene.ts` |
| 模型加载 | `idts3D_ui/src/engine/LODModelLoader.ts`, `ModelManifestLoader.ts` |
| 类型集中位置 | `idts3D_ui/src/types/twin.ts`, `modelConfig.ts`, `modelManifest.ts` |
| 当前配置 | localStorage + `/model-configs/lifter.json` |
| 当前模式 | 已有 `monitor` / `edit` 模式 |
| 当前目标点 | `lifterTargetPositions` 中 hard-coded `F1/F2/F3/F4` |
| 当前动画 | `TwinScene.dispatchLifterTask` 使用 worldZ 移动 |
| 当前对象树 | `TwinScene.refreshModelTree()` 从当前加载模型收集 object tree |
| 当前 fallback | 后端不存在时仍可加载本地 GLB 或几何体场景 |
| 当前缺口 | 没有正式 `src/api` API Client 层 |

## 2. 新增 `src/api` 目录

MVP-11 新增：

```text
idts3D_ui/src/api/
  httpClient.ts
  apiErrors.ts
  modelAssets.ts
  assetVersions.ts
  objectTree.ts
  movableParts.ts
  motionTargets.ts
  scenes.ts
  conversionJobs.ts
```

职责：

- `httpClient.ts`：封装 `fetch`、baseUrl、JSON 解析、超时、统一响应 unwrap。
- `apiErrors.ts`：把 400 / 404 / 409 / network error 转换成前端可识别错误。
- 其他文件按领域暴露函数，不在页面中拼 URL。

## 3. TypeScript DTO 位置

新增或调整：

```text
idts3D_ui/src/types/api.ts
idts3D_ui/src/types/modelAsset.ts
idts3D_ui/src/types/modelObject.ts
idts3D_ui/src/types/motion.ts
idts3D_ui/src/types/sceneManifest.ts
idts3D_ui/src/types/conversionJob.ts
```

规则：

1. 后端 Request DTO 对应前端 Request interface。
2. 后端 Response DTO 对应前端 Response interface。
3. 页面不临时定义后端字段。
4. 现有 `twin.ts` 继续保存运行态 UI / engine 类型；API DTO 不应全部塞进 `twin.ts`。

## 4. API Client 封装方式

示例职责：

```ts
// src/api/modelAssets.ts
export async function getModelManifest(
  assetId: number,
  query: ModelManifestQuery,
): Promise<ModelManifestResponse>
```

调用链：

```text
TwinDemo.vue
-> src/api/scenes.ts:getSceneManifest()
-> src/api/modelAssets.ts:getModelManifest()
-> src/api/objectTree.ts:getObjectTree()
-> 转换为 TwinScene 可消费的数据
-> TwinScene / LODModelLoader
```

## 5. fallback 策略

后端不可用时：

1. API Client 返回 `ApiNetworkError`，不在底层直接吞掉。
2. `TwinDemo.vue` 显示“后端不可用，已使用本地 fallback”。
3. 模型加载回到当前顺序：后端 scene manifest 失败 -> 本地静态 JSON -> `public/models/lifter.glb` -> 几何体 fallback。
4. localStorage 只作为编辑草稿或本地调试配置，不代表 Published。

后端返回 404：

- model manifest / object tree 不存在时进入 fallback。
- UI 提示“后端无配置，使用本地配置”。

后端返回 409：

- `VERSION_STATUS_INVALID` 时不使用该版本。
- monitor 模式提示“当前版本不是 Published”。
- edit 模式可提示切换到 Draft / Ready。

网络错误：

- 保持当前 GLB、对象树、异常、worldZ 功能可用。
- 不清空当前已加载模型。

## 6. scene manifest 进入 TwinScene

MVP-12 执行路径：

1. `getSceneManifest(sceneId, { mode })`。
2. 读取 `devices[]`。
3. 对每个 device 调用 `manifestUrl` 或 `getModelManifest(assetId, versionId)`。
4. 将 device transform 和 model manifest 合并为 engine 输入。
5. `TwinScene` 新增或扩展加载入口，例如 `loadSceneManifest(sceneManifest)`。
6. 如果任一关键设备 manifest 失败，本设备 fallback，不阻断整个页面。

## 7. model manifest 进入 LODModelLoader

`LODModelLoader` 当前已处理 levels / fallback 概念，后续应：

1. 接收后端 `ModelManifestResponse.levels`。
2. 优先加载 `source`，后续再扩展 high / medium / low / proxy。
3. 不在前端拼接静态文件路径。
4. 加载失败时继续使用当前 fallback 逻辑。
5. 加载成功后保留当前性能统计、object tree 收集、材质处理。

## 8. object tree 进入现有对象树 UI

当前对象树来自 `TwinScene.refreshModelTree()` 对已加载模型的收集。

MVP-12 策略：

1. 后端 object tree 可用时，优先展示后端 object tree。
2. 仍保留从 Three.js 对象现场收集的 runtime tree。
3. 点击对象时用 `objectUuid` 或 `objectPath` 反查 scene object。
4. 后端 object tree 与 runtime tree 不一致时，UI 提示并 fallback 到 runtime tree。
5. 查看子级 / 父级、对象点击、异常高亮、callout 不得被破坏。

## 9. edit 模式保存 movable part 与 motion target

MVP-13 中：

1. `setSelectedAsMovable()` 只负责选择对象和预览。
2. 保存按钮调用 `createMovablePart()` 或 `updateMovablePart()`。
3. 目标点位调用 `createMotionTarget()` / `updateMotionTarget()`。
4. 保存成功后刷新 movable part 和 motion target 列表。
5. 保存失败按 400 / 404 / 409 显示字段级提示。
6. 后端不可用时允许本地预览，但必须提示“未保存到后端”。

## 10. monitor 模式只读 Published 配置

MVP-14 中：

1. monitor 模式调用 `getSceneManifest(...mode=monitor)`。
2. model manifest 必须是 Published。
3. movable part 和 motion target 只读。
4. UI 不显示保存入口。
5. 点击目标点位后调用当前 worldZ 动画。
6. 只支持 `linear + world + z`。
7. 不支持 local axis、rotate、path、AGV path、joint。

## 11. 替换 hard-coded F1/F2/F3/F4

分三步：

1. MVP-11：定义 `MotionTargetDto`、API Client，不动现有 `lifterTargetPositions`。
2. MVP-13：edit 模式保存 F1/F2/F3/F4 到后端。
3. MVP-14：monitor 模式优先使用后端 motion target；后端不可用时再使用 `lifterTargetPositions` fallback。

禁止一次性删除 `lifterTargetPositions`，避免破坏当前演示。

## 12. 不破坏当前能力的回归点

每个前端 MVP 必须回归：

- GLB 加载。
- 几何体 fallback。
- 对象树显示。
- 对象点击。
- 查看子级 / 父级。
- 异常高亮。
- 异常 callout。
- WASD / 鼠标视角。
- monitor / edit guard。
- localStorage fallback。
- worldZ 任务移动。
- 后端不可用时 fallback。
- 后端可用时优先走后端。
