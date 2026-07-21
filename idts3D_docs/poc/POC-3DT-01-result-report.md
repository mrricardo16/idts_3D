# POC-3DT-01-CLOSEOUT 实际结果报告

```text
任务：POC-3DT-01-CLOSEOUT
结果：阻塞
结论：Awaiting manual evidence
解锁建议：Do not recommend unlock MVP-10A-01
执行日期：2026-07-21
```

## 执行基线与工作区

| 项目 | 实际状态 |
| --- | --- |
| 分支 / Commit | `main` / `2621bd791a759034fa9888abaa839a682f4c6386` |
| 远程差异 | `main...origin/main [ahead 1]` |
| 暂存区 | 空 |
| `npm ci` | 未执行；避免中断已运行服务和重置 `node_modules`。 |
| 原服务 | `5173` 保持运行且未停止。 |
| 新验收服务 | `5174` Vite 开发服务，含 POC 专用中间件。 |
| 预览服务 | `5175` Vite preview，含同一 POC 专用中间件。 |

本任务只修改 POC 独立入口、POC 引擎适配、Vite POC 中间件、测试夹具、测试和 POC 文档。用户既有 `.gitignore` 修改、DOC-PLAN-08 未跟踪报告和 `tools/` 未跟踪目录均保留且未读取、未修改。未暂存、提交或推送。

## 已修复的验收阻塞项

### 严格 HTTP 404

实现方式：`idts3D_ui/vite.config.ts` 注册 POC 专用 `poc-strict-404-fixture` 中间件，仅拦截 `/__poc_3dt__/missing/*`，开发和 preview 服务器均返回 `404 application/json; charset=utf-8`、`Cache-Control: no-store`。

| 验证路径 | 开发 5174 | 预览 5175 | 结论 |
| --- | --- | --- | --- |
| `/__poc_3dt__/missing/tileset.json` | `404 application/json` | `404 application/json` | 网络层严格 404 成立。 |
| `/__poc_3dt__/missing/child/missing-child.gltf` | `404 application/json` | 由同一中间件覆盖 | 子资源严格失败入口成立。 |
| `/poc-3dtiles/invalid-json/tileset.json` | `200 application/json`，45 bytes | `200 application/json` | 保持独立 JSON 解析失败路径。 |

POC 页面错误状态按 `http`、`parse`、`child` 分类；状态机确保 404 不进入 ready，而重新加载本地 Tileset 会先进入 loading，只有 `load-model` 才进入 ready。非最终的内嵌运行态观察中，严格 404 页面显示实际 `HTTP 404`、GLB 状态仍为已加载、Canvas/Renderer 均为 1；这不是 Chrome/Edge 最终手工证据。

### 合成 worldZ 夹具与真实 GLB 节点检查

真实 GLB 路径为 `idts3D_ui/public/models/lifter.glb`。本次从已加载 POC 页面对象树记录到 **1190** 个节点；诊断面板显示明确 `lifter-platform` 节点为“无”。因此没有根据 `NAUO*` 等 CAD 自动名推断可动部件，也没有修改真实 GLB。

新增明确标注为非生产的合成 glTF 夹具：`idts3D_ui/public/poc-3dtiles/poc-lifter/poc-lifter.gltf`。它包含 `lifter-root`、`lifter-frame`、`lifter-platform`，可由 POC 控件执行 worldZ `0 / 6 / 12`。页面运行态已观察到合成夹具地址、10 节点、明确可动节点 `lifter-platform` 和 worldZ `12`。该文件为仓库内最小 **glTF** 夹具（不是伪装的正式 GLB）；它证明 Three.js + GLTFLoader 子节点 worldZ 路线，不证明真实提升机的正式节点绑定。

### 诊断、导出与生命周期

POC 独立页面已新增运行诊断面板，显示加载状态、Tileset/GLB 地址、Canvas/Renderer、动画循环、FPS、已加载/活动瓦片、选中对象、worldZ、网络/解析错误、生命周期轮次、ready 时间和加载耗时。证据导出生成当前运行态 JSON，未测量性能字段保持 `null`。

生命周期入口使用实际 Vue `PocTilesViewport` 卸载/重建、Three 初始化/释放、TilesRenderer 初始化/释放和 GLB 初始化，不是普通状态重置。每轮必须等到 `Tiles: ready`；30 秒未 ready 会停止并显示错误。内嵌浏览器中曾出现 30 秒未 ready 的停止结果，故不能把该环境的轮次记为十轮通过。

## 自动化验证

| 命令 | 实际结果 |
| --- | --- |
| `npm ci` | 未执行：避免中断当前前端服务和重置依赖目录。 |
| `npm run lint` | 通过，退出码 0。 |
| `npm run type-check` | 通过，退出码 0。 |
| `npm run test:unit` | 通过：4 个测试文件、22 个测试；新增 POC 测试覆盖严格 404、状态恢复、错误分类、厂房/子资源夹具、合成 worldZ、诊断导出、真实节点记录和 ready 门槛。 |
| `npm run build` | 通过。主 JS 为 `790.49 kB`（gzip `209.35 kB`）；Vite 给出大于 500 kB 警告，未作为通过项掩盖。 |
| `git diff --check` | 通过。 |

## 正常 Chrome/Edge 手工验收

本执行环境无法建立受控 Chrome 连接，因而没有 Chrome/Edge WebGL 截图、Network 导出、Performance/Memory 导出或最终证据 JSON。内嵌浏览器只用于支持性运行态观察，明确不作为最终人工证据来源。

以下项目仍为 **Awaiting manual evidence**：

- 厂房外部、正面高门、进入内部和提升机完整容纳关系；
- 真实 GLB 拾取（GLB / Tiles / 空白以及相机变化后）；
- 404、无效 JSON、子资源失败后的真实浏览器恢复；
- 合成夹具 worldZ 的低/中/高/返回低位及移动后拾取截图；
- 10 轮全部达到 ready 的生命周期记录；
- 冷缓存 3 次、暖缓存 3 次的首个可见时间、ready、请求数、字节、FPS、内存、绘制调用和活动瓦片。

一次性人工操作、截图和导出字段见 [POC-3DT-01-manual-evidence-guide.md](POC-3DT-01-manual-evidence-guide.md)。访问地址为 `http://127.0.0.1:5174/poc-3dtiles.html`；如需重新启动，请在 `idts3D_ui` 中执行 `npm run dev -- --port 5174`。

## 许可证据

| 项目 | 实际证据 |
| --- | --- |
| 包名 / 实际版本 | `3d-tiles-renderer@0.5.0` |
| lockfile | `idts3D_ui/package-lock.json` 中 `node_modules/3d-tiles-renderer` 为 `0.5.0`。 |
| 许可证 | 已安装包 `node_modules/3d-tiles-renderer/package.json` 声明 `Apache-2.0`。 |
| 许可证文件 | `idts3D_ui/node_modules/3d-tiles-renderer/LICENSE` 存在。 |
| Three peer dependency | `>=0.167.0` |
| 项目 Three | `0.177.0` |
| 兼容结论 | 当前 POC 依赖版本满足 peer range；该结论不替代法务审查。 |

未接入外部或 Phase B Tileset，也没有生产数据、外部 Tileset 性能或再分发许可证结论。

## 修改、未完成项与风险

修改/新增 POC 文件包括：

- `idts3D_ui/vite.config.ts`
- `idts3D_ui/src/poc/pocStrict404Middleware.ts`
- `idts3D_ui/src/poc/pocTilesRuntime.ts`
- `idts3D_ui/src/poc/pocWorldZ.ts`
- `idts3D_ui/src/poc/pocDiagnostics.ts`
- `idts3D_ui/src/poc/pocGlbNodes.ts`
- `idts3D_ui/src/poc/pocLifecycle.ts`
- `idts3D_ui/src/poc/PocTilesScene.ts`
- `idts3D_ui/src/poc/PocTilesViewport.vue`
- `idts3D_ui/src/views/Poc3dTiles.vue`
- `idts3D_ui/src/styles/poc3dtiles.css`
- `idts3D_ui/public/poc-3dtiles/child-missing/tileset.json`
- `idts3D_ui/public/poc-3dtiles/poc-lifter/poc-lifter.gltf`
- `idts3D_ui/tests/unit/PocTilesRuntime.spec.ts`
- `idts3D_docs/poc/POC-3DT-01-manual-evidence-guide.md`
- 本报告。

已知风险和未完成项：桌面浏览器证据未取得；真实 GLB 的正式升降部件语义映射仍缺失；生产级 Tileset、阶段 B 数据和性能预算未验证；构建主包体积警告仍待独立性能任务评估。

排除项：`.gitignore`、`tools/`、DOC-PLAN-08、TwinDemo、TwinScene、后端、数据库、`ScenesController`、`SceneManifestService`、`SceneManifestResponse`、正式 `src/api/scenes.ts`、MVP-10A-01～05 和 MVP-11～16 均未修改或执行。

## 最终判定

**Awaiting manual evidence**。严格 HTTP 404、错误隔离、合成 worldZ 路线、诊断与自动化质量门已完成；但任务卡要求的正常 Chrome/Edge 可视化、交互、生命周期和性能证据尚未取得，且真实 GLB 的业务升降部件未确认。因此本 POC 不能判定为 Pass 或 Conditional Pass，且 **Do not recommend unlock MVP-10A-01**。MVP-10A-01～MVP-10A-05 仍保持 Blocked，MVP-11～MVP-16 不得提前执行。
