# 项目架构基线

> 状态：ARCH-01 建立。本文描述当前真实实现，不将目标状态写成现状。

## 1. 仓库与规范源

仓库根目录为 `D:\svn文档\07_src_3DModesys`，由 `idts3D_api`、`idts3D_ui`、`idts3D_docs` 组成。根 `AGENTS.md` 和项目级 Skill 是执行规则入口；API 契约、任务卡和治理规则以 Markdown 为规范源，DOCX 仅作参考资料。

## 2. 当前已实现

- MVP-01 至 MVP-06：六项目 .NET 8 Solution、PostgreSQL EF Core、初始 Migration、上传与本地存储、静态 `/assets`、manifest、object-tree、model-stats、资产版本生命周期。
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

`DigitalTwinDbContext` 已暴露 model asset、version、variant、conversion job、object index、manifest、scene、device、motion、audit、tool 等 DbSet。当前业务 API 仅实现至资产版本生命周期；未来表结构不表示其业务 API 已完成。

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

API 契约 Markdown、Contracts DTO 与后续 TypeScript 类型必须同步。当前 TypeScript API 契约与 API Client 尚未建立。ARCH-03A 已建立后端 Application 单元测试和 Architecture Tests：前者使用手写 Fake 覆盖上传、manifest、object tree/model stats 与版本生命周期的业务结果和 ErrorCode；后者仅检查程序集引用及类型签名。该反射范围不包含方法体 IL、局部变量、动态加载或完整源码静态分析。ARCH-03B 已建立前端 ESLint、独立 `vue-tsc -b` type-check、Vitest、Vue Test Utils 和 jsdom 基线；ModelStructure、ModelStats 与 App 的 TwinDemo stub 渲染已有单元测试，且测试不会启动真实 TwinDemo、WebGL 或加载正式 GLB。ARCH-03C 已建立 GitHub Actions CI：`repository-policy` 检查当前变更范围的文本差异和受限路径，`backend-quality` 运行 restore、Release build、Application Tests 与 Architecture Tests，`frontend-quality` 运行 `npm ci`、lint、type-check、unit test 与生产 build。CI 不包含数据库、Migration、API Integration、浏览器 E2E、真实 WebGL 或部署；当前 main 直接 push 仅在提交进入 main 后触发验证，branch protection 与 required checks 尚未配置。PostgreSQL Infrastructure Integration Tests、Migration、事务和 `FOR UPDATE` 测试、API Integration Tests、前端 API Client 测试、浏览器 E2E 与真实 WebGL 渲染测试仍未建立。ARCH-03B/03C 不是完整测试体系。当前交付分支是 `main` 跟踪 `origin/main`。

## 8. 当前部分完成、缺失与限制

| 分类 | 内容 |
|---|---|
| 部分完成 | Worker 空骨架；scene/device/motion 有数据表但无业务 API；前端仍是静态 Demo。 |
| 缺失 | Scene、Movable Part、Motion Target API；正式 API Client；前后端联调；转换流水线；PostgreSQL/API/前端集成测试。 |
| 已知限制 | 配置与机密治理未定；大型 Repository/Entity/EF Configuration 后续存在职责膨胀风险；正式模型目录、来源与大文件交付策略待 ARCH-02A 确认。 |

## 9. 目标方向

目标是保持后端分层，令前端通过集中 API Client 和契约类型消费已发布配置，Worker 复用 Application 用例处理后台任务，并通过测试、CI、编码治理和独立 ARCH 任务控制债务。目标能力均须以独立任务实现。
