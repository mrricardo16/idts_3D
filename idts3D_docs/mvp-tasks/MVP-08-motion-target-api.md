# MVP-08：Motion Target API

## 1. 任务目标

实现 motion target CRUD，把 F1/F2/F3/F4 等目标点位从前端硬编码迁移为后端配置。

## 2. 前置条件

- MVP-07 movable part API 已完成。
- 已存在可用 movable part。
- 已读取 `api-contracts/motion-targets.md`。

## 3. 影响范围

- `idts3D_api/src/HZ.IDTS.DigitalTwin.Api/**`
- `idts3D_api/src/HZ.IDTS.DigitalTwin.Application/**`
- `idts3D_api/src/HZ.IDTS.DigitalTwin.Contracts/**`
- `idts3D_api/src/HZ.IDTS.DigitalTwin.Infrastructure/**`

## 4. 禁止修改范围

- 禁止修改前端源码。
- 禁止实现 local axis / rotate / path / AGV path / joint。
- 禁止让 Published 版本直接修改。
- 禁止接真实任务系统。

## 5. 后端变更

- Entity：`MotionTarget`, `MovablePartBinding`, `AssetVersion`, `OperationAudit`。
- DbContext：无结构变更。
- Migration：无。
- Controller：新增 motion targets endpoints。
- Application Service：CRUD 和范围校验。
- Infrastructure Repository / EF 查询：按 partId 查询。
- Request DTO：`CreateMotionTargetRequest`, `UpdateMotionTargetRequest`。
- Response DTO：`MotionTargetResponse`, `MotionTargetListResponse`。
- 校验规则：targetCode 唯一、targetValue 在 min/max 内、Published 不可修改。
- 错误码：`DUPLICATE_TARGET_CODE`, `VERSION_STATUS_INVALID`, `VALIDATION_FAILED`。

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
- 读取表：`movable_part_binding`, `asset_version`, `motion_target`。
- 写入表：`motion_target`, `operation_audit`。
- 约束：`movable_part_id + target_code` 唯一。
- 索引：使用 MVP-02 已建索引。

## 8. API 契约

完整契约见 `idts3D_docs/api-contracts/motion-targets.md`。

## 9. 前后端对应关系

| 后端 Entity | DTO | API | 前端 TypeScript interface | 前端调用文件 | Vue / engine 消费位置 |
|---|---|---|---|---|---|
| `MotionTarget` | `CreateMotionTargetRequest`, `UpdateMotionTargetRequest`, `MotionTargetResponse` | motion target CRUD | MVP-11 `MotionTargetDto` | MVP-11 `src/api/motionTargets.ts` | MVP-13 edit 保存，MVP-14 worldZ 动画 |

## 10. 执行步骤

1. 输出影响范围并等待确认。
2. 创建 motion target DTO。
3. 实现 GET list。
4. 实现 POST create。
5. 校验 targetCode 唯一。
6. 校验 targetValue 在 movable part 范围内。
7. 实现 PUT update。
8. 实现 DELETE。
9. 实现 Published guard。
10. 写入审计。
11. 运行 `dotnet build`。

## 11. 验收标准

- GET 返回目标点列表。
- POST 可新增 F1/F2/F3/F4。
- 重复 targetCode 返回 409。
- 超出范围返回 400。
- Published 版本修改返回 409。
- `dotnet build` 通过。
- 不需要 `npm run build`。

## 12. 回归测试

- GLB 加载：后续 MVP-12 验证。
- 对象树：不执行。
- 对象点击：后续 MVP-13 验证。
- 查看子级 / 父级：不执行。
- 异常高亮：不执行。
- 异常 callout：不执行。
- WASD / 鼠标视角：不执行。
- monitor / edit guard：验证 Published 不能修改。
- localStorage fallback：不执行。
- worldZ 任务移动：后续 MVP-14 验证。
- 后端不可用时 fallback：后续 MVP-12 验证。
- 后端可用时优先走后端：后续 MVP-14 验证。

## 13. 风险点

- targetValue 与 targetZ 语义混乱。
- 范围校验依赖 movable part min/max。
- 过早支持多运动类型会扩大范围。

## 14. 回滚策略

删除 motion target endpoints、DTO、Service；清理测试 `motion_target` 数据。

## 15. Codex 执行提示词

```text
请执行 MVP-08：Motion Target API。
先读取 AGENTS.md、idts3D_docs/idts-mvp-task-breakdown.md、idts3D_docs/api-contracts/motion-targets.md、idts3D_docs/domain-entity-dto-map.md 和本任务卡。
先输出影响范围，等待我确认后再改。
只实现 motion target CRUD，MVP 只支持 linear + world + z，不修改前端源码。
完成后运行 dotnet build，并用 Swagger 验证 200、400、404、409。
不要 commit，不要 push。
```
