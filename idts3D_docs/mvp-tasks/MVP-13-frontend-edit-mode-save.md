# MVP-13：Edit 模式保存 GLB 动态设备配置

> 状态：正式混合验收依赖 MVP-10A-04；本卡不授权执行。

## 1. 目标与前置条件

Edit 模式保存 GLB 动态设备的 Movable Part 和 Motion Target，Monitor 继续只读。前置为 MVP-10A-04、MVP-07、MVP-08、MVP-11、MVP-12。

## 2. 范围与禁止范围

范围为 GLB 设备选择、可动部件、目标点、API Client 与错误展示。TilesLayer 只读；禁止修改 Tiles 节点业务属性、静态底座资源或场景放置关系，禁止 Monitor 保存、模型文件和本地 fallback 移除。

## 3. 步骤与验收

扫描 GLB 对象选择和 guard；保存 GLB Movable Part / Motion Target；处理 400/404/409；刷新后读取配置；后端不可用时可本地预览但不能伪装保存成功。

验收：Edit 只写 GLB 配置，Monitor 无保存入口，Tiles 存在时不污染底座，GLB Object Tree、worldZ 和 fallback 不回归。回滚撤回 Edit 保存 UI/API 调用，保留读取能力。

## 4. Codex 提示词

请读取 MVP-10A-04、Movable Part/Motion Target 契约和本卡。只修改 GLB 动态设备编辑链路；TilesLayer 必须只读，先输出影响范围并等待授权。
