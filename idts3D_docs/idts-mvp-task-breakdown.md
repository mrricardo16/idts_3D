# IDTS 数字孪生 MVP 开发总纲

> 文档版本：v1.0  
> 文档性质：前后端一体化 MVP 开发总纲  
> 适用范围：`idts3D_api` 后端、`idts3D_ui` 前端、`idts3D_docs` 契约与任务卡  
> 固定技术约束：后端采用 ASP.NET Core Web API / .NET 8 / EF Core 8；前端采用 Vue 3 + Vite + TypeScript + Three.js；数据库 PostgreSQL 优先，SQL Server 备选。

## 1. 项目目标

MVP 一期目标是形成一个可串行开发、可验收、可回滚的数字孪生最小闭环：

```text
上传一个 GLB
-> 后端保存 model asset / asset version / source variant / conversion job
-> 保存或查询 model manifest / object tree / model stats
-> 发布 Published asset version
-> seed scene / device instance / device model binding
-> 前端从后端加载 scene manifest 和 model manifest
-> 前端加载 GLB 并显示 object tree
-> edit 模式保存 movable part 和 motion target
-> monitor 模式只读 Published 配置并驱动 worldZ 动画
-> 后端不可用时 fallback 到当前本地模型能力
```

MVP 一期不做完整 CAD 自动转换、完整 3D Tiles 生产化、权限审批、真实 WCS 调度、SignalR 实时推送、多租户、GPU Picking、复杂路径规划。

## 2. 技术栈

| 层 | 技术 | 说明 |
|---|---|---|
| 前端 | Vue 3 + Vite + TypeScript + Three.js | 保留当前 `TwinDemo.vue`、`TwinScene.ts`、`LODModelLoader.ts` 能力 |
| 后端 API | ASP.NET Core Web API / .NET 8 | 统一响应、Swagger、异常处理中间件 |
| 后端数据 | EF Core 8 | PostgreSQL 优先，SQL Server 备选 |
| Worker | .NET Worker Service | MVP 只记录基础任务日志，不做完整 CAD 转换 |
| 数据库 | PostgreSQL / SQL Server | Provider 边界由 Infrastructure 控制 |
| 日志 | Serilog | API 与 Worker 共用基础日志规范 |

## 3. 目录结构

```text
07_src_3DModesys/
  AGENTS.md
  README.md
  idts3D_ui/
    AGENTS.md
    package.json
    src/
      api/                  # MVP-11 新增，统一 API Client
      types/                # 前端契约类型
      views/TwinDemo.vue
      engine/
  idts3D_api/
    README.md
    global.json             # MVP-01 创建
    HZ.IDTS.DigitalTwin.sln # MVP-01 创建
    src/
      HZ.IDTS.DigitalTwin.Api/
      HZ.IDTS.DigitalTwin.Application/
      HZ.IDTS.DigitalTwin.Domain/
      HZ.IDTS.DigitalTwin.Infrastructure/
      HZ.IDTS.DigitalTwin.Contracts/
      HZ.IDTS.DigitalTwin.Worker/
  idts3D_docs/
    development-rules.md
    idts-digital-twin-project-technical-plan.md
    idts-mvp-task-breakdown.md
    domain-entity-dto-map.md
    frontend-integration-plan.md
    backend-implementation-plan.md
    e2e-acceptance-plan.md
    api-contracts/
    mvp-tasks/
```

## 4. 前后端分层

后端分层：

```text
Api -> Application -> Domain
Api -> Contracts
Application -> Contracts
Application -> Domain
Infrastructure -> Domain
Infrastructure -> Application
Worker -> Application
```

禁止循环引用。`Contracts` 放 DTO、统一响应、错误码和枚举契约；`Domain` 放 Entity 和领域枚举；`Infrastructure` 放 EF Core、文件存储、Provider 切换、Repository 或 EF 查询；`Api` 放 Controller、中间件和 Swagger；`Worker` 在 MVP 只做 conversion job 基础日志。

前端分层：

```text
TwinDemo.vue -> src/api -> 后端 API
TwinDemo.vue -> TwinScene.ts -> LODModelLoader.ts / engine modules
src/types -> 页面、engine、api client 共用契约类型
```

页面不得直接散落 `fetch`。后端不可用时由 API Client 返回可识别错误，页面和 engine 决定 fallback。

## 5. 数据库核心实体

MVP 核心表固定为：

- `model_asset`
- `asset_version`
- `model_asset_variant`
- `model_conversion_job`
- `model_object_index`
- `asset_manifest`
- `scene_node`
- `device_instance`
- `device_model_binding`
- `movable_part_binding`
- `motion_target`
- `operation_audit`
- `tool_package`
- `tool_health_check`

详细字段、Entity、DTO、TypeScript interface、读写接口和前端消费位置见 `domain-entity-dto-map.md`。

## 6. API 契约规则

所有接口必须使用统一响应：

```json
{
  "success": true,
  "code": "OK",
  "message": "",
  "data": {}
}
```

错误响应：

```json
{
  "success": false,
  "code": "VERSION_STATUS_INVALID",
  "message": "monitor 模式只能读取 Published 版本。",
  "errors": []
}
```

统一路由：

- `POST /api/model-assets/upload`
- `GET /api/model-assets/{assetId}/manifest`
- `GET /api/model-assets/{assetId}/object-tree`
- `PUT /api/model-assets/{assetId}/versions/{versionId}/object-tree`
- `PUT /api/model-assets/{assetId}/versions/{versionId}/model-stats`
- `GET /api/model-assets/{assetId}/versions/{versionId}/model-stats`
- `POST /api/model-assets/{assetId}/versions/{versionId}/mark-ready`
- `POST /api/model-assets/{assetId}/versions/{versionId}/publish`
- `POST /api/model-assets/{assetId}/versions/{versionId}/archive`
- `POST /api/model-assets/{assetId}/versions/{versionId}/rollback`
- `GET /api/model-assets/{assetId}/versions/{versionId}/movable-parts`
- `POST /api/model-assets/{assetId}/versions/{versionId}/movable-parts`
- `PUT /api/model-assets/{assetId}/versions/{versionId}/movable-parts/{partId}`
- `DELETE /api/model-assets/{assetId}/versions/{versionId}/movable-parts/{partId}`
- `GET /api/movable-parts/{partId}/motion-targets`
- `POST /api/movable-parts/{partId}/motion-targets`
- `PUT /api/movable-parts/{partId}/motion-targets/{targetId}`
- `DELETE /api/movable-parts/{partId}/motion-targets/{targetId}`
- `GET /api/scenes/{sceneId}/manifest`
- `GET /api/model-conversion-jobs/{jobId}`

完整契约见 `api-contracts/README.md` 和各专题文档。

## 7. DTO 命名规则

| 类型 | 命名规则 | 示例 |
|---|---|---|
| 创建请求 | `Create{Name}Request` | `CreateMovablePartRequest` |
| 更新请求 | `Update{Name}Request` | `UpdateMotionTargetRequest` |
| 查询请求 | `{Name}QueryRequest` | `ModelStatsQueryRequest` |
| 响应 DTO | `{Name}Response` | `ModelManifestResponse` |
| 列表项 | `{Name}ListItemResponse` | `MovablePartListItemResponse` |
| 统一响应 | `ApiResponse<T>` | `ApiResponse<ModelManifestResponse>` |
| 错误项 | `ApiErrorItem` | `ApiErrorItem` |

DTO 放在 `HZ.IDTS.DigitalTwin.Contracts`。Controller 不直接返回 Entity。

## 8. TypeScript 类型同步规则

MVP 阶段可先手写 TypeScript interface，但必须遵守：

1. `idts3D_ui/src/types/api.ts` 放统一响应、错误、基础枚举。
2. `idts3D_ui/src/types/modelAsset.ts` 放 model asset、asset version、model manifest。
3. `idts3D_ui/src/types/modelObject.ts` 放 object tree、model stats。
4. `idts3D_ui/src/types/motion.ts` 放 movable part、motion target。
5. `idts3D_ui/src/types/sceneManifest.ts` 放 scene manifest。
6. 后端 DTO 字段变更时，同任务必须同步 TS interface 和 API Client。
7. 不允许页面临时定义与后端 DTO 重名但字段不同的类型。

后续如引入 OpenAPI 生成类型，必须由独立任务执行，不得混入 MVP-11。

## 9. MVP 执行顺序

1. MVP-00：开发规则切换与文档基线
2. MVP-01：后端解决方案骨架
3. MVP-02：数据库核心实体与 Migration
4. MVP-03：文件存储与 GLB 上传
5. MVP-04：Model Manifest 查询接口
6. MVP-05：Object-tree / Model-stats
7. MVP-06：资产版本状态与发布基线
8. MVP-07：可动部件配置 API
9. MVP-08：Motion Target API
10. MVP-09：场景 / 设备实例 / 设备模型绑定
11. MVP-10：Scene Manifest
12. MVP-11：前端 API Client 与契约类型
13. MVP-12：前端接后端 Manifest / Object-tree
14. MVP-13：前端 Edit 模式保存可动部件与目标点位
15. MVP-14：Monitor 模式只读配置并驱动 worldZ 动画
16. MVP-15：转换任务状态与基础日志
17. MVP-16：端到端联调与验收

`POC-3DT-01` 是 3D Tiles 技术验证支线，不进入 MVP 主链。

## 10. 每阶段验收门禁

| 阶段 | 必须通过的门禁 |
|---|---|
| MVP-00 | 文档链接无缺失、规则无冲突、未改业务代码 |
| MVP-01 | `dotnet build`，Swagger 可打开，Worker 可启动 |
| MVP-02 | migration 可生成并更新数据库，核心表和约束存在 |
| MVP-03 | Swagger 上传 GLB，文件落盘，四类记录入库 |
| MVP-04 | model manifest 支持 monitor / edit 状态规则 |
| MVP-05 | object tree / model stats 可保存和查询 |
| MVP-06 | mark-ready / publish / archive / rollback 状态正确 |
| MVP-07 | movable part CRUD 和版本状态 guard 正确 |
| MVP-08 | motion target CRUD、范围校验、重复校验正确 |
| MVP-09 | scene / device / binding seed 和 active 约束正确 |
| MVP-10 | scene manifest 只返回 active + Published |
| MVP-11 | `npm run build`，API Client 和类型集中封装 |
| MVP-12 | 后端可用优先读 manifest / object tree，后端不可用 fallback |
| MVP-13 | edit 保存 movable part / motion target，monitor 禁止保存 |
| MVP-14 | monitor 只读 Published 配置并执行 worldZ 动画 |
| MVP-15 | conversion job 查询和基础日志可用 |
| MVP-16 | 完整闭环、`npm run build`、`dotnet build` 全部通过 |

## 11. 禁止跨阶段事项

- MVP-01 不创建 Entity / Migration。
- MVP-02 不写业务 Controller。
- MVP-03 不做完整 CAD 转换。
- MVP-04 不让前端直接拼文件 URL。
- MVP-05 不要求 Worker 自动解析 GLB。
- MVP-06 不实现前端发布页面。
- MVP-07 不实现 motion target。
- MVP-08 不扩展 local axis / rotate / path / AGV path / joint。
- MVP-09 不做完整场景管理后台。
- MVP-10 不做 3D Tiles 生产化。
- MVP-11 不改 `TwinDemo.vue` 主流程，只建类型和 API Client。
- MVP-12 不保存 edit 配置。
- MVP-13 不改 monitor 动画策略。
- MVP-14 不开放 monitor 编辑能力。
- MVP-15 不做完整 Worker 队列系统。
- MVP-16 不新增功能，只联调和验收。

## 12. 端到端联调路径

完整联调顺序固定为：

1. 启动数据库。
2. 启动后端。
3. Swagger 上传 GLB。
4. 生成 asset / version / source variant / job。
5. 保存 object tree。
6. 保存 model stats。
7. mark-ready。
8. publish。
9. seed scene / device / binding。
10. GET scene manifest。
11. GET model manifest。
12. GET object tree。
13. 启动前端。
14. 前端优先从后端加载 scene manifest。
15. 前端加载 GLB。
16. 前端显示 object tree。
17. edit 模式设置 movable part。
18. edit 模式保存 motion target。
19. 刷新页面从后端读取配置。
20. monitor 模式只读。
21. monitor 模式点击目标点位后执行 worldZ 动画。
22. 关闭后端验证 fallback。
23. `npm run build` 通过。
24. `dotnet build` 通过。

详细验收脚本见 `e2e-acceptance-plan.md`。

## 13. Codex 执行规则

每次执行具体 MVP 任务时，Codex 必须：

1. 先读 `AGENTS.md`。
2. 先读 `idts3D_docs/development-rules.md`。
3. 先读本文档。
4. 先读当前任务卡。
5. 先读任务卡引用的 API 契约和实体映射。
6. 先输出影响范围。
7. 等待用户确认后再改文件。
8. 不 commit。
9. 不 push。
