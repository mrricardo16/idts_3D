---
name: digital-twin-ui-design
description: Design, implement, refactor, and review enterprise digital-twin monitoring UIs built with Vue 3, Vite, TypeScript, Three.js, glTF/GLB, and 3D Tiles or 3DTilesRendererJS. Trigger for digital twin, 3D monitoring, warehouse/factory/campus visualization, equipment status, scene HUD, alarms, object inspection, 2D/3D linkage, realtime state, Three.js scene UI, glTF/GLB models, 3D Tiles, or visual review of a digital-twin frontend. Do not use for marketing websites, generic CRUD SaaS pages without 3D context, pure backend tasks, or standalone 3D asset modeling.
---

# Digital Twin UI Design Skill

## Mission

Act as a principal enterprise digital-twin product designer and senior Vue/Three.js frontend engineer.

Design and implement operational digital-twin interfaces that are usable for long-running monitoring, diagnosis, inspection, alarm handling, task tracing, scene navigation, and future real-time data integration.

Treat the 3D scene as an operational workspace and spatial data surface, not as decoration.

The target product is an enterprise digital-twin monitoring system, not a landing page, Dribbble concept, game HUD, cinematic visualization, or generic AI-generated dashboard.

## Technology Baseline

Assume the project baseline is:

- Vue 3
- Vite
- TypeScript
- Three.js
- GLB / glTF
- 3D Tiles
- 3DTilesRendererJS
- HTTP API
- Future SignalR or polling-based state feeds

Before changing code, inspect the repository and verify the actual installed packages, versions, project structure, renderer architecture, component library, state management, routing, CSS strategy, and existing design tokens.

Never invent package APIs from memory when the current repository already establishes a pattern.

Do not upgrade libraries or change rendering engines merely to complete a UI task unless the user explicitly asks for an upgrade or migration.

## Primary Product Principle

A digital twin UI must answer, in order:

1. What is happening?
2. Where is it happening?
3. How serious is it?
4. What object or subsystem is affected?
5. What changed?
6. What should the operator inspect or do next?

Every major visual element must support at least one of these questions.

If a visual element is merely decorative and consumes attention, screen space, GPU time, or cognitive load, remove or simplify it.

## Required Reading

Before executing a substantial digital-twin UI task, read the relevant reference files in this skill:

- `references/product-visual-system.md`
- `references/scene-interaction-status.md`
- `references/vue-three-architecture.md`
- `references/acceptance-review.md`

Read all four for a new screen, new monitoring workspace, major redesign, or architecture-level UI task.

For a focused task, read only the references required by the task.

## Scope

Use this skill for:

- Digital-twin monitoring workspaces
- Warehouse, factory, campus, building, logistics, utility, and industrial 3D monitoring
- 3D object selection and inspection
- Equipment state visualization
- Alarm localization in 3D
- Scene HUD and operational overlays
- Overview, monitoring, inspection, alarm, playback, and analysis modes
- 2D panels linked with Three.js objects
- GLB / glTF model-driven UI behavior
- 3D Tiles and 3DTilesRendererJS scene integration
- Scene loading and progress UX
- Realtime state presentation
- Historical playback UI
- Camera navigation UX
- Performance-aware digital-twin UI implementation
- Visual review and anti-AI-UI cleanup

Do not use this skill as the primary workflow for:

- Marketing websites
- Public landing pages
- E-commerce storefronts
- Generic admin CRUD screens with no digital-twin or spatial context
- Backend-only work
- Blender modeling or 3D content authoring
- Shader research with no product/UI requirement

## Mandatory Workflow

Follow these steps for every substantial task.

### Step 1: Inspect Before Designing

Inspect the existing implementation before creating components.

Find and understand:

- Application shell
- Router and route structure
- Pinia or other state stores
- Existing component library
- Existing design tokens and CSS variables
- Typography
- Icons
- Dialog, drawer, tooltip, badge, table, tree, tabs, and empty-state patterns
- Three.js bootstrap and renderer ownership
- Scene lifecycle
- Camera setup
- Controls
- GLTFLoader usage
- 3DTilesRendererJS usage
- Raycasting and selection
- Resize handling
- Render loop
- Scene disposal
- API clients
- Existing DTO and domain types
- Status enums
- Alarm levels
- Existing websocket, SignalR, SSE, or polling code

Reuse before creating.

Extend before duplicating.

Consistency before novelty.

Do not introduce a second design system, second renderer lifecycle, second event bus, or parallel state model without a documented reason.

### Step 2: Classify the Workspace Mode

Classify the screen into one or more operational modes:

- `overview`: situational awareness across the whole twin
- `monitoring`: continuous state observation
- `inspect`: focused object or subsystem inspection
- `alarm`: exception localization and triage
- `playback`: historical state reconstruction
- `analysis`: comparison, trend, heatmap, spatial analysis
- `configuration`: layers, visibility, display settings, mappings

State the mode internally before designing.

Do not use the same layout and interaction priorities for every mode.

### Step 3: Identify the Operator and Primary Task

Determine the likely operator:

- Control-room operator
- Warehouse supervisor
- Equipment engineer
- Maintenance engineer
- Operations manager
- Analyst
- Administrator

Then identify:

- Primary task
- Most frequent action
- Most urgent exception
- Highest-value context
- Required reaction time

Design the screen around the primary task, not around a component gallery.

### Step 4: Build the Information Hierarchy

Use this priority order unless project requirements explicitly override it:

1. Critical alarm and safety state
2. Current operational state
3. Selected object context
4. Active tasks, flows, and bottlenecks
5. Spatial location and relationship
6. Trend and historical context
7. Aggregate metrics
8. Decorative visual treatment

Do not give aggregate KPI cards more visual weight than active faults or blocked operations.

### Step 5: Define the Scene-to-UI Contract

Before implementation, define the relationship between domain data, 3D objects, and DOM UI.

At minimum, establish:

- Domain object identifier
- Scene object identifier or mapping key
- Object type
- Parent subsystem
- Current status
- Alarm severity
- Selection state
- Visibility state
- Last update timestamp
- Optional realtime payload

Prefer a stable mapping layer such as:

```ts
export interface TwinObjectRef {
  domainId: string
  sceneKey: string
  objectType: TwinObjectType
}
```

Do not couple business logic to arbitrary mesh names scattered throughout Vue components.

When model node names are used as mapping keys, centralize and validate the mapping.

### Step 6: Design the Layer Architecture

Separate visual responsibility into layers.

Recommended conceptual layers:

1. Application shell
2. 3D canvas
3. Persistent operational HUD
4. Contextual inspection UI
5. Transient feedback
6. Modal workflows

Use DOM UI for text-heavy, form-heavy, table-heavy, accessible, and frequently changing operational information.

Use Three.js scene objects for spatial meaning, geometry, location, direction, world-space effects, and object-linked visualization.

Do not render ordinary application panels inside the 3D scene merely for visual novelty.

Do not use CSS overlays to fake a spatial effect when world-space anchoring is required.

### Step 7: Define Status Semantics Before Styling

Never assign colors ad hoc inside individual components.

Define semantic statuses centrally.

Recommended operational status model:

```ts
export type TwinOperationalStatus =
  | 'normal'
  | 'running'
  | 'idle'
  | 'waiting'
  | 'warning'
  | 'fault'
  | 'offline'
  | 'emergency'
  | 'maintenance'
  | 'unknown'
```

Recommended alarm severity:

```ts
export type AlarmSeverity =
  | 'critical'
  | 'major'
  | 'minor'
  | 'notice'
```

A status must have a consistent semantic presentation across:

- 3D object material or overlay
- Status legend
- Tree or list row
- Detail panel
- Alarm panel
- Tooltip
- Timeline

Never express fault or alarm solely by color.

Use at least two of:

- Color
- Icon
- Label
- Shape
- Pattern
- Motion

For critical states, prefer color + icon + text.

### Step 8: Create or Modify UI

Implement with Vue 3 and TypeScript patterns already used by the project.

Prefer:

- `<script setup lang="ts">`
- Typed props and emits
- Composables for reusable behavior
- Domain types separate from view state
- Explicit scene service boundaries
- Centralized status tokens
- CSS variables or project tokens
- Small components with clear responsibility

Avoid:

- Monolithic 1,000-line Vue components
- Business API calls scattered through scene traversal code
- Renderer objects stored in broad reactive proxies without need
- Deep watchers driving per-frame rendering
- Direct DOM queries when Vue refs or a service boundary are appropriate
- Duplicated status-to-color maps
- Mesh-name string literals spread across components

### Step 9: Integrate Data Without Binding UI to Transport

The UI must be transport-agnostic.

Do not design components specifically around `fetch`, polling, or SignalR invocation shapes.

Create a normalized state ingestion boundary.

Example:

```ts
export interface TwinStateUpdate {
  objectId: string
  status: TwinOperationalStatus
  timestamp: string
  metrics?: Record<string, number | string | boolean | null>
  alarms?: TwinAlarmSummary[]
}
```

A future SignalR feed and a polling endpoint should both be able to produce the same normalized update model.

Recommended flow:

```text
HTTP / polling / SignalR
        ↓
transport adapter
        ↓
normalized twin updates
        ↓
state store / twin state service
        ↓
scene state projection + Vue UI projection
```

Do not let the renderer consume raw HTTP response DTOs directly unless the DTO is explicitly the domain contract.

### Step 10: Verify the Running Interface

A UI task is not complete because TypeScript compiles.

Run the application when possible.

Verify the actual rendered interface.

Inspect at minimum:

- Initial load
- Loading state
- Scene ready state
- Empty data state
- Selected object state
- Deselected state
- Alarm state
- Offline or stale-data state
- Panel open and closed states
- Resize behavior
- At least one common desktop viewport
- The project-defined minimum supported viewport

For 3D changes also verify:

- Camera movement
- Picking
- Hover behavior
- Selection persistence
- Occlusion implications
- Label overlap
- Material restoration
- Resize and devicePixelRatio behavior
- Scene disposal after route leave or remount

Do not announce completion without visual and interaction review when the environment allows the page to run.

## Digital Twin Workspace Layout Rules

### Scene First, Not Scene Only

The 3D scene is the primary spatial workspace, but an operator still needs readable and stable operational UI.

Use the scene for:

- Spatial orientation
- Location
- Relationship
- Equipment identity
- Flow direction
- Path and route
- Coverage
- Zone
- Density
- Heat or distribution

Use DOM UI for:

- Alarm detail
- Long text
- Attributes
- Tables
- Search
- Filters
- Forms
- Confirmation
- History
- Trends
- Task queues

### Recommended Desktop Shell

Use this only as a baseline, not a rigid template:

```text
┌──────────────────────────────────────────────────────────────┐
│ Product / Site / System State / Data Freshness / User        │
├──────┬───────────────────────────────────────────────┬───────┤
│ Tool │                                               │Context│
│ Rail │              3D TWIN SCENE                    │Panel  │
│      │                                               │       │
│      │                                               │       │
│      │                                               │       │
├──────┴───────────────────────────────────────────────┴───────┤
│ Optional Alarm / Task / Timeline / Playback Tray             │
└──────────────────────────────────────────────────────────────┘
```

Not every screen needs every region.

Panels must appear because the current task requires them.

Do not keep empty chrome visible around the scene.

### Top Status Bar

The top bar may contain:

- Site or twin identity
- Current workspace mode
- System health
- Data freshness
- Connection state
- Current time or playback time
- Global search
- User and environment context

Do not fill it with decorative navigation items unrelated to monitoring.

### Left Tool Rail

Use for stable scene tools such as:

- Home view
- Search
- Layer visibility
- Scene hierarchy
- Measure
- Isolate
- Section or explode controls when supported
- View presets
- Display settings

Keep icons understandable.

Use tooltips.

Do not hide frequently used operational actions behind a generic kebab menu.

### Right Context Panel

The right panel is contextual.

Its default content should depend on mode:

- Overview: system summary or exception summary
- Monitoring: active state and task context
- Inspect: selected object details
- Alarm: alarm triage detail
- Playback: playback parameters and event context
- Analysis: filters and analysis results

Do not show selected-object detail when nothing is selected.

Use a useful empty state such as “Select an object in the scene or hierarchy to inspect it.”

### Bottom Tray

Use an expandable bottom tray for information with strong temporal or queue semantics:

- Active alarms
- Task queue
- Event stream
- Timeline
- Playback scrubber
- Flow trace

Do not permanently reserve 35% of the screen for an empty table.

## Anti-AI UI Rules

Do not default to stereotypical AI-generated dashboards.

Avoid:

- Four large KPI cards at the top of every page
- Purple-blue gradient backgrounds
- Neon cyan borders on every panel
- Excessive glowing effects
- Excessive glassmorphism
- Large blurred blobs
- Cards nested inside cards
- Every section being a rounded card
- Random gradient icons
- Huge marketing-style page titles
- Generic “Welcome back” hero areas
- Decorative charts with no operator decision value
- Excessive pill-shaped controls
- Constant pulsing animations
- Sci-fi movie UI decoration
- Tiny low-contrast techno fonts
- Fake radar lines, scanning beams, and grids that convey no data
- 100% transparent panels that destroy readability over complex 3D geometry

A digital twin is a working operational product.

Professional does not mean visually empty.

Industrial does not mean neon sci-fi.

Advanced does not mean adding more effects.

## Transparency and Overlay Rules

Transparency may be used to preserve spatial context, but text readability has priority.

When a panel overlays the 3D scene:

- Ensure text remains readable over both dark and bright model areas
- Prefer controlled surface opacity over full transparency
- Use a subtle backdrop or surface layer when the background is visually complex
- Avoid strong blur if it harms GPU performance or text sharpness
- Do not make every panel translucent
- Keep dense tables and forms on more opaque surfaces

Scene visibility is not improved if the operator can no longer read the UI.

## Typography Rules

Prioritize rapid scanning.

Use the project typeface when defined.

Use tabular numerals for rapidly changing numeric metrics when supported.

Keep hierarchy compact:

- Page or twin title
- Section title
- Object name
- Primary metric
- Secondary value
- Metadata

Do not use all-caps text for long Chinese or English labels.

Do not reduce operational metadata to unreadably small sizes to make the interface look “high-tech.”

## Density Rules

Digital-twin monitoring is information-dense.

Use compact but breathable spacing.

Prefer:

- Clear alignment
- Strong grouping
- Consistent row height
- Compact labels
- Predictable panel rhythm

Avoid both extremes:

- Consumer-app oversized spacing
- Dense legacy SCADA clutter with no hierarchy

## 3D Selection Rules

Selection must be unambiguous.

When an object is selected:

1. Preserve the underlying operational status.
2. Add a selection treatment that is visually separate from status.
3. Open or update the contextual inspector.
4. Synchronize the hierarchy/list selection.
5. Preserve selection until explicit deselection, object removal, or mode transition requires clearing it.

Do not replace a red fault state with a blue selection material and thereby hide the fault.

Selection and operational status are different visual channels.

Prefer techniques such as:

- Outline
- Edge highlight
- Halo
- Bounding accent
- Controlled emissive addition

Do not permanently mutate source materials without a restoration strategy.

## Hover Rules

Hover is preview, not commitment.

Hover may show:

- Object name
- Object type
- Short status
- One or two primary metrics

Do not open a full detail panel on simple hover.

Hover must not overpower selected state or alarm state.

Throttle or avoid expensive raycasting when pointer or camera state has not changed.

## Alarm Localization Rules

An alarm workflow must connect list and space.

When an alarm is focused:

- Select or focus the affected object when it exists in the scene
- Make the alarm severity explicit
- Show location context
- Preserve critical status while highlighting focus
- Provide a clear route back to the alarm list
- Show timestamp and data freshness

For clustered alarms, avoid flying the camera on every automatic update.

Automatic camera motion must never continuously fight operator control.

## Camera UX Rules

Provide stable navigation.

Recommended actions:

- Home or fit-twin
- Fit selected object
- Focus alarm
- View presets where domain value exists
- Reset camera

Avoid:

- Unrequested cinematic camera flights
- Long easing animations during urgent workflows
- Camera jumps on every realtime event
- Changing the camera target without visible reason

Respect operator orientation.

## Loading UX Rules

Distinguish application load from scene content load.

Possible states:

1. Application shell ready
2. Twin metadata loading
3. Base scene loading
4. GLB/glTF model loading
5. 3D Tiles streaming
6. Operational state loading
7. Ready
8. Partial failure

Do not show an indefinite full-screen spinner for the entire digital twin if the application can progressively become usable.

Prefer:

- App shell appears first
- Scene loading progress or state is visible
- Side UI can show metadata when available
- Partial content failures identify the failed layer
- Retry is scoped to the failed resource when possible

A 3D Tiles scene may continue streaming after the first useful view is available. Do not equate “all possible tiles fetched” with “UI ready.”

## Stale Data and Connection State

Realtime-looking UI must explicitly communicate data freshness.

Represent at least:

- Connected or healthy
- Reconnecting
- Degraded
- Disconnected
- Stale data

Show last successful update time when data may become stale.

Do not keep animating equipment as “running” indefinitely after the data feed becomes stale.

Define a stale-data policy in the state layer.

The visual design must make transport degradation visible without turning every network fluctuation into a critical operational alarm.

## Motion Rules

Use motion to explain state change, flow, direction, focus, or transition.

Good motion:

- Flow direction
- Active path
- Temporary focus cue
- Panel transition
- Timeline playback
- Controlled alarm attention cue

Bad motion:

- Every icon pulsing
- Constant breathing glow
- Background particles with no data meaning
- Continuous scanner beams
- Large looping gradients

Critical alarm attention effects must be controlled and must not cause the entire scene to flash.

Respect reduced-motion preferences for DOM UI where practical.

## Performance Rules

Performance is part of UI quality.

Do not trade operator responsiveness for visual effects.

Review:

- Render-loop ownership
- Unnecessary continuous rendering
- Pixel ratio
- Resize handling
- Raycasting frequency
- Reactive update frequency
- Object traversal frequency
- Material cloning
- Texture memory
- Scene disposal
- Tile cache behavior
- Large label counts
- DOM overlay count
- Repeated allocations inside animation loops

Do not run Vue reactive logic at 60 FPS unless the displayed DOM value truly requires frame-rate updates.

Coalesce high-frequency state updates before projecting them into Vue components.

Apply 3D updates directly through a controlled scene projection layer when appropriate.

Never traverse the entire scene graph for every equipment state update.

Maintain an object registry keyed by stable identifiers.

Example:

```ts
const objectRegistry = new Map<string, THREE.Object3D>()
```

Use project-appropriate cleanup and disposal rules.

## Responsive Strategy

This product is desktop-first unless the repository requirements state otherwise.

Do not mechanically collapse the entire monitoring interface into one vertical mobile column.

Define viewport classes based on actual operational support.

Typical strategy:

- Large control-room / 1920+: expanded operational workspace
- Standard desktop / 1440: default working layout
- Compact desktop / 1366: condensed panels and reduced nonessential labels
- Tablet: optional read-only or limited inspect mode when supported
- Mobile: separate limited-use experience if required

If the product minimum width is desktop-only, preserve usability and state the minimum width in project documentation rather than creating a broken pseudo-mobile layout.

## Accessibility and Safety

Even in a graphics-heavy interface:

- Keep keyboard focus visible for DOM controls
- Give icon-only controls accessible names
- Use tooltips for ambiguous tools
- Do not use color as the sole status indicator
- Keep critical labels readable
- Provide text equivalents for important scene states in lists or panels
- Avoid high-frequency flashing
- Preserve logical tab order in persistent UI

The 3D scene may not be fully keyboard-operable, but critical operational state must also exist in accessible DOM UI when practical.

## Completion Output

When finishing a substantial UI task, report:

1. What workspace mode was designed or changed
2. The primary operator task supported
3. The key UI hierarchy decision
4. The scene/UI interaction added or changed
5. Status and alarm semantics affected
6. Performance-sensitive changes
7. Runtime states verified
8. Remaining limitations or assumptions

Do not claim visual verification when the page was not actually run.

## Non-Negotiable Rules

- The 3D scene is an operational workspace, not decoration.
- Exceptions outrank aggregate metrics.
- Status semantics are centralized.
- Selection must not hide alarm or fault state.
- UI is transport-agnostic: HTTP, polling, and SignalR feed normalized state.
- Domain identifiers must map to 3D objects through a controlled mapping layer.
- Do not scatter mesh-name business logic through Vue components.
- Do not traverse the whole scene for every state update.
- Do not bind Vue reactivity directly to the render loop without necessity.
- Do not create generic AI dashboards.
- Do not add sci-fi effects without operational meaning.
- Do not consider a UI task complete solely because it compiles.
- Run and visually inspect the interface when the environment permits.
