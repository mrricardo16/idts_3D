# Digital Twin UI Acceptance and Review Reference

## 1. Purpose

Use this checklist before declaring a substantial digital-twin UI task complete.

Review both product quality and implementation quality.

Classify findings:

- P0: critical correctness, safety, severe data-state, or severe performance issue
- P1: major operational usability or architecture issue
- P2: important quality improvement
- P3: polish or optional enhancement

Fix P0 and P1 before completion when the task scope permits.

## 2. Product Review

### Operator goal

- Is the primary operator identifiable?
- Is the primary task obvious?
- Is the most urgent exception more visible than aggregate metrics?
- Can the operator understand what is happening within a few seconds?
- Can the operator locate where it is happening?

### Workspace mode

- Is the current mode clear?
- Does the layout match overview, monitoring, inspect, alarm, playback, or analysis needs?
- Is live versus playback state obvious?

### Spatial value

- Does 3D improve understanding?
- Are scene interactions linked to useful operational context?
- Is the scene more than decorative background?

## 3. Visual Hierarchy Review

Check attention order.

The eye should generally find:

1. Critical state
2. Current system state
3. Selected or focused object
4. Active work and exceptions
5. Supporting metrics

Flag as P1 when decorative visuals or KPI cards dominate active faults.

## 4. Anti-AI UI Review

Reject or revise when the interface contains multiple of these without product justification:

- Four-card KPI row
- Purple-blue gradient
- Neon cyan frame around every panel
- Multiple blurred glow blobs
- Excessive glassmorphism
- Rounded card inside rounded card
- Generic startup dashboard composition
- Decorative donut chart
- Random gradient icon tiles
- Large “Welcome back” heading
- Sci-fi scan lines
- Constant pulse animations
- Fake radar grid

The finished UI should look specifically designed for the digital-twin domain.

## 5. Scene Readability Review

Verify:

- Scene background supports object readability
- Selected object is obvious
- Fault remains visible while selected
- Alarm markers are not hidden by ordinary selection
- Context geometry does not overwhelm operational objects
- Status colors are consistent
- Labels do not cover critical scene regions
- Transparency does not make panels unreadable

## 6. Status Review

Verify each supported status:

- normal
- running
- idle
- waiting
- warning
- fault
- offline
- emergency
- maintenance
- unknown

Questions:

- Is semantic mapping centralized?
- Does the same status look consistent in scene and DOM?
- Is fault distinguishable from offline?
- Is unknown distinguishable from normal?
- Is color accompanied by another cue where required?

## 7. Alarm Review

Verify:

- Critical alarm is immediately identifiable
- Severity levels are distinguishable
- Alarm row links to affected object when spatial linkage exists
- Timestamp is visible
- Duration or current state is understandable
- Location context is available
- Selected state does not hide severity
- Alarm focus does not continuously steal camera control

## 8. Selection Review

Test selection from:

- Scene
- Hierarchy
- Search
- Alarm

Verify:

- One canonical selection state
- Inspector synchronization
- Clear deselection
- Camera behavior is predictable
- Selection persists correctly
- Child mesh resolves to business object
- Status visual is preserved

## 9. Hover Review

Verify:

- Hover is lighter than selection
- Tooltip is compact
- Hover does not open full inspector
- Rapid pointer movement does not cause visible UI thrash
- Raycasting does not run unnecessarily when idle

## 10. Camera Review

Test:

- Home view
- Manual navigation
- Fit selected
- Alarm focus
- Reset

Verify:

- User can interrupt noncritical transitions
- No random camera jumps
- No realtime update repeatedly changes target
- Programmatic focus preserves orientation where practical

## 11. Loading Review

Test:

- Slow metadata API
- Slow GLB/glTF
- Slow tileset
- Model load failure
- Tileset failure
- State API failure

Verify:

- App shell can appear progressively
- Useful scene readiness is distinguished from complete tile streaming
- Failed layers are identified
- Partial failures do not unnecessarily destroy the whole app
- Loading text reflects actual stage

## 12. Connection and Freshness Review

Test:

- Healthy
- Reconnecting
- Degraded
- Disconnected
- Stale

Verify:

- Last update time is available when relevant
- Stale data is not presented as confidently live
- Equipment does not keep an indefinite running animation after stale threshold if that would mislead
- Connection degradation is visible
- Network state is not confused with equipment alarm severity

## 13. Empty-State Review

Test:

- No selection
- No alarms
- No search matches
- No visible layers
- No state data

Every empty region must explain what the user can do or why the region is empty.

Avoid blank panels.

## 14. Resize Review

Test project-supported desktop sizes.

At minimum, when requirements are unknown:

- 1920 × 1080
- 1440 × 900
- 1366 × 768

Verify:

- Canvas fits correctly
- Camera projection updates
- Tile renderer resolution state updates when applicable
- Panels do not cover all spatial content
- Text does not clip
- Bottom tray remains operable
- Inspector can scroll
- Tooltips stay within viewport where practical

## 15. Long-Session Usability Review

Ask:

- Is contrast comfortable for hours of use?
- Is there unnecessary animation?
- Are important values easy to scan repeatedly?
- Is the interface visually exhausting?
- Are alarm states too aggressive when no alarm exists?

A control-room interface should not look permanently alarmed.

## 16. Vue Review

Verify:

- Components have clear responsibility
- Props and emits are typed
- No unnecessary `any`
- No giant deep reactive Three.js object graph
- No duplicated status maps
- No broad deep watchers over high-volume state
- UI components do not know SignalR transport details
- Scene lifecycle is cleaned up on unmount

## 17. Three.js Review

Verify:

- One clear renderer owner
- One clear render loop owner
- Resize lifecycle is correct
- Raycasting is scoped
- Object registry exists for frequent state updates
- Full scene traversal is not used per update
- Material strategy preserves original model semantics
- Shared materials are handled safely
- Cloned materials are tracked
- Removed assets are disposed correctly
- Animation frame is canceled on teardown

## 18. GLB / glTF Review

Verify:

- Loading state exists
- Errors are handled
- Expected mapping nodes are validated
- Node mapping is centralized
- Registration happens at controlled lifecycle points
- Pending latest domain state is applied after late model load
- Disposal strategy is explicit

## 19. 3D Tiles Review

Verify:

- Initial scene usability is not blocked by full-detail streaming
- Tile lifecycle is considered dynamic
- State projection can apply to newly loaded content when required
- Custom materials/effects created for tile content are cleaned up
- Registry does not retain disposed tile objects
- Tile update integration follows the installed library's supported API

## 20. Realtime Architecture Review

Verify normalized flow:

```text
transport
↓
normalization
↓
domain state
↓
scene projection + Vue projection
```

Reject when:

- SignalR callback changes mesh material directly
- Polling response object is rendered directly throughout components
- Connection state is not modeled
- Stale policy is missing
- Incremental updates before scene load are lost

## 21. Performance Review

Inspect or profile when possible:

- FPS stability during camera movement
- Main-thread spikes during state batches
- Scene traversal cost
- Material allocation
- Texture memory concerns
- Label count
- Raycast frequency
- Vue component update frequency
- Network update volume
- Tile streaming behavior

Flag P1 when a normal state update causes a full-scene traversal in a large twin.

Flag P1 when route leave retains an active renderer loop.

Flag P1 when high-frequency telemetry forces 60 Hz Vue DOM updates without operational need.

## 22. Interaction Review

Verify:

- Click versus drag is correctly distinguished
- Active tool is visible
- Tool modes do not conflict
- Escape or clear action exits temporary modes when project conventions allow
- Icon-only tools have tooltips
- Destructive or consequential actions require appropriate confirmation

## 23. Accessibility Review

Verify:

- Icon controls have accessible names
- Keyboard focus is visible
- DOM controls have logical focus order
- Important state is not color-only
- Critical operational information exists in readable DOM form when practical
- Motion is controlled

## 24. Error-State Review

Verify the UI can distinguish:

- Model not found
- Model mapping mismatch
- Tileset load failure
- API unavailable
- Realtime disconnected
- No twin data
- Unknown object status

Do not show all cases as “Loading failed.”

## 25. Final Acceptance Summary Template

Use this structure internally or in the completion report:

```text
Workspace mode:
Primary operator:
Primary task:

Information hierarchy decision:

Scene/UI linkage:

Status and alarm model:

Transport/data boundary:

Performance-sensitive implementation:

Verified states:
- loading
- ready
- selected
- alarm
- stale/disconnected
- resize

P0 findings:
P1 findings:
P2 findings:
P3 findings:

Remaining assumptions or limitations:
```

## 26. Definition of Done

A substantial digital-twin UI task is done only when:

- The screen supports a defined operator task
- The 3D scene provides spatial operational value
- Status semantics are consistent
- Alarm and selection states do not conflict
- Scene and DOM UI are linked coherently
- Data transport is not directly coupled to presentation
- Loading and stale states are addressed
- Performance-sensitive scene code has been reviewed
- The running interface has been visually inspected when tooling permits
- P0 and P1 findings are resolved or explicitly reported
