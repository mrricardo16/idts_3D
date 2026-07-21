# POC-3DT-01 性能失败根因诊断与测量口径校正

> 诊断日期：2026-07-21  
> 范围：仅 POC-3DT-01；本报告不改变既有正式结论。  
> 原始数据：[automated-chrome-performance-diagnosis.json](evidence/POC-3DT-01/automated-chrome-performance-diagnosis.json)

## 1. 结论边界

本次工作完成了测量口径校正和根因诊断，未实施性能优化。正式状态不变：

```text
POC-3DT-01：Fail
MVP-10A-01～MVP-10A-05：Blocked
解锁建议：Do not recommend unlock MVP-10A-01
```

诊断发现有两类不同的问题：

1. 旧基线的 FPS 采样只有约 1.1 秒，且采样期间启用了 Playwright Trace、截图和快照，不能作为本轮 30 秒正式 FPS 口径；旧的 `PerformanceResourceTiming.transferSize` 也不是本轮的网络传输主口径。
2. 真实 `lifter.glb` 仍是确定的主要运行时成本：本机 Chrome 中约 1,087 draw calls、8,526,166 triangles、1,123 个 GLB Mesh、约 297–313 MiB JS Heap；厂房 Tiles 仅增加 8 calls 和 96 triangles。CDP 同时证明每次重新加载仍接收约 279.61 MiB 的真实传输字节。

因此，旧的 `41.3 FPS` 不能继续当作正式 FPS 数值；但其“真实 GLB/大传输是风险源”的方向没有被推翻。新的正式采样平均 FPS 高于现有 45 FPS 门槛，而 P5 均约 29.94 FPS、最小值可低至 11.99 FPS，帧时间稳定性仍是必须单独处理的风险，不能据此改变当前 Fail。

## 2. 测量环境与口径

| 项目 | 本次固定值 |
| --- | --- |
| 浏览器 | 本机 headed Chrome 149.0.7827.55 |
| GPU | Intel Iris Xe Graphics / ANGLE Direct3D11 |
| 视口 | 1920 × 1080，deviceScaleFactor 1 |
| 相机 | `factory-exterior` 固定厂房外观预设 |
| 每轮 | ready 后预热 5 秒，连续 rAF 采样 30 秒 |
| 重复 | 每场景、每服务器 3 轮 |
| 官方 FPS 轮 | Trace、视频、截图、故障注入、生命周期切换和请求拦截均关闭 |
| 传输主口径 | Chrome CDP `Network.loadingFinished.encodedDataLength` |

FPS 是相邻 `requestAnimationFrame` 间隔换算的逐帧 FPS；P5 为这些逐帧样本的第 5 百分位，不是旧诊断面板约 1 秒一次的聚合值。每轮样本数为 1,186–1,440。

## 3. 生产预览入口自检与修正

首轮预览诊断发现 `/poc-3dtiles.html` 在生产 `vite preview` 中回退到默认 `index.html`，实际加载 TwinDemo，故没有 POC Canvas。原因是构建仅把 `index.html` 作为 Rollup 输入，开发服务器会直接提供根目录 HTML，因此该问题不会在 Vite 开发服务器中暴露。

已将 `poc-3dtiles.html` 作为 POC 的独立构建输入。修正后产物包含 `dist/poc-3dtiles.html` 和 POC 专用 JS/CSS，预览冒烟验证可创建一个 POC Canvas。此修正仅保证生产预览测量目标正确，不改变 TwinDemo、TwinScene 或正式业务入口。

## 4. 网络与缓存审计

每个服务器均执行 3 个相互独立的冷缓存 Context，以及同一个未重启、未清理缓存的 BrowserContext 内 3 次暖缓存 reload。官方冷暖缓存轮没有 `context.route`。所有关注请求均由 CDP 记录 `requestWillBeSent`、`responseReceived`、`requestServedFromCache`、`loadingFinished` 与 `loadingFailed`。

| 服务器 | 模式 | 每轮 CDP encodedDataLength | HTTP 304 | memory cache | disk cache | 失败 |
| --- | --- | ---: | ---: | ---: | ---: | ---: |
| Vite 开发 | 冷缓存 ×3 | 293,196,341 B / 279.61 MiB | 0 | 0 | 0 | 0 |
| Vite 开发 | 暖缓存 ×3 | 293,193,197 B / 279.61 MiB | 0 | 0 | 0 | 0 |
| Vite 预览 | 冷缓存 ×3 | 293,196,341 B / 279.61 MiB | 0 | 0 | 0 | 0 |
| Vite 预览 | 暖缓存 ×3 | 293,193,197 B / 279.61 MiB | 0 | 0 | 0 | 0 |

三项内容请求在两类服务器的观察值一致：

| 资源 | 状态 | Cache-Control | ETag / Last-Modified | Content-Length | CDP encodedDataLength（冷） |
| --- | ---: | --- | --- | ---: | ---: |
| `/models/lifter.glb` | 200 | `no-cache` | 有 / 有 | 293,192,660 B | 293,192,943 B |
| `minimal/tileset.json` | 200 | `no-cache` | 有 / 有 | 301 B | 571 B |
| `minimal/minimal.gltf` | 200 | `no-cache` | 有 / 有 | 2,556 B | 2,827 B |

暖缓存轮中 GLB 仍为 200，且仍有 293,192,943 B `encodedDataLength`；这说明当前本机服务响应的 `Cache-Control: no-cache` 使大 GLB 每次 reload 都重新验证并接收完整内容。暖缓存中两个小 JSON/glTF 请求仅计 127 B，但它们不足以改变总体传输量。故旧证据中的约 279.6 MiB 量级不是“缓存统计虚高”；它需要改用 CDP 表述，且其真实主因是 `lifter.glb`。

请求拦截影响审计在两类服务器一致：未拦截暖 reload 为 293,193,197 B；安装一个只 `continue()` 的 route 后为 293,196,341 B，差异为两个小 POC 文件恢复实体传输。真实 GLB 两边均为 293,192,943 B。该对照证明请求拦截会改变小资源缓存行为，因此没有用于任何正式缓存或 FPS 轮。

## 5. 五组场景矩阵

以下为三轮平均的正式帧采样；“最低”取三轮中的全局最低值。JS Heap 为轮末 `performance.memory.usedJSHeapSize` 平均值。

| 服务器 | 场景 | 平均 FPS | P5 FPS | 最低 FPS | Calls | Triangles | JS Heap |
| --- | --- | ---: | ---: | ---: | ---: | ---: |
| 开发 | A 厂房 Tiles | 60.10 | 59.52 | 51.02 | 10 | 110 | 9.40 MiB |
| 开发 | B 真实 GLB | 54.84 | 29.94 | 28.57 | 1,087 | 8,526,166 | 300.92 MiB |
| 开发 | C 厂房 + 真实 GLB | 53.17 | 29.94 | 29.24 | 1,095 | 8,526,262 | 313.35 MiB |
| 开发 | D 厂房 + 合成 GLB | 60.10 | 59.52 | 51.28 | 16 | 182 | 9.16 MiB |
| 开发 | E C + 降低诊断刷新 | 51.61 | 29.94 | 14.97 | 1,095 | 8,526,262 | 301.80 MiB |
| 预览 | A 厂房 Tiles | 60.03 | 59.52 | 57.47 | 10 | 110 | 6.12 MiB |
| 预览 | B 真实 GLB | 55.68 | 29.94 | 12.00 | 1,087 | 8,526,166 | 296.71 MiB |
| 预览 | C 厂房 + 真实 GLB | 54.48 | 29.94 | 14.99 | 1,095 | 8,526,262 | 299.03 MiB |
| 预览 | D 厂房 + 合成 GLB | 60.05 | 59.52 | 55.25 | 16 | 182 | 5.91 MiB |
| 预览 | E C + 降低诊断刷新 | 54.48 | 29.94 | 11.99 | 1,095 | 8,526,262 | 298.23 MiB |

每轮原始值还记录了平均/中位数/P5/最低/最高/标准差/样本数、long task、活动及已加载瓦片、几何体、纹理、场景对象、GLB Mesh 和材质数，见原始 JSON。C 场景运行时为：1 个 Canvas、1 个 Renderer、1 个活动/已加载 Tiles、1,206 个场景对象、1,123 个 GLB Mesh、8 个 Tiles Mesh、3 个已见材质 UUID；未出现 POC 资源失败、未出现 page error。

## 6. 根因归因

### 6.1 1,133 级 draw-call 现象

真实 GLB 独立场景已达到 1,087 calls 和 8,526,166 triangles；叠加厂房仅增至 1,095 calls 与 8,526,262 triangles。厂房本身只有 10 calls / 110 triangles，合成 POC GLB 场景也稳定在 16 calls / 182 triangles。由此可排除厂房 Tiles 作为 draw-call 主因，主要来源是未被批处理的真实 GLB Mesh/材质结构与约 850 万三角形。

### 6.2 诊断面板与响应式刷新

正常 C 场景的探针发布频率约每秒一次，E 场景提高到约每 10 秒一次（30 秒采样内约 4 次）。开发服务器 E 的平均 FPS 反而从 53.17 降至 51.61，预览从 54.48 基本不变为 54.48；P5 也没有改善。因此诊断面板刷新不是主要瓶颈。E 的最低帧更低，说明它不能用于掩盖真实低帧区间。

### 6.3 开发服务器与预览差异

两者缓存响应头和实际传输量相同，均未形成 GLB 暖缓存。预览 C 场景平均 FPS 比开发高约 1.30 FPS，但两者的 P5 均为 29.94 FPS，且均存在低帧。当前差异不足以将问题归因到 Vite 开发开销。

### 6.4 Trace、截图、快照干扰

Trace 干扰轮独立于正式矩阵，并启用了 Trace 截图和 DOM snapshots：

| 服务器 | 无 Trace 对照平均 FPS | Trace+截图+快照平均 FPS | 说明 |
| --- | ---: | ---: | --- |
| 开发 | 50.13 | 45.02 | 单轮下降约 5.11 FPS；存在可测干扰 |
| 预览 | 52.52 | 52.48 | 单轮差异很小 |

两轮均只用于验证干扰方向，不能当成正式性能数据。证据 Trace 位于 `evidence/POC-3DT-01/performance-diagnosis/`；正式 FPS 轮没有 Trace、视频或连续截图。

### 6.5 生命周期、Renderer、rAF 与 Controls

运行时探针和页面 DOM 均显示 1 个 Canvas、`rendererCount=1`、`renderLoopStarts=1`。渲染循环内每帧只调用一次 `controlsManager.update()`；不存在重复 Renderer、重复 rAF 或重复 Controls update 的证据。该项不是本次低 P5 的根因。

### 6.6 内存、长任务与加载

真实 GLB 场景轮末 JS Heap 约 297–313 MiB；厂房/合成夹具场景约 6–10 MiB。真实 GLB 相关场景每轮观察到约 2–3 个 Long Task，最大值 688–1,310 ms；轻量场景最大值为 146–200 ms。C 场景的 Tiles `ready` 时间为开发 795–927 ms、预览 840–1,111 ms，但这只是 tiles 内容 ready 时间，不能替代含 293 MiB GLB 的完整页面加载时延。

## 7. 构建包与证据审计

构建后 POC 已独立输出 `poc-3dtiles.html` 与约 86.73 kB 的 POC 入口 JS；共享 Three/Tiles 运行时仍在构建产物内的共享 chunk。Playwright 仅为开发依赖，未被 POC 页面运行时导入；本次构建产物检查未发现 `@playwright/test` 运行时代码。该审计不建议在本诊断任务中做代码分包优化。

证据清单：

- `automated-chrome-performance-diagnosis.json`：12 轮缓存、30 轮矩阵、4 轮干扰和 2 轮运行时结构原始记录。
- `performance-diagnosis/vite-development-trace-and-snapshots.zip`：开发服务器 Trace/截图/快照干扰证据。
- `performance-diagnosis/vite-preview-trace-and-snapshots.zip`：生产预览 Trace/截图/快照干扰证据。
- 旧 `automated-chrome-performance-baseline.json`：保留不覆盖，仅用于说明旧采样口径局限。

已检查预览 Trace 压缩包，包含 `trace.trace`、`trace.network` 和多张 `resources/page@*.jpeg` 截图；官方 30 轮的 console/page error 为 0，CDP 缓存审计的请求失败为 0。

## 8. 后续边界

本报告只完成诊断。任何 GLB 网格合并、实例化、LOD、压缩、缓存头策略、加载链路或渲染策略调整均属于新的、独立授权的性能修复任务。当前不得以本报告建议解锁 MVP-10A-01。

```text
Diagnosis complete
```
