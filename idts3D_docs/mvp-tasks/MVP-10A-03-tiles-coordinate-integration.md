# MVP-10A-03：Tiles 与坐标接入

> 状态：Blocked。依赖 MVP-10A-02。

目标：接入 TilesLayer、显式坐标转换、GLB 同场与拾取隔离。禁止正式 Manifest、数据库、Tiles 业务编辑、将 Tiles 节点写入设备 Object Tree。

验收：三个标定点可复现，GLB 拾取/worldZ 不回归，Tiles 错误被隔离。回滚：关闭 TilesLayer，保留 DeviceLayer。Codex 提示词：按坐标规范和 POC 证据实施，先输出影响范围并等待授权。
