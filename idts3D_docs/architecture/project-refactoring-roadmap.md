# 项目架构治理路线图

> 所有 ARCH 任务独立于业务任务：一个选项卡、一个 commit、直接在 main、验证后推送，不改变既有行为。

| 任务 | 目标与范围 | 前置与时机 | 业务/API/DB/Migration | 验证与回滚 | 阻塞 |
|---|---|---|---|---|---|
| ARCH-02 | 建立 `.editorconfig`、`.gitattributes`、UTF-8/换行和生成文件边界。 | 下次跨平台或批量文档编辑前。 | 否/否/否/否 | 文本属性、UTF-8、git diff；删除配置。 | 不阻塞当前 MVP。 |
| ARCH-03 | 建立后端 Application/Integration Tests、前端 lint/test、基础 CI。 | 下一次状态/事务/API Client 变更前。 | 否/否/否/否 | 本地与 CI 测试；移除新增测试/CI。 | 测试关键变更前阻塞。 |
| ARCH-04 | 治理 connection string、存储路径、user secrets 与环境模板。 | 多人开发或部署前。 | 否/否/否/否 | secret scan、环境启动；回退治理配置。 | 部署前阻塞。 |
| ARCH-05 | 按领域拆分 Domain Entity 文件。 | 新增第二组独立实体时。 | 否/否/否/否 | 编译、依赖和 migration snapshot；回退文件移动。 | 不阻塞当前业务。 |
| ARCH-06 | 按聚合拆分 EF Configuration。 | 新增独立配置组时。 | 否/否/否/否 | schema/migration 与集成回归；回退拆分。 | 不阻塞当前业务。 |
| ARCH-07 | 拆分后端 Repository 能力并控制 Controller 边界。 | 新增无关聚合或端点时。 | 否/否/否/否 | API/事务回归；回退抽象拆分。 | 不阻塞，除非新增能力无法安全落位。 |
| ARCH-08 | 明确 Application DI 组合根位置。 | 新模块注册复杂化时。 | 否/否/否/否 | DI 启动验证；回退注册迁移。 | 不阻塞。 |
| ARCH-09 | 治理 Three.js、Vue、加载/交互状态边界。 | 新页面、区域或复杂场景能力前。 | 否/否/否/否 | 页面、选择、加载、场景回归；回退模块重组。 | 不阻塞当前静态 Demo。 |
| ARCH-10 | 建立前端 API Client 与契约类型。 | MVP-11。 | 否/是/否/否 | API/fallback 测试；回退 Client 变更。 | MVP-11 阻塞。 |
| ARCH-11 | 建立跨端契约、错误码、状态语义和联调检查。 | MVP-12 至 MVP-16 前。 | 否/是/可能/否 | 契约矩阵与端到端验证；回退检查器。 | 联调前阻塞。 |
| ARCH-12 | Worker 队列、幂等、重试、日志和恢复专项设计。 | MVP-15 前。 | 否/可能/可能/可能 | Worker 集成/失败恢复测试；回退独立 Worker 变更。 | MVP-15 阻塞。 |
| ARCH-13 | 持续同步 README、架构基线和规范源。 | 每个 MVP/ARCH 完成时。 | 否/否/否/否 | 文档链接和进度核对；回退文档提交。 | 不阻塞功能，但阻塞交付声明。 |
