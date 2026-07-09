# MVP-00：开发规则切换与文档基线

## 1. 任务目标

只完成文档设计阶段到 MVP 开发阶段的规则切换、文档基线确认和链接修复，不写任何业务代码。

## 2. 前置条件

- 已读取 `AGENTS.md`。
- 已读取 `idts3D_docs/development-rules.md`。
- 已读取 `idts3D_docs/idts-mvp-task-breakdown.md`。
- 当前处于文档设计阶段。

## 3. 影响范围

- `AGENTS.md`
- `idts3D_ui/AGENTS.md`
- `idts3D_api/README.md`
- `README.md`
- `idts3D_docs/**/*.md`

## 4. 禁止修改范围

- 禁止修改 `idts3D_ui/src/**`。
- 禁止修改 `idts3D_ui/public/models/lifter.glb`。
- 禁止创建真实 `.sln` / `.csproj`。
- 禁止执行 `dotnet new`、`npm install`、migration。
- 禁止 commit / push。

## 5. 后端变更

- Entity：无。
- DbContext：无。
- Migration：无。
- Controller：无。
- Application Service：无。
- Infrastructure Repository / EF 查询：无。
- Request DTO：无。
- Response DTO：无。
- 校验规则：只在文档中定义分阶段规则。
- 错误码：无。

## 6. 前端变更

- TypeScript 类型：无。
- API Client：无。
- Vue 页面：无。
- Engine 层：无。
- fallback：只在文档中保留要求。
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

引用 `idts3D_docs/api-contracts/README.md`，本任务不新增接口。

## 9. 前后端对应关系

| 后端 Entity | DTO | API | 前端 TypeScript interface | 前端调用文件 | Vue / engine 消费位置 |
|---|---|---|---|---|---|
| 无 | 无 | 无 | 无 | 无 | 无 |

## 10. 执行步骤

1. 读取根 `AGENTS.md` 和 `idts3D_ui/AGENTS.md`。
2. 检查 `idts3D_docs/development-rules.md` 是否存在。
3. 检查 `idts3D_docs/idts-mvp-task-breakdown.md` 是否存在。
4. 检查 `idts3D_docs/api-contracts/README.md` 是否存在。
5. 检查 `idts3D_docs/domain-entity-dto-map.md` 是否存在。
6. 检查所有 MVP 任务卡是否包含 15 个统一章节。
7. 检查文档是否仍引用不存在的 `doc/` 目录。
8. 运行 `git status --short`。
9. 输出文档基线状态。

## 11. 验收标准

- 文档基线文件存在。
- 根 `AGENTS.md` 存在。
- MVP 开发阶段规则明确允许按任务卡创建后端、写 C#、写 Vue/TS、执行 migration。
- 文档设计阶段仍禁止代码开发。
- 未修改业务代码。
- 不需要 `dotnet build`。
- 不需要 `npm run build`。
- 不需要 Swagger 验证。
- 不需要页面验证。

## 12. 回归测试

- GLB 加载：不执行，本任务不改前端。
- 对象树：不执行，本任务不改前端。
- 对象点击：不执行，本任务不改前端。
- 查看子级 / 父级：不执行，本任务不改前端。
- 异常高亮：不执行，本任务不改前端。
- 异常 callout：不执行，本任务不改前端。
- WASD / 鼠标视角：不执行，本任务不改前端。
- monitor / edit guard：文档规则已保留。
- localStorage fallback：文档规则已保留。
- worldZ 任务移动：不执行，本任务不改前端。
- 后端不可用时 fallback：文档规则已保留。
- 后端可用时优先走后端：文档规则已保留。

## 13. 风险点

- 文档路径遗漏会导致后续 Codex 读取错误。
- 未跟踪文档若不纳入后续提交，会让任务链断裂。
- 如果规则仍写成绝对禁止后端开发，会阻断 MVP-01。

## 14. 回滚策略

撤回本任务只需恢复本次文档改动，不涉及数据库、后端工程或前端源码。

## 15. Codex 执行提示词

```text
请执行 MVP-00：开发规则切换与文档基线。
先读取 AGENTS.md、idts3D_docs/development-rules.md、idts3D_docs/idts-mvp-task-breakdown.md 和本任务卡。
先输出影响范围，等待我确认后再修改。
本任务只允许修改文档和规则文件，禁止修改 idts3D_ui/src/**，禁止创建真实 idts3D_api 工程，禁止 dotnet new、npm install、migration。
完成后输出修改文件、新增文件、是否改业务代码、文档结构、git status 和 git diff --stat。
不要 commit，不要 push。
```
