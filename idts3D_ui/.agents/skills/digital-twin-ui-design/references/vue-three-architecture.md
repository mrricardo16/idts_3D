# Vue and Three.js Architecture Reference

## 1. Architectural Goal

Keep Vue responsible for application UI and application state orchestration.

Keep Three.js responsible for scene rendering and spatial interaction.

Create explicit boundaries between:

- Transport
- Domain state
- Vue UI
- Scene projection
- Rendering lifecycle

Do not make a Vue component simultaneously own HTTP transport, realtime connection, business rules, renderer initialization, model loading, raycasting, alarm material mutation, and detail-panel rendering.

## 2. Recommended Layer Model

```text
src/
├─ api/ or transport/
│  ├─ twin-http.ts
│  ├─ twin-polling.ts
│  └─ twin-signalr.ts
├─ domain/
│  ├─ twin-types.ts
│  ├─ alarm-types.ts
│  └─ status-types.ts
├─ stores/
│  ├─ twin-state.ts
│  ├─ twin-selection.ts
│  └─ twin-ui.ts
├─ scene/
│  ├─ TwinScene.ts
│  ├─ TwinSceneRegistry.ts
│  ├─ TwinCameraController.ts
│  ├─ TwinSelectionController.ts
│  ├─ TwinStatusProjector.ts
│  ├─ loaders/
│  ├─ tiles/
│  └─ effects/
├─ composables/
│  ├─ useTwinSelection.ts
│  ├─ useTwinConnection.ts
│  └─ useTwinWorkspace.ts
├─ components/
│  └─ digital-twin/
└─ views/
   └─ TwinWorkspaceView.vue
```

Adapt to the repository instead of forcing this exact tree.

The important rule is separation of responsibility.

## 3. Scene Ownership

There should be a clear owner for:

- `THREE.Scene`
- Camera
- WebGLRenderer
- Controls
- Animation loop
- Resize lifecycle
- GLB/glTF loaders
- 3D Tiles renderer instances

Avoid initializing separate Three.js renderers in nested feature components unless multi-canvas architecture is intentional.

## 4. Vue Component Boundary

A recommended Vue scene host component may:

- Own the canvas element ref
- Create the scene service on mount
- Pass configuration
- Subscribe to high-level scene events
- Resize through the scene service
- Dispose on unmount

Example shape:

```vue
<script setup lang="ts">
import { onBeforeUnmount, onMounted, ref } from 'vue'
import { TwinScene } from '@/scene/TwinScene'

const canvasRef = ref<HTMLCanvasElement | null>(null)
let twinScene: TwinScene | null = null

onMounted(async () => {
  if (!canvasRef.value) return

  twinScene = new TwinScene(canvasRef.value)
  await twinScene.initialize()
})

onBeforeUnmount(() => {
  twinScene?.dispose()
  twinScene = null
})
</script>

<template>
  <canvas ref="canvasRef" class="twin-canvas" />
</template>
```

This is an architectural example, not a mandatory class name.

## 5. Avoid Deep Vue Reactivity for Three Objects

Three.js objects are mutable runtime graphics objects.

Do not automatically place entire `Scene`, `Object3D`, `Material`, `Texture`, renderer, or controls graphs into deeply reactive state.

Prefer:

- Plain class fields
- Module/service ownership
- `shallowRef` when Vue needs a reference
- Stable IDs in stores rather than entire Three.js object graphs

Store:

```ts
selectedObjectId: string | null
```

instead of:

```ts
selectedObject: THREE.Object3D
```

unless there is a specific justified need.

## 6. Domain State Versus Scene State

Domain state example:

```ts
export interface TwinObjectState {
  objectId: string
  status: TwinOperationalStatus
  lastUpdatedAt: string
  metrics: Record<string, TwinMetricValue>
}
```

Scene state is a projection of domain state.

Do not let a material color become the source of truth for equipment status.

The source of truth is domain state.

## 7. Transport Adapter Boundary

Normalize data from transport.

Example interface:

```ts
export interface TwinStateSource {
  start(onUpdate: (update: TwinStateUpdate) => void): Promise<void>
  stop(): Promise<void>
}
```

Possible implementations:

```text
HttpSnapshotStateSource
PollingStateSource
SignalRStateSource
```

The store and scene projection should not care which source is active.

## 8. Initial Snapshot and Incremental Updates

Design for both:

- Initial full snapshot
- Incremental updates

Recommended flow:

```text
load twin definition
↓
load scene
↓
load initial state snapshot
↓
project snapshot
↓
start incremental feed
```

Handle ordering and race conditions.

An incremental update may arrive before a scene object is loaded.

Keep the latest domain state and apply it when the object becomes available.

Do not discard updates solely because a GLB node or 3D tile is not currently loaded.

## 9. State Batching

High-frequency feeds should be coalesced.

Possible strategy:

```text
incoming updates
↓
normalize
↓
merge latest value by object id
↓
flush on controlled interval / animation frame
↓
update store summaries
↓
project scene changes
```

Do not trigger one complete Vue tree update per telemetry event when update volume is high.

Do not force 60 Hz DOM updates for metrics operators cannot visually read at 60 Hz.

## 10. Scene Projection

Scene projection is the bridge from domain state to graphics.

Recommended responsibilities:

- Locate object from registry
- Determine semantic visual state
- Apply material or overlay changes
- Apply animation state
- Update labels or markers
- Preserve selection and alarm precedence

Keep projectors idempotent.

Avoid allocation inside hot update paths.

## 11. Material Strategy

Define material behavior by object category.

Possible categories:

- Imported PBR equipment
- Structural context
- Dynamic route
- Status overlay
- Selection outline
- Alarm marker

Do not globally replace all imported glTF materials with `MeshBasicMaterial` just to achieve a status color.

Preserve model readability and physically based material information unless the product intentionally uses schematic mode.

Possible status techniques:

- Emissive adjustment where material supports it
- Overlay shell
- Outline
- Vertex/material variant mapping
- Marker or label
- Shader uniform

Choose based on model architecture and performance.

## 12. Material Cloning

Cloning is not free.

Do not clone every material every time status changes.

If clone-per-object is necessary:

- Clone once
- Cache by object or state strategy
- Restore deliberately
- Dispose deliberately

Track shared materials before mutation.

Changing a shared material may unintentionally recolor multiple objects.

## 13. GLTFLoader Lifecycle

Centralize loader setup when possible.

When compression is used, configure the required loader integrations according to the installed Three.js version and current project architecture.

Loader concerns may include:

- Draco
- KTX2
- Meshopt
- Loading manager
- Progress
- Error handling

Reuse loader or decoder instances when the project architecture supports it.

Do not instantiate decoder infrastructure for every component mount without reason.

## 14. GLB / glTF Registration

After load:

1. Validate model root
2. Traverse once for registration if necessary
3. Resolve mapping keys
4. Record supported twin objects
5. Apply any pending latest domain states
6. Attach interaction metadata

Do not traverse the whole model again for every API update.

## 15. Scene Disposal

When a scene or asset is removed, consider:

- Geometry disposal
- Material disposal
- Texture disposal
- Render target disposal
- Controls disposal
- Event listener removal
- Resize observer removal
- Animation frame cancellation
- 3D Tiles renderer cleanup
- Registry cleanup
- DOM overlay cleanup

Do not dispose shared assets twice.

Track ownership.

## 16. 3DTilesRendererJS Integration

Keep tile renderer ownership in the scene or tile service layer.

The render lifecycle must respect the renderer integration pattern used by the installed library.

For status projection onto dynamic tile content:

- Listen to appropriate tile/model lifecycle events supported by the installed version
- Register or inspect loaded content
- Apply the latest semantic state
- Remove custom resources on disposal

Do not assume every tile content object remains permanently loaded.

Do not keep stale object references after tile content is disposed.

If multiple tilesets exist, review whether shared cache or queues are appropriate for the installed version and project workload.

## 17. Render Loop

There must be one clearly defined render loop per renderer.

Typical responsibilities:

```text
update controls if needed
update camera matrices if needed
update tile renderer if needed
apply frame-based scene systems
render scene
schedule next frame
```

Do not run unrelated application business logic inside the animation loop.

## 18. Render-on-Demand

Consider render-on-demand when the scene is mostly static and project behavior allows it.

Continuous rendering may still be necessary for:

- Active equipment animation
- Flow animation
- Camera damping
- Timeline playback
- Shader animation
- High-frequency visual updates

Choose based on actual needs.

Do not assume all digital-twin scenes require permanent maximum-rate rendering.

## 19. Frame Budget

Treat frame responsiveness as a UI requirement.

Watch for:

- Large scene traversals
- Material recreation
- Vue state churn
- Label layout
- Excessive raycasts
- Large transparent surfaces
- Expensive postprocessing
- Excessive shadows
- High device pixel ratio

When debugging UI lag, separate:

- JavaScript main-thread work
- Vue update work
- GPU/render work
- Network/tile streaming

## 20. Resize

Use one resize strategy.

On resize:

- Determine canvas display size
- Update camera aspect or projection parameters
- Update renderer size
- Update relevant tile renderer resolution state
- Reposition DOM overlays as needed

Avoid resize loops caused by canvas sizing itself changing layout repeatedly.

## 21. Device Pixel Ratio

Do not blindly use the maximum device pixel ratio.

A digital-twin workspace on a high-DPI large monitor can become GPU-expensive.

Use a project-defined cap when appropriate.

Performance and text clarity both matter.

## 22. Raycasting Performance

Avoid:

- Raycasting every frame when pointer is still
- Raycasting against helper objects
- Raycasting huge object sets without filtering

Consider:

- Layers
- Candidate roots
- BVH acceleration when project requirements justify it
- Change-driven picking
- Object registry ownership resolution

Do not add a heavy dependency without measuring the need.

## 23. Events

Scene events should be typed.

Example:

```ts
export interface TwinSceneEvents {
  selected: { objectId: string | null }
  hovered: { objectId: string | null }
  ready: undefined
  loadError: { layerId: string; error: unknown }
}
```

Avoid a global string event bus with undocumented payloads.

## 24. Store Boundaries

Possible stores:

### Twin definition store

- Sites
- Zones
- Object metadata
- Layer configuration

### Twin live state store

- Current object statuses
- Metrics
- Freshness
- Connection state

### Selection store

- Selected object
- Focus source

### UI store

- Panels
- Workspace mode
- Filters
- Active tool

Do not put raw Three.js scene graphs in a broad Pinia store.

## 25. Vue Watchers

Watchers are for explicit reactive side effects.

Do not use broad deep watchers over large object-state maps to update the entire scene.

Prefer targeted update events or batched changed-ID sets.

Example conceptual pattern:

```ts
const changedObjectIds = new Set<string>()
```

Flush changed IDs to the scene projector.

## 26. Component Design

Recommended component types:

- `TwinViewport`
- `TwinTopStatusBar`
- `TwinToolRail`
- `TwinObjectInspector`
- `TwinAlarmTray`
- `TwinLayerPanel`
- `TwinSceneLegend`
- `TwinConnectionStatus`
- `TwinPlaybackBar`
- `TwinSearch`

Names must follow project naming conventions.

Do not create components merely to reduce line count.

Create components around responsibility and reuse.

## 27. Error Handling

Distinguish:

- Fatal application error
- Scene initialization error
- Model layer error
- Tileset error
- State API error
- Realtime feed degradation
- Object mapping error

Do not replace the entire workspace with a generic 500 page when one optional model layer fails.

## 28. Logging and Diagnostics

Development diagnostics should identify:

- Missing scene mappings
- Duplicate scene keys
- Unknown object statuses
- Failed model layers
- Tileset failures
- Stale feed transitions
- Material projection failures

Avoid noisy console logging for every telemetry update.

## 29. TypeScript Rules

Prefer explicit domain unions and interfaces.

Avoid `any` in scene/domain boundaries.

Use type guards when normalizing external API data.

Do not trust arbitrary status strings from the backend.

Normalize them to known semantic states.

Example:

```ts
function normalizeOperationalStatus(value: string): TwinOperationalStatus {
  // explicit mapping and fallback to 'unknown'
}
```

## 30. Future SignalR Readiness

Prepare for realtime transport without prematurely coupling the UI to SignalR.

The UI should already understand:

- Snapshot state
- Incremental update
- Connection state
- Reconnect state
- Stale state
- Latest update timestamp

When SignalR is introduced, it should replace or augment a transport adapter, not require rewriting every scene component.

## 31. Polling Readiness

Polling must not create overlapping requests.

The transport layer should define:

- Interval
- Timeout
- Cancellation
- Backoff or error policy
- Visibility/background behavior when needed
- Last successful update

The UI consumes normalized connection and freshness state.

## 32. Security Boundary

Do not expose secrets, tokens, or private API configuration in UI code.

Do not assume 3D asset URLs are public.

Follow the repository's authorization and API client patterns.

## 33. Architectural Rejection Criteria

Reject or refactor an implementation when:

- API callbacks directly recolor meshes throughout the codebase
- Mesh names are business logic in multiple Vue components
- The entire Three.js scene is placed into deep Vue reactivity
- One component owns renderer, networking, alarms, and all panels
- Every telemetry event triggers a full-scene traversal
- Tile content disposal leaves stale registry references
- Selection state exists separately in scene, tree, and alarm panel
- SignalR types leak directly into UI components
- Material clones grow on every status update
- Route leave leaves animation loops active
