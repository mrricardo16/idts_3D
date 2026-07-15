# MVP-10A-03：Tiles 与坐标接入

> 状态：**Blocked**；依赖 MVP-10A-02。

## 1. 任务状态
Blocked。只在 POC 结论已批准且用户单独授权后实施。

## 2. 任务目标
将 POC 批准的 3D Tiles 库接入 `TilesLayer`，以显式 `CoordinateTransformer` 让 Tiles 与 GLB 同场，并保持分层拾取和 GLB 业务隔离。

## 3. 背景与问题
坐标规范明确统一世界空间、变换顺序和三个标定点，但现场原点、轴、单位、比例、旋转、高程和容差仍可能为 TBD；库和版本只能取自获批 POC。

## 4. 前置条件
10A-02 已验收；获批 POC 报告包含库版本、许可证、样本来源、三个标定点、轴/单位/变换和失败行为。

## 5. 解锁条件
用户确认 POC 的 Go/Conditional Go 条件已满足，并授权该库、测试样本和坐标结论用于本卡。

## 6. 输入
POC 结果、坐标规范、性能预算、回退方案、10A-02 图层骨架、当前 GLB 拾取/worldZ 回归清单。

## 7. 输出 / 交付物
TilesLayer 加载/更新/隐藏/卸载和错误生命周期、CoordinateTransformer 输入输出、标定记录、分层拾取规则、测试数据许可记录和 GLB-only 回退证据。

## 8. 允许修改范围
前端 TilesLayer、CoordinateTransformer、InteractionManager 的最小分层扩展、受控测试入口/测试文件；路径按 10A-02 实际结构确认。

## 9. 禁止修改范围
不接正式后端 Manifest、数据库、Migration、API/DTO、生产配置、Tiles 编辑、Tiles Object Tree 或 CAD/IFC/生产切片。

## 10. 现有文件
`TwinScene.ts`、`InteractionManager.ts`、`RendererManager.ts`、`ResourceDisposer.ts`、`AnimationManager.ts`、`LODModelLoader.ts` 及 10A-02 实际图层模块。

## 11. 计划新增文件
候选为 `TilesLayer.ts` 实现、`CoordinateTransformer.ts` 实现、分层拾取测试和受控 Tiles 示例配置；不得预设为正式资产或生产文件。

## 12. 前端影响
本卡同步实现 Tiles/坐标；DeviceLayer 的 GLB 加载、Object Tree、worldZ、告警、高亮和 fallback 必须不变，Tiles 失败只影响 TilesLayer。

## 13. 后端影响
明确不修改。不得新增资源读取 API 或让前端请求正式 Scene Manifest。

## 14. 数据库影响
明确不修改；样本来源、版本与许可只记录在证据中，不写入业务数据库。

## 15. API / DTO / TypeScript 契约
不引入正式 API/DTO/TS Scene Manifest 字段；仅使用 10A-01 已定义的内部接口边界，运行时正式契约留给 10A-04。

## 16. 前后端一对一映射
无后端同步修改。映射表须写明：POC 受控输入 → TilesLayer/CoordinateTransformer；正式 `baseLayers` → 10A-04；`devices` → 既有 DeviceLayer，不交叉消费。

## 17. 执行步骤
1. 锁定获批库版本；2. 实现加载/更新/取消/卸载；3. 实现缩放→旋转→平移的显式转换；4. 用三点标定；5. 分层 Raycaster；6. 注入错误；7. 回归 GLB。

## 18. 数据准备
仅使用 POC 中许可明确的阶段 A/B 样本；记录来源、许可证、大小、版本和重现方法，禁止真实客户/现场数据。

## 19. 构建命令
使用现有锁定包管理器执行 typecheck/lint/build；新增库仅在 POC 已批准且用户授权后安装，并记录 lockfile 变化。

## 20. 自动化测试
覆盖变换顺序、三点数据的可重复计算、Tiles 失败隔离、取消陈旧请求和 Tiles 不注册为 GLB 拾取/Object Tree；执行现有 GLB 回归测试。

## 21. 手工验证
检查 Y-up/Z-up、单位、比例、平移、旋转、缩放、三点误差；同场展示；GLB 点击/worldZ/告警；根 tileset、子瓦片、解析和依赖异常回退。

## 22. 验收标准
三个标定点按 POC 批准标准可复现；Tiles 与 GLB 同场；Tiles 不进入 Object Tree；GLB worldZ/交互不回归；关闭 TilesLayer 后 GLB-only 可用。

## 23. 回归测试
执行 10A-02 全部 GLB 回归，以及冷暖缓存、场景切换、取消、连续进出和纯 GLB 对照记录。

## 24. 失败停止条件
库/版本或样本许可证未获批、任一标定点不可复现、GLB 业务回归、Tiles 错误扩散到 DeviceLayer、或需正式数据契约时停止。

## 25. 风险
坐标误差被视觉重合掩盖、动态瓦片资源泄漏、拾取污染、依赖升级和测试资产许可风险。

## 26. 回滚方案
关闭/卸载 TilesLayer，保留 DeviceLayer 和原 GLB 路径；回退到 POC 批准的依赖锁定版本，不改数据库。

## 27. 证据与报告
保存库/版本、许可证、三点表、变换输入输出、截图/录屏、HAR、错误日志、性能原始数据、回归和回退记录。

## 28. 完成定义
完成受控 Tiles/坐标技术接入且无需正式 Manifest；不等同于数据库/API/生产静态资源配置完成。

## 29. 下一任务入口
MVP-10A-04；需 10A-01 冻结的正式契约与用户单独授权。

## 30. Codex 执行提示词
```text
请执行 MVP-10A-03。只用获批 POC 的 3D Tiles 库、版本和许可样本，在 TilesLayer 接入显式坐标变换和分层拾取；验证三点、GLB worldZ/交互和失败隔离。不得接 API、数据库或正式 Manifest，失败时必须关闭 TilesLayer 并保留 GLB。
```
