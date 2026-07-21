# POC-3DT-01-CLOSEOUT 正常浏览器人工证据指南

## 前置条件

本指南仅用于 POC 独立入口。不要进入 TwinDemo、TwinScene 或任何正式业务页面。

1. 保持已有 Vite 服务运行；本次收尾已额外启动 `http://127.0.0.1:5174/poc-3dtiles.html`，该端口包含 POC 专用严格 404 中间件。
2. 使用正常 Google Chrome 或 Microsoft Edge，确认硬件加速和 WebGL 已启用；不要使用 Codex 内嵌浏览器作为最终证据。
3. 打开 DevTools 的 Console、Network、Performance（或 Memory）面板；Network 中勾选 Preserve log。性能冷缓存测试时勾选 Disable cache。
4. 页面加载后先等待诊断面板显示 GLB 地址、Canvas 数量和 Tiles 状态。所有截图需同时包含页面状态和浏览器地址栏，必要时包含诊断面板。

## 取证顺序

### 1. 正常同场、厂房和高门

1. 访问 `http://127.0.0.1:5174/poc-3dtiles.html`，等待 `Tiles：ready`。
2. 截取厂房外观：屋顶、左右/后墙、正面高门和真实 `lifter.glb` 必须同场可见。
3. 记录诊断面板中的 Canvas = 1、Renderer = 1、动画循环“运行”、活动瓦片、GLB 节点数量和 Console 错误数。
4. 用左键旋转、滚轮缩放、键盘移动；从正面高门进入。分别截图门外、门内、提升机底部/地坪、提升机顶部/屋顶，确认无明显穿地、穿墙、穿顶。

### 2. GLB 拾取

1. 点击真实 GLB，截图“GLB 拾取”和诊断面板的“选中对象”。
2. 点击厂房 Tiles，确认不会把 Tiles 写入 GLB 拾取结果；点击空白处并记录结果。
3. 旋转和缩放后再次点击真实 GLB，保留两张截图。若失败，记录相机状态和 Console/Network 错误。

### 3. 严格 HTTP 404、无效 JSON 和子资源失败

每次故障后都点击“加载本地最小 Tileset”，等待 `Tiles：ready`，然后再执行下一项。

1. 点击“注入严格 HTTP 404”。Network 必须显示 `/__poc_3dt__/missing/tileset.json` 为 **404** 且 `Content-Type: application/json`。截图页面错误、Network 条目、GLB 状态和诊断面板。页面不能白屏，GLB 与控制器必须保留。
2. 点击“注入 Tileset JSON 解析错误”。Network 中 `/poc-3dtiles/invalid-json/tileset.json` 应为 `200 application/json`；页面必须显示解析错误，不能把它写成 404。截图后恢复 ready。
3. 点击“注入子资源失败”。Network 中根 `child-missing/tileset.json` 正常，子 URI `/__poc_3dt__/missing/child/missing-child.gltf` 必须为 **404**。记录父内容/GLB 是否仍可用和恢复后的 ready。

### 4. 合成 worldZ 技术路线

真实 `lifter.glb` 没有经确认的 `lifter-platform` 节点，严禁选择 CAD 自动命名节点代替。点击“加载合成 POC worldZ 夹具”后，诊断面板应显示：

- GLB 地址：`/poc-3dtiles/poc-lifter/poc-lifter.gltf`
- 明确可动节点：`lifter-platform`

依次点击低位 0、中位 6、高位 12、低位 0，并在每一步截图诊断面板的 worldZ。每一步点击 `lifter-platform`，确认仍可拾取。该项只能证明 Three.js/GLTFLoader 子节点的 worldZ 技术路线；不得表述为真实提升机节点绑定已完成。

### 5. 十轮真实 Vue 生命周期

1. 仅在页面已经 `Tiles：ready` 后点击“连续执行 10 轮真实生命周期”。
2. 该入口每轮都会实际卸载并重建 `PocTilesViewport`、Three 渲染器、TilesRenderer 和 GLB；每轮先等待 `Tiles：ready`，30 秒未到 ready 会停止并显示错误，不能算通过。
3. 第 1、5、10 轮分别记录 Canvas、Renderer、动画循环、错误数、内存，并在第 10 轮导出证据 JSON。
4. 通过标准：每轮离开时 Canvas/Renderer/动画循环被释放；下一轮恢复后 Canvas = 1、Renderer = 1；没有新增 Console 错误或持续内存增长。

### 6. 冷/暖缓存性能

1. 冷缓存：Network 勾选 Disable cache，强制刷新，执行至少 3 次。每次记录首个可见时间、ready 时间、请求数、传输字节、稳定 FPS、内存、绘制调用、活动瓦片。
2. 暖缓存：取消 Disable cache，再进入至少 3 次，记录同一组指标。
3. 不可测的指标写“未测量”，不得补写估计值。将 3+3 组原始数值、Performance 导出或截图一并归档。

## 证据导出与结论

点击“导出当前 POC 证据 JSON”，浏览器会下载 `POC-3DT-01-evidence-YYYYMMDD-HHmmss.json`。该文件是当前运行态快照：未测量项为 `null`，不代表通过。

将截图、Network/Performance 导出和 JSON 文件交由项目负责人复核。仅在本指南所有必测项完成后，才可把结果报告由 `Awaiting manual evidence` 调整为 `Conditional Pass` 或 `Pass`；合成夹具 worldZ 场景最高只能为 `Conditional Pass`。
