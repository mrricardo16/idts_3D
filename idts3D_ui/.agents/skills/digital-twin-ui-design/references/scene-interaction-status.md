# Scene Interaction and Status Reference

## 1. Scene Interaction Model

Treat scene interaction as an explicit state machine.

Recommended states:

```ts
export type SceneInteractionMode =
  | 'navigate'
  | 'select'
  | 'measure'
  | 'isolate'
  | 'inspect'
  | 'playback'
```

Do not let multiple pointer tools silently compete for the same click.

The active interaction mode must be visible when it changes normal pointer behavior.

## 2. Object Identity

Every selectable operational object needs a stable domain identity.

Recommended metadata:

```ts
export interface TwinObjectMeta {
  domainId: string
  sceneKey: string
  objectType: string
  displayName: string
  parentId?: string
  tags?: string[]
}
```

Possible scene mapping sources:

- `Object3D.userData`
- glTF node extras mapped into `userData`
- Central manifest
- Backend twin-model mapping
- 3D Tiles metadata

Do not rely on `object.uuid` as a persistent business identifier across model reloads.

## 3. Object Registry

Build a registry after model or tile content becomes available.

Example conceptual API:

```ts
export interface TwinSceneRegistry {
  register(meta: TwinObjectMeta, object: THREE.Object3D): void
  unregister(sceneKey: string): void
  getByDomainId(domainId: string): THREE.Object3D | undefined
  getMetaByObject(object: THREE.Object3D): TwinObjectMeta | undefined
}
```

The registry should support scene state projection without full-scene traversal.

For hierarchical imported models, resolve selection to the nearest registered twin object rather than treating every mesh child as a separate business object.

## 4. Picking

Use raycasting or the project selection mechanism intentionally.

Picking rules:

- Ignore non-interactive helper objects
- Ignore hidden layers
- Resolve child meshes to operational owner objects
- Preserve pointer intent between click and drag
- Avoid triggering selection after camera orbit drag
- Avoid full-detail work on pointermove

Recommended flow:

```text
pointer input
↓
interaction mode check
↓
pick candidates
↓
resolve scene object to twin object
↓
update selection state
↓
scene projection
↓
Vue inspector projection
```

## 5. Hover

Hover should be lightweight.

Use a delayed or stable hover threshold if complex geometry produces rapid object changes.

Do not update a Vue store on every raw pointermove event.

Possible optimization:

- Track pointer change
- Track camera change
- Raycast only when needed
- Reuse raycaster and vectors
- Update tooltip only when hovered twin identity changes

## 6. Selection

Recommended selection state:

```ts
export interface TwinSelectionState {
  selectedId: string | null
  source: 'scene' | 'tree' | 'alarm' | 'search' | 'route'
}
```

Selection synchronization rules:

- Scene selection updates hierarchy
- Hierarchy selection focuses scene when appropriate
- Alarm focus selects the affected object
- Search result selection locates the object
- Route changes restore or clear selection according to product rules

Avoid circular event loops between scene and Vue components.

Use one canonical selection state.

## 7. Selection Visual Channels

Never overwrite status semantics.

Recommended precedence model:

```text
Base visual identity
+ operational status
+ alarm severity
+ selection overlay
+ temporary focus cue
```

Selection is additive.

Possible approaches:

- Outline pass
- Edge geometry
- Secondary shell mesh
- Bounding box accent
- Controlled emissive contribution

When cloning materials, track and dispose clones.

When temporarily mutating materials, preserve original state.

## 8. Operational Status Projection

Maintain a dedicated projector.

Example conceptual interface:

```ts
export interface SceneStatusProjector {
  applyStatus(objectId: string, status: TwinOperationalStatus): void
  applyAlarm(objectId: string, severity: AlarmSeverity | null): void
  clear(objectId: string): void
}
```

Do not write status material logic directly in API callbacks.

Status projection must be idempotent.

Repeatedly applying the same state should not create new materials, new helpers, or duplicate effects.

## 9. Scene State Precedence

Define visual precedence explicitly.

Recommended logical precedence:

```text
emergency / critical alarm
fault / major alarm
warning / minor alarm
offline or stale
maintenance
running / idle / waiting / normal
unknown
```

Selection does not replace this precedence.

Filtering may dim nonmatching objects, but critical alarms should remain discoverable unless the user explicitly hides the alarm layer.

## 10. Stale State

A stale-data state is not the same as offline equipment.

Possible state dimensions:

```ts
interface TwinFreshnessState {
  lastUpdatedAt: string | null
  isStale: boolean
  connectionState: 'healthy' | 'reconnecting' | 'degraded' | 'disconnected'
}
```

When data becomes stale:

- Stop representing state as confidently live
- Mark freshness in DOM UI
- Consider a scene-level stale treatment
- Do not convert every object to `fault`

## 11. Alarm Focus

Focusing an alarm may:

1. Open alarm context
2. Select affected object
3. Ensure affected layer is visible
4. Fit or focus camera
5. Show location breadcrumb

Do not automatically isolate the object unless isolation supports the workflow.

Do not repeatedly refocus camera on live alarm updates.

## 12. World-Space Labels

Use labels only when they improve spatial understanding.

Good use cases:

- Key equipment names
- Active fault labels
- Zone identifiers
- Selected object annotation
- Critical process value near the source

Avoid labels for every object in a dense scene.

Label management should consider:

- Distance
- Camera angle
- Priority
- Occlusion
- Screen overlap
- Current mode
- Selected state
- Alarm severity

Use priority-based label visibility.

## 13. DOM Overlay Labels

DOM overlays are useful for crisp text but can become expensive in large numbers.

Rules:

- Limit active count
- Do not create one Vue component per hidden object
- Batch position updates when possible
- Avoid updating layout when the camera is static
- Hide low-priority labels

Do not use thousands of persistent DOM labels over a 3D Tiles scene.

## 14. 3D Markers

Markers may represent:

- Alarm location
- Sensor location
- Task destination
- Waypoint
- Incident

A marker should encode a domain meaning.

Do not scatter glowing circles simply to make the scene look technical.

## 15. Paths and Flows

For conveyors, AGV routes, process flows, or logistics paths:

- Use direction when direction matters
- Distinguish planned and active paths
- Distinguish blocked path from inactive path
- Keep animation speed meaningful and restrained
- Limit simultaneous animated flows

Do not animate every route continuously.

## 16. Scene Modes

### Overview

Prioritize:

- Whole-twin orientation
- Exception count
- Major operational zones
- Alarm locations
- System health

Avoid dense technical detail.

### Monitoring

Prioritize:

- Current statuses
- Active flows
- Queue or task state
- Bottlenecks
- Data freshness

### Inspect

Prioritize:

- Selected object
- Current metrics
- Alarm context
- Current task
- Relationships
- Recent changes

### Alarm

Prioritize:

- Severity
- Affected object
- Location
- Duration
- Related failures
- Handling state

### Playback

Prioritize:

- Playback time
- Current historical state
- Event markers
- Speed
- Pause/play
- Return to live

Always make it obvious when the user is not viewing live data.

### Analysis

Prioritize:

- Scope
- Filters
- Layer or metric selection
- Spatial comparison
- Legend
- Result interpretation

## 17. Camera Control

Define camera ownership.

Potential sources of camera requests:

- User navigation
- Search
- Alarm focus
- Tree selection
- Home view
- Playback event

Centralize programmatic camera commands.

Example conceptual API:

```ts
export interface TwinCameraController {
  fitTwin(): void
  fitObject(objectId: string): void
  focusAlarm(alarmId: string): void
  setPreset(presetId: string): void
  cancelTransition(): void
}
```

User input should be able to interrupt noncritical camera transitions.

## 18. Camera Animation

Use short controlled transitions when transitions improve orientation.

Avoid:

- Multi-second cinematic flights
- Orbiting around the object automatically
- Camera shake
- Continuous auto-tour during monitoring

For urgent alarm focus, speed and clarity are more important than cinematic motion.

## 19. Model Layer Visibility

Layer visibility may include:

- Building shell
- Structure
- Equipment
- Conveyor
- Rack
- AGV
- Sensors
- Zones
- Routes
- Labels
- Alarm markers

Use semantic layer names from the domain.

Do not expose raw internal mesh group names directly to operators unless the product is an engineering model browser.

## 20. Isolation

Object isolation should:

- Preserve context option when useful
- Clearly indicate isolation mode
- Provide one-step exit
- Avoid losing selected identity
- Preserve alarm information

A common pattern is dimming nonselected context rather than fully hiding everything.

## 21. Explode and Section Views

Only introduce explode or section controls when the model and workflow support them.

Do not add generic “explode” functionality merely because it is common in 3D demos.

The UI must explain the current nonstandard view state.

## 22. glTF / GLB Handling UX

For GLB or glTF assets:

- Track load state
- Handle load errors
- Register operational nodes after load
- Validate expected mapping keys
- Avoid silently ignoring missing mapped objects
- Dispose scene resources according to project lifecycle

When an expected node is missing, surface a developer-visible diagnostic rather than failing silently.

Do not expose internal asset filenames as user-facing labels unless they are meaningful.

## 23. 3D Tiles UX

3D Tiles content is progressive.

UI rules:

- Distinguish initial useful scene readiness from ongoing tile streaming
- Avoid blocking the entire app while higher-detail tiles continue loading
- Keep a layer-level error state
- Avoid attaching per-tile Vue components
- Treat loaded content lifecycle as dynamic

When using 3DTilesRendererJS, account for model load and disposal lifecycle when applying custom scene state or materials.

If scene state must apply to dynamically loaded tile content, make the projection repeatable for newly loaded content.

## 24. Playback

Historical playback requires an explicit live/historical state model.

Example:

```ts
export type TwinTimeMode = 'live' | 'playback'
```

The interface must clearly show:

- Live or playback mode
- Current playback timestamp
- Playback speed
- Pause/play state
- Data range
- Return to live action

Do not let live updates visually overwrite the historical scene while playback is active unless the architecture explicitly supports a split comparison mode.

## 25. 2D / 3D Linkage

Every major spatial list should consider whether it needs scene linkage.

Examples:

Alarm row → focus object
Task row → highlight route or destination
Equipment tree → select object
Search result → locate object
Zone list → fit zone
Metric anomaly → focus affected subsystem

Do not add linkage merely for novelty.

Use linkage when spatial location improves understanding or action.
