# MVP-09：场景 / 设备实例 / 设备模型绑定

## 1. 任务目标

建立默认 scene、device instance、device model binding，让后端知道场景中有哪些设备以及设备绑定哪个 Published asset version。

## 2. 前置条件

- MVP-06 已完成。
- 至少存在一个 Published asset version。
- `scene_node`, `device_instance`, `device_model_binding` 表可用。

## 3. 影响范围

- `idts3D_api/src/HZ.IDTS.DigitalTwin.Application/**`
- `idts3D_api/src/HZ.IDTS.DigitalTwin.Infrastructure/**`
- `idts3D_api/src/HZ.IDTS.DigitalTwin.Contracts/**`
- 可选 `idts3D_api/src/HZ.IDTS.DigitalTwin.Api/**`

## 4. 禁止修改范围

- 禁止做完整场景管理后台。
- 禁止修改前端源码。
- 禁止绑定非 Published 版本给 monitor。
- 禁止允许同一 device instance 多个 active binding。

## 5. 后端变更

- Entity：`SceneNode`, `DeviceInstance`, `DeviceModelBinding`, `AssetVersion`, `OperationAudit`。
- DbContext：无结构变更。
- Migration：无。
- Controller：可选 seed / 查询端点；若无端点，使用 seed service。
- Application Service：默认场景、设备、绑定创建。
- Infrastructure Repository / EF 查询：active binding 查询和唯一校验。
- Request DTO：`CreateSceneNodeRequest`, `CreateDeviceInstanceRequest`, `CreateDeviceModelBindingRequest` 或 seed DTO。
- Response DTO：`SceneNodeResponse`, `DeviceInstanceResponse`, `DeviceModelBindingResponse`。
- 校验规则：asset version 必须 Published，同设备只能一个 active。
- 错误码：`VERSION_STATUS_INVALID`, `CONFLICT`, `NOT_FOUND`。

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
- 读取表：`asset_version`, `model_asset`, `scene_node`, `device_instance`, `device_model_binding`。
- 写入表：`scene_node`, `device_instance`, `device_model_binding`, `operation_audit`。
- 约束：`device_code` 唯一，`device_instance_id + asset_version_id` 唯一，同设备 active 唯一。
- 索引：scene、device、binding 状态索引。

## 8. API 契约

本任务主要为 `api-contracts/scenes.md` 的 scene manifest 提供数据。若新增 seed API，必须在任务输出中说明，但完整 manifest 契约仍以 `scenes.md` 为准。

## 9. 前后端对应关系

| 后端 Entity | DTO | API | 前端 TypeScript interface | 前端调用文件 | Vue / engine 消费位置 |
|---|---|---|---|---|---|
| `SceneNode`, `DeviceInstance`, `DeviceModelBinding` | scene/device/binding DTO | seed 或内部服务 | MVP-11 `SceneManifestResponse` | MVP-11 `src/api/scenes.ts` | MVP-12 场景加载 |

## 10. 执行步骤

1. 输出影响范围并等待确认。
2. 确认 Published asset version。
3. 创建默认厂区 scene node。
4. 创建默认区域 scene node。
5. 创建设备 `LIFTER-001`。
6. 创建 active device model binding。
7. 校验绑定版本为 Published。
8. 校验同设备只有一个 active binding。
9. 写入审计。
10. 运行 `dotnet build`。

## 11. 验收标准

- 数据库有默认 scene_node。
- 数据库有默认 device_instance。
- 数据库有 active device_model_binding。
- active binding 指向 Published asset_version。
- 同一 device_instance 只有一个 active binding。
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
- monitor / edit guard：验证绑定只指向 Published。
- localStorage fallback：不执行。
- worldZ 任务移动：不执行。
- 后端不可用时 fallback：后续 MVP-12 验证。
- 后端可用时优先走后端：后续 MVP-12 验证。

## 13. 风险点

- seed 数据依赖 Published version，顺序错误会失败。
- active binding 唯一约束需要事务或服务层锁。
- 场景坐标和模型 root transform 可能重复叠加。

## 14. 回滚策略

删除本任务 seed 的 scene_node、device_instance、device_model_binding 和审计数据。

## 15. Codex 执行提示词

```text
请执行 MVP-09：场景 / 设备实例 / 设备模型绑定。
先读取 AGENTS.md、idts3D_docs/idts-mvp-task-breakdown.md、idts3D_docs/domain-entity-dto-map.md、idts3D_docs/api-contracts/scenes.md 和本任务卡。
先输出影响范围，等待我确认后再改。
只做默认 scene/device/binding 数据和服务，不做完整场景管理后台，不修改前端源码。
完成后运行 dotnet build，并验证 active binding 指向 Published。
不要 commit，不要 push。
```
