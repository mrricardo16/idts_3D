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
10A-01 决定记录、现有 `scenes.md`/Entity 映射、10A-03 图层接口、当前 SceneManifestService/Controller 和前端 API Client 基线。
## 7. 输出 / 交付物
同步的 Entity/DbContext/Migration（如冻结要求）、Service、Controller、DTO、API 契约、TS、API Client、TwinScene 消费、错误与兼容/回滚证据。
## 8. 允许修改范围
冻结结论指定的 `idts3D_api` Domain/Contracts/Application/Infrastructure/Api、`idts3D_ui/src/types`/`src/api`/engine、契约和测试；实施前列出精确文件。
## 9. 禁止修改范围
不得重定义冻结字段、把 Tiles 变为设备、让页面直连 API、修改 Tiles 业务属性、实施 CAD/IFC/生产切片或无关重构。
## 10. 现有文件
`api-contracts/scenes.md`、现有 Scene Manifest Controller/Service/DTO、`SceneNode`/`DeviceInstance`/`DeviceModelBinding` 映射、`TwinScene.ts`、`TwinDemo.vue`、10A-02/03 图层模块。
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
