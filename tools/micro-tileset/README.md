# POC-3DT-01 微型 3D Tiles 数据生成工具链

此工具链只生成开发和 POC 使用的微型静态仓库数据，不是现场数据、CAD/IFC 生产转换流程或性能承诺。它不读取、不复制也不打包 `lifter.glb`；提升机仍由现有 DeviceLayer 和普通 GLB 路径加载。

## 输出和分片

- `root.glb`：80m × 60m × 15m 仓库的低模地面、外墙、立柱和四个区域标识。
- `area-a.glb`、`area-b.glb`、`area-c.glb`、`area-d.glb`：四个仓储区的重复静态货架。
- `tileset.json`：3D Tiles 1.1，content URI 直接使用相对 GLB URI，不生成 B3DM。
- `tile-metadata.json`：Blender 在导出时按每个分区对象的实际 world-space 包围盒计算的元数据。Node 生成器只读取该文件，不手写 `boundingVolume`。

所有生成物位于受控且被 Git 忽略的 `output/`：稳定交付目录为 `output/tileset/`，验证与构建报告位于 `output/reports/`。

## 坐标约定

单位统一为米。Blender 场景原点为仓库地面中心 `(0, 0, 0)`；Blender 原生坐标为 `X` 东西向、`Y` 南北向、`Z` 向上。GLB 按 glTF Y-up 导出，工具在 metadata 中将实测 Blender 点显式转换为 `[x, z, -y]`：导出的 `X` 仍为东西向、`Y` 向上、`Z` 向南。`tileset.json` 使用这一导出坐标系中的包围盒。

## 前置条件

- Blender 4.x，可通过 `-BlenderPath` 参数或 `BLENDER_PATH` 指定，不写死本机绝对路径。
- Node.js。
- 可执行的 `gltf-transform`，构建会对每个 GLB 执行 `validate` 与 `inspect`。
- 可选：`3d-tiles-tools`。传入 `-Analyze` 时，若命令存在则执行 `analyze -i <tileset-dir> -o <report-dir>`；若不存在，报告会记录跳过原因。

不安装或启用 Draco、Meshopt、KTX2、WebP。

## 使用方法

```powershell
.\tools\micro-tileset\build.ps1 -BlenderPath "C:\path\to\blender.exe"

# 或者
$env:BLENDER_PATH = "C:\path\to\blender.exe"
.\tools\micro-tileset\build.ps1

# 可选的 3d-tiles-tools 分析
.\tools\micro-tileset\build.ps1 -Analyze
```

先执行 Node 生成器测试：

```powershell
node --test .\tools\micro-tileset\tests\generate-tileset.test.mjs
```

构建报告 `output/reports/build-report.json` 列出每个 GLB 的文件大小、节点数、Mesh 数、三角面数、Blender 实测包围盒、导出 glTF 包围盒和总耗时。每个 GLB 的 glTF Transform 验证和 inspect 输出也保留在 `output/reports/`。
