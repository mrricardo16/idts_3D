# IDTS 3D 分阶段开发规则

本文档统一本仓库的文档设计阶段和 MVP 开发阶段边界，解决旧文档中“禁止创建后端 / 禁止写 C# / 禁止 migration”与后续 MVP 开发任务之间的冲突。

## 1. 阶段定义

| 阶段 | 触发条件 | 允许事项 | 禁止事项 |
|---|---|---|---|
| 文档设计阶段 | 用户要求整理方案、任务书、契约、任务卡 | 修改 Markdown / README / AGENTS 规则 | 写业务代码、创建真实后端工程、执行 migration、装依赖、commit、push |
| MVP 开发阶段 | 用户明确要求执行某个 MVP 任务卡 | 严格按任务卡写后端、前端、数据库、API Client、测试 | 跨任务扩展、一次做多个 MVP、无确认改文件、无确认 commit / push |
| 验收阶段 | 单个 MVP 实现完成后 | 运行 build、Swagger、页面、回归验证并输出证据 | 用“应该通过”替代实际验证 |

## 2. 文档设计阶段规则

当前阶段只允许把仓库文档改造成后续可执行的“前后端一体化开发任务书”。

允许修改：

- `AGENTS.md`
- `README.md`
- `idts3D_ui/AGENTS.md`
- `idts3D_api/README.md`
- `idts3D_docs/**/*.md`

禁止修改：

- `idts3D_ui/src/**`
- `idts3D_ui/public/models/lifter.glb`
- `idts3D_ui/package.json`
- `idts3D_ui/package-lock.json`
- 真实 `.csproj`、`.sln`、C# 源码、Migration 文件

禁止执行：

- `dotnet new`
- `dotnet ef migrations add`
- `dotnet ef database update`
- `npm install`
- `commit`
- `push`

## 3. MVP 开发阶段规则

进入 MVP 开发阶段必须由用户明确指定任务，例如“执行 MVP-03”。

执行前必须：

1. 读取 `AGENTS.md`、`idts3D_ui/AGENTS.md`、本文件、`idts-mvp-task-breakdown.md`、当前任务卡。
2. 读取当前任务卡引用的 API 契约与实体映射文档。
3. 扫描当前仓库实际文件。
4. 输出影响范围、禁止修改范围、计划验证命令、风险点。
5. 等待用户确认。

执行中必须：

- 每次只实现一个 MVP。
- 只修改当前任务卡列出的文件范围。
- 新增字段时同步更新 Entity、DTO、API 契约、TypeScript interface、API Client 和消费位置。
- 不把后续任务提前实现。
- 不移除本地 fallback，除非任务卡明确要求。
- 不让 monitor 模式获得编辑能力。

执行完成后必须：

- 运行任务卡要求的验证命令。
- 输出验收项逐条结果。
- 输出回归测试结果。
- 输出 `git status` 和 `git diff --stat`。

## 4. Commit / Push 规则

默认禁止 commit / push。

只有用户明确要求提交或推送时，才允许执行，并且提交前必须先输出：

- 将要提交的文件列表。
- 本次是否包含业务代码。
- 本次是否包含模型文件或大型二进制。
- 验证命令结果。

## 5. 统一命名

所有文档、任务卡、API 契约和代码实现必须使用下列英文术语：

| 中文 | 固定英文 |
|---|---|
| 模型资产 | model asset |
| 资产版本 | asset version |
| 模型清单 | model manifest |
| 场景清单 | scene manifest |
| 对象树 | object tree |
| 模型统计 | model stats |
| 可动部件 | movable part |
| 运动目标点 | motion target |
| 设备实例 | device instance |
| 设备模型绑定 | device model binding |
| 转换任务 | conversion job |

## 6. 统一 API 响应

成功响应固定为：

```json
{
  "success": true,
  "code": "OK",
  "message": "",
  "data": {}
}
```

错误响应固定为：

```json
{
  "success": false,
  "code": "VERSION_STATUS_INVALID",
  "message": "monitor 模式只能读取 Published 版本。",
  "errors": []
}
```

所有 API 契约必须写明：

- Method
- Route
- Query 参数
- Path 参数
- Request Body JSON
- Response Body JSON
- 400 / 404 / 409 示例
- 字段类型
- 枚举值
- monitor / edit 模式调用权限
- 前端 TypeScript interface
- 后端 Request DTO / Response DTO
- 后端 Entity
- 读取表
- 写入表
