# Product and Visual System Reference

## 1. Product Positioning

The target product is an enterprise digital-twin monitoring platform.

It should visually communicate:

- Operational confidence
- Spatial clarity
- Real-time awareness
- Technical maturity
- Enterprise consistency
- Long-session usability

It should not visually imitate:

- A game
- A science-fiction movie
- A cryptocurrency dashboard
- A marketing landing page
- A generic admin template
- A large-screen showpiece with no daily workflow

## 2. Design Personality

Preferred visual attributes:

- Precise
- Calm
- Technical
- Layered
- Dense but structured
- Modern
- Controlled
- Spatially aware

Avoid:

- Loud
- Decorative
- Cartoonish
- Excessively futuristic
- Excessively glossy
- Overly soft consumer-app styling

## 3. Design System Strategy

Always inspect and reuse project tokens first.

If no design system exists, create semantic tokens before styling screens.

Recommended token groups:

```text
--dt-color-bg-app
--dt-color-bg-scene
--dt-color-surface-1
--dt-color-surface-2
--dt-color-surface-raised
--dt-color-surface-overlay
--dt-color-border-subtle
--dt-color-border-strong
--dt-color-text-primary
--dt-color-text-secondary
--dt-color-text-muted
--dt-color-accent
--dt-color-focus

--dt-status-normal
--dt-status-running
--dt-status-idle
--dt-status-waiting
--dt-status-warning
--dt-status-fault
--dt-status-offline
--dt-status-emergency
--dt-status-maintenance
--dt-status-unknown

--dt-alarm-critical
--dt-alarm-major
--dt-alarm-minor
--dt-alarm-notice
```

Do not define component-specific colors such as `--machine-red` and `--warning-panel-yellow` when they represent shared semantic states.

## 4. Fallback Visual Direction

Only use this fallback when the repository has no established brand or visual system.

Use a dark-neutral operational environment with restrained cool accents.

Suggested characteristics:

- Very dark neutral app background
- Slightly lighter panels
- Controlled surface opacity over the scene
- Neutral white primary text
- Cool gray secondary text
- One primary accent hue
- Semantic warning and fault colors reserved for actual state

Do not overuse cyan merely because the product contains 3D.

Do not use alarm red as decorative accent.

The primary accent must never compete with critical severity.

## 5. Surface Hierarchy

Use surface depth to clarify function.

Suggested hierarchy:

- Scene: deepest visual layer
- Persistent HUD: low-to-medium elevation
- Context panel: clear reading surface
- Tray: stable operational surface
- Popover: elevated temporary surface
- Dialog: strongest modality

Avoid placing every region inside a bordered card.

Use whitespace, alignment, separators, and surface tone before adding more containers.

## 6. Border and Radius Rules

Use restrained radii.

Enterprise digital-twin interfaces generally benefit from small to medium radii.

Do not make every control pill-shaped.

Use borders to:

- Separate dense data regions
- Define interaction targets
- Clarify selection
- Show focus

Avoid decorative glowing borders.

## 7. Shadow Rules

Use shadows sparingly because the 3D scene already creates visual complexity.

Prefer subtle elevation for:

- Floating context panels
- Menus
- Popovers
- Dialogs

Do not apply heavy shadows to every panel.

## 8. Transparency Rules

Choose opacity based on content density.

Recommended approach:

- Small HUD chips: moderate transparency may be acceptable
- Tool rail: moderate opacity
- Object inspector: high readability, more opaque
- Alarm list: high readability, more opaque
- Dense table: near-opaque
- Playback controls: moderate to high opacity

Never choose a fixed opacity because it “looks futuristic.”

Test panels over bright, dark, and detailed scene backgrounds.

## 9. Typography Scale

Use the project system first.

When defining a fallback scale, keep it compact and functional.

Example roles:

```text
Twin title         18-22 px
Workspace heading  16-18 px
Panel heading      14-16 px
Body               13-14 px
Operational label  12-13 px
Metadata            11-12 px
```

These are guidance, not mandatory pixel constants.

Do not use 32-48 px marketing headings inside monitoring workspaces.

Use font weight and spacing before dramatic size jumps.

## 10. Numeric Presentation

For frequently changing metrics:

- Use tabular numbers when available
- Align decimals when comparison matters
- Show units consistently
- Avoid excessive decimal precision
- Distinguish value from unit
- Communicate stale values

Example:

```text
1,248  pallets
  82.4 %
  12.7 °C
```

Do not animate every numeric change with counting effects.

## 11. Status Visual Language

Status must be a shared system.

A recommended semantic interpretation:

- normal: healthy and available
- running: active operation or motion
- idle: healthy but inactive
- waiting: paused by dependency or queue condition
- warning: degraded but operating
- fault: failed and requires intervention
- offline: no communication or unavailable
- emergency: urgent safety or emergency state
- maintenance: intentionally out of normal operation
- unknown: state cannot be determined

Do not collapse `offline`, `fault`, and `unknown` into one gray state.

## 12. Alarm Visual Language

Alarm severity is not the same as equipment status.

An object may be:

```text
status = running
alarm = minor
```

or:

```text
status = fault
alarm = critical
```

Model these dimensions separately when the backend supports them.

Recommended attention hierarchy:

```text
critical > major > minor > notice
```

Critical attention may use stronger contrast or controlled motion.

Do not make minor and critical alarms visually equivalent.

## 13. Iconography

Use one coherent icon set already present in the repository.

Avoid mixing:

- Outline icons
- Filled icons
- Emoji
- Custom neon SVGs
- Multiple icon libraries

Use domain icons only when they improve recognition.

A generic status dot is often better than an inaccurate machine icon.

## 14. Scene Legend

A legend is necessary when scene color encodes operational state.

The legend should:

- Use the exact shared status semantics
- Be compact
- Be hideable if persistent screen space is constrained
- Explain filters or dimmed states
- Show only currently relevant statuses when appropriate

Do not create a decorative legend disconnected from actual scene materials.

## 15. Tooltips

Scene tooltip content should be short.

Recommended fields:

- Object name
- Object code
- Current status
- One critical metric or active alarm count

Use the context inspector for full details.

A tooltip should not become a floating mini-dashboard.

## 16. Inspector Structure

Recommended selected-object inspector hierarchy:

```text
Object identity
↓
Operational status and alarms
↓
Primary live metrics
↓
Current task / route / dependency
↓
Technical attributes
↓
Recent events or history shortcut
↓
Actions
```

Do not begin with 20 static model properties when the object is currently in fault.

## 17. Alarm Panel Structure

Recommended fields:

- Severity
- Alarm title
- Affected object
- Location
- Start time
- Duration
- Current acknowledgement or handling state
- Short diagnostic message
- Related metrics
- Action or drill-down

Sort and group based on operational rules, not simply by string order.

## 18. Empty States

Examples:

No selected object:

> Select an object in the scene or hierarchy to inspect its live state.

No alarms:

> No active alarms in the current scope.

No matching objects:

> No twin objects match the current filters.

Scene layer failed:

> This scene layer could not be loaded. Other twin data remains available.

Avoid cheerful marketing illustrations inside serious operational workflows unless the product design explicitly uses them.

## 19. Loading States

Use context-specific loading indicators.

Examples:

- Scene metadata loading
- Base model loading
- Tile stream initializing
- State data synchronizing
- Object history loading

Do not show one global spinner for unrelated background activity.

## 20. Notification Rules

Use notifications for meaningful results.

Do not toast every state update.

Use:

- Inline state for persistent operational conditions
- Alarm panel for active alarms
- Toast for user-triggered action results
- Banner for connection or system-wide degradation

## 21. Dark Mode

A dark scene workspace is often appropriate, but do not assume the entire enterprise product must always be dark.

If the broader SaaS shell supports light mode:

- Keep scene controls consistent
- Ensure 3D overlay surfaces remain readable
- Preserve semantic status colors
- Avoid duplicating status palettes with different meaning

The scene background and application theme may be managed separately.

## 22. Anti-Pattern Review

Reject the design when any of these are true:

- The screen could be relabeled as a crypto dashboard with minimal changes
- The 3D model is visible but operational data is hard to locate
- All attention goes to KPI cards instead of exceptions
- Every panel glows
- Every object pulses
- Alarm red is used for decoration
- Selection hides fault state
- The detail panel contains only static model metadata
- Operators cannot tell whether data is fresh
- The interface looks impressive in a screenshot but is slow to operate
