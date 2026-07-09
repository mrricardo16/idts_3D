# MVP-16：端到端联调与验收

## 1. 任务目标

按 `e2e-acceptance-plan.md` 完成从 GLB 上传到前端 monitor worldZ 动画的完整闭环验收，不新增功能。

## 2. 前置条件

- MVP-01 到 MVP-15 已完成。
- 数据库可启动。
- 后端可启动。
- 前端可启动。
- 有可用 GLB 测试文件。

## 3. 影响范围

- `idts3D_api/**` 只允许修复联调中确认的当前任务范围内问题。
- `idts3D_ui/src/**` 只允许修复联调中确认的当前任务范围内问题。
- `idts3D_docs/e2e-acceptance-plan.md`
- 验收记录文档，如 `idts3D_docs/e2e-acceptance-report.md`

## 4. 禁止修改范围

- 禁止新增功能。
- 禁止重构无关代码。
- 禁止修改模型文件。
- 禁止跳过失败项。
- 禁止 commit / push。

## 5. 后端变更

- Entity：原则上无新增。
- DbContext：原则上无新增。
- Migration：原则上无新增。
- Controller：只修复已实现接口的联调问题。
- Application Service：只修复已实现业务闭环问题。
- Infrastructure Repository / EF 查询：只修复当前闭环问题。
- Request DTO：不得新增字段，除非契约与实现不一致并经确认。
- Response DTO：不得新增字段，除非契约与实现不一致并经确认。
- 校验规则：按契约修正。
- 错误码：按契约修正。

## 6. 前端变更

- TypeScript 类型：只修复契约不一致。
- API Client：只修复联调错误。
- Vue 页面：只修复显示、fallback、guard 问题。
- Engine 层：只修复 manifest / object tree / worldZ 映射问题。
- fallback：必须验证并保留。
- 状态字段：不新增非必要状态。
- UI 提示：只补必要错误提示。

## 7. 数据库变更

- 新增表：无。
- 修改表：原则上无。
- 读取表：完整闭环涉及全部核心表。
- 写入表：上传、object tree、model stats、发布、seed、movable part、motion target、job。
- 约束：验证唯一、外键、active binding、Published。
- 索引：不新增，除非性能阻塞并经确认。

## 8. API 契约

引用全部 `api-contracts/*.md` 和 `e2e-acceptance-plan.md`。

## 9. 前后端对应关系

| 后端 Entity | DTO | API | 前端 TypeScript interface | 前端调用文件 | Vue / engine 消费位置 |
|---|---|---|---|---|---|
| 全部核心 Entity | 全部 MVP DTO | 全部 MVP API | `src/types/*.ts` | `src/api/*.ts` | `TwinDemo.vue`, `TwinScene.ts`, `LODModelLoader.ts` |

## 10. 执行步骤

1. 输出影响范围并等待确认。
2. 启动数据库。
3. 启动后端。
4. Swagger 上传 GLB。
5. 验证 asset/version/source variant/job。
6. 保存 object tree。
7. 保存 model stats。
8. mark-ready。
9. publish。
10. seed scene/device/binding。
11. GET scene manifest。
12. GET model manifest。
13. GET object tree。
14. 启动前端。
15. 验证前端优先从后端加载。
16. edit 保存 movable part。
17. edit 保存 motion target。
18. 刷新验证后端配置持久化。
19. monitor 验证只读。
20. monitor 点击目标点执行 worldZ。
21. 关闭后端验证 fallback。
22. 运行 `npm run build`。
23. 运行 `dotnet build`。
24. 输出验收报告。

## 11. 验收标准

- 24 步端到端链路全部有结果。
- Swagger 关键接口全部返回预期。
- 前端后端可用时优先走后端。
- 后端不可用时 fallback。
- edit 可保存配置。
- monitor 只读并可执行 worldZ。
- `npm run build` 通过。
- `dotnet build` 通过。

## 12. 回归测试

- GLB 加载：必须通过。
- 对象树：必须通过。
- 对象点击：必须通过。
- 查看子级 / 父级：必须通过。
- 异常高亮：必须通过。
- 异常 callout：必须通过。
- WASD / 鼠标视角：必须通过。
- monitor / edit guard：必须通过。
- localStorage fallback：必须通过。
- worldZ 任务移动：必须通过。
- 后端不可用时 fallback：必须通过。
- 后端可用时优先走后端：必须通过。

## 13. 风险点

- 联调发现契约字段不一致。
- 运行数据污染导致重复 code 冲突。
- fallback 和后端正式数据来源混淆。
- build 通过但页面流程失败。

## 14. 回滚策略

本任务不新增功能。若修复代码导致回归，撤回本任务修复，保留 MVP-01 到 MVP-15 已验证部分，并记录失败项。

## 15. Codex 执行提示词

```text
请执行 MVP-16：端到端联调与验收。
先读取 AGENTS.md、idts3D_docs/idts-mvp-task-breakdown.md、idts3D_docs/e2e-acceptance-plan.md、全部 api-contracts 文档和本任务卡。
先输出影响范围，等待我确认后再改。
本任务不新增功能，只做联调验证和必要修复；禁止跨任务重构，禁止修改 lifter.glb。
按 24 步闭环执行，完成后运行 npm run build 和 dotnet build，并输出验收报告、git status、git diff --stat。
不要 commit，不要 push。
```
