# MVP-10A-04：混合 Manifest 加载

> 状态：**Blocked**；依赖 MVP-10A-01 与 MVP-10A-03。

## 1. 任务状态
Blocked；仅按 10A-01 经审核冻结的决定实施。
## 2. 任务目标
让正式 Scene Manifest 以 `baseLayers + devices` 从数据来源、后端、API Client 到 TwinScene 一次同步落地。
## 3. 背景与问题
现有 scenes API 只支持 `devices` 且 `tilesets` 为空占位；正式静态资源关系、schemaVersion 和兼容策略尚不能由代码或草案猜定。
## 4. 前置条件
10A-01 冻结的 JSON/映射/数据与 Migration 结论，以及 10A-03 TilesLayer/坐标接入和回退证据均已验收。
## 5. 解锁条件
用户单独授权，并确认正式数据来源、旧客户端策略、Migration 方案（如需）和发布/回滚窗口。
## 6. 输入
10A-01 冻结报告、`api-contracts/scenes.md`/Entity 映射、10A-03 图层接口、下列真实 Model Manifest 基线，以及 Scene Manifest 计划新增边界。当前未检出 Scene Manifest Service、Controller、Response DTO 或前端 Manifest API Client。
## 7. 输出 / 交付物
同步的 Entity/DbContext/Migration（如冻结要求）、Service、Controller、DTO、API 契约、TS、API Client、TwinScene 消费、错误与兼容/回滚证据。
## 8. 允许修改范围
冻结结论指定的 `idts3D_api` Domain/Contracts/Application/Infrastructure/Api、`idts3D_ui/src/types`/`src/api`/engine、契约和测试；实施前列出精确文件。
## 9. 禁止修改范围
不得重定义冻结字段、把 Tiles 变为设备、让页面直连 API、修改 Tiles 业务属性、实施 CAD/IFC/生产切片或无关重构。
## 10. 现有文件
`api-contracts/scenes.md`（规范，不等于已实现端点）、`SceneNode`/`DeviceInstance`/`DeviceModelBinding` 映射、`TwinScene.ts`、`TwinDemo.vue`、10A-02/03 图层模块；以及本卡第 31 节列明的现有 Model Manifest 链路。Scene Manifest Controller/Service/DTO 不是现有文件。
## 11. 计划新增文件
仅可新增冻结决定要求的 Entity/EF Configuration/Migration、DTO、TS 类型、`src/api/scenes.ts` 与测试；候选路径/表名不得在本卡前假定存在。
## 12. 前端影响
本卡同步 `baseLayers`/`devices` 类型、集中 API Client 和 TwinScene 分层消费；先 baseLayers 后 devices，部分失败可见，Tiles 失败保持 GLB/fallback。
## 13. 后端影响
同步读取正式数据来源、组装 DTO、Controller 路由与统一错误响应；Controller 不直连 DbContext，Application 不依赖 Infrastructure。
## 14. 数据库影响
按 10A-01：若无需新表，明确复用来源；若需要，Entity、DbSet、EF Configuration、Migration、上线/回滚顺序必须成套。未冻结即停止。
## 15. API / DTO / TypeScript 契约
字段名、类型、可空性、schemaVersion、错误码、monitor/edit 权限、旧 `tilesets` 读取或废弃策略全部与 10A-01 一致，文档、DTO、TS 和 Client 同步。
## 16. 前后端一对一映射

| 数据来源 | Entity / 配置 | DTO | API 字段 | TypeScript | API Client | 前端消费位置 |
|---|---|---|---|---|---|---|
| 静态底座（10A-01 冻结） | 10A-01 冻结 | BaseLayer DTO | `baseLayers[]` | BaseLayer interface | scenes client | TilesLayer |
| 动态设备（既有） | SceneNode/DeviceInstance/Binding | device DTO | `devices[]` | device interface | scenes client | DeviceLayer |

## 17. 执行步骤
1. 复核冻结；2. 实施数据/Entity/Migration（如需）；3. 实施 Service/DTO/Controller；4. 同步契约/TS/Client；5. 分层消费；6. 注入部分失败；7. 验证旧客户端与回滚。
## 18. 数据准备
提供获授权的场景、静态资源引用、Published GLB binding 和可回滚配置；不使用未授权现场数据。
## 19. 构建命令
按仓库脚本运行 .NET restore/build/test 与前端 typecheck/lint/build；Migration 仅在明确授权环境执行，先 dry-run/脚本审查。
## 20. 自动化测试
覆盖 Service 映射、400/404/409、schema/兼容、部分失败、API Client、Tiles 失败保留 GLB 和 Migration 方向（如适用）。
## 21. 手工验证
验证 API 响应、baseLayers→devices 顺序、GLB Object Tree 独立、monitor/edit、旧客户端、数据/契约回退和网络错误可见。
## 22. 验收标准
映射表无断点；所有端使用同一冻结字段；静态/动态层隔离；Tiles 失败仍有 GLB；数据库变更（如有）可执行且可回滚。
## 23. 回归测试
现有 devices、GLB 加载/Object Tree/拾取/worldZ/fallback、MVP-11 基线和 10A-03 坐标/错误隔离均不回归。
## 24. 失败停止条件
字段或表未冻结、Migration 不可回滚、API/TS 漂移、旧客户端行为未定义、页面绕过 Client 或 Tiles 影响 GLB 时停止。
## 25. 风险
跨端版本不一致、发布顺序错误、数据迁移损失、部分失败被吞没和旧 tilesets 误解。
## 26. 回滚方案
按冻结的契约/数据顺序关闭新入口，回退 Migration/配置（如适用），恢复既有 `devices` 与 GLB-only；保留诊断证据。
## 27. 证据与报告
保存字段映射、迁移脚本/审查、API 响应、测试、HAR、错误注入、兼容测试与回滚记录。
## 28. 完成定义
正式 Manifest 完整同步且可回退；性能/生命周期总验收仍属于 10A-05。
## 29. 下一任务入口
MVP-10A-05，经用户单独授权执行回退、性能和生命周期闭环。
## 30. Codex 执行提示词
```text
请执行 MVP-10A-04。严格按 10A-01 冻结契约同步数据库（如需）、Entity、DbContext、Migration、Service、Controller、DTO、API、TS、Client 与 TwinScene。用 baseLayers→devices 分层加载，Tiles 失败保留 GLB；验证兼容与回滚，不得猜字段或绕过 API Client。
```

## 31. 当前事实、正式策略与跨端执行门禁

### 当前事实表

| 类型 | 当前真实存在 | 实际路径 / 符号 | 本任务处理 |
|---|---:|---|---|
| Scene Manifest Controller | 否 | 未检出 `ScenesController` | MVP-10A-01 冻结后计划新增；候选命名不得先行固定 |
| Scene Manifest Service | 否 | 未检出 `SceneManifestService` | MVP-10A-01 冻结后计划新增 |
| Scene Manifest Response DTO | 否 | 未检出 `SceneManifestResponse` | MVP-10A-01 冻结后计划新增 |
| Model Manifest Controller | 是 | `idts3D_api/src/HZ.IDTS.DigitalTwin.Api/Controllers/ModelAssetsController.cs`，`ModelAssetsController.GetManifest` | 保持 GLB model manifest 基线，不得伪装为 Scene Manifest |
| Model Manifest Service | 是 | `...Application/ModelAssets/IModelManifestService.cs`、`ModelManifestService.cs` | 仅作为 device `manifestUrl` 的既有依赖 |
| Model Manifest DTO | 是 | `...Contracts/ModelAssets/ModelManifestResponse.cs` | 不承载 `baseLayers` |
| Scene 相关 Entity | 是 | `...Domain/Entities/DigitalTwinEntities.cs`：`SceneNode`、`DeviceInstance`、`DeviceModelBinding` | `devices` 既有来源；不表达静态底座 |
| 前端 Manifest API Client | 否 | 未检出 `idts3D_ui/src/api/**` 或 `scenes.ts` | 10A-04 计划新增，名称/路径由 10A-01 冻结并在实施前核对 |
| TwinScene 消费入口 | 是 | `idts3D_ui/src/engine/TwinScene.ts`；页面 `src/views/TwinDemo.vue` | 10A-02/03 后按冻结输入扩展 |

### 正式实现策略

本卡**不得**在实施时选择方案。MVP-10A-01 冻结报告必须在“Model Manifest 与 Scene Manifest 关系”中选定并批准下列之一：

| 冻结值 | 10A-04 可执行规则 |
|---|---|
| A：扩展当前 Model Manifest 链路 | 仅当冻结报告明确说明其如何同时保持 GLB asset 语义和 scene 聚合边界时执行；否则停止 |
| B：保留 Model Manifest，新增独立 Scene Manifest 链路 | Model Manifest 继续供设备 `manifestUrl` 使用；新增 Controller/Service/DTO/API Client 仅按冻结名称、路由和字段实施 |
| C：既有其他真实链路 | 仅当冻结报告给出实际文件、符号和已验证职责时执行；当前代码扫描没有此链路 |

没有获批策略、字段、数据来源或命名时停止，不得根据类名、旧任务卡或 `scenes.md` 自行补全。

### 现有 / 计划新增映射

| 数据来源 | Entity / 配置 | DTO | API 字段 | TypeScript | API Client | 前端消费位置 | 状态 |
|---|---|---|---|---|---|---|---|
| 已发布 GLB 版本 | `ModelAsset`、`AssetVersion`、`AssetManifest` | `ModelManifestResponse` | 既有 model manifest 字段 | 当前 `src/types/modelManifest.ts` | 不适用：当前为本地 `ModelManifestLoader` | `LODModelLoader`、`TwinScene` | 现有 |
| 场景设备绑定 | `SceneNode`、`DeviceInstance`、`DeviceModelBinding` | Scene Manifest device DTO | `devices[]` | Scene Manifest device interface | scene client | `DeviceLayer` | MVP-10A-01 冻结；10A-04 计划新增 |
| 静态底座放置 | `scene_resource`/`scene_layer` 或冻结的复用来源 | BaseLayer DTO | `baseLayers[]` | BaseLayer interface | scene client | `TilesLayer` | MVP-10A-01 冻结；10A-04 计划新增 |
| 版本、兼容和错误 | 冻结的数据来源 | Scene Manifest response/error DTO | `schemaVersion`、错误码、旧 `tilesets` 策略 | 对应 TS 类型 | scene client | 数据来源/失败状态 UI | MVP-10A-01 冻结 |

### 跨端执行顺序

| 步骤 | 输入 → 输出 | 停止条件 | 回滚 |
|---:|---|---|---|
| 1 | 冻结报告 → 已批准策略/字段清单 | 无批准或未冻结项 | 不实施 |
| 2 | 真实 Model Manifest 链路 → 复用边界记录 | 误把 Model Manifest 当 Scene Manifest | 保持既有 GLB 链路 |
| 3 | 冻结策略 → 新增/复用文件清单 | 方案不匹配当前代码 | 不创建文件 |
| 4 | 冻结数据来源 → 可回滚配置/读取规则 | 来源未证明 | 不写数据 |
| 5 | 冻结表结论 → Entity/EF/Migration（如需） | Migration 未冻结或不可回滚 | 不生成 Migration |
| 6 | Entity/查询 → Application Service | Application 需引用 Infrastructure | 停止并重划边界 |
| 7 | Service → Controller/DTO | DTO/路由未冻结 | 不发布端点 |
| 8 | DTO → API 契约 | 契约与 DTO 不一致 | 恢复上一契约 |
| 9 | 契约 → TypeScript | 字段/可空性不一致 | 不接入页面 |
| 10 | TypeScript → 集中 API Client | 页面/engine 直连请求 | 删除未批准调用 |
| 11 | 解析输入 → TwinScene 分层消费 | Tiles 影响 GLB Object Tree/worldZ | 关闭 TilesLayer |
| 12 | 部分失败 → 可见 fallback | GLB 不可用或状态不可见 | GLB-only |
| 13 | 后端测试 → Service/Controller 契约证据 | 400/404/409/兼容失败 | 回滚新端点 |
| 14 | 前端测试 → Client/类型/图层证据 | 类型或失败隔离失败 | 回滚新 client/layer 接线 |
| 15 | 授权 PostgreSQL/Swagger → 真实环境证据 | 未获环境授权或验证失败 | 不上线/按 Migration 回滚 |
| 16 | E2E → 混合闭环报告 | 任一 Mandatory 失败 | GLB-only 并回到责任任务 |
