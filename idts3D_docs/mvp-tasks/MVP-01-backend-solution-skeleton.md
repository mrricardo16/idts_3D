# MVP-01：后端解决方案骨架

## 1. 任务目标

创建 .NET 8 后端 solution 和 Api / Application / Domain / Infrastructure / Contracts / Worker 项目骨架，只完成启动、引用、Swagger、Serilog、统一响应和异常中间件占位，不实现业务 API。

## 2. 前置条件

- MVP-00 已完成。
- 已读取 `backend-implementation-plan.md`。
- 本任务开始前用户已确认允许创建真实 `idts3D_api` 工程。

## 3. 影响范围

- `idts3D_api/global.json`
- `idts3D_api/HZ.IDTS.DigitalTwin.sln`
- `idts3D_api/src/HZ.IDTS.DigitalTwin.*`
- `idts3D_api/README.md`
- `idts3D_docs/mvp-tasks/MVP-01-backend-solution-skeleton.md`

## 4. 禁止修改范围

- 禁止创建 Entity、DbContext、Migration。
- 禁止实现上传、manifest、object tree、movable part、motion target、scene 业务接口。
- 禁止修改 `idts3D_ui/src/**`。
- 禁止修改 `idts3D_ui/public/models/lifter.glb`。
- 禁止新增非骨架必需依赖。
- 禁止 commit / push。

## 5. 后端变更

- Entity：无。
- DbContext：无。
- Migration：无。
- Controller：只保留健康检查或 WeatherForecast 默认文件应删除/替换为空健康端点。
- Application Service：创建项目，不写业务服务。
- Infrastructure Repository / EF 查询：创建项目，不写 EF 查询。
- Request DTO：创建 `ApiResponse<T>`、`ApiErrorItem`。
- Response DTO：同上。
- 校验规则：无业务校验。
- 错误码：定义基础 `OK`, `VALIDATION_FAILED`, `NOT_FOUND`, `CONFLICT`, `INTERNAL_ERROR`。

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
- 读取表：无。
- 写入表：无。
- 约束：无。
- 索引：无。

## 8. API 契约

引用 `api-contracts/README.md` 的统一响应结构。本任务不实现业务契约。

## 9. 前后端对应关系

| 后端 Entity | DTO | API | 前端 TypeScript interface | 前端调用文件 | Vue / engine 消费位置 |
|---|---|---|---|---|---|
| 无 | `ApiResponse<T>`, `ApiErrorItem` | 健康端点 | 后续 MVP-11 建立 | 无 | 无 |

## 10. 执行步骤

1. 输出影响范围并等待确认。
2. 进入 `idts3D_api`。
3. 创建 `global.json` 锁定 .NET 8。
4. 创建 `HZ.IDTS.DigitalTwin.sln`。
5. 创建 Api / Application / Domain / Infrastructure / Contracts / Worker 项目。
6. 配置项目引用方向，禁止循环引用。
7. 在 Api 配置 Swagger。
8. 在 Api 配置 Serilog 基础日志。
9. 在 Api 配置统一异常中间件占位。
10. 在 Contracts 定义统一响应结构。
11. 确认 Worker 可启动但不执行业务任务。
12. 运行 `dotnet build idts3D_api/HZ.IDTS.DigitalTwin.sln`。

## 11. 验收标准

- `dotnet build` 通过。
- Swagger 可打开。
- Api 可启动。
- Worker 可启动。
- 未创建数据库表和 migration。
- 未修改 `idts3D_ui/src/**`。
- 不需要 `npm run build`。
- Swagger 只验证启动和统一响应示例。

## 12. 回归测试

- GLB 加载：不执行，本任务不改前端。
- 对象树：不执行。
- 对象点击：不执行。
- 查看子级 / 父级：不执行。
- 异常高亮：不执行。
- 异常 callout：不执行。
- WASD / 鼠标视角：不执行。
- monitor / edit guard：不执行。
- localStorage fallback：不执行。
- worldZ 任务移动：不执行。
- 后端不可用时 fallback：不执行。
- 后端可用时优先走后端：不执行。

## 13. 风险点

- 项目引用方向错误会导致后续分层混乱。
- 默认模板代码残留会污染 Swagger。
- 提前引入 EF 或业务代码会跨到 MVP-02。

## 14. 回滚策略

删除 MVP-01 创建的 solution、project、global.json 和骨架文件，保留 `idts3D_api/README.md`。

## 15. Codex 执行提示词

```text
请执行 MVP-01：后端解决方案骨架。
先读取 AGENTS.md、idts3D_docs/idts-mvp-task-breakdown.md、idts3D_docs/backend-implementation-plan.md 和本任务卡。
先输出影响范围，等待我确认后再改。
只创建后端骨架，不创建 Entity、DbContext、Migration，不实现业务 API，不修改前端源码。
完成后运行 dotnet build，输出修改文件、新增文件、验证结果、git status、git diff --stat。
不要 commit，不要 push。
```
