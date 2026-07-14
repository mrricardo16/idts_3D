# MVP-16：混合场景端到端联调与验收

> 状态：正式执行依赖 MVP-10A-05；本卡不授权执行。

## 1. 目标与前置条件

验收 3D Tiles 静态底座、GLB 动态设备、后端配置闭环、失败回退和资源释放。前置为 MVP-01～MVP-15、MVP-10A-01～05、已批准 POC 结论、数据库/后端/前端可启动及获授权测试数据。

## 2. 范围与禁止范围

只允许修复经确认的当前闭环问题；禁止新增功能、无关重构、模型修改、跳过失败项或把 POC/空占位当作正式验收。

## 3. 验收链路

~~~text
加载 3D Tiles 静态底座
→ 加载 GLB 动态设备
→ 校验位置、方向、比例和标定点
→ GLB Object Tree、拾取、高亮
→ Edit 保存 Movable Part / Motion Target
→ 发布并在 Monitor 只读加载
→ worldZ、告警和高亮
→ Tiles 与 GLB 持续共存
→ Tiles 失败时 GLB fallback
→ 场景切换/退出后请求取消和资源释放
~~~

## 4. 回归、风险、回滚与 Codex 提示词

回归覆盖 GLB、Object Tree、点击、层级、异常、WASD、Edit/Monitor、worldZ、Tiles 开关、失败注入、重复进入退出和后端 fallback。风险为契约漂移、坐标偏差、资源泄漏或 fallback 混淆；回滚优先关闭 TilesLayer 并恢复 GLB-only 路径。

请确认 MVP-10A-05、POC 批准和验收计划均已满足。按完整链路记录证据，不新增功能，先输出影响范围并等待授权。
