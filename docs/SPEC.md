# MVP Game Specification

## 1. Project Overview
* **Genre:** Real-Time Wave Survival / Auto-Battler Hybrid.
* **Camera:** Top-down orthographic view, locked to the Commander with slight dynamic zoom (zooms out as enemy density increases, zooms in when clear).
* **Core Loop:** Draft army (Pre-game) -> Enter Arena -> Move Commander & Issue State Commands -> Survive.
* **Victory Conditions:**
  1. All pre-set enemies for the wave are spawned and killed before the timer expires (best score).
  2. Timer expires — any remaining enemies continue spawning/fighting. A negative timer begins, progressively penalizing the player's score until all enemies are eliminated.
* **Loss Condition:** Commander HP reaches 0.

## 2. Army Drafting (Pre-Game)
* **Budget:** Player starts with 100 Points.
* **UI:** Menu to add/remove units based on cost.
* **Flow:** Clicking "Start" locks the roster, deducts points, and spawns the Commander and drafted army in the center of the arena.
* **Mid-Wave Reinforcement:** Remaining budget points can be spent during the wave to spawn additional units.

## 3. Controls & Command States
* **Movement:** Player directly controls the Commander via WASD.
* **Command Toggles:** Arrow keys broadcast global state changes to all player units via a C# Event System.
  * **Right Arrow** = Attack
  * **Up Arrow** = Form Up
  * **Down Arrow** = Regroup

| State | Movement Behavior | Combat Behavior |
| :--- | :--- | :--- |
| **Attack** | Break formation. Move toward absolute closest enemy. | Fire weapon when in range and off cooldown. |
| **Form Up** | Path toward Commander to form a loose perimeter. | If enemy is in range, enter **shoot-preparation stage** (brief pause simulating reload/aim), fire, then resume movement. Units move slower than Regroup when engaged in combat due to stop-and-shoot cycles. |
| **Regroup** | Path directly to Commander's exact position urgently. When within **Commander radius**, unit switches to **Form Up** and can shoot again. | **Do not attack** while regrouping. Ignore enemies to prioritize repositioning. |

## 4. Entity Attribute Matrix
All combat is ranged. Accuracy uses **Aim Deviation** (rotating the perfect vector to the target by a random angle within the "Accuracy Spread"). Projectiles are visible traveling bullets with speed and lifetime. Projectiles pass harmlessly through friendly units.

| Entity | Cost | HP | Speed | Dmg | Range | Cooldown / Fire Rate | Accuracy Spread |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| **Commander** | N/A | 100 | 5.0 | 12 | 8.0 | 0.8s | ± 5° (Reliable) |
| **Close-Quarters** | 10 | 30 | 4.5 | 8 | 4.0 | 0.4s | ± 25° (Wide spray) |
| **Machine Gunner** | 15 | 20 | 3.5 | 4 | 8.0 | **5-shot burst. 0.1s interval, 1.5s reload.** | ± 15° (High volume) |
| **Sharpshooter** | 20 | 10 | 4.0 | 25 | 12.0 | 2.0s | ± 2° (Pinpoint) |
| **Swarm Bug** | N/A | 15 | 3.5 | 5 | 3.0 | 1.2s | ± 15° (Acid spit) |

## 5. Enemy Spawning & Arena
* **Arena:** Massively bounded flat plane (`#2F2D2A`). No obstacles.
* **Spawning:** Wave manager spawns "Swarm Bugs" just outside the camera viewport. Rate increases linearly every 30 seconds. Each wave has a pre-set total enemy count.
* **Enemy Targeting:** Enemies target the nearest unit (including the Commander).

## 6. Strict Technical Architecture
* **Engine:** Unity (C#), latest LTS.
* **Multithreading (CPU):** Deferred to post-MVP. Single-threaded baseline first, then offload O(N^2) calculations to background worker threads.
* **Rendering (GPU):** Enable **GPU Instancing** for all projectiles and enemies. Use single shared materials per unit type.
* **Memory Management:** **Object Pooling** is mandatory for all projectiles and enemies. No `Instantiate()`/`Destroy()` during gameplay.
* **Physics:** Do **NOT** use rigidbodies or complex physics. Use simple Vector math overlap checks. Units use soft local-avoidance (boids) with lightweight friendly collision.
* **Architecture:** Use **Composition over Inheritance** (e.g., `HealthComponent`, `RangedAttackComponent`) and drive stats via ScriptableObjects (`UnitData`).

## 7. Design, Assets & Procedural Animation
* **Asset Format:** Primitive shapes (colored quads) as MVP placeholders. Final art will use single static `.png` sprites per entity type. No sprite sheets.
* **Aesthetic:** Grimdark Dystopian Sci-Fi.
* **Procedural Animation (Code-Driven):**
  * **Idle:** Slow, continuous subtle vertical scaling (Y-axis breathes by 2%).
  * **Movement ("Waddle"):** When velocity > 0, rhythmically rotate sprite on Z-axis (± 5 degrees).
  * **Shooting ("Recoil"):** Instantly scale down by 10%, smooth interpolate back over 0.1s.
  * **Hit Flash:** Swap material color to pure white for 0.1s upon taking damage.

## 8. QA, Playtesting & Architecture Tracking
* **Debug Overlay:** MVP includes a UI toggle (Press F3) displaying: FPS, frame time, Active Command State, Active Player Units, Active Enemies, Pooled Projectiles, Pooled Enemies, Spawn Rate.
* **Target Performance:** 60 FPS on standard hardware.
