# digital-twin-ui-design

Codex Skill for enterprise digital-twin monitoring frontends using Vue 3, Vite, TypeScript, Three.js, GLB/glTF, 3D Tiles, and 3DTilesRendererJS.

## Install in a repository

Copy this folder to:

```text
<REPO_ROOT>/.agents/skills/digital-twin-ui-design/
```

Expected structure:

```text
.agents/
└── skills/
    └── digital-twin-ui-design/
        ├── SKILL.md
        ├── README.md
        ├── agents/
        │   └── openai.yaml
        └── references/
            ├── product-visual-system.md
            ├── scene-interaction-status.md
            ├── vue-three-architecture.md
            └── acceptance-review.md
```

## Explicit invocation examples

```text
$digital-twin-ui-design
重新设计数字孪生监控主界面。保持现有 Three.js 场景加载逻辑和 API 不变，重点提升监控信息层级、设备状态识别、告警定位、选中对象详情和 2D/3D 联动。完成后运行页面并按 Skill 验收。
```

```text
$digital-twin-ui-design
审查当前 TwinWorkspaceView 和 Three.js 场景交互。重点检查业务 ID 与 GLB 节点映射、全场景遍历、状态材质更新、选中态是否覆盖故障态、Vue 响应式与 render loop 耦合问题。按 P0/P1/P2/P3 输出。
```

```text
$digital-twin-ui-design
为当前 3DTilesRendererJS 场景增加设备告警定位和对象详情联动。不要把 UI 直接绑定到 SignalR；先定义 normalized TwinStateUpdate，并保持 HTTP、轮询、未来 SignalR 可替换。
```

```text
$digital-twin-ui-design
将当前数字孪生页面从展示型大屏重构为日常监控工作台。3D 场景作为空间工作区，异常优先于 KPI。禁止通用 AI Dashboard、过度霓虹、四卡片 KPI 和无意义科技动效。
```

## Recommended project usage

Keep this skill in the repository because its rules are specific to the project's frontend architecture and digital-twin product behavior.

Use explicit `$digital-twin-ui-design` invocation for major redesign, architecture review, status/alarm interaction, scene/UI linkage, and visual acceptance tasks.

The skill description is also written to support implicit activation for digital-twin UI tasks.
