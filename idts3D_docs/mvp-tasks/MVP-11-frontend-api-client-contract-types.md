# MVP-11：前端 API Client 与契约类型

## 1. 任务目标

新增前端 `src/api` 和契约 TypeScript 类型层，为后续前端接后端做准备，不改现有加载主流程。

## 2. 前置条件

- MVP-03 到 MVP-10 的后端契约已稳定。
- 已读取 `frontend-integration-plan.md`。
- 已读取全部 `api-contracts/*.md`。

## 3. 影响范围

- `idts3D_ui/src/api/**`
- `idts3D_ui/src/types/api.ts`
- `idts3D_ui/src/types/modelAsset.ts`
- `idts3D_ui/src/types/modelObject.ts`
- `idts3D_ui/src/types/motion.ts`
- `idts3D_ui/src/types/sceneManifest.ts`
- `idts3D_ui/src/types/conversionJob.ts`

## 4. 禁止修改范围

- 禁止修改 `TwinDemo.vue` 主流程。
- 禁止修改 `TwinScene.ts`。
- 禁止修改 `LODModelLoader.ts`。
- 禁止移除 localStorage fallback。
- 禁止修改 `public/models/lifter.glb`。

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

- TypeScript 类型：新增统一响应、错误、model asset、object tree、model stats、asset version、scene manifest、movable part、motion target、conversion job interface。
- API Client：新增 `httpClient.ts`, `modelAssets.ts`, `assetVersions.ts`, `objectTree.ts`, `movableParts.ts`, `motionTargets.ts`, `scenes.ts`, `conversionJobs.ts`。
- Vue 页面：不改。
- Engine 层：不改。
- fallback：只定义错误返回和后续 fallback 判断，不改变当前行为。
- 状态字段：不改。
- UI 提示：不改。

## 7. 数据库变更

- 新增表：无。
- 修改表：无。
- 读取表：无。
- 写入表：无。
- 约束：无。
- 索引：无。

## 8. API 契约

引用 `api-contracts/README.md`、`model-assets.md`、`object-tree-model-stats.md`、`asset-version.md`、`movable-parts.md`、`motion-targets.md`、`scenes.md`、`conversion-jobs.md`。

## 9. 前后端对应关系

| 后端 Entity | DTO | API | 前端 TypeScript interface | 前端调用文件 | Vue / engine 消费位置 |
|---|---|---|---|---|---|
| 多个 | 全部契约 DTO | 全部 MVP API | `src/types/*.ts` | `src/api/*.ts` | 后续 MVP-12 到 MVP-14 |

## 10. 执行步骤

1. 输出影响范围并等待确认。
2. 创建 `src/types/api.ts`。
3. 创建 model asset / object tree / motion / scene / conversion job 类型文件。
4. 创建 `src/api/httpClient.ts`，封装统一响应解析。
5. 创建 `src/api/apiErrors.ts`，区分 400 / 404 / 409 / network error。
6. 创建各领域 API Client 文件。
7. 确保没有修改 `TwinDemo.vue` 主流程。
8. 运行 `npm run build`。

## 11. 验收标准

- `src/api` 目录存在。
- API Client 不散落在页面。
- TypeScript interface 与 API 契约字段一致。
- 网络错误可被调用方识别。
- 400 / 404 / 409 可被调用方识别。
- `npm run build` 通过。
- 不需要 `dotnet build`。

## 12. 回归测试

- GLB 加载：运行页面或 build 确认未改加载主流程。
- 对象树：未接入后端，不改变当前行为。
- 对象点击：未改。
- 查看子级 / 父级：未改。
- 异常高亮：未改。
- 异常 callout：未改。
- WASD / 鼠标视角：未改。
- monitor / edit guard：未改。
- localStorage fallback：未改。
- worldZ 任务移动：未改。
- 后端不可用时 fallback：API 错误类型已支持，后续 MVP-12 接入。
- 后端可用时优先走后端：后续 MVP-12 接入。

## 13. 风险点

- TS interface 与后端 DTO 字段名不一致。
- httpClient 吞掉错误导致页面无法 fallback。
- 过早修改页面主流程会扩大范围。

## 14. 回滚策略

删除本任务新增的 `src/api/**` 和新增契约类型文件即可，不影响现有页面。

## 15. Codex 执行提示词

```text
请执行 MVP-11：前端 API Client 与契约类型。
先读取 AGENTS.md、idts3D_docs/idts-mvp-task-breakdown.md、idts3D_docs/frontend-integration-plan.md、全部 api-contracts 文档和本任务卡。
先输出影响范围，等待我确认后再改。
只新增 src/api 和契约类型，不改 TwinDemo.vue 主流程，不改 TwinScene.ts，不移除 localStorage fallback。
完成后运行 npm run build，输出验证结果、git status、git diff --stat。
不要 commit，不要 push。
```
# DOC-3DT-02 对齐说明

MVP-11 的正式混合场景契约类型依赖 MVP-10A；在 MVP-10A 解除 Blocked 前，本任务只可保持既有 GLB/API Client 基线，不得在页面中临时散落 Tiles 类型。未来 baseLayers、静态资源和变换类型必须以 scene-resource-manifest-design.md 的已审核版本为唯一来源，并由 API 契约、后端 DTO 与 TypeScript 类型同步实施。
