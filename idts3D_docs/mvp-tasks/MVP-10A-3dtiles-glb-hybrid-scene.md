# MVP-10A：3D Tiles + GLB 正式混合场景接入

> 状态：**Blocked**。本总卡只编排独立任务，不授予 POC、实现、数据库或发布授权。

## 1. 总目标与业务价值

在不改变 GLB 动态设备业务语义的前提下，使场景能分层承载静态 3D Tiles 底座和 GLB 动态设备。3D Tiles 负责厂房、固定设施等只读静态底座；GLB `devices` 继续负责 Object Tree、Movable Part、Motion Target、Edit/Monitor、worldZ、告警、拾取和高亮。

本卡不包含 CAD/IFC 转换、生产切片、完整 Tiles 资产平台、未获授权的真实数据或现场性能承诺。

## 2. 解锁条件

以下条件必须全部具备，且由用户或项目负责人确认，才可逐卡解除 Blocked：

1. POC-3DT-01 已获单独执行授权、完成测试并形成完整结果报告；
2. POC 结论为用户批准的 Go，或带明确关闭条件的 Conditional Go；
3. ADR、坐标规范、混合架构、资源生命周期、Manifest 设计、性能预算和回退方案已审核；
4. MVP-09 与 MVP-10 的既有 GLB 设备/Scene Manifest 基线可用，或获批准的等效前置已记录；
5. 当前目标任务的上一张子卡已独立验收、回归并可回滚；
6. 每张实施任务都另行取得用户授权。计划文档完成不构成任何一项解锁。

## 3. 子任务依赖与输入输出

~~~text
POC-3DT-01 结果获批
        │
        └→ MVP-10A-01（冻结设计决定）
                  → MVP-10A-02（图层骨架）
                  → MVP-10A-03（Tiles/坐标）
                  → MVP-10A-04（正式 Manifest）
                  → MVP-10A-05（回退/性能/生命周期）
                  → MVP-11～MVP-16 正式混合闭环
~~~

| 子卡 | 输入 | 可验收输出 |
|---|---|---|
| 10A-01 | 获批 POC、现有 Scene Manifest、设计草案 | 冻结的跨端契约、数据来源和兼容/回滚决定 |
| 10A-02 | 10A-01 冻结结果、当前 TwinScene 入口 | 单一渲染上下文的图层骨架及 GLB 回归证据 |
| 10A-03 | 10A-02、获批 POC 库/样本/坐标证据 | 隔离 TilesLayer 与可复现坐标接入证据 |
| 10A-04 | 10A-01、10A-03 | 后端至 API Client 至 TwinScene 的正式 Manifest 闭环 |
| 10A-05 | 10A-04、性能预算与生命周期设计 | 回退矩阵、性能/释放证据及是否进入下游的结论 |

## 4. 跨任务不变量

- `baseLayers` 永远表达静态底座；`devices` 永远表达 GLB 动态设备。不得用 `device_instance` 或 `device_model_binding` 伪装静态底座。
- Tiles 内部节点不进入 GLB Object Tree，TilesLayer 不承载 worldZ、业务告警或 Edit 保存。
- `TwinScene` 只有一个 Renderer、Camera、Controls 和 Animation Loop；场景切换、请求取消和资源释放必须按资源所有者执行。
- `monitor` 仅读取 Published；`edit` 不得直接改变 monitor 的 Published 配置。
- 当前 `tilesets: []` 是兼容占位，不是正式 3D Tiles 能力或契约。
- 未获 POC、用户审核或现场资料确认的库版本、字段、表、坐标和性能结果必须保持 TBD 并注明归属。

## 5. 与其他 MVP 的关系

MVP-09/10 提供 GLB 设备与当前 `devices` 基线；MVP-11 仅建立现有 GLB API Client/类型基线；MVP-12 依赖 10A-04 才消费正式混合 Manifest；MVP-13 仅编辑 GLB；MVP-14 依赖 10A-05 验证 Tiles 共存下的 GLB Monitor；MVP-15 不扩展为 Tiles 生产切片；MVP-16 以本链路证据进行正式验收。

## 6. 总体验收、失败处理与回滚

总体验收必须证明：静态底座与 GLB 同场、三个标定点可复现、GLB Object Tree/拾取/worldZ/告警未回归、Tiles 故障时 GLB-only 可用、场景切换和退出无重复循环或资源残留。任一关键坐标、交互、回退、生命周期或证据项失败即停止进入下游，回到责任子卡处理。

总体回滚优先关闭或卸载 TilesLayer，回到经验证的 GLB-only 入口；契约、配置、依赖和数据库回滚只能按 10A-01/04/05 已批准的具体方案执行。

## 7. 用户逐卡授权与完成定义

用户须先批准 POC，再对每张 10A 子卡分别授权。总卡完成仅在五张子卡均通过独立验收、回归、回滚演练并由 MVP-10A-05 输出总体验收报告后成立；此时才可由用户决定是否进入 MVP-11～MVP-16。

## 8. Codex 执行提示词

```text
请执行当前获授权的一张 MVP-10A 子卡。先确认 POC 结果和用户批准、读取冻结契约与上游证据，输出受影响层、禁止范围、验证、失败停止和回滚。不得跳卡、不得实施 CAD/IFC 或生产切片，不得把 Tiles 当设备，也不得解除其他任务门禁。
```
