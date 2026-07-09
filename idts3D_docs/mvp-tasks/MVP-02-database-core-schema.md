# MVP-02：数据库核心实体与 Migration

## 1. 任务目标

建立 MVP 所需核心 Entity、DbContext、EF Core 配置、唯一约束、外键、索引和初始 Migration。

## 2. 前置条件

- MVP-01 已完成并且后端 solution 可 build。
- 已读取 `domain-entity-dto-map.md`。
- 已确认数据库 Provider，默认 PostgreSQL。

## 3. 影响范围

- `idts3D_api/src/HZ.IDTS.DigitalTwin.Domain/**`
- `idts3D_api/src/HZ.IDTS.DigitalTwin.Infrastructure/**`
- `idts3D_api/src/HZ.IDTS.DigitalTwin.Api/appsettings*.json`
- `idts3D_docs/domain-entity-dto-map.md`

## 4. 禁止修改范围

- 禁止实现业务 Controller。
- 禁止实现 GLB 上传。
- 禁止修改前端源码。
- 禁止修改模型文件。
- 禁止跨任务添加非核心表。

## 5. 后端变更

- Entity：创建 14 张核心表对应 Entity。
- DbContext：创建 `DigitalTwinDbContext` 和 DbSet。
- Migration：创建 `InitDigitalTwinSchema`。
- Controller：无。
- Application Service：无。
- Infrastructure Repository / EF 查询：仅配置 EF。
- Request DTO：无。
- Response DTO：无。
- 校验规则：通过约束实现唯一和外键。
- 错误码：无新增。

## 6. 前端变更

- TypeScript 类型：无。
- API Client：无。
- Vue 页面：无。
- Engine 层：无。
- fallback：无。
- 状态字段：无。
- UI 提示：无。

## 7. 数据库变更

- 新增表：`model_asset`, `asset_version`, `model_asset_variant`, `model_conversion_job`, `model_object_index`, `asset_manifest`, `scene_node`, `device_instance`, `device_model_binding`, `movable_part_binding`, `motion_target`, `operation_audit`, `tool_package`, `tool_health_check`。
- 修改表：无。
- 读取表：无。
- 写入表：migration 创建结构。
- 约束：按 `domain-entity-dto-map.md`。
- 索引：按 `domain-entity-dto-map.md`。

## 8. API 契约

引用全部 `api-contracts/*.md` 的实体和读写表。本任务不实现接口。

## 9. 前后端对应关系

| 后端 Entity | DTO | API | 前端 TypeScript interface | 前端调用文件 | Vue / engine 消费位置 |
|---|---|---|---|---|---|
| 14 个核心 Entity | 后续 MVP 创建 | 后续 MVP 创建 | 后续 MVP-11 创建 | 后续 MVP-11 创建 | 后续 MVP-12 到 MVP-14 |

## 10. 执行步骤

1. 输出影响范围并等待确认。
2. 创建 14 个 Entity。
3. 创建领域枚举。
4. 创建 `DigitalTwinDbContext`。
5. 为每个 Entity 创建 Fluent API Configuration。
6. 配置 snake_case 表名和列名。
7. 配置主键、外键、唯一约束、索引。
8. 配置 PostgreSQL / SQL Server Provider 边界。
9. 创建 `InitDigitalTwinSchema` migration。
10. 执行数据库更新。
11. 用数据库工具或 SQL 查询确认表存在。
12. 运行 `dotnet build`。

## 11. 验收标准

- `dotnet ef migrations add InitDigitalTwinSchema` 成功。
- `dotnet ef database update` 成功。
- 14 张表存在。
- 唯一约束、外键、索引存在。
- `dotnet build` 通过。
- 未实现业务 Controller。
- 未修改 `idts3D_ui/src/**`。
- 不需要 `npm run build`。
- Swagger 不需要业务接口验证。

## 12. 回归测试

- GLB 加载：不执行。
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

- PostgreSQL jsonb 与 SQL Server nvarchar(max) 差异。
- active binding 唯一约束可能需要过滤索引或服务层兜底。
- Entity 字段和 DTO 文档不一致会影响后续任务。

## 14. 回滚策略

回滚数据库 migration，删除本任务新增 Entity、DbContext 配置和 migration 文件。

## 15. Codex 执行提示词

```text
请执行 MVP-02：数据库核心实体与 Migration。
先读取 AGENTS.md、idts3D_docs/idts-mvp-task-breakdown.md、idts3D_docs/domain-entity-dto-map.md、idts3D_docs/backend-implementation-plan.md 和本任务卡。
先输出影响范围，等待我确认后再改。
只做 Entity、DbContext、EF 配置和初始 Migration，不实现业务 Controller，不修改前端源码。
完成后运行 dotnet build，并输出 migration、数据库表验证、git status、git diff --stat。
不要 commit，不要 push。
```
