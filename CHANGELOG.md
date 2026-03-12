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
