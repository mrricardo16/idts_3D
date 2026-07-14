# MVP-11：当前 GLB API Client 与契约类型基线

> 状态：按新依赖重写；本卡不授权执行。

## 1. 目标与前置条件

建立当前 GLB 资产、Model Manifest、Object Tree、Movable Part、Motion Target、conversion job 和当前 Scene Manifest devices 的 API Client 与 TypeScript 类型。前置为 MVP-03～MVP-10 当前契约稳定；正式混合类型属于 MVP-10A-01 与 MVP-10A-04，不属于本卡。

## 2. 范围与禁止范围

范围为 src/api 与当前 GLB 契约类型；不改 TwinDemo、TwinScene、LODModelLoader、模型文件或当前 fallback。禁止定义 baseLayers、正式 Tiles 类型、Scene Resource DTO，或猜测未来混合 Manifest 字段。

## 3. 变更与契约

后端、数据库、Migration 无变更。前端创建统一响应、错误、GLB/model/object/motion/current scene devices/conversion 类型，以及相应 API Client。类型只能来自当前 api-contracts 文档；页面不得散落请求。

## 4. 步骤、验收与回归

1. 核对当前 API 契约与字段。2. 创建类型和 httpClient。3. 创建领域 Client。4. 不改页面或 engine。5. 验证 400、404、409 和网络错误可识别。

验收：当前 GLB 契约类型与文档一致，Client 集中，未引入 baseLayers/Tiles/Scene Resource 类型，既有加载与 fallback 未改。回归覆盖 GLB、Object Tree、点击、worldZ 和 fallback 未改变。

## 5. 风险、回滚与 Codex 提示词

风险是当前 DTO 漂移或错误被吞没；回滚删除本卡新增 Client/类型，不影响既有页面。请先读取当前 API 契约和本卡，输出影响范围并等待实施授权；只实现当前 GLB/API Client 基线，不定义混合场景类型。
