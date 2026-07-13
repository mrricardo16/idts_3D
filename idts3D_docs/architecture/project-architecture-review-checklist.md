# 项目架构复检清单

## 任务开始前

- [ ] 已判定任务类型、业务领域、受影响层和已有模块。
- [ ] 已检查是否扩大类/Repository/Controller/页面/utils 职责，是否出现反向依赖。
- [ ] 已说明 API 契约、数据库、Migration、Worker、Three.js 与前后端同步影响。
- [ ] 已登记可能新增债务，并判断是否应拆为独立 ARCH 任务。
- [ ] 只有一个写入型选项卡；当前为 `main`；工作区干净；`main...origin/main` 为 `0/0`。
- [ ] 已判断本任务是否会改变文本编码或触发全仓库换行变化。
- [ ] 已判断是否包含二进制资产、自动生成文件，或应由 `.gitignore` 排除的本地输出。

## 任务完成后

- [ ] 新增 Application 业务规则已增加有效 Application Test；新增跨层依赖已通过 Architecture Tests。
- [ ] Controller 未直接依赖 DbContext 或 Infrastructure 实现，Domain 和 Contracts 保持纯净。
- [ ] 手写 Fake 未替代 PostgreSQL Integration Tests；Migration、事务、Provider SQL、`FOR UPDATE` 和并发行为仍由真实 PostgreSQL 测试验证。
- [ ] 已明确 Architecture Tests 仅覆盖程序集引用与类型签名，不等同于完整 IL 或源码静态分析；测试失败未通过降低断言处理。
- [ ] Controller 保持薄，Service/Repository 职责明确，Domain 纯净，DTO 与契约一致。
- [ ] TypeScript 类型已同步；页面未直接操作底层引擎；无重复逻辑或无边界 Common/Utils 扩张。
- [ ] 事务、审计和错误码受控；新增债务已登记。
- [ ] 已执行适用的 build/test/lint/API/页面验证；无自动测试时已列人工回归。
- [ ] 提交前仍已在本地执行适用命令，push 后已检查 GitHub Actions；CI 失败未被忽略。
- [ ] workflow 仅申请必要权限；新增 CI Job 不依赖开发数据库、本机路径、正式 GLB、Secrets 或部署。
- [ ] 已明确当前 CI 不替代 PostgreSQL/API Integration/浏览器 E2E/真实 WebGL 测试；required checks 未启用时未描述为 pre-merge gate。
- [ ] 新增或修改 Controller、路由或 HTTP 状态映射时已补充 API Integration Test；Application Test 不能替代 HTTP 管道测试。
- [ ] API Fake 测试未被描述为 PostgreSQL 集成测试；WebApplicationFactory 使用隔离配置，未读取开发连接串或写入正式 FileStorage。
- [ ] 500 响应未泄露内部异常；新增 API Integration Tests 已接入 CI。
- [ ] 本次治理任务保持一个提交。
- [ ] 新增前端纯逻辑已增加有效单元测试；新增 Vue 组件行为已判断组件测试或浏览器测试。
- [ ] Three.js 纯逻辑测试未冒充真实 WebGL 渲染测试；测试不依赖未提交的正式 GLB。
- [ ] 新增 API Client 已补充请求、错误码和 fallback 测试。
- [ ] lint、type-check、unit test 和 build 均已通过；未通过禁用规则或降低断言处理失败。
- [ ] 测试后未留下 dist、coverage 或测试报告产物进入 Git。
- [ ] `git diff --check` 通过，diff 仅含任务范围，一个任务一个 commit。
- [ ] 新增文本符合 `.editorconfig`，代表性文本和二进制文件的 Git 属性正确。
- [ ] 未意外修改现有源码换行，未误加入构建产物，未误忽略正式模型资产，且不存在不应入库的 debug/reports 文件。
- [ ] 已推送 main，`main...origin/main` 为 `0/0`，最终工作区干净。
