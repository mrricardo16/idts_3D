# IDTS 3D 数字孪生系统

## 目录结构

- `idts3D_ui`：Vue3 + TypeScript + Three.js 前端工程，由原 `idts3ddemo` 技术验证 Demo 迁移而来。
- `idts3D_api`：.NET 8 后端 API 与 Worker 工程目录，后续创建正式后端解决方案。
- `idts3D_docs`：项目方案、需求补充、MVP 计划、任务卡。

## 前端运行

```bash
cd idts3D_ui
npm install
npm run dev
npm run build
```

## 版本控制边界

- Git 管理项目源码、配置、脚本和文档。
- `idts3D_ui/node_modules`、`idts3D_ui/dist`、缓存、日志和临时文件不纳入 Git。
- `idts3D_ui/public/models/lifter.glb` 是大模型运行文件，默认不作为普通 Git 对象提交；如需纳入版本控制，建议使用 Git LFS。
