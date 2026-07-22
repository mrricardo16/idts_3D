# PERF-FIX-01｜版本化模型 URL 与静态资源缓存策略 POC 结果

## 结论

**Cache POC Fail**。

版本化 URL、流式读取、immutable 响应头、Range、版本失效和页面渲染均已验证；但本机 Chrome 在同一 `BrowserContext` 的三次真实 `page.reload()` 中，仍对约 279.61 MiB 的真实 GLB 每轮完整传输。开发服务和生产构建预览服务的暖缓存平均传输均为冷缓存的 100%，不满足 `<= 冷缓存 1%` 或 `<= 5 MiB` 的门槛。

本结果只说明“仅靠版本化 URL + immutable HTTP 缓存”不能解决此真实资源的 reload 重传，未实施任何模型、Mesh、LOD、3D Tiles 或渲染优化。

## 执行基线与范围

| 项目 | 值 |
| --- | --- |
| 起始分支 / Commit | `main` / `8f980e9b6cf954f9959c6ab264d8381160921acc` |
| 执行分支 | `perf/poc-3dt-cache`，从已同步的 `main` 创建 |
| 浏览器 | 本机 Chrome，headed，1920 × 1080，deviceScaleFactor 1 |
| 缓存采样 | 冷缓存：每服务 3 个独立 Context；暖缓存：同一 Context 首次加载后连续 3 次 `page.reload()`；无 Trace、视频、连续截图、请求拦截或故障注入 |
| 范围 | 仅 POC 页面、POC Vite 中间件、POC 单元/E2E、原始 CDP JSON 和本报告 |

原始模型 `idts3D_ui/public/models/lifter.glb` 只被 `stat`、SHA-256 流和 `createReadStream` 读取，没有复制或修改。

## 版本化资产设计

| 项目 | 值 |
| --- | --- |
| 原始路径 | `/models/lifter.glb` |
| 文件大小 | `293,192,660 B` |
| 完整 SHA-256 | `b91bde3f0d7ffc6a484b2eab2588774d3301bbce6e32c4fc084f3eef1fb48bd2` |
| URL 短哈希 | `b91bde3f0d7ffc6`（完整哈希仍保留在 URL） |
| 版本化 URL | `/__poc_cache__/models/lifter/b91bde3f0d7ffc6a484b2eab2588774d3301bbce6e32c4fc084f3eef1fb48bd2/lifter.glb` |
| 清单 | `/__poc_cache__/manifest.json`，`Cache-Control: no-cache` |
| 版本化 GLB | `Cache-Control: public, max-age=31536000, immutable` |
| ETag | 稳定的完整 SHA-256 ETag |
| Last-Modified | 原始 GLB 的文件修改时间 |
| Content-Length / Content-Type | `293192660` / `model/gltf-binary` |
| Accept-Ranges | `bytes` |

中间件只拦截 `/__poc_cache__/*`，从原始文件流式读取；`/models/lifter.glb` 基线路径未被中间件拦截或改写。页面有显式 `baseline` 与 `versioned-cache` 切换，默认仍为 baseline。

Range 自动化验证在 Vite development 和 Vite preview 均通过：完整资源为 `200`，`bytes=0-15` 为 `206` 且 `Content-Range: bytes 0-15/293192660`，越界 Range 为 `416` 且 `Content-Range: bytes */293192660`。错误内容哈希 URL 返回 JSON `404`，不会回退为 HTML。

## CDP 实际传输结果

主统计口径为 `Network.loadingFinished.encodedDataLength`，不将 `PerformanceResourceTiming.transferSize`、文件体积或页面累计资源量当作实际传输量。

| 服务器 | 冷缓存 1 | 冷缓存 2 | 冷缓存 3 | 冷均值 | 暖 reload 1 | 暖 reload 2 | 暖 reload 3 | 暖均值 | 暖/冷 |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| Vite development | 293,193,017 B | 293,193,017 B | 293,193,017 B | 293,193,017 B | 293,193,017 B | 293,193,017 B | 293,193,017 B | 293,193,017 B | 100% |
| Vite preview | 293,193,017 B | 293,193,017 B | 293,193,017 B | 293,193,017 B | 293,193,017 B | 293,193,017 B | 293,193,017 B | 293,193,017 B | 100% |

两端所有真实 GLB 请求均为 HTTP `200`；`fromDiskCache=false`、`requestServedFromCache=false`、无 `304`、无加载失败。三次暖 reload 均完整重传，因此门槛不通过：

- 1% 冷缓存约为 `2,931,930 B`；实测暖均值为 `293,193,017 B`。
- 5 MiB 为 `5,242,880 B`；实测暖均值远高于此值。
- 结论不可通过降低门槛或把普通页面导航替换为任务要求的 reload 来改变。

## 版本失效与页面回归

使用两个现有的小型 POC glTF 夹具验证版本失效，未复制真实 GLB：

| 阶段 | URL / 传输 | 结论 |
| --- | --- | --- |
| v1 首次 | `minimal.gltf`，内容哈希 `1e17fc81…271345`，CDP `2,906 B` | 首次请求新资源 |
| v1 暖加载 | 相同 v1 URL，`fromDiskCache=true`，CDP `0 B` | 命中磁盘缓存 |
| v2 首次 | `poc-lifter.gltf`，内容哈希 `a36d5c65…a3f1c7`，CDP `2,684 B` | 新 URL 重新传输，未串用 v1 |

真实 GLB versioned-cache 页面在开发和预览两端均达到 `Tiles: ready`、GLB 已加载、Canvas / Renderer 为 `1 / 1`；阻断 Console 错误和 `pageerror` 均为 0。预览服务曾出现浏览器根 `favicon.ico` 自动探测 404，已通过 POC 独立入口图标处理；最终自动化证据中无该事件。

## 验证与证据

| 命令 | 结果 |
| --- | --- |
| `npm run lint` | 通过 |
| `npm run type-check` | 通过 |
| `npm run test:unit` | 7 files / 36 tests 通过 |
| `npm run build` | 通过；保留既有大 chunk 警告 |
| `npm run test:poc:perf-fix-01` | 3 E2E 通过；结果数据判定 Cache POC Fail |

原始 CDP 证据：[`perf-fix-01-cache-result.json`](evidence/POC-3DT-01/perf-fix-01-cache-result.json)。

## 未解决问题和边界

- 真实 GLB 的 Draw calls、Triangles、P5 FPS、JS Heap、业务节点与运动部件不在本任务范围，均未修改。
- 正式生产静态资源托管层（Nginx / ASP.NET Core）尚未确认；本中间件仅为 Vite POC，不是生产配置。
- `POC-3DT-01` 继续为 **Fail**。
- `MVP-10A-01`～`MVP-10A-05` 继续为 **Blocked**；**Do not recommend unlock MVP-10A-01**。

下一建议仅供后续独立授权时评估：先诊断 Chrome 对该 293 MB 单资源在 reload 下的可缓存大小限制及正式托管层行为，再决定是否需要生产托管策略或其他已授权的资源交付方案；本任务不执行 PERF-FIX-02。
