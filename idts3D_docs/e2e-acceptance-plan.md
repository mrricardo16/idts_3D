# 端到端联调与验收计划

> 本计划定义 MVP-16 的正式混合场景验收；不以 POC、空 tilesets 占位或单纯构建成功代替正式验收。

## 1. 前置条件

MVP-01～MVP-15 和 MVP-10A-01～05 已完成；POC-3DT-01 结果已获用户批准；数据库、后端、前端和授权测试数据可用；坐标、性能、回退和生命周期证据可复核。

## 2. 端到端路径

1. 加载正式 baseLayers 静态 3D Tiles 底座。
2. 加载 devices GLB 动态设备。
3. 校验三个标定点、位置、方向、比例和轴向。
4. 验证 GLB Object Tree、拾取、高亮、层级查看、异常 callout、WASD/Controls。
5. Edit 保存 GLB Movable Part / Motion Target，发布配置。
6. Monitor 只读加载，执行 GLB worldZ，验证告警与高亮。
7. 验证 Tiles 与 GLB 持续共存。
8. 分别注入 Tiles 根 URL、子瓦片和解析失败，验证 GLB-only fallback。
9. 验证场景切换、请求取消、连续进入/退出和资源释放。

## 3. 失败处理与输出

坐标偏差、契约漂移、GLB 交互回归、Tiles 失败未隔离、资源泄漏或性能不满足门槛均为未通过，回到对应 MVP-10A 子任务或下游任务修复。完成报告必须包括 API/页面证据、纯 GLB 对照、性能与内存导出、Network HAR、回退结果、未通过项、Git 状态和 diff 摘要。
