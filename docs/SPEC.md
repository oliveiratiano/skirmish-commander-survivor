# MVP Game Specification

## 1. Project Overview
* **Genre:** Real-Time Wave Survival / Auto-Battler Hybrid.
* **Camera:** Isometric orthographic view (tilted angle), locked to the Commander with slight dynamic zoom (zooms out as enemy density increases, zooms in when clear).
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

## 7. Design, Assets & Animation
* **View:** Isometric. Camera is tilted so the scene is seen from above and behind; entity sprites face the camera.
* **Aesthetic:** Grimdark Dystopian Sci-Fi.
* **Sprite Art (Keyframe Animation):**
  * Each entity type has a small set of **animation frames** (same isometric angle and pose style for all frames). Frames are played by state so the game reads as animated, not a single floating image.
  * **Per-cell aspect:** width:height = 1:1.2 per frame (cell size **256×307 px**; see strict sheet dimensions below).
  * **Format:** `.png` with transparency. One **sprite sheet** per entity; the runtime picks the correct frame by index.
  * **Frame sets per entity (fixed for grid):** Idle **3** frames (indices 0–2) · Walk **6** frames (indices 3–8) · Shoot **2** frames (indices 9–10). Hit = procedural flash (no extra frame). Index **11** = unused/spare.
  * **State logic:** Idle when not moving and not shooting; Walk when moving; Shoot triggers shoot frames then reverts. No directional variants (single facing; sprites always face the camera).
* **Technical:** Animation is driven by game state (velocity, firing). Use Unity **Animator** + **Animation** clips (or a small script) to swap `SpriteRenderer.sprite` or advance sprite-sheet index by state. One controller per entity type (or shared with overridden clips).
* **Procedural (optional):** Light code-driven effects may remain (e.g. hit flash tint, subtle scale on shoot) in addition to keyframe art.

* **Asset placement and layout (streamlined — no manual slice editing):**
  * **Location:** One PNG per entity in `Assets/_Project/Art/<EntityName>/`, e.g. `Commander/Commander.png`, `SwarmBug/SwarmBug.png`.
  * **Strict dimensions (required for pipeline):** The sprite sheet PNG **must** be exactly **1536×614 pixels** (width × height). Cell size is **256×307 px** (6 columns × 2 rows). No other dimensions are supported; wrong sizes cause slice errors (e.g. rect outside texture). Source of truth in code: `GameConstants.SPRITE_SHEET_*`.
  * **One sheet, fixed grid:** Each entity uses a **single sprite sheet** with this **fixed grid**. Unity slices by grid; the game uses **sprite index** (0–11). Left-to-right, top-to-bottom:
    * **Row 0:** Idle (3) then Walk (3): indices **0, 1, 2** = idle · **3, 4, 5** = walk.
    * **Row 1:** Walk (3) then Shoot (2) then spare: indices **6, 7, 8** = walk · **9, 10** = shoot · **11** = unused (or hit).
  * **Unity:** Import the PNG → Texture Type = **Sprite (2D and UI)**, Sprite Mode = **Multiple**. Auto-import under `Assets/_Project/Art/` applies 6×2 slice; or use menu **Commander Survival → Slice Selected Texture 6x2 (12 sprites)**. Pixels Per Unit: **64**. Filter Mode as needed.
  * **Summary:** One PNG per entity, **exactly 1536×614 px**, 6×2 grid (256×307 per cell). Game uses indices 0–2 idle, 3–8 walk, 9–10 shoot.

## 8. QA, Playtesting & Architecture Tracking
* **Debug Overlay:** MVP includes a UI toggle (Press F3) displaying: FPS, frame time, Active Command State, Active Player Units, Active Enemies, Pooled Projectiles, Pooled Enemies, Spawn Rate.
* **Target Performance:** 60 FPS on standard hardware.
