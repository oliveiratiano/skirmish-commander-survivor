# ADR-001: Initial Technical Decisions

## Status
Accepted

## Context
Commander Survival is a real-time wave survival / auto-battler hybrid that needs to handle potentially thousands of concurrent entities (projectiles, enemies, allied units) while maintaining 60 FPS. The tech stack must support composition-based architecture, efficient batched rendering, and a clear path toward multithreaded computation for O(N^2) operations like nearest-neighbor targeting.

## Decision
- **Engine:** Unity (C#), latest LTS. Chosen for mature tooling, large ecosystem, strong GPU instancing support, and broad AI-assisted development knowledge base.
- **Architecture:** Strict Composition over Inheritance. Entities are built from reusable components (`HealthComponent`, `RangedAttackComponent`, `MovementComponent`, etc.). Stats are driven by `ScriptableObject` data assets (`UnitData`).
- **Physics:** No Rigidbodies or Unity physics. All movement, collision avoidance, and hit detection use pure Vector math (overlap checks, distance comparisons, boids-style soft repulsion).
- **Memory:** Object Pooling is mandatory for all projectiles and enemies. No `Instantiate()`/`Destroy()` during gameplay.
- **Rendering:** GPU Instancing with single shared materials per unit type.
- **Multithreading:** Deferred to a post-MVP milestone. Build single-threaded first to stabilize behavior, then offload expensive calculations (targeting, steering) to worker threads.
- **Input:** Keyboard only for MVP. WASD for Commander movement, Arrow keys for command states (Up = Follow, Down = Retreat, Right = Engage).
- **Events:** C# delegates/events for global command state broadcasts. No `SendMessage` or string-based messaging.
- **Visuals:** Primitive shapes (colored quads) as placeholders. Single static sprite per entity type in final art. All animation is procedural (code-driven).

## Consequences
- **Positive:** Composition + ScriptableObjects makes adding new unit types trivial. No-physics approach avoids engine overhead and keeps frame budgets predictable. Object pooling eliminates GC spikes. Staged multithreading reduces debugging complexity during prototyping.
- **Negative:** Unity LTS locks us into a specific feature set. Manual vector math for collision means reimplementing what physics engines provide out of the box. Single-threaded MVP may hit performance walls earlier with very high entity counts, but this is acceptable for the prototype phase.
- **Risk:** If Unity's rendering pipeline doesn't scale to target entity counts, may need to evaluate DOTS/ECS. This is a future concern, not an MVP blocker.
