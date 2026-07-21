# POC-3DT-01 验收结果报告

```text
任务：POC-3DT-01 验收闭环
执行日期：2026-07-21
结论：Fail
解锁建议：Do not unlock MVP-10A-01
```

本报告以本地离线厂房夹具、真实 `lifter.glb` 与真实 Chrome/Edge E2E 为证据。它不构成生产 Tileset、现场网络、真实设备业务节点或正式场景契约的验收。

## 1. 执行基线与范围

| 项目 | 实际值 |
| --- | --- |
| Git 基线 | `main` / `d4682a7`（验收修改前） |
| 本轮浏览器服务 | Playwright 启动的本地 Vite：`127.0.0.1:5180` |
| 自动化浏览器 | 本机 Chrome channel；本机 Edge channel |
| 性能视口 | `1440 × 1000` |
| Tileset | `/poc-3dtiles/minimal/tileset.json` |
| 动态模型 | `/models/lifter.glb` |
| 依赖 | `3d-tiles-renderer@0.5.0`、`three@0.177.0` |

本轮仅修改 POC 专用 Playwright 测试、Playwright 取证配置、POC 证据目录与本报告。未修改 TwinDemo、TwinScene、后端、数据库、正式 API/契约、模型 GLB、`.gitignore`、`tools/` 或任何 MVP-10A 任务卡。

## 2. 浏览器功能证据

| 验收项 | 证据与结果 |
| --- | --- |
| Chrome WebGL、同 Canvas | Chrome E2E 通过：厂房 Tiles 与真实 GLB 均已加载，诊断为 Canvas / Renderer = `1 / 1`。 |
| Edge 同 Canvas | Edge E2E 通过：`loads the local factory and real GLB into one local Chrome canvas`，耗时 6.0 s。 |
| 真实 GLB 拾取 | Chrome E2E 向 Canvas 发送真实指针事件，在候选点击网格中选中真实 GLB mesh `NAUO682`；未使用测试专用选择接口。 |
| 404 与恢复 | Chrome E2E 确认根 URI 返回 `404 application/json`，GLB 与单 Canvas 保留，重载本地夹具后恢复 `ready`。 |
| JSON 与子资源故障 | Chrome E2E 确认 JSON 解析失败、子 glTF `404` 以及恢复路径；运行期未白屏。 |
| worldZ | 合成 POC glTF 的 `lifter-platform` 通过 `0 / 6 / 12` 三点。真实 `lifter.glb` 没有经确认的业务升降节点，因此该项只证明技术路线。 |
| 10 轮生命周期 | 完整 Chrome E2E（10 项）通过，包含 10 次真实 Vue 卸载/重建；最终仍为 Canvas / Renderer = `1 / 1`，无页面 `.poc-error`。 |

用户提供的正常 Chrome 150 WebGL 运行态 JSON 同样记录：`canvasCount=1`、`rendererCount=1`、Tiles 和真实 GLB 均为本地资源；页面与严格 404 Network 截图已归档。其一次运行的 ready 用时为 33,475.1 ms，不能替代本轮自动化性能基线。

## 3. Playwright 证据复核

已检查并归档下列原始证据：

- [真实 GLB 拾取截图](evidence/POC-3DT-01/automated-chrome-real-glb-pick.png)：页面为 `Tiles: ready`，面板显示 `GLB 拾取：NAUO682`。
- [冷缓存截图](evidence/POC-3DT-01/automated-chrome-cold-cache-1.png) 与 [暖缓存截图](evidence/POC-3DT-01/automated-chrome-warm-cache-3.png)：均为 `Tiles: ready`，GLB 地址、单 Canvas/Renderer、活动 Tiles = 1 可见。
- [冷缓存 Trace](evidence/POC-3DT-01/automated-chrome-cold-cache-1-trace.zip) 与 [暖缓存 Trace](evidence/POC-3DT-01/automated-chrome-warm-cache-trace.zip)：已检查 ZIP，均包含 `trace.trace`、`trace.network` 和页面资源快照。
- [性能、Console 与 Network 原始 JSON](evidence/POC-3DT-01/automated-chrome-performance-baseline.json)：包含每一轮请求路径、传输字节、Console、Page error、FPS、内存、draw calls 与活动 Tiles。
- [用户 Chrome 正常 Network 截图](evidence/POC-3DT-01/user-chrome-ready-network.png) 与 [用户 Chrome 严格 404 截图](evidence/POC-3DT-01/user-chrome-strict-404-network.png)。

六轮性能取证的 POC Console error 和 `pageerror` 均为 0。冷缓存第 1 轮出现一次 `/favicon.ico` 404，已保留在 JSON 的 `ignoredConsoleErrors`；它不属于 POC Tiles/GLB 资源或运行时错误。

## 4. 冷缓存 / 暖缓存性能基线

测试方法：冷缓存为 3 个独立 Chrome context，且经 CDP 设置 `Network.setCacheDisabled=true`；暖缓存为同一 Chrome context 预热后 3 次重载。`firstVisibleAndReadyMs` 对应 POC 的 `load-model → ready` 诊断门槛。未在本轮伪造 1920 × 1080、60 秒稳定窗口、1% low FPS 或设备/GPU 信息。

| 指标 | 冷缓存（3 轮均值） | 暖缓存（3 轮均值） | 观察 |
| --- | ---: | ---: | --- |
| 页面导航至 ready | 5,804.6 ms | 5,690.1 ms | 暖缓存改善仅 114.5 ms。 |
| Tiles 首可见 / ready | 1,033.0 ms | 1,045.9 ms | 无改善。 |
| 请求数 | 7 | 7 | 无减少。 |
| 传输量 | 279.619 MiB | 279.615 MiB | 实际上没有有效的 GLB 缓存节省。 |
| 平均 FPS | 46.3 | **41.3** | 暖缓存低于阶段 A 暂定 `>=45` FPS 门槛。 |
| draw calls | 1,095 | 1,095 | 稳定。 |
| 活动 Tiles | 1 | 1 | 稳定。 |
| JS heap 范围 | 301.6–871.1 MiB | 300.7–879.0 MiB | 采样波动大，不能证明 60 秒内存稳定。 |

Network 记录显示每轮都传输了 `/models/lifter.glb` 的约 293,192,960 bytes；暖缓存没有避免该大文件传输。还可见 `/models/lifter/lifter.medium.glb` 与 `lifter.low.glb` 的探测请求。这些结果只描述本机 Vite POC，不可外推为真实厂区 Tileset 或生产网络性能。

## 5. 自动化质量门

| 命令 | 结果 |
| --- | --- |
| `npm run test:e2e` | Chrome 10 / 10 通过；包含本轮真实 GLB 拾取、冷/暖各 3 轮与 10 轮生命周期。 |
| `npm run test:e2e:edge -- --grep "loads the local factory and real GLB into one local Chrome canvas"` | Edge 1 / 1 通过。 |
| `npm run lint` | 通过。 |
| `npm run type-check` | 通过。 |
| `npm run test:unit` | 通过：5 个文件、24 个测试。 |
| `npm run build` | 通过；仍有主 JS `790.67 kB`（gzip `209.38 kB`）超过 Vite 500 kB 的警告。 |
| `git diff --check` | 通过。 |

## 6. 判定与后续门禁

**Fail**，依据如下：

1. 暖缓存平均 FPS 为 **41.3**，低于 [3D 性能预算](../performance/3d-performance-budget.md) 阶段 A 暂定门槛 `>=45`。
2. 暖缓存传输量与冷缓存几乎相同（约 279.6 MiB），293 MB 的真实 GLB 仍在每轮完整传输，不能将当前路径称为“已具备暖缓存性能收益”。
3. 内存仅为短窗口取样，且范围为 300.7–879.0 MiB；未按预算规定完成 60 秒稳定观察，不能证明 10 轮后的内存增长门槛。

功能、故障隔离、真实 GLB Canvas 拾取、合成 worldZ 和生命周期的浏览器证据均为通过；但性能结论不满足 Conditional Pass 所需的全部条件。要重新评估，至少需要修复/明确真实 GLB 的缓存策略并在固定 `1920 × 1080`、记录的 CPU/GPU 环境下重新完成 3 冷 + 3 暖、60 秒稳定采样及 1% low FPS。

因此，**MVP-10A-01～MVP-10A-05 仍为 Blocked**。本报告不会、也不能自动解锁 MVP-10A-01；只有问题修复、复验获得满足条件的报告后，才由用户单独明确授权下一任务。
