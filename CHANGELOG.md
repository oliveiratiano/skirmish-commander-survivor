# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com),
and this project adheres to [Semantic Versioning](https://semver.org).

## [Unreleased]
### Added
- Project documentation scaffolding (SPEC, ADR-001, README, CHANGELOG)
- M1: Unity project structure, Commander movement (WASD), flat arena, orthographic camera with dynamic zoom, F3 debug overlay
- M2: UnitData ScriptableObjects, army drafting UI (100-point budget), unit spawning in formation, Follow behavior, boids-style friendly collision avoidance
- M3: Object pooling system, WaveManager with escalating spawn rate, visible projectile system with aim deviation, RangedAttackComponent with burst fire, HealthComponent, HitFlashComponent
- M4: Full 3-state command system (Engage/Follow/Retreat) via arrow keys, shoot-preparation stage in Follow mode, command state HUD
- M5: GameFlowManager with wave timer, victory/loss conditions, overtime negative-timer scoring, mid-wave reinforcement UI (Tab), game over screen with restart
- M6: ProceduralAnimator (idle breathing, movement waddle, shooting recoil), GPU instancing on materials
- Bounded arena (~120x120 units) with danger zone boundary (DPS outside safe area, visual red strips and edge lines)
- Editor menu setup script (Commander Survival > Full Setup) for one-click project initialization

### Changed
- Isometric camera and visuals: camera tilted (30°) with fixed offset; entity quads tilted to face camera; Y-based depth sorting (IsometricSorting); projectiles draw on top; SPEC updated with isometric view and sprite ratio 1:1.2.
- Command renames (Engage→Attack, Follow→Form Up, Retreat→Regroup); single `COMMANDER_RADIUS` constant; Regroup switches to Form Up when unit is within radius.
- Visual command feedback: Commander shows command icon on issue; units show same icon 1s after receiving (placeholder colored quads per command) (Slice 7).
- Shift+arrow enables relay: receiving units pass command to nearby allies of the same type after 0.5s hop delay; death breaks chain (Slice 5, same-type-only relay).
- AI reaction delay: units wait 0.3s before acting on a new command; effective command lags behind received command (Slice 6).
- Attention mechanic: keys 1/2/3/4 prompt unit types (additive, 5s timeout); arrow sends command only to prompted types in oval when set, else all in oval; prompted units show indicator (Slice 4).
- Commands are directional: only units inside the shout oval in front of the Commander receive them; others keep current state (Slice 3).
- Commander has a facing direction from last movement (default down); FacingDirection exposed; shout oval constants added for Slice 3 (Slice 2).
- Command state is now per-unit (each unit stores its own state; CommandSystem broadcasts to all on arrow key). No gameplay change; foundation for directional commands (BACKLOG Slice 1).
- Arena size reduced from 400x400 to 120x120 units (4-6 screens)
- Swarm Bug stats retuned: HP 15->20, speed 3.5->4.0, damage 5->7
- Wave difficulty increased: 100 enemies (was 60), faster spawn ramp (1.5s base, 0.2s min), steeper interval decay
