# MVP-03：文件存储与 GLB 上传

## 1. 任务目标

实现 GLB 上传、SHA256 计算、文件落盘、model asset / asset version / source variant / conversion job 入库。

## 2. 前置条件

- MVP-02 已完成。
- 数据库可连接。
- 文件存储根目录已配置。
- 已读取 `api-contracts/model-assets.md`。

## 3. 影响范围

- `idts3D_api/src/HZ.IDTS.DigitalTwin.Api/**`
- `idts3D_api/src/HZ.IDTS.DigitalTwin.Application/**`
- `idts3D_api/src/HZ.IDTS.DigitalTwin.Contracts/**`
- `idts3D_api/src/HZ.IDTS.DigitalTwin.Infrastructure/**`
- `idts3D_api/**/appsettings*.json`

## 4. 禁止修改范围

- 禁止做 CAD 转换。
- 禁止做 STEP / IFC。
- 禁止做 LOD / 3D Tiles。
- 禁止修改前端源码。
- 禁止跨任务实现 manifest 查询。

## 5. 后端变更

- Entity：使用 `ModelAsset`, `AssetVersion`, `ModelAssetVariant`, `ModelConversionJob`。
- DbContext：使用既有 DbSet。
- Migration：无，除非发现 MVP-02 缺字段则停止并回报。
- Controller：新增 `ModelAssetsController.Upload`。
- Application Service：新增上传用例。
- Infrastructure Repository / EF 查询：实现 hash 去重、文件保存。
- Request DTO：`UploadModelAssetRequest`。
- Response DTO：`UploadModelAssetResponse`。
- 校验规则：文件非空、扩展名 `.glb`、assetCode 必填、assetType 枚举。
- 错误码：`FILE_TYPE_NOT_ALLOWED`, `VALIDATION_FAILED`, `CONFLICT`。

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
- 读取表：`model_asset`。
- 写入表：`model_asset`, `asset_version`, `model_asset_variant`, `model_conversion_job`, `operation_audit`。
- 约束：`asset_code` 唯一，`source_file_hash` MVP 全局唯一。
- 索引：使用 MVP-02 已建索引。

## 8. API 契约

完整契约见 `idts3D_docs/api-contracts/model-assets.md#1-post-apimodel-assetsupload`。

## 9. 前后端对应关系

| 后端 Entity | DTO | API | 前端 TypeScript interface | 前端调用文件 | Vue / engine 消费位置 |
|---|---|---|---|---|---|
| `ModelAsset`, `AssetVersion`, `ModelAssetVariant`, `ModelConversionJob` | `UploadModelAssetRequest`, `UploadModelAssetResponse` | `POST /api/model-assets/upload` | MVP-11 创建 `UploadModelAssetRequest`, `UploadModelAssetResponse` | MVP-11 `src/api/modelAssets.ts` | 后续上传管理页，MVP 当前无页面 |

## 10. 执行步骤

1. 输出影响范围并等待确认。
2. 创建上传 Request / Response DTO。
3. 配置文件存储选项。
4. 创建文件存储服务。
5. 实现 SHA256 计算。
6. 实现 assetCode 和 hash 冲突处理。
7. 创建 `model_asset`。
8. 创建 Draft `asset_version`。
9. 创建 source `model_asset_variant`。
10. 创建 upload / inspect `model_conversion_job`。
11. 写入审计记录。
12. 在 Swagger 上传 GLB 验证。
13. 运行 `dotnet build`。

## 11. 验收标准

- Swagger 可上传 GLB。
- 非 GLB 返回 400。
- assetCode 重复返回 409。
- 文件落盘。
- 数据库出现 asset/version/variant/job。
- `dotnet build` 通过。
- 不需要 `npm run build`。
- 页面验证不执行。

## 12. 回归测试

- GLB 加载：不执行，本任务只后端上传。
- 对象树：不执行。
- 对象点击：不执行。
- 查看子级 / 父级：不执行。
- 异常高亮：不执行。
- 异常 callout：不执行。
- WASD / 鼠标视角：不执行。
- monitor / edit guard：通过接口只允许 edit/后台语义。
- localStorage fallback：不执行。
- worldZ 任务移动：不执行。
- 后端不可用时 fallback：不执行。
- 后端可用时优先走后端：后续 MVP-12 验证。

## 13. 风险点

- 大文件上传大小限制。
- Windows 路径和 URL 暴露混淆。
- hash 命中策略若不清晰会导致重复资产。

## 14. 回滚策略

删除上传接口、DTO、文件存储服务和测试上传文件；清理测试数据。

## 15. Codex 执行提示词

```text
请执行 MVP-03：文件存储与 GLB 上传。
先读取 AGENTS.md、idts3D_docs/idts-mvp-task-breakdown.md、idts3D_docs/api-contracts/model-assets.md、idts3D_docs/domain-entity-dto-map.md 和本任务卡。
先输出影响范围，等待我确认后再改。
只实现 POST /api/model-assets/upload，不做 manifest 查询、CAD 转换、LOD、3D Tiles，不修改前端源码。
完成后运行 dotnet build，并用 Swagger 上传 GLB 验证。
不要 commit，不要 push。
```
