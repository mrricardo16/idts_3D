# AGENTS.md

本文件是 `idts3D_ui` 前端工程维护规则。仓库根目录 `AGENTS.md` 的规则优先适用于所有任务；本文件只补充前端边界。

## 项目边界

1. 本项目是 Vue 3 + Vite + TypeScript + Three.js 前端工程，由原技术预研 Demo 演进为 IDTS 3D 数字孪生前端。
2. 不属于旧 `HZ.IDTS.UI` 或 `HZ.IDTS.API`，不得修改旧项目代码。
3. 前端开发任务只有在用户明确指定前端任务卡或联调任务时，才允许修改 `idts3D_ui/src/**`。
4. 正式后端接入必须通过 `src/api` API Client 层，不允许在页面和 engine 中散落直接 `fetch`。
5. 没有后端、后端不可用或没有 `public/models/lifter.glb` 时，必须保留 fallback 到本地模型 / 几何体场景的能力。

## 模型文件规则

1. 不允许提交 STEP / STP / IGES / FBX 等 CAD 源模型文件。
2. `source-models` 可以保留 `.gitkeep`。
3. 转换后的轻量 GLB 放入 `public/models/lifter.glb`。
4. 真实 GLB 中关键对象应保留可识别的 `mesh.name`。

## 代码规则

1. Three.js 逻辑必须模块化，不要全部堆在 Vue 文件中。
2. 变更前必须说明影响范围。
3. README 必须随功能或模型处理流程同步更新。
4. 前端字段、后端 DTO、API 契约、TypeScript interface 必须保持同名或有明确映射。
5. `monitor` 模式不得保存配置；`edit` 模式保存必须调用任务卡指定 API。
6. API 调用集中在 `src/api`。
7. 类型集中在 `src/types`。
8. 页面不直接拼接后端 URL。
9. `TwinScene.ts` 只消费解析后的 scene manifest、model manifest、object tree、movable part、motion target 数据。
10. `LODModelLoader.ts` 只负责模型加载，不直接关心数据库实体。
11. `localStorage` 只能作为 fallback 或编辑草稿缓存，不能伪装成 Published 后端配置。

## 验证规则

前端修改完成后，根据 lockfile 判断包管理器：

- 有 `pnpm-lock.yaml` 用 pnpm。
- 有 `yarn.lock` 用 yarn。
- 有 `package-lock.json` 用 npm。
- 都没有时按 `README.md` 和 `package.json` 判断，不要擅自切换包管理器。

构建命令以 `package.json` scripts 为准。
如果无法执行 build，必须说明原因，不要假装通过。

## 编码规则

修改代码时必须保持文件编码为 UTF-8，不要使用 ANSI/GBK。不要把已有中文注释、中文文案、中文日志改成乱码。如果发现文件疑似非 UTF-8 编码，先提示用户确认，不要直接重写整个文件。
