# DOC-3DT-03：文档语义一致性修复与 POC 可执行化报告

> 状态：Ready for user review。本报告不授权代码、数据库、依赖、POC、构建、测试、git add、commit 或 push。

## 1. 执行结论

当前唯一执行顺序为：

~~~text
文档审核
→ POC-3DT-01
→ POC 结果审核
→ 用户批准 Go 或获批 Conditional Go
→ MVP-09 / MVP-10
→ MVP-10A-01 → 10A-02 → 10A-03 → 10A-04 → 10A-05
→ MVP-11 → MVP-12 → MVP-13 → MVP-14 → MVP-15 → MVP-16
~~~

MVP-09/MVP-10 可与 POC 并行，但单人默认优先 POC。POC 不阻塞这两项或无关纯后端任务，但直接阻塞 MVP-10A。MVP-10A 总卡及五张子卡均为 Blocked；POC 文档为 Ready for user execution approval。

## 2. P0 处理结果

- 总纲已重写执行顺序、阶段门禁、禁止跨阶段事项和端到端路径，不再依赖顶部补丁与旧 GLB 主链并存。
- MVP-11～MVP-16 已以正文重写：MVP-11 保持当前 GLB/API Client 基线；MVP-12 使用 baseLayers/devices 分层加载；MVP-13 仅编辑 GLB；MVP-14 验证 Tiles 共存和 GLB fallback；MVP-15 排除 Tiles 生产切片；MVP-16 验收静态底座、GLB、后端配置、回退与资源释放。
- POC 性能预算、测试计划、结果模板和任务卡已具备暂定环境、阶段 A/B 阈值、用例、失败注入、重复进出、冷暖缓存、纯 GLB 对照及证据记录要求。

## 3. P1 处理结果

- README、总纲和完整实施计划采用 POC 优先且允许 MVP-09/MVP-10 并行的顺序。
- POC 明确复用现有 Three.js、Renderer、Controls、Loader、GLB 拾取、worldZ、Vue 生命周期和 fallback 形态，禁止另建脱离工程的长期 Demo 引擎。
- MVP-10A 拆分为 01 契约数据冻结、02 图层骨架、03 Tiles坐标接入、04 混合 Manifest 加载、05 回退性能生命周期。
- 混合架构图已将 Renderer、Camera、Controls、AnimationLoop、ResourceManager、CoordinateTransformer、InteractionManager 置于 TwinScene 直属层。
- ADR 已去除“索引后续处理”的过时边界，并引用 POC 优先和 MVP-10A 子任务链。

## 4. P2 处理结果

- 测试计划定义 Mandatory 用例和每例记录字段。
- 结果模板加入提交/锁文件/依赖版本、硬件驱动、批准角色、冷暖缓存、对照、HAR、证据、问题、整改和 Conditional Go 关闭记录。

## 5. POC 暂定阈值

阶段 A：60 秒观察窗口，平均 FPS 不低于 45，1% Low 不低于 25，可操作时间中位数不高于 8 秒，10 次进出正常，内存相对增长不高于 20%且绝对增长不高于 200 MB，零未处理异常、重复循环或关键监听，Tiles 失败时 GLB fallback 通过。

阶段 B：平均 FPS 不低于 30，1% Low 不低于 18，可操作时间中位数不高于 20 秒，相对纯 GLB 平均 FPS 下降不超过 40%，10 次进出无持续线性内存增长，Tiles 失败时 GLB fallback 通过。上述仅为 POC 暂定基线，不是生产阈值。

## 6. 仍待用户确认

1. 阶段 B Tileset 的来源、许可、托管与现场资料授权。
2. 现场绝对坐标容差、原点、高程、轴向和标定点基准。
3. Conditional Go 的可接受条件、责任人、期限和批准记录。
4. MVP-10A-01 中的最终数据库、DTO、TypeScript、API 与旧 tilesets 兼容决策。

## 7. 检查结果

链接、UTF-8、关键字和 Git 范围检查结果以本任务完成时的终端复核为准。历史 DOC-3DT-00、DOC-3DT-01 与 DOC-3DT-02 只作为审计证据；DOC-3DT-03、任务索引、总纲、ADR、POC 和 MVP-10A 任务链构成当前执行规范。

未执行：业务代码、数据库、依赖安装、POC、构建、测试、git add、commit、push。
