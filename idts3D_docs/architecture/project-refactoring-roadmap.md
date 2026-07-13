# 项目架构治理路线图

> 所有 ARCH 任务独立于业务任务：一个选项卡、一个 commit、直接在 main、验证后推送，不改变既有行为。

| 任务 | 目标与范围 | 前置与时机 | 业务/API/DB/Migration | 验证与回滚 | 阻塞 |
|---|---|---|---|---|---|
| ARCH-02（已完成） | 已建立 `.editorconfig`、`.gitattributes`、UTF-8/换行基线、文本/二进制属性及产物政策；未批量改写历史文件。 | 已完成。未完成项转入 ARCH-02A 和 ARCH-14。 | 否/否/否/否 | 文本属性、UTF-8、git diff；删除本任务治理文件可回退。 | 不阻塞当前 MVP。 |
| ARCH-02A | 正式模型资产与大文件入库治理：确认正式/临时模型目录、文件来源和授权、大小、Git LFS、部署是否依赖仓库内模型、下载脚本或外部制品存储、`.gitignore` 精确例外规则及现有本地 GLB 是否正式提交。 | 在提交、取消忽略或部署 `public/models` 下正式 GLB 前；需要单独用户确认。 | 否/否/否/否 | 来源与授权审查、大小阈值、Git 属性/忽略规则、部署验证；回退仅限该任务资产策略。 | 正式模型入库和依赖仓库模型的部署前阻塞。 |
| ARCH-14 | 历史文本一致性治理：逐文件处理 BOM、混合换行、尾随空白和末尾换行问题。 | ARCH-02 发现的历史问题经范围确认后。 | 否/否/否/否 | 逐文件 UTF-8、EOL、diff 与功能回归；按文件回退。 | 不阻塞当前 MVP，不得与业务任务混做。 |
| ARCH-03A（已完成） | 已建立 Application + Architecture Tests，覆盖业务规则、ErrorCode、协作行为、程序集引用和类型签名边界；不包含 PostgreSQL、API 集成或完整静态分析。 | 已完成。 | 否/否/否/否 | restore、build、两个完整测试项目；移除新增测试基础设施。 | 为后续 Application 业务规则和跨层依赖提供本地门禁。 |
| ARCH-03B（已完成） | 已建立前端 ESLint、独立 type-check、Vitest、Vue Test Utils、jsdom、ModelStructure/ModelStats 纯逻辑测试和 App stub 基础渲染测试；不包含真实 WebGL、API Client、浏览器 E2E 或视觉测试。 | 已完成。 | 否/否/否/否 | `npm ci`、lint、type-check、unit tests、build；移除独立前端测试基础设施。 | 为 ARCH-03C 本地门禁提供基础。 |
| ARCH-03C | GitHub Actions CI：repository-policy、后端 restore/Release build/Application Tests/Architecture Tests，以及前端 npm ci/lint/type-check/unit test/build。 | 本地测试基线建立后。仅当本次 workflow 在 GitHub Actions 成功运行后才标记已完成。 | 否/否/否/否 | CI 运行；删除 workflow。 | 实施中，待首次远程 run 成功。 |
| ARCH-03D | API Integration Tests。 | Controller、路由或 HTTP 映射变更前。 | 否/否/否/否 | API 集成测试；移除独立测试基础设施。 | 未完成。 |
| ARCH-03E | 共享 fixture、测试模型来源与测试数据治理。 | 扩展测试资产或跨测试复用数据前。 | 否/否/否/否 | fixture 来源、隔离与清理验证；回退测试数据治理。 | Planned。 |
| ARCH-03F | PostgreSQL Infrastructure Integration Tests。 | 具备真实 PostgreSQL/Docker 测试环境后。 | 否/否/否/否 | Migration、事务、`FOR UPDATE`、并发与回滚集成验证；移除独立测试基础设施。 | Planned / Environment Blocked。 |
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
