# POC-3DT-01：Three.js + 3D Tiles + GLB 最小验证

> 状态：**Ready for review**。本卡不授权编码、依赖安装、POC 执行或生产接入。

关联文档：[ADR-001](../architecture/ADR-001-3dtiles-glb-hybrid-architecture.md)、[坐标规范](../architecture/coordinate-system-and-transform-spec.md)、[测试计划](../poc/POC-3DT-01-test-plan.md)、[结果模板](../poc/POC-3DT-01-result-report-template.md)、[性能预算](../performance/3d-performance-budget.md)、[回退方案](../operations/3dtiles-fallback-and-rollback-plan.md)。

## 1. 目标与门禁

在独立页面和独立 tiles engine 模块中验证 3D Tiles 静态底座与当前 GLB（或可识别 fallback）能否安全同场展示。验证结果为 MVP-10A 的直接门禁：POC 不阻塞 MVP-09、MVP-10 和无关纯后端任务，但 POC 结果报告完成、用户批准 Go 或获批 Conditional Go、相关文档审核通过前，MVP-10A 必须保持 Blocked。

POC 不修改 TwinDemo 主流程，不作为生产入口，不落库，不修改 API / DTO / TypeScript 正式契约，不做 CAD/IFC 转换或 3D Tiles 切片。

## 2. 前置条件

- 本轮文档审核通过且用户明确授权单独执行 POC。
- 阶段 A 使用公开、许可明确的小型 Tileset，并有当前 GLB 或 fallback。
- 阶段 B 使用获得授权的代表性中大型 Tileset 与当前提升机 GLB。
- 数据来源、许可、托管和是否可再分发均已记录；真实客户、公司或现场数据未经授权不得进入公开仓库。
- 已采用坐标规范、POC 测试计划和性能预算。

## 3. 预期实施范围（仅在另行授权后）

- 独立 POC 页面与独立 tiles engine 模块。
- POC 内的 Renderer、Camera、Controls、Raycaster、动画循环、加载状态和资源释放。
- idts3D_docs/poc 下的证据与结果报告。

## 4. 禁止范围

- TwinDemo、正式 TwinScene 主流程与生产入口。
- idts3D_api、数据库、Entity、DbContext、Migration、正式 API/DTO。
- 正式 Scene Manifest、正式 TypeScript 契约、device_instance / device_model_binding 数据建模。
- CAD/IFC 自动转换、生产切片平台、真实未授权数据、性能达标宣传。

## 5. 两阶段验证与必测项

| 阶段 | 数据 | 必测内容 |
|---|---|---|
| A | 许可明确的小型 Tileset + 当前 GLB 或 fallback | 加载链路、轴向/比例初校、GLB 共显、拾取、失败回退 |
| B | 获授权代表性中大型 Tileset + 当前提升机 GLB | 性能基线、坐标标定、worldZ 动画、生命周期和重复进出 |

必须记录并验证：

1. 场景原点、单位、Y-up / Z-up、平移、旋转、缩放和标定点；未知现场值记为 TBD - 待 POC 或现场资料确认。
2. GLB 与 Tiles 同场展示，GLB 拾取和高亮仍可用，Tiles 不被塞入 GLB 业务 Object Tree。
3. GLB worldZ 动画在 Tiles 存在时正常；Camera、Controls、Raycaster 和单一更新循环兼容。
4. Tiles 加载失败、请求取消、场景退出和重复进出时，GLB 仍可用且资源可释放。
5. 首屏、FPS、内存、网络请求等性能证据按测试计划采集；不得虚构数值。

## 6. POC 结论和产物

结论只能为 Go、Conditional Go 或 No-Go：

- Go：所有 Mandatory 用例有可复现证据，且达到已审核的 POC 基线。
- Conditional Go：存在边界条件或整改项；必须写明责任人、期限、风险与用户批准条件。
- No-Go：共显、可校准坐标、GLB 交互/动画、生命周期或可复现证据任一关键项失败。

执行后必须使用结果报告模板记录证据。Codex 可执行与记录，最终批准权属于用户或项目负责人。

## 7. 验收与回归

- 不改生产主流程；POC 页面与正式入口隔离。
- GLB/fallback、拾取、worldZ、相机控制和错误回退均有证据。
- 页面销毁后 animation frame、事件、请求、geometry、material、texture 与 Tiles 资源均按生命周期规范释放。
- 未产生数据库、API、DTO、TypeScript 正式契约或模型文件改动。

回滚仅删除经批准的独立 POC 页面/模块和受控测试配置；不得通过修改正式入口来“回滚”。

## 8. Codex 执行提示词

~~~text
请执行 POC-3DT-01。先读取项目规则、ADR-001、坐标规范、POC 测试计划、性能预算和本任务卡。
先输出影响范围并等待授权。本任务只能使用独立页面和独立 tiles engine；不得修改 TwinDemo、正式 TwinScene、后端、数据库、正式契约或模型文件。
阶段 A 使用许可明确的小型样本，阶段 B 仅在数据授权后执行。记录 Go / Conditional Go / No-Go 证据与结果报告；不要把 POC 结论当作用户批准。
~~~
