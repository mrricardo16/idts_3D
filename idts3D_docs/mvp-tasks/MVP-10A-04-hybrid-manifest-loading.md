# MVP-10A-04：混合 Manifest 加载

> 状态：Blocked。依赖 MVP-10A-03。

目标：同步 API、DTO、TypeScript、API Client 与正式数据来源，消费 baseLayers + devices。禁止保留页面临时类型、把 Tiles 当设备或绕过 API Client。

验收：契约一对一，分层加载，GLB Object Tree 独立，失败可识别。回滚：恢复当前 devices 契约和 GLB-only 加载。Codex 提示词：仅依据 10A-01 冻结契约实施，先输出跨端影响并等待授权。
