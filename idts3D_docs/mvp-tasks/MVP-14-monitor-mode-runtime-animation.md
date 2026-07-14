# MVP-14：Monitor 模式 GLB 动画与 Tiles 共存

> 状态：正式混合验收依赖 MVP-10A-05；本卡不授权执行。

## 1. 目标与前置条件

Monitor 从 Published 配置读取 GLB Movable Part / Motion Target，并驱动 worldZ 动画。前置为 MVP-10A-05、MVP-13 和已发布配置。

## 2. 范围与禁止范围

范围为 GLB Monitor、motion Client、worldZ 映射、告警和高亮验证。禁止开放 Monitor 编辑、驱动 Tiles 业务动画、实现路径/关节/真实任务系统、移除 F1/F2/F3/F4 fallback 或修改模型。

## 3. 步骤、验收与回归

加载 Published GLB 配置；在 TilesLayer 存在时执行 worldZ、告警和高亮；注入 Tiles 失败并验证 GLB Monitor 保持可用；保留后端不可用和硬编码目标点 fallback。

验收：Tiles 存在或失败均不破坏 GLB Monitor，Monitor 只读，GLB Object Tree、拾取、动画和 fallback 正常。回滚恢复 GLB-only Monitor 路径。

## 4. Codex 提示词

请确认 MVP-10A-05 已完成，读取运行时、回退与生命周期契约。只驱动 GLB worldZ；TilesLayer 不承载业务动画，先输出影响范围并等待授权。
