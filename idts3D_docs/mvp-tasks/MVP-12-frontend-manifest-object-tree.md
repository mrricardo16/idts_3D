# MVP-12：混合 Manifest 分层加载与 GLB Object Tree

> 状态：正式执行依赖 MVP-10A-04；本卡不授权执行。

## 1. 目标与前置条件

消费审核后的 Scene Manifest，按 baseLayers 和 devices 分层加载，保持 GLB Object Tree 的业务语义。前置为 MVP-10A-04、当前 GLB API Client 和后端契约已完成。

## 2. 范围与禁止范围

范围为 TwinDemo、TwinScene、Loader、API Client 与类型的正式混合加载改动。禁止把 Tiles 内部节点塞入设备 Object Tree，禁止保存 Edit 配置、改变 worldZ 策略、移除 GLB fallback 或修改模型文件。

## 3. 加载链路与契约

~~~text
Scene Manifest
→ baseLayers
→ devices
→ TilesLayer / DeviceLayer 分层加载
→ GLB Object Tree
~~~

后端、DTO、TypeScript、API Client 仅按 MVP-10A-04 冻结契约同步；数据库不在本卡新增。

## 4. 步骤、验收与回归

扫描当前加载入口和对象树；加载 baseLayers 后加载 devices；隔离各层失败；后端不可用或 Tiles 失败时保留 GLB fallback；显示可识别数据来源。

验收：静态底座和 GLB 可分层加载，GLB Object Tree、拾取、高亮、WASD、异常和 worldZ 不回归，Tiles 失败时 GLB 保持可用。回滚恢复经验证 GLB-only 加载路径。

## 5. Codex 提示词

请确认 MVP-10A-04 已完成并读取冻结契约。只实现 baseLayers/devices 分层加载和 GLB Object Tree 边界；不得把 Tiles 节点作为设备业务树，先输出影响范围并等待授权。
