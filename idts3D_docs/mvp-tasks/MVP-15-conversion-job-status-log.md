# MVP-15：GLB 转换任务状态与基础日志

> 状态：按新依赖重写；本卡不授权执行。

## 1. 目标与前置条件

提供当前 GLB 资产转换 job 的查询与基础日志。前置为 MVP-03、MVP-02 和 conversion-jobs 契约。

## 2. 范围与禁止范围

范围是 ModelConversionJob 查询、基础状态、进度、消息和日志 URL。禁止 CAD/IFC 到 3D Tiles 生产切片、Tileset 工具链、完整 Worker 队列或未经授权前端状态页。

## 3. 验收、风险与回滚

验收上传后的 GLB job 可查询，400/404/409 清晰，失败消息和日志 URL 可用，不泄露物理路径。风险是把基础日志扩大为生产转换平台；回滚删除查询能力，保留 MVP-03 创建 job 的行为。

## 4. Codex 提示词

请读取 GLB conversion-jobs 契约和本卡。仅实现当前 GLB 基础 job 查询和日志，不实现任何 CAD/IFC/Tileset 切片或队列平台；先输出影响范围并等待授权。
