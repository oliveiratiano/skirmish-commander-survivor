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

**Test:** Shift+Retreat with some units in oval. Command spreads. Unit dies mid-chain = chain stops.

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

**What:** Commander shows icon on issue. Units show same icon 1s after receiving. One visual per command (Engage, Follow, Retreat).

**Files:** New `CommandFeedbackDisplay` component, `CommandSystem.cs`, placeholder art/icons

**Test:** Commander icon on shout; unit icons 1s later; flow matches design.

| Status |
|--------|
| [ ] Not started |

---

## To Review

- **Command names:** Review naming of command states (Engage, Follow, Retreat). In particular, "Follow" may be misleading: e.g. Sharpshooter stands still while an enemy is in range (and has long range), which is intended behavior, but "Follow" might suggest movement toward the Commander. Consider names that better reflect "hold position / shoot when in range" vs "move toward Commander" if we rename.

---

## Future Features (Not Yet Broken Down)

Add new sections below as features are discussed and sliced. Use the same format: Goal, Design decisions, Slices with Test criteria.

<!--
Example template:

## Feature: [Name]

**Goal:** ...

**Design decisions:** ...

### Slice N: [Title]
**What:** ...
**Files:** ...
**Test:** ...
| Status | [ ] Not started |
-->
