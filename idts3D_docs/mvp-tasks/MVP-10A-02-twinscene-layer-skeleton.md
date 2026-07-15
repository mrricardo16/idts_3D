# MVP-10A-02：TwinScene 图层骨架

> 状态：**Blocked**；依赖 MVP-10A-01 的审核冻结结果。

## 1. 任务状态
Blocked。未获本卡用户授权不得实现。

## 2. 任务目标
在当前 `TwinScene` 中建立可隔离的 TilesLayer、DeviceLayer、AnnotationLayer、HelperLayer 骨架，并保持唯一 Renderer、Camera、Controls 与 Animation Loop。

## 3. 背景与问题
当前入口是 `idts3D_ui/src/views/TwinDemo.vue`，三维核心是 `idts3D_ui/src/engine/TwinScene.ts`，GLB 由 `LODModelLoader.ts`/`ModelManifestLoader.ts` 加载，交互由 `InteractionManager.ts` 处理；现有结构尚无正式图层边界。

## 4. 前置条件
10A-01 已冻结接口边界；当前 TwinScene 入口、渲染、控制、动画、GLB 加载、拾取和释放链已逐项核对。

## 5. 解锁条件
用户确认 10A-01 产物并单独授权本卡；不得以本任务书或 POC 草案替代冻结结果。

## 6. 输入
10A-01 跨端边界、现有 TwinScene/RendererManager/ControlsManager/InteractionManager/ResourceDisposer 代码、GLB 回归清单。

## 7. 输出 / 交付物
图层所有权图、实际改造/新增模块清单、只建骨架的接口、加载/卸载顺序、GLB 回归与重复循环检查证据。

## 8. 允许修改范围
仅前端引擎和必要的调用接线：实施前确认的 `TwinScene.ts`、图层模块、资源/交互封装和对应最小测试；不改后端或契约。

## 9. 禁止修改范围
不加载真实 Tiles、不安装库、不接正式 Manifest、不改数据库/API/DTO、GLB 业务语义、Object Tree、worldZ 或页面业务 UI。

## 10. 现有文件
`src/views/TwinDemo.vue`、`src/engine/TwinScene.ts`、`RendererManager.ts`、`ControlsManager.ts`、`InteractionManager.ts`、`LODModelLoader.ts`、`ResourceDisposer.ts`。

## 11. 计划新增文件
候选：`src/engine/layers/TilesLayer.ts`、`DeviceLayer.ts`、`AnnotationLayer.ts`、`HelperLayer.ts`、`CoordinateTransformer.ts`；路径、命名和是否新增须在实施前按当前目录确认。

## 12. 前端影响
本卡同步修改前端引擎骨架；`TwinDemo.vue` 仅允许最小生命周期接线。保持现有 GLB fallback、Object Tree、拾取、worldZ、Camera/Controls 行为。

## 13. 后端影响
明确不修改。不得请求 Scene Manifest 或新增 Controller/DTO。

## 14. 数据库影响
明确不修改；无表、DbContext、Migration、配置或数据准备。

## 15. API / DTO / TypeScript 契约
本卡只消费 10A-01 的抽象边界，不落地正式字段或 API Client；正式 TypeScript/API Client 同步推迟至 10A-04。

## 16. 前后端一对一映射
无运行时后端映射。记录 `DeviceLayer ← existing devices`、`TilesLayer ← future baseLayers (10A-04)`，不得把未来字段伪装为当前契约。

## 17. 执行步骤
1. 复核所有权；2. 提取四层及共享服务；3. 维持单一循环/渲染上下文；4. 让 DeviceLayer 复用现有 GLB 链；5. 设置空 TilesLayer；6. 接入页面卸载；7. 回归 GLB。

## 18. 数据准备
不需要 Tiles、后端或数据库数据；使用现有 GLB/fallback 回归样本。

## 19. 构建命令
以 `idts3D_ui/package.json` 与 lockfile 确认的包管理器运行 lint/typecheck/build；命令须在实施前输出，禁止为本卡安装依赖。

## 20. 自动化测试
为图层所有权、单一循环、卸载/取消和 DeviceLayer 不回归补充或运行现有最小测试；若项目缺少测试框架，记录缺口并执行构建检查。

## 21. 手工验证
加载现有 GLB/fallback；检查唯一 Renderer/Camera/Controls/Loop、GLB 拾取/Object Tree/worldZ、页面进出十次无重复循环。

## 22. 验收标准
目标树完整；所有者唯一；TilesLayer 为空时不影响 GLB；无真实 Tiles 请求；图层职责和销毁责任可追溯。

## 23. 回归测试
GLB 加载、fallback、Object Tree、点击、高亮、父子级、WASD/Controls、worldZ、告警和页面卸载均通过既有基线。

## 24. 失败停止条件
出现第二 Renderer/Camera/Controls/Loop、GLB 语义回归、需要契约/依赖未冻结，或无法证明卸载责任时停止。

## 25. 风险
将图层骨架演变为真实 Tiles 接入、重复 RAF、资源所有权不清和页面直接操作 scene graph。

## 26. 回滚方案
移除本卡图层骨架接线，恢复验证过的原 TwinScene GLB-only 结构；不涉及数据或接口回滚。

## 27. 证据与报告
保存所有权图、文件清单、构建/测试输出、重复进入记录、GLB 回归结果和 `git diff`。

## 28. 完成定义
图层骨架经独立验收且不加载 Tiles；不等同于坐标接入或正式 Manifest 完成。

## 29. 下一任务入口
MVP-10A-03；需 POC 批准的库/坐标证据和用户单独授权。

## 30. Codex 执行提示词
```text
请执行 MVP-10A-02。先核对 TwinScene 现有 Renderer/Camera/Controls/Loop、GLB 加载、拾取和释放链；只建立四层骨架及唯一所有权，不加载真实 Tiles、不改 API/数据库/GLB 业务语义。完成后回归 GLB 与页面重复进出，并等待下一卡授权。
```
