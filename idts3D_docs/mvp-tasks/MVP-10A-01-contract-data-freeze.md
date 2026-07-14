# MVP-10A-01：混合契约与数据冻结

> 状态：Blocked。前置为 POC 获批准。

目标：冻结 baseLayers + devices、DTO、TypeScript、旧 tilesets 兼容、model_asset/asset_version 与 scene_resource/scene_layer 的最终决策。范围包含契约、数据库设计、兼容和回滚分析；禁止实现代码、Migration 或猜测未审核字段。

验收：所有跨端字段、版本、数据来源、错误与回滚路径可审查且一对一映射。回滚：保留当前 devices 契约并撤销未实施草案。Codex 提示词：先读取 POC 结果和设计文档，输出跨端影响，等待授权后只冻结契约。
