# POC-3DT-01：Three.js + 3DTilesRendererJS 最小验证

## 1. 任务目标

独立验证 Three.js 中加载 3D Tiles 的可行性，不并入 MVP 主线，不影响 `TwinDemo` 主流程。

## 2. 前置条件

- 当前前端 GLB 或几何体 fallback 可正常运行。
- 用户明确要求执行 POC。
- 已确认允许新增独立 POC 页面或独立 engine 模块。

## 3. 影响范围

- `idts3D_ui/src/engine/tiles/**`
- `idts3D_ui/src/views/TilesetPoc.vue`
- 可选最小入口文件，必须先输出影响范围。
- `idts3D_docs/**` POC 记录。

## 4. 禁止修改范围

- 禁止修改 `TwinDemo` 主流程。
- 禁止修改 `idts3D_ui/public/models/lifter.glb`。
- 禁止做厂区正式 tileset。
- 禁止做 CAD 切片。
- 禁止做生产级 manifest。
- 禁止影响 MVP 主线。

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

- TypeScript 类型：按 POC 最小需要新增 tiles 类型。
- API Client：无。
- Vue 页面：新增独立 `TilesetPoc.vue`。
- Engine 层：新增 `src/engine/tiles/**`。
- fallback：GLB 设备仍应可显示或 fallback。
- 状态字段：POC 加载状态。
- UI 提示：显示 tileset 加载成功 / 失败。

## 7. 数据库变更

- 新增表：无。
- 修改表：无。
- 读取表：无。
- 写入表：无。
- 约束：无。
- 索引：无。

## 8. API 契约

无。POC 不使用正式后端 API。

## 9. 前后端对应关系

| 后端 Entity | DTO | API | 前端 TypeScript interface | 前端调用文件 | Vue / engine 消费位置 |
|---|---|---|---|---|---|
| 无 | 无 | 无 | tiles POC 类型 | 无 | `TilesetPoc.vue`, `src/engine/tiles/**` |

## 10. 执行步骤

1. 输出影响范围并等待确认。
2. 新增独立 POC 页面。
3. 新增 tiles loader 最小封装。
4. 加载一个最小 `tileset.json`。
5. 同场景显示 GLB 设备或 fallback 几何体。
6. 验证 Z-up / Y-up。
7. 验证缩放比例。
8. 验证相机控制兼容。
9. 验证 Raycaster 是否冲突。
10. 验证 dispose 后资源释放。
11. 运行 `npm run build`。
12. 输出 POC 记录。

## 11. 验收标准

- `npm run build` 通过。
- tileset 可加载。
- GLB 设备或 fallback 几何体可同时显示。
- 相机控制正常。
- Raycaster 不破坏主流程。
- dispose 无明显资源泄漏。
- 未修改后端。

## 12. 回归测试

- GLB 加载：通过或 fallback 通过。
- 对象树：不接入 POC，确认主流程未改。
- 对象点击：确认主流程未改。
- 查看子级 / 父级：确认主流程未改。
- 异常高亮：确认主流程未改。
- 异常 callout：确认主流程未改。
- WASD / 鼠标视角：POC 页面和主页面分别验证。
- monitor / edit guard：确认主流程未改。
- localStorage fallback：确认主流程未改。
- worldZ 任务移动：确认主流程未改。
- 后端不可用时 fallback：确认主流程未改。
- 后端可用时优先走后端：不涉及。

## 13. 风险点

- 3D Tiles 坐标系与 GLB 坐标系不一致。
- tiles renderer 生命周期不当导致内存泄漏。
- POC 入口误污染 MVP 主页面。

## 14. 回滚策略

删除 POC 页面、tiles engine 模块和 POC 记录，恢复前端原有入口。

## 15. Codex 执行提示词

```text
请执行 POC-3DT-01：Three.js + 3DTilesRendererJS 最小验证。
先读取 AGENTS.md、idts3D_docs/idts-mvp-task-breakdown.md 和本任务卡。
先输出影响范围，等待我确认后再改。
本任务是独立 POC，不并入 MVP 主线，禁止修改 TwinDemo 主流程，禁止修改 lifter.glb，禁止做 CAD 切片或生产级 manifest。
完成后运行 npm run build，输出 POC 验证记录、git status、git diff --stat。
不要 commit，不要 push。
```
