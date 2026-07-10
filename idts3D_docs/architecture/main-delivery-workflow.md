# main 直接交付流程

## 开始

1. 一个 Codex 选项卡只执行一个任务，同一时间只允许一个写入型选项卡。
2. 从最新 `origin/main` 的 `main` 开始；默认不创建业务、MVP 或 ARCH 分支。
3. 执行 `git status --short`、`git fetch origin`、`git rev-list --left-right --count main...origin/main`、`git pull --ff-only origin main`。
4. 工作区不干净、非 main、非 `0/0` 或无法 fast-forward 时停止；不得 merge、rebase、cherry-pick、reset --hard 或 force push。
5. 先输出任务边界和架构影响，获得写入确认后实施。

## 实施与验证

- 只修改允许范围，不跨任务；发现债务只登记。
- 执行任务要求的 build/test/lint/API/页面验证。
- 执行 `git diff --check`、`git diff --stat`、`git status --short`，并检查生成文件、日志、本地配置、上传文件和机密未进入 Git。

## 提交与结束

1. 提交前再次 `git fetch origin` 和比较 `main...origin/main`。
2. 远端变化时仅允许 `git pull --ff-only origin main`；成功后重新验证，失败则停止。
3. 一个任务一个 commit，推送 `origin main`，禁止 force push。
4. 推送后 `git fetch origin`，确认 `0/0`、最新提交正确且工作区干净；结束当前选项卡。

只读阶段禁止 commit/push。未经用户确认的写入阶段禁止 commit/push。用户确认写入任务后，验证完成必须 commit 并 push `origin/main`。
