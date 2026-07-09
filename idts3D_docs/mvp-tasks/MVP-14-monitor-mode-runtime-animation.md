# MVP-14：Monitor 模式只读配置并驱动 worldZ 动画

## 1. 任务目标

monitor 模式从后端 Published 配置读取 movable part 和 motion target，并用后端目标点驱动现有 worldZ 动画，逐步替代 hard-coded F1/F2/F3/F4。

## 2. 前置条件

- MVP-13 已完成。
- 后端已有 Published 配置。
- 后端 movable part 和 motion target 可读。

## 3. 影响范围

- `idts3D_ui/src/views/TwinDemo.vue`
- `idts3D_ui/src/engine/TwinScene.ts`
- `idts3D_ui/src/api/movableParts.ts`
- `idts3D_ui/src/api/motionTargets.ts`
- `idts3D_ui/src/types/motion.ts`

## 4. 禁止修改范围

- 禁止开放 monitor 编辑能力。
- 禁止实现 local axis / rotate / path / AGV path / joint。
- 禁止接真实任务系统。
- 禁止移除 hard-coded F1/F2/F3/F4 fallback。
- 禁止修改模型文件。

## 5. 后端变更

- Entity：无。
- DbContext：无。
- Migration：无。
- Controller：无。
- Application Service：无。
- Infrastructure Repository / EF 查询：无。
- Request DTO：无。
- Response DTO：无。
- 校验规则：无新增。
- 错误码：无新增。

## 6. 前端变更

- TypeScript 类型：使用 `MovablePartDto`, `MotionTargetDto`。
- API Client：读取 Published movable part 和 motion target。
- Vue 页面：目标位置下拉改为后端数据优先。
- Engine 层：把后端 `targetValue` / `targetZ` 转为当前 worldZ 运动输入。
- fallback：后端不可用或无 Published 配置时使用 `lifterTargetPositions`。
- 状态字段：数据来源、目标点来源、版本状态提示。
- UI 提示：后端配置不可用时显示 fallback。

## 7. 数据库变更

- 新增表：无。
- 修改表：无。
- 读取表：通过 API 间接读取 `movable_part_binding`, `motion_target`, `asset_version`。
- 写入表：无。
- 约束：monitor 只读 Published。
- 索引：无。

## 8. API 契约

引用 `api-contracts/movable-parts.md` 和 `api-contracts/motion-targets.md`。

## 9. 前后端对应关系

| 后端 Entity | DTO | API | 前端 TypeScript interface | 前端调用文件 | Vue / engine 消费位置 |
|---|---|---|---|---|---|
| `MovablePartBinding` | `MovablePartResponse` | `GET movable-parts` | `MovablePartDto` | `src/api/movableParts.ts` | `TwinDemo.vue`, `TwinScene.ts` |
| `MotionTarget` | `MotionTargetResponse` | `GET motion-targets` | `MotionTargetDto` | `src/api/motionTargets.ts` | `TwinDemo.vue`, worldZ 动画 |

## 10. 执行步骤

1. 输出影响范围并等待确认。
2. 扫描当前 `dispatchTask` 和 `dispatchLifterTask`。
3. 扫描 `lifterTargetPositions` fallback。
4. monitor 加载 Published movable part。
5. monitor 加载 enabled motion target。
6. 将后端目标点映射到目标下拉。
7. 只支持 `linear + world + z`。
8. 点击目标后调用现有 worldZ 动画。
9. 后端配置不可用时 fallback 到 hard-coded F1/F2/F3/F4。
10. 保持 edit 模式保存逻辑。
11. 运行 `npm run build`。

## 11. 验收标准

- monitor 模式能读取后端 movable part。
- monitor 模式能读取后端 motion target。
- 点击后端目标点后载货台沿 worldZ 移动。
- monitor 模式不显示保存入口。
- 后端不可用时 hard-coded F1/F2/F3/F4 fallback 可用。
- `npm run build` 通过。

## 12. 回归测试

- GLB 加载：通过。
- 对象树：通过。
- 对象点击：通过。
- 查看子级 / 父级：通过。
- 异常高亮：通过。
- 异常 callout：通过。
- WASD / 鼠标视角：通过。
- monitor / edit guard：monitor 只读，edit 可编辑。
- localStorage fallback：保留。
- worldZ 任务移动：后端目标点和 fallback 目标点都验证。
- 后端不可用时 fallback：关闭后端验证。
- 后端可用时优先走后端：网络面板确认。

## 13. 风险点

- targetValue 与当前世界坐标基准不一致。
- 后端配置与当前模型对象不匹配。
- 移除 fallback 会让演示依赖后端。

## 14. 回滚策略

撤回 monitor 后端目标点读取和映射逻辑，恢复 hard-coded F1/F2/F3/F4 驱动；保留 MVP-13 edit 保存能力。

## 15. Codex 执行提示词

```text
请执行 MVP-14：Monitor 模式只读配置并驱动 worldZ 动画。
先读取 AGENTS.md、idts3D_docs/idts-mvp-task-breakdown.md、idts3D_docs/frontend-integration-plan.md、api-contracts/movable-parts.md、api-contracts/motion-targets.md 和本任务卡。
先扫描 worldZ、F1/F2/F3/F4、monitor guard、可动部件 runtime 状态和 fallback 代码，输出影响范围，等待我确认后再改。
MVP 只支持 linear + world + z，禁止实现 local axis、rotate、path、AGV path、joint 或真实任务系统，禁止让 monitor 可编辑。
完成后运行 npm run build，并回归后端目标点动画、fallback、monitor 禁止编辑。
不要 commit，不要 push。
```
