# DOC-PLAN-02：CI 基线修复与本地复核报告

> 任务类型：CI 事实修复与验证。
> 执行日期：2026-07-15。
> 本任务未 commit、push 或触发新的远程 CI。

## 1. 任务结论

**Ready for commit review。**

已从 GitHub Actions 原始失败日志确认当前基线 CI `29313188014` 的唯一已运行 Job 是 `repository-policy`，失败于 `Check changed paths and text diff`：`DOC-3DT-03-semantic-consistency-fix-report.md:58: new blank line at EOF.`。后端与前端质量 Job 均未启动。

本任务仅删除该文件末尾多余空行；后端和前端 CI 等价质量链均在本地通过，锁文件未变化，Git 范围仅含预期文档。远程 CI 尚未重新运行，必须在用户审核、提交并推送后才能获得新的远程证据。

## 2. 仓库基线

| 项目 | 事实 |
|---|---|
| 分支 | `main` |
| 基线 commit | `5a9a2c5339e11bd3c77072ce276a8a4940c09739` |
| 最近提交 | `5a9a2c5 Revise 3D tiles task docs and POC authorization flow` |
| 失败远程运行 | GitHub Actions CI `29313188014` |
| 工作流 | `.github/workflows/ci.yml` |

## 3. 执行前工作区

执行前工作区与 DOC-PLAN-02 允许状态一致：DOC-PLAN-01 的 6 份已修改文档、DOC-PLAN-00 未跟踪报告及 DOC-PLAN-01 未跟踪报告存在；无业务代码、数据库、Migration、配置、模型、暂存、删除或未完成 Git 操作。

## 4. GitHub Actions 失败复核

| 项目 | 复核结果 |
|---|---|
| Workflow 名称 | `CI` |
| 触发方式 | `push` 到 `main` |
| 触发 commit | `5a9a2c5339e11bd3c77072ce276a8a4940c09739` |
| 失败 Job | `repository-policy` |
| 失败 Step | `Check changed paths and text diff` |
| 原始错误 | `idts3D_docs/reviews/DOC-3DT-03-semantic-consistency-fix-report.md:58: new blank line at EOF.` |
| backend-quality | 未启动，因依赖 repository-policy |
| frontend-quality | 未启动，因依赖 repository-policy |
| 其他已运行失败 | 无；下游质量问题尚未被远程运行验证 |

本机 `gh` 已认证，已执行 `gh run view 29313188014` 与 `gh run view 29313188014 --log-failed`；以上内容来自原始日志，而非摘要推断。

## 5. repository-policy 根因

Workflow 实际调用 `.github/scripts/check-repository-policy.sh <base-sha> <head-sha>`。该脚本先执行 `git diff --check`，再拒绝受限生成物和本地文件路径。

失败文件修复前的末尾字节为 `... E3 80 82 0A 0A`，即正文后存在两个 LF。用脚本中同一条关键命令复现：

```text
git diff --check 2a3b32f493f2b1ee235de15fe19de7437f05a588 5a9a2c5339e11bd3c77072ce276a8a4940c09739
```

结果为 Exit Code 1，并输出同一 `new blank line at EOF` 错误。本机未安装 Bash，故不能直接运行 Workflow 的 Bash 脚本；这不影响对该脚本实际 Git 检查规则的读取和等价复核。

## 6. 最小修复

| 项目 | 内容 |
|---|---|
| 文件 | `idts3D_docs/reviews/DOC-3DT-03-semantic-consistency-fix-report.md` |
| 修改 | 仅删除最后一个多余空行；正文、标题、章节和结论未变 |
| 编码 | 严格 UTF-8 可解码；无 BOM |
| 文件结尾 | 仅一个 LF；无末尾空白行 |
| 内容范围 | `git diff` 仅显示删除一个空白行 |

## 7. 仓库策略本地验证

| 命令/规则 | Exit Code | 结果 | 说明 |
|---|---:|---|---|
| `git diff --check <CI base> <CI head>`（修复前复现） | 1 | 预期失败 | 复现远程原始错误 |
| `git diff --check`（修复后） | 0 | 通过 | 覆盖当前已跟踪文档差异 |
| 受限路径/文件规则 | 0 | 通过 | 按 `check-repository-policy.sh` 的实际 case 规则检查当前预期文档路径；无 `bin`、`obj`、`node_modules`、`dist`、日志、环境文件或数据库文件变更 |
| UTF-8/末尾检查 | 0 | 通过 | DOC-3DT-03 与本报告严格 UTF-8、无 BOM、仅一个结尾 LF |
| 相对链接检查 | 0 | 通过（本轮范围） | 本轮新增或修改文档的相对链接均可解析；未修改的需求补充文档保留 12 个指向 `E:/idtsphoto/...zip` 的历史外部截图引用，不作为仓库内相对链接处理 |

差异说明：Workflow 脚本本身需要 Bash；当前 Windows 环境未安装 Bash，且本任务禁止 `git add`/commit，无法让脚本对未提交工作树创建同等提交范围。因此采用脚本实际的 `git diff --check` 和受限路径规则做本地等价检查，并如实保留此差异。

## 8. 后端 CI 等价验证

环境变量与 Workflow 一致设置为 `DOTNET_NOLOGO=true`、`DOTNET_CLI_TELEMETRY_OPTOUT=true`；实际 SDK 为 8.0.100。

| 步骤 | 命令 | Exit Code | 结果 |
|---|---|---:|---|
| 信息 | `dotnet --info` | 0 | SDK 8.0.100 |
| Restore | `dotnet restore idts3D_api/HZ.IDTS.DigitalTwin.sln` | 0 | 所有项目最新 |
| Build | `dotnet build idts3D_api/HZ.IDTS.DigitalTwin.sln --configuration Release --no-restore` | 0 | 0 warning，0 error |
| Application Tests | 指定 `.csproj`、`Release --no-build` | 0 | 54 passed，0 failed，0 skipped |
| Architecture Tests | 指定 `.csproj`、`Release --no-build` | 0 | 8 passed，0 failed，0 skipped |
| API Integration Tests | 指定 `.csproj`、`Release --no-build` | 0 | 22 passed，0 failed，0 skipped |

## 9. 前端 CI 等价验证

锁文件为 `idts3D_ui/package-lock.json`，故按 Workflow 使用 npm。安装前后 SHA-256 均为 `29DB61CBB676A0045C372D7885D2D0C4B176611EE4861A1A73E4FD54D5BC069E`。

| 步骤 | 命令 | Exit Code | 结果 |
|---|---|---:|---|
| Node/npm | `node --version` / `npm --version` | 0 | v24.15.0 / 11.12.1 |
| 安装 | `npm ci` | 0 | 281 packages；锁文件未变；0 vulnerabilities |
| Lint | `npm run lint` | 0 | 0 warning/0 error |
| Type Check | `npm run type-check` | 0 | 通过 |
| Unit Test | `npm run test:unit` | 0 | 3 files、9 tests passed |
| Build | `npm run build` | 0 | 通过；保留既有大 chunk 警告，非失败 |

## 10. 修改文件

1. `idts3D_docs/reviews/DOC-3DT-03-semantic-consistency-fix-report.md`（仅移除末尾多余空行）

DOC-PLAN-01 的以下既有预期修改保留但未在本任务无关改写：`README.md`、`idts3D_api/README.md`、`idts3D_docs/architecture/project-architecture-baseline.md`、`idts3D_docs/idts-mvp-task-breakdown.md`、`idts3D_docs/mvp-tasks/MVP-08-motion-target-api.md`、`idts3D_docs/mvp-tasks/README.md`。

## 11. 新增文件

- `idts3D_docs/reviews/DOC-PLAN-02-ci-baseline-repair-report.md`

## 12. 未修改范围

未修改业务代码、测试代码、数据库、Migration、API、DTO、Entity、Service、Controller、Vue/TypeScript 源码、CI Workflow、策略脚本、package.json、锁文件、模型或 POC/MVP 实现任务。DOC-PLAN-00 与 DOC-PLAN-01 报告未修改。

## 13. 当前工作区状态

当前工作区仅含 DOC-PLAN-00/01/02 的预期文档变更和 DOC-3DT-03 的最小格式修复；无已暂存或删除文件。构建产生的 `bin`/`obj`、前端 `node_modules`/`dist` 未进入 Git 变化。

## 14. 是否具备提交条件

**是：Ready for commit review。**

理由：CI 根因已由原始日志复核；DOC-3DT-03 仅做最小格式修复；本地策略等价检查、后端和前端质量链全部通过；锁文件未变；Git 范围仅为预期文档；未执行 commit 或 push。

该结论不表示远程 CI 已重新通过。新的远程 CI 只能在用户审核后提交并推送后产生。

## 15. 下一步

等待用户审核当前文档变更。用户如授权提交，可按项目 `main` 交付流程重新核对远端同步、提交、推送并等待新的 GitHub Actions 运行；本任务不执行这些动作。
