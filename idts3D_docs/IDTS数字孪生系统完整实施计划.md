# IDTS 数字孪生系统完整实施计划

## DOC-3DT-02 近期执行边界

本计划的长期 3D Tiles 生产化目标保持，但近期先完成受控 POC：3D Tiles 作为静态厂区底座，GLB 作为动态设备和业务交互对象。POC 验证同场展示、坐标、拾取、worldZ、生命周期、错误回退和性能证据；不落库、不做 CAD 切片、不提交未授权现场数据。

POC 不阻塞 MVP-09 和 MVP-10；它阻塞 MVP-10A。正式接入后的 Scene Manifest 方向为 baseLayers + devices，静态底座不属于 device_instance / device_model_binding。最终资源表结构、坐标参数与生产阈值须在 POC 和现场资料后审核，不能视为已实施。


> 文档定位：正式项目建设计划，覆盖项目启动、需求冻结、开发排期、接口联调、现场实施、验收交付和上线运维。  
> Markdown 版本用途：作为后续执行计划、任务拆分、阶段验收和排期跟踪依据。  
> Word 版本用途：作为立项评审、项目汇报、实施沟通和验收评审材料。

## 1. 项目概述

IDTS 数字孪生系统面向智能仓储、自动化物流和厂区级三维监控场景建设，目标是形成一套覆盖厂区空间、设备运行、任务过程、库存货位、告警事件、统计报表和现场运维的正式数字孪生系统。系统通过统一三维场景将厂区、楼栋、楼层、区域、设备、货位和物资进行可视化表达，并通过 WCS / WMS / MES / ERP 数据联动，把现场设备状态、任务进度、告警、库存和效率指标映射到三维场景和业务看板中。

项目建设目标包括：

- 建设智能仓储三维可视化底座，实现厂区、楼栋、楼层、区域、设备、货位、物资的统一三维呈现。
- 建立 GLB 设备模型与 3D Tiles 静态厂区层的协同展示体系，支持现场运行状态和业务过程呈现。
- 建立 WCS / WMS / MES / ERP 数据联动机制，支撑设备状态、任务过程、告警、库存和报表可视化。
- 建立模型资产、模型版本、可动部件、任务目标点位和场景设备实例的正式管理能力。
- 支持项目现场交付、运行维护、版本升级、发布回滚和后续扩展。

系统最终建设形态为：前端三维可视化与业务管理界面、后端 Web API、模型资产处理 Worker、数据库、文件存储、接口适配服务、部署运维体系共同组成的数字孪生项目平台。

## 2. 建设范围

### 2.1 一期范围

一期目标是完成模型资产管理、GLB 设备加载、基础配置闭环和单设备状态最小闭环，为后续多设备接入、业务联动和现场交付建立工程基础。

| 类别 | 计划建设内容 | 不纳入本期内容 | 前置条件 | 验收标准 |
|---|---|---|---|---|
| 后端基础 | .NET 8 后端基础架构、分层结构、统一响应、错误处理、Swagger、EF Core 8 接入 | 完整业务平台治理 | 技术栈确认、数据库选型确认 | 后端接口可启动、接口文档可访问、基础健康检查通过 |
| 数据库核心表 | model_asset、asset_version、asset_manifest、model_object_index、device_instance、device_model_binding、movable_part_binding、motion_target、operation_audit 等核心表 | 全量报表、权限审批、复杂任务调度表 | ER 草案冻结 | Migration 可执行，核心表和约束可验证 |
| GLB 模型上传 | GLB 上传、文件存储、模型 hash 去重、资产元数据入库 | CAD 全格式自动转换 | 文件存储策略、上传大小限制 | 同一文件重复上传可识别，资产记录可查询 |
| manifest | manifest 查询、版本 manifest 保存、前端按 manifest 加载模型 | 多区域复杂 manifest 编排 | 资产版本状态定义 | 前端可按后端 manifest 加载 GLB |
| object-tree / model-stats | object-tree 保存与查询、model-stats 保存与查询 | 自动语义识别和复杂模型修复 | GLB 样例和节点命名规则 | 对象树可展示，模型统计可作为发布前检查依据 |
| 可动部件 | 可动部件配置、partCode、motionType、axisMode、min / max / home | 自动识别全部机械结构 | object-tree 可用 | 可保存、查询并校验可动部件绑定 |
| motion target | 目标点位配置、排序、启用状态、目标值或三维目标点 | 复杂运动学求解 | 可动部件配置可用 | 可保存并用于运行过程呈现 |
| 场景绑定 | 场景、设备实例、设备模型绑定、设备位置和旋转配置 | 多厂区多租户治理 | 场景编码和设备编码确认 | 场景 manifest 可返回设备和模型绑定 |
| 前端加载 | 前端从后端加载 manifest、object-tree、设备模型绑定 | 复杂大屏和报表 | 一期接口可用 | 前端可从后端数据源加载模型和对象树 |
| 模式边界 | monitor / edit 模式边界、只读与配置操作区分 | 全量权限审批流 | 操作角色和页面边界确认 | 监控视图不修改配置，配置视图保存草稿配置 |
| 单设备闭环 | 提升机或单设备 WCS mock / 实时状态最小闭环 | 多设备全量联动 | 单设备状态字段和坐标规则 | 状态数据可驱动模型位置、状态和告警展示 |

### 2.2 二期范围

二期目标是完成 WCS 设备状态正式对接和多类设备状态映射，形成现场运行监控能力。

- WCS 设备状态正式对接，包括设备状态、坐标、任务状态、告警状态、货物信息和时间戳。
- 堆垛机、AGV、提升机、输送线状态映射。
- 任务过程可视化，包括任务号、起点、终点、路径结果、任务阶段和执行状态。
- 告警高亮和告警详情，包括设备高亮、弹窗、callout 和历史告警记录。
- 货位状态和物资查询，包括货位空 / 满 / 锁定 / 异常、托盘、SKU、规格、数量和订单信息。
- 基础大屏视图，包括设备运行概览、货位利用率、告警统计和任务完成情况。

### 2.3 三期范围

三期目标是完成生产化扩展、历史回放、报表统计、厂区底座生产化和运维治理。

- 3D Tiles 厂区静态底座生产化，覆盖厂区、建筑、楼层、固定货架、地面和管廊等静态对象。
- CAD / BIM / IFC / STEP 模型处理流程，包括文件校验、转换、优化、派生资产和版本发布。
- 模型优化、LOD、压缩、派生资产和发布前性能门禁。
- 多区域加载和分区资源释放。
- 历史回放，包括设备轨迹、任务过程、告警事件和关键指标回放。
- 报表统计，包括设备效率、货位周转、故障统计、库存快照和月度统计。
- 权限、审计、发布回滚、部署运维监控和工具链健康检查。

### 2.4 明确不属于默认范围

| 默认范围外事项 | 边界说明 | 纳入条件 |
|---|---|---|
| AGV 调度算法 | 不默认承担 AGV 调度算法 | 需另立算法专项，明确输入、输出、验收和责任主体 |
| 路径规划算法 | 不默认承担路径规划算法 | 需第三方调度系统或 WCS 提供路径结果；如需本系统计算需另行立项 |
| 冲突规避算法 | 不默认承担冲突规避算法 | 由第三方调度系统 / WCS / 上游系统提供冲突处理结果 |
| 任务分配算法 | 不默认承担任务分配算法 | 上游系统提供任务分配结果，本系统负责展示和追踪 |
| 产能优化算法 | 不默认承担产能优化算法 | 上游系统提供产能分析结果，本系统负责呈现指标 |
| 全格式 CAD 自动转换 | 不默认支持所有 CAD 商业格式自动转换 | 需按格式、工具授权、转换质量和现场流程单独评估 |
| 远程控制指令 | 不默认下发远程控制指令 | 需单独确认安全边界、权限、二次确认、审计和应急机制 |

## 3. 总体技术架构

系统采用前后端分离、模型资产集中管理、运行数据接口适配、三维场景实时更新的总体架构。前端采用 Vue 3 + TypeScript + Three.js；后端采用 C# / ASP.NET Core Web API / .NET 8 / EF Core 8；三维场景采用 GLB 设备层 + 3D Tiles 静态厂区层；模型资产处理通过 Worker 和工具链完成；现场数据通过 WCS / WMS / MES / ERP 接口接入。

```text
浏览器前端
  -> Vue 3 + TypeScript 管理界面
  -> Three.js 场景实时更新
  -> GLB 设备层
  -> 3D Tiles 静态厂区层
  -> RuntimeStateAdapter 状态映射
API 网关 / 后端接口
  -> ASP.NET Core Web API / .NET 8
  -> 业务服务
  -> 数据库
  -> 文件存储 / 对象存储
  -> Worker 模型资产处理
  -> WCS / WMS / MES / ERP 接口适配
```

| 架构层 | 建设内容 | 关键职责 | 验收标准 |
|---|---|---|---|
| 前端架构 | Vue 3 + TypeScript + Three.js | 场景渲染、设备展示、交互、查询、告警、大屏、回放 | 页面可加载场景，设备状态可映射，交互响应满足指标 |
| 后端架构 | C# / ASP.NET Core Web API / .NET 8 | 资产、场景、设备、接口、报表、审计服务 | API 分层清晰，接口契约稳定，错误返回统一 |
| 数据库架构 | EF Core 8 管理核心数据表 | 模型资产、版本、对象树、设备实例、绑定、审计 | 表结构、索引、约束、Migration 可验收 |
| 模型资产架构 | GLB、3D Tiles、manifest、object-tree、model-stats | 资产版本、派生资产、发布回滚、性能门禁 | 资产可上传、校验、发布、回滚 |
| 接口架构 | WCS / WMS / MES / ERP 接口适配 | 状态、任务、库存、生产、报表数据接入 | 接口字段、频率、错误处理和补传机制明确 |
| Worker 架构 | 后台模型资产处理任务 | 模型检查、统计、转换、LOD、日志、重试 | 长任务不阻塞 API，失败可追踪 |
| 文件存储架构 | 本地文件存储或对象存储 | 保存原始模型、派生资产、日志、离线工具包 | 文件可定位、可校验、可备份 |
| 部署架构 | 开发、测试、生产环境 | 前端、后端、Worker、数据库、存储、监控 | 可安装、可升级、可回滚、可恢复 |

## 4. 三维模型建设计划

### 4.1 静态厂区模型

静态厂区模型主要承载空间背景和固定结构，推荐采用 3D Tiles 作为生产化底座。

| 对象 | 建设方式 | 数据要求 | 验收标准 |
|---|---|---|---|
| CAD 规划建筑物 | CAD / BIM / IFC / STEP 转换为 3D Tiles 或静态 GLB | 建筑轮廓、楼层高度、坐标基准 | 场景比例和位置与现场规划一致 |
| 厂房 | 3D Tiles | 厂房边界、高度、门洞、主要通道 | 可分区加载，视角切换不卡顿 |
| 楼栋 | 3D Tiles | 楼栋编码、楼层关系、坐标 | 支持楼栋定位和显隐控制 |
| 楼层 | 3D Tiles 或分层静态资产 | 楼层编号、高度、平面范围 | 支持楼层切换和区域定位 |
| 地面 | 3D Tiles | 地面边界、路径、区域划分 | 坐标系与设备层一致 |
| 固定货架 | 3D Tiles 或低精度 GLB | 货架编号、库位布局 | 与货位编码对应，可用于查询高亮 |
| 管廊 | 3D Tiles | 管廊位置、层级、固定结构 | 不影响设备层拾取和性能 |
| 固定结构 | 3D Tiles | 固定设备基础、围栏、站台 | 与动态设备接驳点对齐 |

### 4.2 动态业务设备模型

动态业务设备需要支持点击、状态映射、异常高亮、可动部件绑定和任务过程呈现，推荐采用独立 GLB。

| 设备 | 推荐格式 | 建设重点 | 验收标准 |
|---|---|---|---|
| 提升机 | GLB | 载货台、升降部件、高度轴、任务状态、异常状态 | WCS 高度数据可驱动载货台位置变化 |
| 堆垛机 | GLB | X/Y/Z 坐标、货叉伸缩、取货、放货、待机位 | 坐标和货叉动作可按状态数据呈现 |
| AGV | GLB | 坐标、方向、速度、电量、路径点、任务状态 | 坐标和路径结果可驱动轨迹展示 |
| 输送线关键段 | GLB | 线段状态、启停、堵塞、故障、货物经过 | 线段状态可颜色化，异常可提醒 |
| 机械手 | GLB | 关节、动作状态、取放点、异常 | 可根据接口数据展示动作过程 |

### 4.3 模型资产处理流程

| 流程步骤 | 处理内容 | 输出 | 验收标准 |
|---|---|---|---|
| 上传 | 上传 GLB、3D Tiles 包或待转换模型 | 原始文件记录 | 文件大小、格式、hash 可校验 |
| 文件校验 | 检查格式、扩展名、大小、hash、完整性 | 校验结果 | 异常文件被拒绝并返回明确原因 |
| hash 去重 | 根据 sourceFileHash 判断重复资产 | 去重结果 | 重复上传可关联既有资产 |
| 版本创建 | 为资产创建版本记录 | asset_version | Draft / Ready / Published 等状态清晰 |
| object-tree | 提取或保存模型对象树 | model_object_index | 对象可定位、可绑定、可展示层级 |
| model-stats | 统计模型大小、mesh、材质、贴图、顶点、三角面、draw call 估算 | model-stats | 超预算模型可提示 |
| manifest | 生成前端加载清单 | asset_manifest | 前端可按版本加载资源 |
| 派生资产 | 生成 high / medium / low、LOD、压缩资产、tileset | 派生资产文件 | 资源可访问、可回滚 |
| 发布 | 将 Ready 版本发布为 Published | 发布记录 | 发布写入审计，不破坏旧版本 |
| 回滚 | 切换到历史可用版本 | 回滚记录 | 回滚后前端加载稳定 |
| 审计 | 记录上传、转换、配置、发布、回滚操作 | operation_audit | 操作人、时间、内容可追溯 |

### 4.4 模型命名与绑定规范

| 字段 / 规则 | 用途 | 要求 | 验收标准 |
|---|---|---|---|
| objectUuid | 模型对象唯一标识 | 版本内唯一，优先稳定 | 可用于对象绑定和失效检测 |
| objectName | 模型节点名称 | 保留设备和部件可识别名称 | 前端对象树可读 |
| objectPath | 对象路径 | 反映层级关系 | objectUuid 不稳定时可辅助匹配 |
| businessName | 业务名称 | 面向实施和运维人员 | 与设备或部件业务含义一致 |
| partCode | 可动部件编码 | 同一模型版本内唯一 | 可被任务和目标点引用 |
| motionType | 运动类型 | linear、rotate、custom 等 | 与设备动作一致 |
| axisMode | 运动轴模式 | X、Y、Z、local、custom | 与现场坐标和模型轴一致 |
| min / max / home | 运动边界 | 满足 min <= home <= max | 越界配置不可发布 |
| motion target | 目标点位 | targetCode、targetValue、targetX/Y/Z | 可用于运行过程呈现 |
| 绑定失效检测 | 模型版本变化后检查绑定是否仍有效 | objectUuid / objectPath 双重校验 | 失效绑定进入待处理清单 |

## 5. WCS 与上游系统对接计划

### 5.1 WCS 对接

| 数据类别 | 字段示例 | 用途 | 接入方式 | 验收标准 |
|---|---|---|---|---|
| 设备状态 | deviceCode、deviceType、status、timestamp | 驱动设备颜色、图标、告警状态 | Web API / WebSocket / 轮询 | 状态变化到模型更新 ≤2 秒 |
| 设备坐标 | x、y、z、heading、speed | 驱动 AGV、堆垛机、提升机位置 | WebSocket / 轮询 | 坐标映射正确，无明显跳变 |
| 任务状态 | taskNo、stage、start、end、target、priority | 展示任务进度和设备当前任务 | Web API / 事件推送 | 任务状态与现场一致 |
| 告警状态 | alarmCode、alarmLevel、alarmText、time | 告警高亮和告警详情 | Web API / 事件推送 | 告警可查询、可定位、可追踪 |
| 货物信息 | palletCode、sku、materialName、quantity、orderNo | 展示搬运物料和货位详情 | Web API | 点击设备或货位可查看详情 |
| 时间戳 | sourceTime、receiveTime | 数据新鲜度和回放 | 全接口统一 | 延迟和乱序可识别 |
| 路径结果 | pathPoints、pathId、pathStatus | 三维展示路径和轨迹 | WCS / 第三方调度系统 | 路径只展示结果，不进行算法计算 |
| 调度结果 | dispatchId、assignedDevice、reason、status | 展示任务分配和调度状态 | WCS / 第三方调度系统 | 调度结果可追踪，责任边界清晰 |

### 5.2 WMS 对接

| 数据类别 | 字段示例 | 用途 | 验收标准 |
|---|---|---|---|
| 入库订单 | orderNo、sku、quantity、targetLocation | 入库过程展示和货位定位 | 订单状态可映射到三维货位 |
| 出库订单 | orderNo、locationCode、sku、priority | 出库任务展示 | 出库货位可高亮 |
| 货位状态 | locationCode、status、lockedFlag | 空 / 满 / 锁定 / 异常展示 | 货位颜色和状态一致 |
| 库存信息 | sku、quantity、batch、inboundTime | 物资查询和报表 | 查询结果可定位到货位 |
| 托盘 | palletCode、locationCode、materialList | 托盘详情展示 | 点击托盘可查看物资 |
| SKU | sku、materialName、spec | 物资主数据 | 支持编码、名称、规格查询 |
| 物资详情 | materialCode、name、spec、quantity、orderNo | 详情面板和大屏展示 | 字段与接口协议一致 |

### 5.3 MES 对接

| 数据类别 | 字段示例 | 用途 | 验收标准 |
|---|---|---|---|
| 生产计划 | planNo、lineCode、materialDemandTime | 生产物流联动展示 | 生产计划可关联配送任务 |
| 物料配送任务 | taskNo、materialCode、targetPoint | AGV / 输送线配送过程展示 | 配送状态可追踪 |
| 生产线缺料 | lineCode、materialCode、urgency | 缺料提醒和任务优先级展示 | 缺料事件可在大屏提醒 |
| AGV 配送状态 | taskNo、deviceCode、deliveryStatus | 展示配送进度 | 状态与上游一致 |

### 5.4 ERP 对接

| 数据类别 | 字段示例 | 用途 | 验收标准 |
|---|---|---|---|
| 库存总量 | date、skuCount、totalQuantity | 库存总览和月度报表 | 可按日 / 月展示趋势 |
| 效率报表 | deviceCode、utilization、finishRate | 设备效率展示 | 指标口径与 ERP 一致 |
| 故障成本 | deviceCode、repairCost、month | 成本分析展示 | 月度统计可查询 |
| 月度统计 | month、metricCode、metricValue | 管理报表 | 可导出、可归档 |

### 5.5 数据边界

- 上游系统负责提供真实业务数据，包括订单、库存、生产计划、设备控制结果、调度计算结果、路径结果、冲突处理结果和效率指标。
- 数字孪生系统负责接收数据、状态映射、三维呈现、提醒、查询、定位、回放和报表展示。
- 调度算法、路径算法、冲突规避、任务分配和产能优化由第三方调度系统 / WCS / 上游系统提供结果。
- 数字孪生系统不改变上游系统的业务主数据口径，涉及统计指标时以接口协议和数据字典为准。

## 6. 调度与算法边界

本章作为项目范围控制的核心边界。IDTS 数字孪生系统负责接收、映射、呈现、追踪和回放，不承担现场调度、路径规划、冲突处理、任务分配和产能优化的算法计算责任。

本系统不承担以下算法：

- AGV 调度算法。
- 路径规划算法。
- 冲突规避算法。
- 任务分配算法。
- 优先级决策算法。
- 产能优化算法。

第三方调度系统 / WCS / 上游系统负责：

- 任务分配。
- 路径结果。
- 冲突处理结果。
- 等待状态。
- 拥堵状态。
- 任务优先级。
- 调度效率数据。
- 产能分析结果。

本系统负责：

- 接收调度结果。
- 三维展示路径。
- 显示任务进度。
- 显示等待 / 拥堵 / 异常状态。
- 轨迹回放。
- 任务追踪。
- 效率数据展示。

| 边界项 | 第三方调度系统 / WCS / 上游系统职责 | 数字孪生系统职责 | 验收方式 |
|---|---|---|---|
| 任务分配 | 输出任务分配结果、设备分配结果、任务优先级 | 展示任务归属、状态和进度 | 接口字段与展示一致 |
| 路径结果 | 输出路径点、路径编号、路径状态 | 三维展示路径线、轨迹和设备位置 | 路径点映射准确 |
| 冲突处理 | 输出等待、避让、拥堵、异常处理结果 | 展示等待、拥堵、异常状态和提醒 | 状态展示与上游一致 |
| 产能分析 | 输出产能分析结果、效率指标和瓶颈结论 | 展示指标、报表和趋势 | 指标口径与上游一致 |
| 调度效率 | 输出任务完成率、设备利用率、等待时长 | 展示效率看板和报表 | 数据可追溯到接口来源 |

文档、接口和验收过程中应避免将路径计算、调度合理性验证、路径优化计算等描述为本系统默认职责。

## 7. 设备业务功能实施计划

### 7.1 提升机

| 功能 | 输入 | 输出 | 依赖 | 验收标准 |
|---|---|---|---|---|
| 状态映射 | 运行、待机、故障、维护状态 | 模型颜色、状态标签 | WCS 状态接口 | 状态变化 ≤2 秒呈现 |
| 高度映射 | 载货台高度、目标楼层 | 载货台位置变化 | WCS 高度数据 | 高度与接口值一致 |
| 载货台可动部件 | partCode、axisMode、min / max / home | 升降动画 | 可动部件配置 | 越界数据可提示 |
| WCS 高度数据驱动 | height、timestamp | 实时升降过程 | WCS 坐标接口 | 平滑显示，无明显跳变 |
| 异常高亮 | alarmCode、alarmLevel | 红色高亮、告警详情 | 告警接口 | 异常可定位到设备 |
| 任务状态 | taskNo、stage、cargoStatus | 当前任务面板 | WCS 任务接口 | 任务阶段可追踪 |
| 接驳点 | 与输送线 / 堆垛机 / AGV 接驳点编码 | 接驳关系展示 | 场景配置 | 接驳点位置准确 |

### 7.2 堆垛机

| 功能 | 输入 | 输出 | 依赖 | 验收标准 |
|---|---|---|---|---|
| X/Y/Z 坐标 | row、bay、level、x、y、z | 堆垛机位置和升降展示 | WCS 坐标接口 | 坐标与货架位置对应 |
| 货叉伸缩 | forkStatus、forkPosition | 货叉伸缩动画 | 可动部件配置 | 伸缩范围合法 |
| 取货 | taskStage、sourceLocation | 取货动作和货物状态 | WCS 任务接口 | 取货过程可追踪 |
| 放货 | targetLocation、cargoStatus | 放货动作和货位状态 | WMS / WCS 数据 | 放货后货位状态更新 |
| 返回待机位 | idlePosition、taskEnd | 返回路径结果展示 | WCS 路径结果 | 待机位展示正确 |
| 任务号 | taskNo | 任务详情面板 | WCS 任务接口 | 点击可查看任务详情 |
| 货位 | locationCode、locationStatus | 货位高亮 | WMS 货位接口 | 货位编码和坐标准确 |
| 货物信息 | palletCode、sku、quantity | 货物详情 | WMS / WCS 接口 | 字段与接口一致 |
| 异常 | alarmCode、alarmText | 高亮和提醒 | 告警接口 | 异常可查询和回放 |

### 7.3 AGV

| 功能 | 输入 | 输出 | 依赖 | 验收标准 |
|---|---|---|---|---|
| 坐标 | x、y、z | AGV 模型位置 | WCS / AGV 系统 | 坐标映射准确 |
| 方向 | heading、rotation | 模型朝向 | WCS 坐标接口 | 朝向与运动方向一致 |
| 速度 | speed | 速度标签和状态 | AGV 状态接口 | 数值展示正确 |
| 电量 | batteryLevel、chargeStatus | 电量标签、低电提醒 | AGV 状态接口 | 低电状态可提醒 |
| 任务号 | taskNo | 当前任务面板 | WCS 任务接口 | 可按任务追踪 AGV |
| 起点 | startPoint | 起点标记 | WCS 路径结果 | 起点位置准确 |
| 终点 | endPoint | 终点标记 | WCS 路径结果 | 终点位置准确 |
| 路径点 | pathPoints | 路径线和轨迹 | 第三方路径结果 | 路径结果展示准确 |
| 任务状态 | waiting、running、loading、unloading、finished、abnormal | 任务状态展示 | WCS 任务接口 | 状态与上游一致 |
| 充电状态 | charging、chargeStation | 充电状态和位置 | AGV 系统 | 充电状态可视化 |
| 故障状态 | alarmCode、faultText | 故障高亮和详情 | 告警接口 | 故障可定位 |
| 第三方调度结果展示 | dispatchResult、assignedTask、pathStatus | 调度结果呈现和追踪 | 第三方调度系统 / WCS | 不进行 AGV 调度算法计算 |

### 7.4 输送线

| 功能 | 输入 | 输出 | 依赖 | 验收标准 |
|---|---|---|---|---|
| 线段状态 | segmentCode、status | 线段颜色和状态标签 | WCS 状态接口 | 状态准确映射 |
| 启停 | runningFlag | 运行 / 停止展示 | 设备状态接口 | 启停变化及时呈现 |
| 堵塞 | blockFlag、blockPosition | 堵塞高亮和提醒 | 告警接口 | 堵塞点可定位 |
| 故障 | alarmCode、alarmText | 故障高亮和告警详情 | 告警接口 | 故障可查询 |
| 货物经过 | cargoId、segmentCode、time | 货物流动过程 | WCS / WMS 数据 | 货物经过记录可追踪 |
| 交接点 | handoverPointCode、targetDevice | 与其他设备接驳关系 | 场景配置 | 接驳点关系准确 |

### 7.5 货位与物资

| 功能 | 输入 | 输出 | 依赖 | 验收标准 |
|---|---|---|---|---|
| 货位状态 | empty、full、locked、abnormal | 货位颜色和标签 | WMS / WCS 货位接口 | 状态与上游一致 |
| 托盘 | palletCode | 托盘详情 | WMS 数据 | 点击货位可查看托盘 |
| SKU | sku | 物资编码展示 | WMS / ERP 数据 | 支持 SKU 查询 |
| 物资名称 | materialName | 详情面板 | 物资主数据 | 名称准确 |
| 规格 | spec | 详情面板 | 物资主数据 | 规格准确 |
| 数量 | quantity | 库存数量 | WMS / ERP 数据 | 数量口径明确 |
| 入库时间 | inboundTime | 货物详情 | WMS 数据 | 时间格式统一 |
| 订单 | orderNo | 订单关联 | WMS / MES 数据 | 可追溯订单 |
| 查询高亮 | 查询条件、匹配结果 | 三维定位和高亮 | 后端查询接口 | 模糊查询可定位结果 |

## 8. 前端实施计划

| 模块 | 目标 | 输入 | 输出 | 依赖 | 验收标准 |
|---|---|---|---|---|---|
| SceneEngine | 管理 Three.js 场景、相机、渲染循环和基础资源 | 场景配置、设备清单、渲染参数 | 可运行三维场景 | Three.js、场景 manifest | 场景可加载、渲染稳定、资源可释放 |
| AssetManifestLoader | 从后端加载资产 manifest | assetId、versionId、sceneId | 模型资源清单 | 后端 manifest API | 不拼接硬编码模型路径 |
| ObjectTreeAdapter | 适配 object-tree 数据 | object-tree API 返回 | 对象树、节点映射 | object-tree 接口 | 可选中对象并查看层级 |
| DeviceLayer | 管理 GLB 设备加载、实例化、状态映射 | 设备实例、模型绑定、状态数据 | 设备模型、状态表现 | SceneEngine、AssetManifestLoader | 设备可加载、定位、更新状态 |
| TilesetLayer | 加载 3D Tiles 厂区底座 | tileset manifest、坐标配置 | 静态厂区场景 | Three.js 3D Tiles 渲染组件 | 分区加载稳定，与设备层对齐 |
| InteractionLayer | 处理点击、选中、查询定位、视角聚焦 | 鼠标、触摸、对象索引 | 选中状态和详情请求 | SceneEngine、ObjectTreeAdapter | 点击设备和货位可定位 |
| OverlayLayer | 展示路径、标签、告警、任务浮层 | 状态、任务、告警、路径结果 | 三维叠加层 | DeviceLayer、RuntimeStateAdapter | 标签不遮挡关键操作，告警可见 |
| RuntimeStateAdapter | 将 WCS / WMS / MES / ERP 数据映射为前端状态 | 运行状态、任务、库存、告警 | 标准运行状态模型 | 接口适配层 | 状态字段可扩展，异常数据可处理 |
| MonitorMode | 现场监控模式 | Published 配置、运行数据 | 只读监控界面 | SceneEngine、RuntimeStateAdapter | 不修改配置，可查看状态和告警 |
| EditMode | 配置模式 | asset、object-tree、movable part、motion target | 配置草稿和校验结果 | 后端配置 API | 可保存配置，发布前可校验 |
| PerformanceMonitor | 监测 FPS、draw call、三角面、内存趋势 | 渲染指标 | 性能面板和告警 | SceneEngine | 性能指标可采集和导出 |
| ResourceDisposer | 释放 geometry、material、texture 和事件监听 | 卸载事件、场景切换 | 资源释放结果 | SceneEngine、DeviceLayer、TilesetLayer | 切换场景后资源不持续增长 |

## 9. 后端实施计划

| 层 / 项目 | 职责 | 主要模块 | 关键服务 | 接口边界 | 验收标准 |
|---|---|---|---|---|---|
| HZ.IDTS.DigitalTwin.Api | HTTP 接口、Swagger、统一响应、认证预留、错误处理 | ModelAssets、Scenes、Devices、MovableParts、MotionTargets、Integrations | Controller、Filter、Middleware | 只处理请求响应，不执行长任务 | 接口文档清晰，错误结构统一 |
| HZ.IDTS.DigitalTwin.Application | 应用用例编排、事务边界、DTO、业务校验 | AssetService、SceneService、BindingService、RuntimeStateService | 应用服务、校验器、事务协调 | 调用 Domain 和 Infrastructure | 用例边界清晰，可单元测试 |
| HZ.IDTS.DigitalTwin.Domain | 领域对象、枚举、业务规则和值对象 | ModelAsset、AssetVersion、SceneNode、DeviceInstance、MovablePart、MotionTarget | 领域规则、状态机 | 不依赖基础设施 | 状态转换和规则可测试 |
| HZ.IDTS.DigitalTwin.Infrastructure | EF Core、文件存储、对象存储、外部工具调用、接口适配 | DbContext、Repository、FileStorage、ToolRunner、WcsAdapter | 数据访问、存储、适配器 | 不承载业务流程决策 | 数据读写和外部调用可追踪 |
| HZ.IDTS.DigitalTwin.Worker | 后台任务、模型处理、失败重试、日志采集 | ConversionWorker、StatsWorker、ManifestWorker、HealthCheckWorker | 队列消费、任务执行、日志写入 | 长任务异步处理 | 失败可重试，日志可查询 |
| HZ.IDTS.DigitalTwin.Contracts | 前后端共享契约、事件消息、枚举和接口模型 | DTO、EventContract、EnumContract | 契约定义 | 与前端和接口适配层对齐 | 契约版本清晰，兼容性可管理 |

## 10. 数据库实施计划

| 表名 | 用途 | 关键字段 | 唯一约束 | 外键关系 | 所属阶段 |
|---|---|---|---|---|---|
| model_asset | 模型资产主表 | asset_id、asset_code、asset_name、asset_type、source_file_hash、current_version_id | asset_code、source_file_hash | current_version_id -> asset_version | 一期 |
| asset_version | 模型资产版本 | version_id、asset_id、version_no、status、source_file_id、published_time | asset_id + version_no | asset_id -> model_asset | 一期 |
| model_asset_variant | 派生资产版本 | variant_id、version_id、variant_type、file_id、file_size_mb | version_id + variant_type | version_id -> asset_version | 三期 |
| model_conversion_job | 模型处理任务 | job_id、asset_id、version_id、job_type、status、progress、exit_code | job_id | asset_id -> model_asset、version_id -> asset_version | 一期 |
| model_object_index | 模型对象索引 | object_id、version_id、object_uuid、object_name、object_path、parent_uuid | version_id + object_uuid | version_id -> asset_version | 一期 |
| asset_manifest | 资产加载清单 | manifest_id、version_id、manifest_json、status、created_time | version_id + status | version_id -> asset_version | 一期 |
| scene_node | 场景节点 | node_id、scene_id、parent_id、node_type、node_code、transform | scene_id + node_code | parent_id -> scene_node | 一期 |
| device_instance | 设备实例 | device_id、device_code、device_type、scene_id、position、rotation、scale | device_code | scene_id -> scene_node 或 scene 表 | 一期 |
| device_model_binding | 设备模型绑定 | binding_id、device_id、asset_id、version_id、binding_status | device_id + version_id | device_id -> device_instance、version_id -> asset_version | 一期 |
| movable_part_binding | 可动部件绑定 | part_id、version_id、object_uuid、object_path、part_code、motion_type、axis_mode、min_value、max_value、home_value | version_id + part_code | version_id -> asset_version | 一期 |
| motion_target | 目标点位 | target_id、part_id、target_code、target_value、target_x、target_y、target_z、sort_no | part_id + target_code | part_id -> movable_part_binding | 一期 |
| operation_audit | 操作审计 | audit_id、operator_id、operation_type、target_type、target_id、detail_json、created_time | audit_id | target_id 逻辑关联业务对象 | 三期 |
| tool_package | 工具包登记 | tool_id、tool_name、version、download_url、sha256、install_path、required | tool_name + version | 无强外键 | 三期 |
| tool_health_check | 工具链健康检查 | check_id、tool_id、status、output、checked_time | check_id | tool_id -> tool_package | 三期 |

## 11. API 实施计划

### 11.1 一期 API

| API | 用途 | 请求 | 响应 | 校验 | 错误情况 | 所属阶段 |
|---|---|---|---|---|---|---|
| POST /api/model-assets/upload | 上传 GLB 模型资产 | multipart 文件、assetName、assetType | assetId、versionId、sourceFileHash、jobId | 文件格式、大小、hash | 400 文件无效；409 hash 重复；500 存储失败 | 一期 |
| GET /api/model-assets/{assetId} | 查询模型资产详情 | assetId | asset 基本信息、当前版本 | assetId 存在 | 404 资产不存在 | 一期 |
| GET /api/model-assets/{assetId}/manifest | 查询资产 manifest | assetId、versionId 可选 | manifest、版本、资源路径 | Published / 指定版本可用 | 404 manifest 不存在；409 版本状态不可加载 | 一期 |
| GET /api/model-assets/{assetId}/object-tree | 查询对象树 | assetId、versionId 可选 | nodes[] | 版本可用 | 404 对象树不存在 | 一期 |
| PUT /api/model-assets/{assetId}/versions/{versionId}/object-tree | 保存对象树 | nodes[] | 保存结果 | objectUuid / objectPath 合法 | 400 数据无效；409 版本状态不允许 | 一期 |
| PUT /api/model-assets/{assetId}/versions/{versionId}/model-stats | 保存模型统计 | stats json | 保存结果 | 指标字段完整 | 400 数据无效；409 版本状态不允许 | 一期 |
| GET /api/model-conversion-jobs/{jobId} | 查询转换任务状态 | jobId | status、progress、message、logUrl | jobId 存在 | 404 任务不存在 | 一期 |
| GET /api/model-assets/{assetId}/versions/{versionId}/movable-parts | 查询可动部件 | assetId、versionId | items[] | 版本存在 | 404 配置不存在 | 一期 |
| POST /api/model-assets/{assetId}/versions/{versionId}/movable-parts | 新增可动部件 | objectUuid、partCode、motionType、axisMode、min/max/home | partId | partCode 唯一、范围合法 | 400 字段无效；409 重复绑定 | 一期 |
| PUT /api/model-assets/{assetId}/versions/{versionId}/movable-parts/{partId} | 更新可动部件 | 可动部件字段 | 更新结果 | partId 存在、范围合法 | 400 无效；404 不存在；409 状态冲突 | 一期 |
| DELETE /api/model-assets/{assetId}/versions/{versionId}/movable-parts/{partId} | 删除或禁用可动部件 | partId | success | 版本状态允许 | 404 不存在；409 状态不允许 | 一期 |
| GET /api/movable-parts/{partId}/motion-targets | 查询目标点位 | partId | items[] | partId 存在 | 404 不存在 | 一期 |
| POST /api/movable-parts/{partId}/motion-targets | 新增目标点位 | targetCode、targetValue、targetX/Y/Z | targetId | targetCode 唯一、目标值合法 | 400 无效；409 重复 | 一期 |
| GET /api/scenes/{sceneId}/manifest | 查询场景加载清单 | sceneId | scene、devices、tilesets、bindings | scene 可用 | 404 场景不存在；409 绑定无效 | 一期 |

### 11.2 二期 API

| API | 用途 | 请求 | 响应 | 校验 | 错误情况 | 所属阶段 |
|---|---|---|---|---|---|---|
| WCS 状态接入 API | 接收或拉取设备状态 | deviceCode、status、timestamp | 接收结果或状态列表 | 设备编码、时间戳 | 400 字段缺失；409 时间戳过旧 | 二期 |
| 设备状态查询 API | 查询设备当前状态 | deviceCode、deviceType | device state | 设备存在 | 404 设备不存在 | 二期 |
| 告警查询 API | 查询当前和历史告警 | alarmLevel、deviceCode、timeRange | alarms[] | 时间范围合法 | 400 参数错误 | 二期 |
| 货位查询 API | 查询货位状态 | locationCode、status、area | locations[] | 区域编码合法 | 404 货位不存在 | 二期 |
| 物资查询 API | 按 SKU、托盘、名称、规格查询 | keyword、sku、palletCode | materials[] | 查询条件合法 | 400 条件无效 | 二期 |
| 任务状态查询 API | 查询任务状态和设备执行状态 | taskNo、deviceCode | task detail | 任务号或设备编码存在 | 404 任务不存在 | 二期 |

### 11.3 三期 API

| API | 用途 | 请求 | 响应 | 校验 | 错误情况 | 所属阶段 |
|---|---|---|---|---|---|---|
| 权限 API | 管理角色、用户、功能权限 | role、user、permission | permission result | 权限编码合法 | 403 无权限；409 冲突 | 三期 |
| 审计 API | 查询操作审计 | operator、timeRange、targetType | audit list | 时间范围合法 | 400 参数错误 | 三期 |
| 报表 API | 查询效率、库存、故障、任务统计 | reportType、timeRange、filters | report data | 报表口径存在 | 404 报表不存在 | 三期 |
| 发布回滚 API | 发布或回滚模型和场景版本 | assetId、versionId、action | publish result | 版本状态合法 | 409 状态冲突 | 三期 |
| 工具链健康检查 API | 检查转换工具可用性 | toolId | status、version、message | toolId 存在 | 500 工具不可用 | 三期 |

## 12. 分阶段开发计划

| 阶段 | 周期 | 目标 | 主要任务 | 交付物 | 验收标准 | 前置依赖 |
|---|---|---|---|---|---|---|
| 阶段 0：需求与接口冻结 | 1~2 周 | 冻结业务范围、接口字段、坐标系和验收口径 | 需求评审、接口清单、设备清单、模型资料清单、风险清单 | 需求确认稿、接口字段表、坐标基准说明、排期基线 | 范围、接口、数据、模型输入明确 | 甲方业务负责人、接口负责人、模型资料负责人到位 |
| 阶段 1：后端基础与模型资产 | 2~4 周 | 建立后端基础、数据库核心表、GLB 上传和 manifest 基础能力 | 后端骨架、Migration、文件存储、GLB 上传、hash 去重、manifest | 后端基础服务、核心表、上传 API、manifest API | 上传 GLB 后可查询资产和 manifest | 阶段 0 完成，数据库和文件存储环境可用 |
| 阶段 2：可动部件与前端配置闭环 | 2~4 周 | 完成 object-tree、model-stats、可动部件、motion target 和前端加载 | object-tree、model-stats、可动部件 API、目标点位 API、前端加载 | 配置闭环、模型对象树、运动目标点 | 前端可加载后端 manifest，保存并读取配置 | 阶段 1 接口可用 |
| 阶段 3：单设备 WCS 状态接入 | 2~3 周 | 完成提升机或单设备状态最小闭环 | WCS mock、状态接口、坐标映射、告警高亮 | 单设备状态接入成果 | 单设备状态可驱动模型变化，延迟 ≤2 秒 | 阶段 2 完成，单设备接口字段明确 |
| 阶段 4：多设备状态接入 | 4~8 周 | 扩展堆垛机、AGV、提升机、输送线状态映射 | 多设备接口、运行状态、任务状态、路径结果展示、告警 | 多设备监控视图 | 多设备同时运行时状态准确、帧率达标 | WCS 正式接口稳定 |
| 阶段 5：货位、物资、大屏和报表 | 3~6 周 | 接入库存、货位、物资和基础统计 | WMS / ERP 数据、货位查询、物资查询、大屏、报表 | 货位视图、物资查询、大屏、报表 | 查询可定位，报表指标口径明确 | WMS / ERP 接口和数据字典可用 |
| 阶段 6：3D Tiles 厂区底座生产化 | 4~8 周 | 完成厂区静态底座生产化和多区域加载 | CAD / BIM / IFC / STEP 流程、3D Tiles、坐标对齐、LOD | 厂区底座、tileset、坐标校准报告 | 底座与设备层对齐，分区加载稳定 | 厂区模型资料和坐标基准明确 |
| 阶段 7：权限、审计、部署和运维 | 4~8 周 | 完成生产化治理、发布回滚和运维监控 | 权限、审计、部署脚本、备份恢复、监控、回滚 | 运维手册、部署包、验收报告 | 可部署、可升级、可回滚、可审计 | 前序阶段稳定，生产环境准备完成 |

## 13. 详细任务拆分

### 13.1 阶段任务表

| 任务编号 | 任务名称 | 任务内容 | 负责人角色 | 输入 | 输出 | 验收标准 | 依赖 |
|---|---|---|---|---|---|---|---|
| P0-01 | 需求范围冻结 | 确认建设范围、默认范围外事项、阶段目标和验收口径 | 项目负责人 / 业务负责人 | 需求材料、现场业务说明 | 范围确认表 | 范围项均有阶段归属 | 无 |
| P0-02 | 接口字段冻结 | 梳理 WCS / WMS / MES / ERP 字段、频率、错误码和时间戳 | 接口负责人 | 上游接口说明 | 接口字段表 | 字段、频率、责任系统明确 | P0-01 |
| P0-03 | 坐标系冻结 | 确认厂区、设备、AGV、堆垛机、提升机坐标基准 | 三维负责人 / 现场实施 | CAD、设备坐标、现场尺寸 | 坐标基准说明 | 坐标原点、方向、单位明确 | P0-01 |
| P0-04 | 模型资料清单 | 收集 CAD / BIM / IFC / GLB、设备清单和命名规则 | 模型负责人 | 模型文件、设备清单 | 模型资料清单 | 文件版本、格式、来源明确 | P0-01 |
| P1-01 | 后端项目骨架 | 规划 HZ.IDTS.DigitalTwin.Api、Application、Domain、Infrastructure、Worker、Contracts | 后端负责人 | 技术架构 | 后端工程结构方案 | 分层职责清晰 | P0-01 |
| P1-02 | 数据库 Migration | 建设核心表 Migration 和索引约束 | 后端开发 / 数据库负责人 | ER 草案 | 数据库脚本 | 核心表创建成功，约束正确 | P1-01 |
| P1-03 | 文件存储 | 建设文件存储目录、对象存储适配和校验规则 | 后端开发 | 文件存储方案 | 文件存储服务 | 文件可上传、读取、校验 | P1-01 |
| P1-04 | GLB 上传 | 建设 GLB 上传、hash 去重、资产元数据入库 | 后端开发 | GLB 样例、上传限制 | 上传 API | 重复文件可识别，资产可查询 | P1-02、P1-03 |
| P1-05 | manifest | 建设 manifest 保存和查询 | 后端开发 | 资产版本、资源路径 | manifest API | 前端可按 manifest 加载模型 | P1-04 |
| P1-06 | 转换任务日志 | 建设转换任务状态和基础日志 | 后端开发 / 运维 | 任务状态模型 | job API、日志记录 | 任务状态可查询，失败可追踪 | P1-04 |
| P2-01 | object-tree | 保存和查询模型对象树 | 后端开发 | GLB 对象树 | object-tree API | 对象层级可查询 | P1-04 |
| P2-02 | model-stats | 保存模型统计指标 | 后端开发 / 模型负责人 | 模型统计结果 | model-stats API | 性能指标可用于发布检查 | P2-01 |
| P2-03 | 可动部件 | 建设可动部件配置接口 | 后端开发 / 前端开发 | object-tree、partCode 规则 | movable-parts API | 可配置、可查询、可校验 | P2-01 |
| P2-04 | motion target | 建设目标点位配置接口 | 后端开发 / 前端开发 | 可动部件配置 | motion-target API | 目标点可保存和排序 | P2-03 |
| P2-05 | scene manifest | 建设场景、设备实例、设备模型绑定和场景加载清单 | 后端开发 | 场景配置、设备清单 | scene manifest API | 场景设备可按配置加载 | P1-05、P2-03 |
| P2-06 | 前端加载 | 前端接入 manifest、object-tree、场景设备绑定 | 前端开发 | 后端 manifest、object-tree | 前端加载闭环 | 设备模型和对象树可展示 | P2-05 |
| P2-07 | 配置模式闭环 | 前端配置可动部件和目标点位并保存 | 前端开发 / 后端开发 | 可动部件 API、目标点 API | 配置页面 | 配置可保存、读取和校验 | P2-03、P2-04 |
| P3-01 | WCS mock | 建设单设备状态样例数据和接口适配 | 接口开发 / 后端开发 | 单设备字段表 | 状态样例接口 | 状态可驱动模型变化 | P2-06 |
| P3-02 | 提升机状态接入 | 接入提升机状态、高度、任务和告警 | 接口开发 / 前端开发 | WCS 单设备接口 | 提升机状态展示 | 高度和状态准确呈现 | P3-01 |
| P3-03 | 告警 | 建设设备告警高亮和告警详情 | 前端开发 / 后端开发 | 告警字段、告警字典 | 告警视图 | 告警可定位和查询 | P3-02 |
| P4-01 | AGV 状态展示 | 接入 AGV 坐标、方向、电量、任务、路径结果 | 前端开发 / 接口开发 | AGV 状态接口 | AGV 运行展示 | 轨迹和状态准确展示 | P3-03 |
| P4-02 | 堆垛机状态展示 | 接入堆垛机 X/Y/Z、货叉、任务、货位和异常 | 前端开发 / 接口开发 | 堆垛机接口 | 堆垛机运行展示 | 取放货过程可视化 | P3-03 |
| P4-03 | 输送线状态展示 | 接入线段启停、堵塞、故障、货物经过 | 前端开发 / 接口开发 | 输送线接口 | 输送线运行展示 | 线段状态准确 | P3-03 |
| P4-04 | 多设备联动 | 统一 RuntimeStateAdapter 和设备状态模型 | 前端负责人 | 多设备状态接口 | 多设备监控视图 | 设备并发运行帧率达标 | P4-01、P4-02、P4-03 |
| P5-01 | 货位查询 | 接入货位编码、状态、区域和高亮定位 | 后端开发 / 前端开发 | WMS 货位接口 | 货位查询功能 | 货位可查询和定位 | P4-04 |
| P5-02 | 物资查询 | 按托盘、SKU、名称、规格进行查询 | 后端开发 / 前端开发 | WMS / ERP 物资数据 | 物资查询功能 | 查询结果可定位 | P5-01 |
| P5-03 | 大屏 | 建设设备、货位、告警、任务概览大屏 | 前端开发 / 业务负责人 | 看板指标口径 | 大屏页面 | 指标准确、视图稳定 | P5-01、P5-02 |
| P5-04 | 报表 | 建设设备效率、货位周转、故障统计报表 | 后端开发 / 前端开发 | ERP / WCS 历史指标 | 报表功能 | 指标口径清晰，可导出 | P5-03 |
| P6-01 | 3D Tiles | 建设厂区 3D Tiles 底座、tileset manifest 和分区加载 | 三维开发 / 模型负责人 | CAD / BIM / IFC / STEP | 3D Tiles 底座 | 与设备层对齐，加载稳定 | P0-03、P0-04 |
| P6-02 | 模型优化 | 建设 LOD、压缩、派生资产和性能门禁 | 模型负责人 / 前端开发 | 模型资产、统计指标 | 优化资产 | FPS 和加载指标达标 | P6-01 |
| P6-03 | 历史回放 | 建设轨迹、任务、告警事件回放 | 后端开发 / 前端开发 | 历史数据接口 | 回放功能 | 可按时间回放关键过程 | P4-04 |
| P7-01 | 权限审计 | 建设角色、功能权限、审计查询 | 后端开发 / 安全负责人 | 用户和角色清单 | 权限和审计功能 | 操作可追踪，权限可控制 | P5-04 |
| P7-02 | 部署运维 | 建设部署脚本、备份恢复、监控、版本升级和回滚 | 运维负责人 / 后端开发 | 服务器信息、部署规范 | 部署运维包 | 可部署、可备份、可恢复 | P7-01 |

### 13.2 Markdown 执行检查项

- [ ] P0 阶段完成需求范围、接口字段、坐标系、模型资料清单冻结。
- [ ] P1 阶段完成后端基础、数据库 Migration、文件存储、GLB 上传、hash 去重和 manifest 查询。
- [ ] P2 阶段完成 object-tree、model-stats、可动部件、motion target、scene manifest 和前端加载闭环。
- [ ] P3 阶段完成单设备 WCS mock / 实时状态最小闭环、提升机状态接入和告警展示。
- [ ] P4 阶段完成 AGV、堆垛机、提升机、输送线多设备状态展示。
- [ ] P5 阶段完成货位查询、物资查询、大屏和报表。
- [ ] P6 阶段完成 3D Tiles 厂区底座生产化、模型优化和历史回放。
- [ ] P7 阶段完成权限审计、部署运维、升级回滚和现场验收材料。
- [ ] 所有调度、路径、冲突、产能相关描述均保持第三方结果展示边界。
- [ ] 性能、稳定性、补传完整性和现场验收指标完成验证。

## 14. 工期评估

### 14.1 单人 + 辅助工具开发

| 建设目标 | 周期预估 | 适用范围 | 风险说明 |
|---|---|---|---|
| 可演示 MVP | 8~12 周 | 模型资产、GLB 上传、manifest、object-tree、可动部件、单设备状态闭环 | 对接口和模型质量依赖高 |
| 单区域试点 | 4~6 个月 | 单区域多设备状态接入、货位查询、大屏基础视图 | 单人并行处理接口、前端、后端、模型任务压力较大 |
| 较完整现场版 | 8~12 个月 | 多设备、多区域、报表、回放、3D Tiles、权限审计、部署运维 | 现场接口变更和模型转换复杂度影响较大 |

### 14.2 3~5 人小团队

| 建设目标 | 周期预估 | 适用范围 | 风险说明 |
|---|---|---|---|
| 可演示 MVP | 4~6 周 | 模型资产和单设备闭环 | 需前后端、模型、接口并行配合 |
| 单区域试点 | 3~4 个月 | 单区域多设备、货位、大屏和基础报表 | 接口冻结和现场数据质量决定排期稳定性 |
| 较完整现场版 | 6~9 个月 | 生产化、权限审计、回放、3D Tiles 和部署运维 | 模型处理、性能优化和现场验收会形成关键路径 |

工期取决于以下条件：WCS 接口是否明确、CAD 模型是否可用、GLB 模型质量、坐标系是否统一、现场硬件性能、上游数据质量、是否包含远程控制。

## 15. 甲方资料与接口清单

| 类别 | 需提供内容 | 用途 | 提供阶段 | 频率 / 格式要求 |
|---|---|---|---|---|
| 模型资料 | CAD / BIM / IFC / GLB 模型 | 场景建模、设备建模、3D Tiles / GLB 转换 | 阶段 0 | 提供原始文件、版本号、坐标说明 |
| 厂区尺寸 | 厂区边界、楼栋位置、楼层高度、通道尺寸 | 坐标校准和场景比例 | 阶段 0 | 图纸和现场尺寸一致 |
| 楼栋楼层信息 | 楼栋编码、楼层编码、区域划分 | 场景节点和定位 | 阶段 0 | 表格或图纸 |
| 设备清单 | 提升机、堆垛机、AGV、输送线、机械手清单 | 设备实例建设 | 阶段 0 | Excel / 接口清单 |
| 设备编码 | deviceCode、deviceType、区域、规格 | 设备绑定和接口映射 | 阶段 0 | 唯一且稳定 |
| 设备坐标 | x、y、z、rotation、scale | 设备放置和坐标映射 | 阶段 0 | 统一坐标系 |
| WCS 状态接口 | 设备状态、运行状态、故障状态 | 状态映射 | 阶段 2~4 | Web API / WebSocket，字段字典 |
| WCS 任务接口 | taskNo、stage、start、end、priority | 任务过程展示 | 阶段 3~4 | 事件推送或轮询 |
| WCS 坐标接口 | deviceCode、x、y、z、heading、speed | 设备位置更新 | 阶段 3~4 | AGV ≥2 次 / 秒，其他按设备协议 |
| WCS 告警接口 | alarmCode、alarmLevel、alarmText、time | 告警高亮和详情 | 阶段 3~4 | 告警字典同步 |
| AGV 路径结果 | pathId、pathPoints、start、end、status | 路径结果展示 | 阶段 4 | 由第三方调度系统 / WCS 提供 |
| 堆垛机坐标 | row、bay、level、x、y、z | 堆垛机运行展示 | 阶段 4 | 坐标频率 ≥1 次 / 秒或按设备协议 |
| 提升机高度 | height、floor、cargoStatus | 提升机升降展示 | 阶段 3 | 高度单位和范围明确 |
| 输送线状态 | segmentCode、status、blockFlag | 输送线运行展示 | 阶段 4 | 线段编码稳定 |
| 货位编码 | locationCode、row、bay、level、area | 货位定位和高亮 | 阶段 5 | 与 WMS 编码一致 |
| 库存数据 | locationCode、sku、quantity、batch | 库存展示和报表 | 阶段 5 | 按业务要求同步 |
| 物资 SKU | sku、name、spec、unit | 物资查询 | 阶段 5 | 主数据字典 |
| WMS 接口 | 入库、出库、库存、货位、托盘 | 货位和库存联动 | 阶段 5 | 字段字典和测试数据 |
| MES 接口 | 生产计划、配送任务、缺料状态 | 生产物流联动 | 阶段 5 | 生产节拍和事件规则 |
| ERP 接口 | 库存总量、效率报表、故障成本、月度统计 | 报表展示 | 阶段 5 | 默认按小时 / 月度同步 |
| 告警码字典 | 告警码、等级、说明、处理建议 | 告警显示和处理 | 阶段 3 | 与 WCS / 设备系统一致 |
| 权限用户清单 | 用户、角色、组织、权限范围 | 权限审计 | 阶段 7 | 与甲方管理口径一致 |
| 部署服务器信息 | 操作系统、CPU、内存、GPU、网络、存储 | 部署和性能评估 | 阶段 0 / 阶段 7 | 提供测试和生产环境信息 |

## 16. 测试与验收计划

| 测试类型 | 测试内容 | 验收指标 | 阶段 |
|---|---|---|---|
| 单元测试 | 领域规则、状态转换、DTO 校验、工具函数 | 核心规则覆盖，失败可定位 | 全阶段 |
| 接口测试 | 模型资产、场景、设备、WCS / WMS / MES / ERP 接口 | 请求、响应、错误码、字段校验符合契约 | 全阶段 |
| 模型加载测试 | GLB、3D Tiles、manifest、object-tree 加载 | 模型可加载、对象树可展示、资源可释放 | 一期 / 三期 |
| 资产上传测试 | GLB 上传、hash 去重、文件存储、版本创建 | 重复上传可识别，异常文件可拒绝 | 一期 |
| WCS 数据接入测试 | 状态、坐标、任务、告警、路径结果接入 | 状态变化到模型更新 ≤2 秒 | 二期 |
| 设备状态映射测试 | AGV、堆垛机、提升机、输送线状态映射 | 状态、坐标、任务显示与接口一致 | 二期 |
| 仿真展示测试 | 数据驱动的运行过程呈现、轨迹展示、回放 | 路径、轨迹、任务过程可追踪 | 二期 / 三期 |
| 性能测试 | 16 台堆垛机 + 120 台 AGV 场景 | 帧率 ≥25fps，用户操作响应 ≤1 秒 | 二期 / 三期 |
| 稳定性测试 | 长时间运行、内存、资源释放、日志 | 连续运行 ≥720 小时 / 月 | 三期 |
| 断线补传测试 | 通信中断、缓存、恢复、补传 | 数据补传完整性 ≥99.9% | 二期 / 三期 |
| 多客户端测试 | 多位置、多客户端同时展示 | 状态一致，性能满足现场要求 | 二期 / 三期 |
| 权限测试 | 角色、功能权限、审计 | 无权限功能不可操作，操作可追踪 | 三期 |
| 发布回滚测试 | 模型版本、场景版本、配置版本发布和回滚 | 发布可审计，回滚后可稳定运行 | 三期 |
| 现场验收测试 | 现场设备、接口、模型、性能、告警、报表 | 软件故障恢复 ≤30 分钟，验收项签字确认 | 三期 |

关键验收指标：

- 状态变化到模型更新 ≤2 秒。
- 用户操作响应 ≤1 秒。
- 16 台堆垛机 + 120 台 AGV 场景帧率 ≥25fps。
- 连续运行 ≥720 小时 / 月。
- 软件故障恢复 ≤30 分钟。
- 数据补传完整性 ≥99.9%。

## 17. 部署与运维计划

| 运维项 | 建设内容 | 输出 | 验收标准 |
|---|---|---|---|
| 开发环境 | 前端、后端、数据库、文件存储、工具链 | 开发环境说明 | 开发人员可按文档启动 |
| 测试环境 | 独立测试数据库、测试接口、测试模型 | 测试部署包 | 测试环境与生产配置隔离 |
| 生产环境 | 前端站点、后端服务、Worker、数据库、存储 | 生产部署方案 | 满足性能和稳定性指标 |
| 前端部署 | 静态资源构建、版本发布、缓存策略 | 前端部署包 | 页面可访问，资源版本正确 |
| 后端部署 | API 服务、配置、日志、健康检查 | 后端部署包 | API 健康检查通过 |
| Worker 部署 | 后台任务服务、队列、工具链路径 | Worker 部署包 | 任务可执行，失败可追踪 |
| 数据库部署 | Migration、索引、备份策略 | 数据库脚本 | 脚本可重复验证 |
| 文件存储部署 | 本地存储或对象存储、目录规范、权限 | 存储配置 | 文件可读写、可备份 |
| 日志 | API、Worker、接口、操作日志 | 日志规范 | 错误可追踪 |
| 备份 | 数据库、文件、配置、模型资产 | 备份计划 | 可按周期执行 |
| 恢复 | 数据恢复、文件恢复、版本回滚 | 恢复方案 | 可在演练中恢复 |
| 监控 | 服务健康、接口延迟、任务失败、磁盘容量 | 监控指标 | 异常可提醒 |
| 工具链安装 | 模型转换、检查、优化工具 | 工具链安装说明 | 工具版本和 hash 可校验 |
| 离线工具包 | 离线安装包、校验脚本、使用说明 | 离线工具包 | 离线环境可安装 |
| 版本升级 | 前端、后端、数据库、模型资产升级流程 | 升级手册 | 升级步骤可执行 |
| 回滚 | 服务回滚、数据库回滚、模型版本回滚 | 回滚手册 | 回滚后系统可用 |

## 18. 风险与控制

| 风险 | 说明 | 影响 | 控制措施 |
|---|---|---|---|
| WCS 接口不稳定 | 字段、频率、协议或状态口径变化 | 影响设备状态映射和任务展示 | 阶段 0 冻结接口；建立模拟数据；接口变更走评审 |
| 上游数据字段不完整 | 缺少设备、任务、货位、物资关键字段 | 影响展示完整性和验收 | 建立字段清单和缺失字段处理策略 |
| 坐标系不统一 | CAD、3D Tiles、GLB、WCS 坐标原点和方向不一致 | 影响模型对齐和设备定位 | 提前冻结坐标基准，建立转换规则和对齐测试 |
| CAD 转换复杂 | 不同格式、图层、精度和授权差异较大 | 影响厂区底座交付周期 | 优先确认可用格式，按格式评估转换链路 |
| GLB 模型过大 | 模型面数、贴图和材质过多 | 影响加载速度和帧率 | 建立 model-stats 和发布前性能门禁 |
| 3D Tiles 对齐困难 | tileset 与 GLB 设备层坐标难以一致 | 影响空间准确性 | 阶段 0 冻结坐标，阶段 6 前完成最小对齐验证 |
| 可动部件绑定失效 | 模型版本更新后 objectUuid 或 objectPath 改变 | 影响动画和状态映射 | 建立绑定失效检测和人工确认流程 |
| 性能不达标 | 大场景、多设备并发导致帧率下降 | 影响现场展示和验收 | LOD、分区加载、资源释放、性能监测同步建设 |
| 调度算法责任误解 | 将调度、路径规划、冲突规避误认为数字孪生系统职责 | 显著扩大项目范围和工期 | 明确第三方调度 / WCS / 上游系统提供结果，数字孪生系统只负责呈现和追踪 |
| 远程控制安全风险 | 控制指令可能影响现场设备安全 | 带来安全、责任和审计风险 | 远程控制单独评审，建立权限、二次确认、审计和应急机制 |
| 现场部署环境不一致 | 测试环境和生产环境硬件、网络、权限不同 | 影响部署和性能 | 提前收集服务器信息，部署前做环境检查 |
| 工期范围膨胀 | 一期范围扩展到全量业务平台 | 影响排期和质量 | 阶段化验收，范围变更必须评审并调整计划 |

## 19. 交付物清单

| 阶段 | 源码 | 数据库脚本 | API 文档 | 前端页面 | 模型资产 | 部署文档 | 操作文档 | 测试报告 | 验收报告 | 培训材料 | 运维手册 |
|---|---|---|---|---|---|---|---|---|---|---|---|
| 阶段 0 | 无 | 无 | 接口字段草案 | 无 | 模型资料清单 | 环境建议 | 需求确认说明 | 无 | 阶段确认单 | 评审材料 | 无 |
| 阶段 1 | 后端基础 | 核心表脚本 | 一期资产 API | 基础联调页面 | GLB 样例 | 开发部署说明 | 资产上传说明 | 接口测试报告 | 一期后端验收单 | 基础使用说明 | 初版运维说明 |
| 阶段 2 | 前后端配置闭环 | 配置表脚本 | 可动部件和目标点 API | 模型加载、对象树、配置页面 | 已绑定设备模型 | 联调部署说明 | 配置操作说明 | 配置闭环测试报告 | 配置闭环验收单 | 配置培训材料 | 运行检查清单 |
| 阶段 3 | 单设备状态接入 | 状态表脚本 | WCS 单设备接口 | 单设备监控页面 | 单设备模型配置 | 单设备部署说明 | 单设备监控说明 | 单设备接入测试报告 | 单设备验收单 | 现场演示材料 | 告警处理说明 |
| 阶段 4 | 多设备状态接入 | 多设备状态脚本 | 多设备状态 API | AGV、堆垛机、输送线视图 | 多设备模型资产 | 多设备部署说明 | 监控操作说明 | 多设备联调测试报告 | 多设备验收单 | 监控培训材料 | 性能检查手册 |
| 阶段 5 | 大屏和报表 | 报表表脚本 | 查询和报表 API | 货位、物资、大屏、报表页面 | 货位场景资产 | 大屏部署说明 | 查询和报表说明 | 报表测试报告 | 大屏和报表验收单 | 业务使用培训 | 报表维护说明 |
| 阶段 6 | 3D Tiles 和回放 | 回放表脚本 | 回放和 tileset API | 厂区底座和回放页面 | 3D Tiles 底座、LOD 资产 | 模型处理部署说明 | 回放操作说明 | 性能和回放测试报告 | 厂区底座验收单 | 模型维护培训 | 模型资产运维手册 |
| 阶段 7 | 权限审计和运维 | 权限审计脚本 | 权限、审计、健康检查 API | 权限、审计、运维页面 | 发布版本资产 | 生产部署和回滚文档 | 系统操作手册 | 全量测试报告 | 总体验收报告 | 培训材料 | 完整运维手册 |

## 20. 结论与推荐路线

推荐路线在当前文档基线下先审核文档并优先完成 POC-3DT-01，取得用户批准的 Go 或 Conditional Go 后，再推进 MVP-09、MVP-10、MVP-10A-01～05 和 MVP-11～MVP-16。MVP-09/MVP-10 可以与 POC 并行，但不再固定要求排在 POC 之前。后续多设备、货位、生产化、权限、审计、部署、运维和远程控制仍按独立阶段评审。

实施顺序建议如下：

- 先做模型资产和 GLB 设备闭环。
- 再做可动部件配置。
- 再执行 POC-3DT-01 并审核结果。
- 再执行 MVP-09、MVP-10 和 MVP-10A-01～05。
- 再执行 MVP-11～MVP-16 的混合场景闭环。
- 后续再扩展状态接入、AGV、堆垛机、输送线、货位、生产化、权限、审计和远程控制。
- 调度算法由第三方提供结果，数字孪生系统只负责展示与追踪。

项目推进中应以接口冻结、坐标冻结、模型资料冻结和阶段验收为关键控制点。凡涉及调度、路径、冲突、产能的内容，均应保持第三方结果展示边界，避免将算法职责并入数字孪生系统默认建设范围。
