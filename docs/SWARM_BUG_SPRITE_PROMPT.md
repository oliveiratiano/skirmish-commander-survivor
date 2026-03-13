# Swarm Bug — sprite generation prompt

Use this prompt to generate the three directional sprite sheets for the **Swarm Bug** enemy. The game uses three images per character: movement **up**, **right**, and **down** (right is also flipped for left).

---

## Character brief: Swarm Bug

- **Role:** Enemy unit. Spawns in waves, targets the nearest player unit or Commander. Fires a ranged “acid spit” attack (short range, moderate spread).
- **Style:** Grimdark dystopian sci-fi. Creature or bio-mechanical enemy that reads as a swarm/insectoid threat. Same isometric view and single-facing style as the rest of the game (camera-facing).
- **Scale:** Slightly smaller than the Commander on screen; the sprite will be scaled in-engine, so draw the creature to read clearly at roughly **256×256 px** per frame (one cell).
- **Animation:** Movement only. No dedicated “attack” or “shoot” frames in the sheet; attack feedback is handled in code. So all 25 frames per sheet are a **walk/movement cycle** for that direction. When idle, the game shows only the **center frame** (middle of the 5×5 grid).

---

## Technical requirements (mandatory)

- **Three PNGs** per character. Name them for direction, e.g.:
  - `SwarmBug_up.png`
  - `SwarmBug_right.png`
  - `SwarmBug_down.png`
- **Exact dimensions:** Each image must be **1280×1280 pixels** (width × height). No other size will work with the pipeline.
- **Grid:** **5 columns × 5 rows** → **25 cells**. Each cell is **256×256 px**. Layout is **left-to-right, top-to-bottom**, index 0–24.
- **Frame usage:**
  - **Idle:** The game uses only the **center cell** (row 3, column 3 in 1-based; index **12** in 0-based). That cell should be a standing/idle pose for the Swarm Bug facing that direction.
  - **Walk:** **All 25 cells** (0–24) form the full walk cycle for that direction. Each cell must be a **unique frame**; do not repeat the same drawing for multiple cells. The cycle should loop smoothly (frame 24 leads back to frame 0).
- **Per cell:** Draw the Swarm Bug centered in a 256×256 px square. Same isometric angle and style in every frame. Transparent background (PNG with alpha).
- **Right vs left:** The game uses the **right** sheet flipped horizontally for “left”. So you only provide up, right, and down; no separate left sheet.

---

## Copy-paste prompt for image generation

**Swarm Bug enemy — directional sprite sheet (choose one direction per image: up, right, or down).**

- **One PNG, exactly 1280×1280 px**, 5×5 grid. Each of the 25 cells is **256×256 px**. Left-to-right, top-to-bottom order (indices 0–24).
- **Content:** Grimdark sci-fi **Swarm Bug** enemy — insectoid or bio-mechanical creature that fits a “swarm” ranged enemy (acid-spit attacker). Isometric view, single facing per sheet; character drawn to fit 256×256 per cell, transparent background.
- **Center cell (row 3, col 3, index 12):** Standing/idle pose for this direction.
- **All other cells (0–24):** Full **walk/movement cycle** for this direction. **Every cell must be a unique frame;** no duplicate poses. Smooth loop so the animation can repeat.
- **Style:** Grimdark dystopian sci-fi. Consistent lighting and palette across all 25 cells. No shoot/attack frames in the sheet.

Generate **three separate images** with the same rules, one for **movement up**, one for **movement right**, and one for **movement down**. Use the same Swarm Bug design in all three; only the direction of movement and pose cycle change.

---

## After generation

1. Save the three PNGs as **1280×1280** (resize if needed).
2. Place them in your Unity project under `Assets/_Project/Art/SwarmBug/` (or any folder under `Assets/_Project/Art/`).
3. In Unity, select **SwarmBugData** (or the UnitData used for the enemy) in the Project window.
4. In the Inspector, use **Load Sprites (Up)...**, **Load Sprites (Right)...**, and **Load Sprites (Down)...** to assign each PNG to the matching direction.
5. If a texture has no slices, select it and use **Commander Survival → Slice Selected Texture 5x5 (25 sprites)** then load again.
