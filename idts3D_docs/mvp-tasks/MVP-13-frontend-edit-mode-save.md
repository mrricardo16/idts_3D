# MVP-13：前端 Edit 模式保存可动部件与目标点位

## 1. 任务目标

edit 模式下把当前选择的 movable part 和 motion target 保存到后端，monitor 模式继续只读。

## 2. 前置条件

- MVP-12 已完成。
- MVP-07 / MVP-08 后端 API 可用。
- MVP-11 API Client 已有 movable part / motion target 方法。

## 3. 影响范围

- `idts3D_ui/src/views/TwinDemo.vue`
- `idts3D_ui/src/engine/TwinScene.ts`
- `idts3D_ui/src/api/movableParts.ts`
- `idts3D_ui/src/api/motionTargets.ts`
- `idts3D_ui/src/types/motion.ts`

## 4. 禁止修改范围

- 禁止让 monitor 模式保存配置。
- 禁止移除本地预览和 fallback。
- 禁止修改模型文件。
- 禁止实现 monitor 后端目标点动画替换，该项留给 MVP-14。

## 5. 后端变更

- Entity：无。
- DbContext：无。
- Migration：无。
- Controller：无。
- Application Service：无。
- Infrastructure Repository / EF 查询：无。
- Request DTO：无。
- Response DTO：无。
- 校验规则：前端按后端错误展示。
- 错误码：无新增。

## 6. 前端变更

- TypeScript 类型：使用 `MovablePartDto`, `MotionTargetDto`。
- API Client：调用 create/update/delete movable part 和 motion target。
- Vue 页面：edit 模式增加保存按钮、字段提示、错误提示。
- Engine 层：提供当前选中对象、objectUuid/objectPath、worldZ 范围。
- fallback：后端不可用时允许本地预览，但提示未保存。
- 状态字段：保存状态、后端错误、当前 partId、targetId。
- UI 提示：400 字段错误、404 对象不存在、409 重复或版本状态错误。

## 7. 数据库变更

- 新增表：无。
- 修改表：无。
- 读取表：通过 API 间接读取 `movable_part_binding`, `motion_target`。
- 写入表：通过 API 写入 `movable_part_binding`, `motion_target`, `operation_audit`。
- 约束：由后端执行。
- 索引：无。

## 8. API 契约

引用 `api-contracts/movable-parts.md` 和 `api-contracts/motion-targets.md`。

## 9. 前后端对应关系

| 后端 Entity | DTO | API | 前端 TypeScript interface | 前端调用文件 | Vue / engine 消费位置 |
|---|---|---|---|---|---|
| `MovablePartBinding` | `CreateMovablePartRequest`, `MovablePartResponse` | movable part CRUD | `MovablePartDto` | `src/api/movableParts.ts` | `TwinDemo.vue`, `TwinScene.ts` |
| `MotionTarget` | `CreateMotionTargetRequest`, `MotionTargetResponse` | motion target CRUD | `MotionTargetDto` | `src/api/motionTargets.ts` | `TwinDemo.vue` |

## 10. 执行步骤

1. 输出影响范围并等待确认。
2. 扫描当前 `setSelectedAsMovable` 和 `clearMovablePart`。
3. 扫描当前 `lifterTargetPositions`。
4. 增加 edit 保存 movable part 表单字段。
5. 调用 movable part API 保存。
6. 增加 motion target 保存逻辑。
7. 处理 400 / 404 / 409。
8. monitor 模式隐藏或禁用保存入口。
9. 后端不可用时保留本地预览。
10. 刷新后从后端读取配置验证。
11. 运行 `npm run build`。

## 11. 验收标准

- edit 模式可保存 movable part。
- edit 模式可保存 motion target。
- monitor 模式不能保存。
- 重复 partCode / targetCode 有提示。
- object 不存在有提示。
- 后端不可用时不伪装保存成功。
- 刷新后可从后端读取配置。
- `npm run build` 通过。

## 12. 回归测试

- GLB 加载：通过。
- 对象树：可选择对象。
- 对象点击：点击后可设为 movable part。
- 查看子级 / 父级：仍可用。
- 异常高亮：仍可用。
- 异常 callout：仍可用。
- WASD / 鼠标视角：仍可用。
- monitor / edit guard：monitor 不能保存。
- localStorage fallback：后端不可用时可本地预览。
- worldZ 任务移动：仍使用现有 F1/F2/F3/F4。
- 后端不可用时 fallback：提示未保存到后端。
- 后端可用时优先走后端：保存后刷新仍存在。

## 13. 风险点

- 保存后端配置和本地预览状态不同步。
- monitor 模式误显示保存入口。
- 409 错误未正确映射导致用户不知道重复字段。

## 14. 回滚策略

撤回 edit 保存相关 UI 和 API 调用，恢复本地运行态配置；保留 MVP-12 后端读取能力。

## 15. Codex 执行提示词

```text
请执行 MVP-13：前端 Edit 模式保存可动部件与目标点位。
先读取 AGENTS.md、idts3D_docs/idts-mvp-task-breakdown.md、idts3D_docs/frontend-integration-plan.md、api-contracts/movable-parts.md、api-contracts/motion-targets.md 和本任务卡。
先扫描 TwinDemo.vue、TwinScene.ts 中 edit、monitor guard、可动部件、motion target、localStorage fallback 相关代码，输出影响范围，等待我确认后再改。
禁止让 monitor 模式保存配置，禁止移除 fallback，禁止修改 lifter.glb。
完成后运行 npm run build，并回归 edit 保存、monitor 禁止保存、GLB、对象树、worldZ。
不要 commit，不要 push。
```
