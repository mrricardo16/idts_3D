# 项目架构基线

> 状态：ARCH-01 基线，已由 DOC-PLAN-01 于 2026-07-15 校准实现、测试与 CI 事实。本文描述当前实现，不将目标状态、本地结果和 CI 结果互相替代。

## 1. 仓库与规范源

仓库根目录为 `D:\svn文档\07_src_3DModesys`，由 `idts3D_api`、`idts3D_ui`、`idts3D_docs` 组成。根 `AGENTS.md` 和项目级 Skill 是执行规则入口；API 契约、任务卡和治理规则以 Markdown 为规范源，DOCX 仅作参考资料。

## 2. 当前已实现

- MVP-01 至 MVP-07：六项目 .NET 8 Solution、PostgreSQL EF Core、初始 Migration、上传与本地存储、静态 `/assets`、manifest、object-tree、model-stats、资产版本生命周期与独立 MovablePart Service / Repository / Controller 边界。
- MVP-08：MotionTarget Entity、DbSet、Mapping、初始 Migration 表、DTO、Controller、Application Service、Repository、Draft/Ready 写入 guard、Published monitor 读取 guard、范围/唯一性校验及 OperationAudit 写入均已实现。该项为“Implementation Complete / Verification Incomplete”：本地自动测试通过，但真实 PostgreSQL、Swagger、事务、行锁和审计落库尚未实际验证。
- 后端项目：`Api`、`Application`、`Contracts`、`Domain`、`Infrastructure`、`Worker`。
- 前端静态数字孪生 Demo：Vue 3、Vite、TypeScript、Three.js、GLB、对象树、选择、LOD 和 mock 状态。

## 3. 当前后端结构与调用链

```text
Api Controller / Middleware
  -> Application Service
  -> IModelAssetRepository / IModelAssetFileStorage
  -> Infrastructure Repository / LocalModelAssetFileStorage
  -> DigitalTwinDbContext / PostgreSQL 或本地文件目录
```

允许依赖为 `Api -> Application/Contracts/Infrastructure`、`Application -> Domain/Contracts`、`Infrastructure -> Application/Domain/Contracts`、`Worker -> Application/Contracts/Infrastructure`。Domain 不依赖外层，Application 不依赖 Infrastructure。

`DigitalTwinDbContext` 已暴露 model asset、version、variant、conversion job、object index、manifest、scene、device、motion、audit、tool 等 DbSet。当前业务 API 已实现至 Motion Target CRUD；scene/device 表结构存在，但 Scene/Device 业务 API 尚未实现。未来表结构不表示其业务 API 已完成。

## 4. 前端结构与调用链

```text
TwinDemo.vue
  -> TwinScene.ts
  -> engine managers / LODModelLoader / ModelManifestLoader
  -> 静态 model-config、static manifest、GLB
  -> mock device status 与 localStorage 草稿
```

当前不存在正式 `src/api`、store、composable 或 router 边界。页面和 engine 目前消费静态资源，不消费后端业务 API。该状态是 MVP-11 前的 Demo 基线，不是前后端联调完成状态。

## 5. 数据、文件与 Worker 边界

- PostgreSQL：Infrastructure 的 `DigitalTwinDbContext` 及初始 Migration；Provider 当前为 PostgreSQL。
- 文件存储：`LocalModelAssetFileStorage` 保存资产，Api 将受控目录通过 `/assets` 暴露；API 不应返回物理路径。
- Worker：存在 Hosted Service 骨架，尚无转换队列、重试、幂等或实际处理流水线。

## 6. 仓库文本与产物基线

ARCH-02 已建立根级 `.editorconfig` 与 `.gitattributes`。新建或修改文本以 UTF-8、默认 LF、基础缩进、末尾换行和无意义行尾空白规则为基线；Markdown 保留语义行尾空格，Windows 脚本使用 CRLF。图片、文档、GLB、字体、压缩包和可执行产物均按二进制处理。

构建产物、本地文件、debug/reports 与正式静态资产的入库边界由 `repository-text-and-artifact-policy.md` 说明。ARCH-02 不批量标准化历史文件；既有 BOM、混合换行和空白问题均保留并登记为独立债务。当前 `public/models/**/*.glb` 忽略规则可能误伤正式运行资产，留待 ARCH-02A 审核资产来源、交付方式与大文件策略。

## 7. 契约、测试与交付

API 契约 Markdown、Contracts DTO 与后续 TypeScript 类型必须同步。当前 TypeScript API 契约与 API Client 尚未建立。ARCH-03A 已建立后端 Application 单元测试和 Architecture Tests；ARCH-03D 已建立 API Integration Tests，包含 Motion Target 的 TestServer 路由/绑定覆盖。2026-07-15 本地构建为 0 warning/0 error，测试共 84 项通过；这些结果不连接真实 PostgreSQL 或真实文件存储，也不验证 Migration、事务、Repository SQL、Swagger 实例或真实 GLB。ARCH-03B 已建立前端 ESLint、独立 `vue-tsc -b` type-check、Vitest、Vue Test Utils 和 jsdom 基线；测试不会启动真实 TwinDemo、WebGL 或加载正式 GLB。ARCH-03C 的 GitHub Actions CI 配置存在：`repository-policy`、`backend-quality` 和 `frontend-quality` 分别覆盖路径政策、后端 Release build/三类测试、前端 npm ci/lint/type-check/unit/build。当前基线提交的最近 CI `29313188014` 失败于 repository-policy 的 DOC-3DT-03 行尾空白检查，后端与前端质量作业未启动；因此当前基线没有“最近 CI 通过”证据。CI 不包含数据库、Migration、浏览器 E2E、真实 WebGL 或部署；branch protection 与 required checks 尚未配置。ARCH-03A 至 ARCH-03D 不是完整测试体系。当前交付分支是 `main` 跟踪 `origin/main`。

## 8. 当前部分完成、缺失与限制

Movable Part CRUD 已实现。Motion Target CRUD 的实现及本地自动验证已完成，但真实 PostgreSQL Draft CRUD、唯一约束映射、`target_z`/`target_value` 落库一致性、create/update/delete 审计、Published 写保护、事务与行锁顺序仍登记为 `MVP-08-VERIFY`。BE-DEBT-003、BE-DEBT-004、BE-DEBT-008 保持 Open，BE-DEBT-006B 保持 Open / Environment Blocked。

| 分类 | 内容 |
|---|---|
| 部分完成 | Worker 空骨架；scene/device 有数据表但无业务 API；Motion Target 缺真实 PostgreSQL/Swagger 验证；前端仍是静态 Demo。 |
| 缺失 | Scene/Device API；正式 API Client；前后端联调；转换流水线；PostgreSQL Infrastructure Integration Tests、浏览器 E2E 与真实 WebGL 测试。 |
| 已知限制 | 配置与机密治理未定；大型 Repository/Entity/EF Configuration 后续存在职责膨胀风险；正式模型目录、来源与大文件交付策略待 ARCH-02A 确认。 |

## 9. 目标方向

目标是保持后端分层，令前端通过集中 API Client 和契约类型消费已发布配置，Worker 复用 Application 用例处理后台任务，并通过测试、CI、编码治理和独立 ARCH 任务控制债务。目标能力均须以独立任务实现。
