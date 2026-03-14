# Feature Backlog

This document registers planned features and breaks them into incremental, testable slices. Use it as a guide when implementing changes—each slice should be playable and verifiable before moving to the next. Chat context may be lost between sessions; this file provides the continuity.

---

## Format

Each feature entry includes:
- **Goal:** What the feature accomplishes
- **Design decisions:** Locked-in choices (from discussion/SPEC)
- **Slices:** Ordered implementation steps; each slice is independently testable
- **Test criteria:** How to verify the slice works

---

## Feature: Directional Command System

**Goal:** Replace global command broadcast with directional "shouting." Commands affect only units in an oval area in front of the Commander. Optional attention mechanic filters by unit type. Optional propagation lets commands relay unit-to-unit. Each unit stores its own command state.

**Design decisions (locked):**
- Oval shout area, offset forward along Commander's facing
- Commander facing = last movement direction; can shout while moving
- Per-unit command state (each unit holds its own)
- Number keys (1, 2, 3...) prompt unit types additively; 5s timeout
- Unprompted arrow = broadcast to ALL units in oval
- Shift + arrow = enable propagation/relay
- Relay: ~0.5s per-hop delay, unlimited hops, death breaks chain
- AI reaction delay: 0.3s before units act on new command
- Visual: Commander shows icon on issue; units show same icon 1s later
- Parameters (oval size, relay radius, etc.) tunable for future upgrades

---

### Slice 1: Per-Unit Command State

**What:** Each unit stores its own `CommandState`. `CommandSystem` broadcasts to all units on command. Behavior identical to current (all units still receive every command).

**Files:** `UnitAIController.cs`, `CommandSystem.cs`

**Test:** Play normally. All units follow global commands. No observable difference from current build.

| Status |
|--------|
| [x] Done |

---

### Slice 2: Commander Facing Direction

**What:** Commander has a facing derived from last movement. Default facing when never moved (e.g. down).

**Files:** `CommanderController.cs`, `GameConstants.cs`

**Test:** Commander faces direction of last movement. Optional: F3 debug line shows facing.

| Status |
|--------|
| [x] Done |

---

### Slice 3: Oval Shout Area + Directional Broadcast

**What:** Only units inside the oval receive commands. Oval = ellipse centered forward of Commander. Units outside keep current state.

**Files:** New `ShoutGeometry` (or helper in `GameConstants`), `CommandSystem.cs`, `GameConstants.cs`

**Test:** Face one cluster, issue command. Only units in front receive; units behind keep old state.

| Status |
|--------|
| [x] Done |

---

### Slice 4: Attention Mechanic

**What:** Keys 1/2/3/4 prompt unit types (additive). Arrow: if prompted, send only to prompted types in oval; else all in oval. 5s timeout. Minimal visual on prompted units.

**Files:** `CommandSystem.cs`, `InputHandler.cs`, `UnitAIController.cs`

**Test:** 1 + arrow = only Close-Quarters in oval. 1+2 + arrow = both types. Unprompted arrow = all in oval. Prompt expires after 5s.

| Status |
|--------|
| [x] Done |

---

### Slice 5: Propagation Flag + Relay System

**What:** Shift+arrow enables relay. Receiving units pass command to nearby allies after hop delay. Death breaks chain.

**Files:** `InputHandler.cs`, `CommandSystem.cs`, `UnitAIController.cs`, `GameConstants.cs`

**Test:** Shift+Regroup with some units in oval. Command spreads. Unit dies mid-chain = chain stops.

| Status |
|--------|
| [x] Done |

---

### Slice 6: AI Reaction Delay

**What:** Units wait 0.3s before acting on new command.

**Files:** `UnitAIController.cs`, `GameConstants.cs`

**Test:** Issue command. Units keep old behavior 0.3s, then switch.

| Status |
|--------|
| [x] Done |

---

### Slice 7: Visual Command Feedback

**What:** Commander shows icon on issue. Units show same icon 1s after receiving. One visual per command (Attack, Form Up, Regroup).

**Files:** New `CommandFeedbackDisplay` component, `CommandSystem.cs`, placeholder art/icons

**Test:** Commander icon on shout; unit icons 1s later; flow matches design.

| Status |
|--------|
| [x] Done |

---

## To Review

_(None at this time. Arena floor refactor is complete; no floor-related issues remain. Command names were updated to Attack / Form Up / Regroup; Regroup transitions to Form Up when within Commander radius.)_

---

## Planned Features (Recommended order: 1 → 2 → 3 → 4 → 5)

---

## Feature: Border Damage Immunity for Entering Enemies

**Goal:** Enemies do not take border/danger-zone damage while "entering" the arena; they take it only after they have entered and then leave the safe zone.

**Design decisions:** Add a flag on enemy units (e.g. on `UnitAIController` or a small component) meaning "immune to boundary damage." When the unit first satisfies an "has entered arena" condition (e.g. position inside safe rectangle, or inside a smaller "core" zone), clear the flag. `ArenaBoundary.ApplyBoundaryDamage` skips enemies that have the flag set. Enemies that are spawned outside of arena boundaries start with the flag set.

**Slices:** To be broken down when starting (e.g. Slice 1: add flag and set on spawn when outside arena; Slice 2: define "entered" and clear flag; Slice 3: ArenaBoundary respects flag).

| Status |
|--------|
| [ ] Not started |

---

## Feature: Respawn Radius Around Commander + Camera Limits (Spawns Outside FOV)

**Goal:** Enemies never spawn within a minimum distance of the Commander (respawn radius). Camera zoom and mobility are limited so that spawns always occur outside the player's field of view (no spawning on-screen).

**Design decisions (to refine when starting):** (1) **Respawn radius:** Constant (e.g. in `GameConstants`). In `GetSpawnPosition`, reject or re-sample candidate positions until one is at least that far from the Commander (or derive positions from a ring/arc at that distance). (2) **Camera limits:** Cap max orthographic size (and optionally scroll range) so that the visible viewport never extends into the spawn region. Optionally clamp camera position so the viewport cannot pan into spawn-only zones. Result: spawn region is always off-screen.

**Slices:** To be broken down when starting (e.g. Slice 1: add respawn radius and enforce in `GetSpawnPosition`; Slice 2: cap camera zoom/mobility so spawn region stays outside FOV; tune constants).

| Status |
|--------|
| [ ] Not started |

---

## Feature: Swarm Bug Swarm Behavior

**Goal:** Swarm bugs behave as a swarm: when outnumbered they avoid allied units and seek peers; when the local swarm is "large enough" they commit to attack. Add some random variation to enemy behavior so runs feel less samey.

**Design decisions (to refine when starting):** Define "outnumbered" (e.g. local player count vs local swarm count within a radius). "Large enough" = threshold (count or ratio). Movement: when retreating, move away from threats toward nearby allies; when engaging, current target-seeking. Random variation: e.g. slight variance in thresholds, timing, or movement choices so behavior isn't perfectly deterministic.

**Slices:** To be broken down when starting (e.g. count allies in radius; retreat/seek-peers state; engage threshold; random variation in behavior).

| Status |
|--------|
| [ ] Not started |

---

## Feature: Second Stage — Hive Eradication

**Goal:** A second stage/mode where the player must eliminate hive(s). Enemies spawn from hives; spawn rate or count is tied to how many units are already outside the hive / in the arena (so the encounter stays manageable and design-controllable).

**Design decisions (to refine when starting):** What is a "hive" (static object, HP, spawn point). Spawn rule: e.g. "total units in arena ≤ cap" or "spawn interval increases when arena is full." Win condition: all hives destroyed (and optionally all spawned enemies). How the player selects or enters this stage (menu, after wave 1, etc.) TBD.

**Slices:** To be broken down when starting (e.g. hive entity and spawn; spawn rule from "units in arena"; stage flow and win condition).

| Status |
|--------|
| [ ] Not started |

---

## Feature: Sound Design (Commands, Responses, Combat, Death)

**Goal:** Add audio for commands (Commander shout), unit responses (tiered by group size + variation so it doesn't sound like one voice × N), and combat/death (e.g. bullet sounds, death cries). Use a small set of clips with random variation (pitch/volume or variants) so it feels natural and performant.

**Design decisions (to refine when starting):** Command sounds per command type (Attack, Form Up, Regroup). Response sounds: 2–3 "tiers" (e.g. few units vs many) with random variation per play. One-shot combat/death sounds with optional variation. No requirement to play one clip per unit per event (pool by event type / tier).

**Slices:** To be broken down when starting (e.g. audio setup and assets; command + response hooks; combat/death hooks; variation and tiers).

| Status |
|--------|
| [ ] Not started |
