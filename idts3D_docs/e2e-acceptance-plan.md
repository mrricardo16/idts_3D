# 端到端联调与验收计划

本文档定义 MVP-16 的完整闭环验收路径。MVP-16 不新增功能，只联调和验证 MVP-01 到 MVP-15 的结果。

## 1. 前置条件

- MVP-01 到 MVP-15 已完成。
- `idts3D_api` 可 build。
- `idts3D_ui` 可 build。
- 有一个可用 GLB 文件。
- 数据库连接可用。
- 后端 Swagger 可访问。

## 2. 完整闭环步骤

| 序号 | 步骤 | 验收点 |
|---:|---|---|
| 1 | 启动数据库 | PostgreSQL 优先，连接串可用 |
| 2 | 启动后端 | Swagger 可打开，健康日志无错误 |
| 3 | Swagger 上传 GLB | `POST /api/model-assets/upload` 返回 assetId/versionId/jobId |
| 4 | 生成 asset/version/source variant/job | 数据库出现 `model_asset`, `asset_version`, `model_asset_variant`, `model_conversion_job` |
| 5 | 保存 object tree | `PUT object-tree` 成功，`model_object_index` 有记录 |
| 6 | 保存 model stats | `PUT model-stats` 成功，`asset_manifest.model_stats_json` 有内容 |
| 7 | mark-ready | `asset_version.version_status=Ready` |
| 8 | publish | `asset_version.version_status=Published`，旧 Published 归档 |
| 9 | seed scene/device/binding | `scene_node`, `device_instance`, `device_model_binding` 有 active 绑定 |
| 10 | GET scene manifest | 只返回 active + Published |
| 11 | GET model manifest | 返回后端生成 GLB URL |
| 12 | GET object tree | 返回后端保存的 object tree |
| 13 | 启动前端 | `npm run dev` 页面可打开 |
| 14 | 前端优先从后端加载 scene manifest | 网络面板看到 scene manifest 请求 |
| 15 | 前端加载 GLB | 3D 模型显示 |
| 16 | 前端显示 object tree | 对象树来自后端或有明确 fallback 提示 |
| 17 | edit 模式设置 movable part | 选择对象并保存 movable part |
| 18 | edit 模式保存 motion target | 保存 F1/F2/F3/F4 或测试目标点 |
| 19 | 刷新页面从后端读取配置 | movable part 和 motion target 不丢失 |
| 20 | monitor 模式只读 | 保存入口隐藏或禁用 |
| 21 | monitor 点击目标点位执行 worldZ 动画 | 载货台沿 worldZ 移动 |
| 22 | 关闭后端验证 fallback | 页面不崩溃，显示 fallback 提示 |
| 23 | `npm run build` 通过 | TypeScript 和 Vite build 通过 |
| 24 | `dotnet build` 通过 | 后端 solution build 通过 |

## 3. Swagger 验收命令和接口

必须逐个验证：

- `POST /api/model-assets/upload`
- `PUT /api/model-assets/{assetId}/versions/{versionId}/object-tree`
- `PUT /api/model-assets/{assetId}/versions/{versionId}/model-stats`
- `POST /api/model-assets/{assetId}/versions/{versionId}/mark-ready`
- `POST /api/model-assets/{assetId}/versions/{versionId}/publish`
- `GET /api/scenes/{sceneId}/manifest`
- `GET /api/model-assets/{assetId}/manifest`
- `GET /api/model-assets/{assetId}/object-tree`
- `POST /api/model-assets/{assetId}/versions/{versionId}/movable-parts`
- `POST /api/movable-parts/{partId}/motion-targets`
- `GET /api/model-conversion-jobs/{jobId}`

## 4. 前端页面验收

必须验证：

- GLB 加载。
- 对象树显示。
- 对象点击定位。
- 查看子级 / 父级。
- 异常高亮。
- 异常 callout。
- WASD / 鼠标视角。
- monitor / edit guard。
- localStorage fallback。
- worldZ 任务移动。
- 后端不可用时 fallback。
- 后端可用时优先走后端。

## 5. 失败处理

| 失败 | 判断 | 处理 |
|---|---|---|
| 上传失败 | 400 / 409 | 检查文件类型、assetCode、hash 策略 |
| manifest 404 | 后端无 manifest | 回到 MVP-04 或 MVP-03 修复 |
| object tree 不一致 | 后端 tree 与 runtime tree 不匹配 | 回到 MVP-12 修复映射 |
| monitor 可编辑 | guard 失效 | 回到 MVP-13 / MVP-14 修复 |
| worldZ 不动 | motion target 或 engine 映射失败 | 回到 MVP-14 修复 |
| fallback 失败 | 后端关闭后页面崩溃 | 回到 MVP-12 修复 API 错误处理 |

## 6. 完成输出

MVP-16 完成后必须输出：

- 后端 build 结果。
- 前端 build 结果。
- Swagger 接口逐项结果。
- 页面回归逐项结果。
- fallback 验证结果。
- 未完成项。
- `git status` 摘要。
- `git diff --stat` 摘要。
