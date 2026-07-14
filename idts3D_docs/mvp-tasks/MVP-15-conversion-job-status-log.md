# MVP-15：转换任务状态与基础日志

## 1. 任务目标

实现 conversion job 查询和基础日志字段读取，MVP 不做完整 CAD 转换。

## 2. 前置条件

- MVP-03 已创建 `model_conversion_job`。
- MVP-02 conversion job 字段完整。
- 已读取 `api-contracts/conversion-jobs.md`。

## 3. 影响范围

- `idts3D_api/src/HZ.IDTS.DigitalTwin.Api/**`
- `idts3D_api/src/HZ.IDTS.DigitalTwin.Application/**`
- `idts3D_api/src/HZ.IDTS.DigitalTwin.Contracts/**`
- `idts3D_api/src/HZ.IDTS.DigitalTwin.Infrastructure/**`
- `idts3D_api/src/HZ.IDTS.DigitalTwin.Worker/**`

## 4. 禁止修改范围

- 禁止实现完整 CAD 转换。
- 禁止实现 STEP / IFC 转换。
- 禁止实现 3D Tiles 切片。
- 禁止实现完整 Worker 队列系统。
- 禁止修改前端源码，除非用户明确要求增加状态页。

## 5. 后端变更

- Entity：`ModelConversionJob`, `ModelAsset`, `AssetVersion`。
- DbContext：无结构变更。
- Migration：无，缺字段则停止并回报。
- Controller：`ModelConversionJobsController.GetById`。
- Application Service：查询 job 和日志 URL。
- Infrastructure Repository / EF 查询：按 jobId 查询。
- Request DTO：`GetConversionJobRequest`。
- Response DTO：`ConversionJobResponse`。
- 校验规则：jobId > 0。
- 错误码：`NOT_FOUND`, `VALIDATION_FAILED`, `CONFLICT`。

## 6. 前端变更

- TypeScript 类型：无，本任务默认不改前端。
- API Client：无，本任务默认不改前端。
- Vue 页面：无。
- Engine 层：无。
- fallback：无。
- 状态字段：无。
- UI 提示：无。

## 7. 数据库变更

- 新增表：无。
- 修改表：无。
- 读取表：`model_conversion_job`, `model_asset`, `asset_version`。
- 写入表：Worker 可更新 `model_conversion_job` 基础状态和日志字段。
- 约束：job status 枚举。
- 索引：job status、asset/version 外键索引。

## 8. API 契约

完整契约见 `idts3D_docs/api-contracts/conversion-jobs.md`。

## 9. 前后端对应关系

| 后端 Entity | DTO | API | 前端 TypeScript interface | 前端调用文件 | Vue / engine 消费位置 |
|---|---|---|---|---|---|
| `ModelConversionJob` | `GetConversionJobRequest`, `ConversionJobResponse` | `GET /api/model-conversion-jobs/{jobId}` | MVP-11 `ConversionJobDto` | MVP-11 `src/api/conversionJobs.ts` | 后续状态页，MVP 可不接 UI |

## 10. 执行步骤

1. 输出影响范围并等待确认。
2. 检查 conversion job 表字段。
3. 创建查询 DTO。
4. 实现 GET by id。
5. 返回 jobType、status、progress、message、日志 URL。
6. Worker 只补基础状态日志，不做转换。
7. Swagger 验证 200 / 400 / 404 / 409。
8. 运行 `dotnet build`。

## 11. 验收标准

- 上传后的 job 可查询。
- 不存在 job 返回 404。
- jobId 无效返回 400。
- failed job 可看到 message。
- 日志 URL 字段存在。
- `dotnet build` 通过。
- 不需要 `npm run build`，除非用户要求接前端状态页。

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

- 误把基础日志扩展成完整转换系统。
- job 状态和 finishedTime 不一致。
- 物理日志路径泄露到响应中。

## 14. 回滚策略

删除 conversion job 查询 Controller、Service、DTO；保留 MVP-03 创建 job 的能力。

## 15. Codex 执行提示词

```text
请执行 MVP-15：转换任务状态与基础日志。
先读取 AGENTS.md、idts3D_docs/idts-mvp-task-breakdown.md、idts3D_docs/api-contracts/conversion-jobs.md、idts3D_docs/domain-entity-dto-map.md 和本任务卡。
先输出影响范围，等待我确认后再改。
只实现 GET /api/model-conversion-jobs/{jobId} 和基础日志字段，不实现完整 CAD 转换、STEP/IFC、3D Tiles 或完整 Worker 队列。
完成后运行 dotnet build，并用 Swagger 验证。
不要 commit，不要 push。
```
# DOC-3DT-02 对齐说明

本任务的 conversion job 和日志仅覆盖当前 GLB 资产链路。未来 Tileset 生产工具链、CAD/IFC 到 3D Tiles 转换、生产级切片及其队列不得混入 MVP-15；它们属于 POC 后另行审核的生产化能力，不是当前 MVP 承诺。
