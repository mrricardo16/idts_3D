# POC-3DT-01 测试计划

> 状态：Ready for user execution approval。用户单独授权前不得执行 POC。

## 1. 数据、许可与环境

阶段 A 使用公开且许可明确的小型 Tileset 与当前 GLB 或 fallback；阶段 B 使用已获授权的代表性中大型 Tileset 与当前提升机 GLB。每个样本记录来源、许可证、再分发限制、授权证据、大小、瓦片数量和版本。真实客户或现场数据未经授权不得使用或提交。

测试环境和暂定阈值以 3d-performance-budget.md 为准。

## 2. 用例记录字段

每个用例必须记录：测试编号、阶段 A/B、Mandatory/Optional、前置条件、输入数据、操作步骤、预期结果、失败条件、执行次数、采样窗口、证据文件命名、问题编号和结论。

## 3. Mandatory 用例

| 编号 | 用例 | 核心预期 |
|---|---|---|
| POC-FUNC-001 | 小型 Tileset 加载 | 阶段 A 可观察加载成功或明确失败 |
| POC-FUNC-002 | GLB 同场展示 | Tiles 与 GLB/fallback 可共存 |
| POC-COORD-001 | 三个标定点 | 记录误差与可重复性 |
| POC-COORD-002 | Y-up / Z-up 与比例 | 显式变换可解释、可复现 |
| POC-INT-001 | GLB Raycaster 拾取 | GLB 设备可选中 |
| POC-INT-002 | 高亮与 Object Tree | GLB 交互不回归，Tiles 节点不进入设备树 |
| POC-ANIM-001 | worldZ 动画 | Tiles 存在时 GLB 动画正常 |
| POC-ERR-001 | Tiles 根 URL 失败 | GLB fallback 可用 |
| POC-ERR-002 | 子瓦片请求失败 | 错误隔离、GLB 可用 |
| POC-ERR-003 | Tiles 解析失败 | 错误可观察、GLB 可用 |
| POC-LIFE-001 | 连续进出 10 次 | 无重复循环/监听和持续泄漏 |
| POC-LIFE-002 | 场景切换与请求取消 | 陈旧请求不写入新场景 |
| POC-PERF-001 | 纯 GLB 对照 | 记录对照指标 |
| POC-PERF-002 | 阶段 A 冷/暖缓存 | 3 + 3 轮数据完整 |
| POC-PERF-003 | 阶段 B 冷/暖缓存 | 3 + 3 轮数据完整 |
| POC-LICENSE-001 | 数据许可检查 | 来源、许可和授权证据齐备 |

## 4. 结论

Go 要求全部 Mandatory 用例达到审核后的暂定基线并有可复现证据。Conditional Go 必须列出问题、责任人、期限、缓解措施和用户批准条件。关键共显、坐标、GLB 交互/动画、回退、生命周期或证据失败为 No-Go。Codex 仅给出建议，用户或项目负责人作批准结论。
