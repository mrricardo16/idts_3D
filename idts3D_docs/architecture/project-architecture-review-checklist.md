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

- [ ] Controller 保持薄，Service/Repository 职责明确，Domain 纯净，DTO 与契约一致。
- [ ] TypeScript 类型已同步；页面未直接操作底层引擎；无重复逻辑或无边界 Common/Utils 扩张。
- [ ] 事务、审计和错误码受控；新增债务已登记。
- [ ] 已执行适用的 build/test/lint/API/页面验证；无自动测试时已列人工回归。
- [ ] `git diff --check` 通过，diff 仅含任务范围，一个任务一个 commit。
- [ ] 新增文本符合 `.editorconfig`，代表性文本和二进制文件的 Git 属性正确。
- [ ] 未意外修改现有源码换行，未误加入构建产物，未误忽略正式模型资产，且不存在不应入库的 debug/reports 文件。
- [ ] 已推送 main，`main...origin/main` 为 `0/0`，最终工作区干净。
