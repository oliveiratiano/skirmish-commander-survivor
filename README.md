# Commander Survival (Working Title)

A real-time wave survival / auto-battler hybrid. Control a vulnerable Commander, draft an automated army, and issue real-time behavioral commands to survive endless swarms.

## Quick Start

### Prerequisites
- Unity 6 (6000.3.11f1)
- Git

### Open the Project
1. Clone the repository: `git clone <repo-url>` (avoid `--depth 1` or `--filter` to ensure assets download)
2. Open Unity Hub, click "Add" and select the project root folder
3. Open the project in Unity

### First-Time Setup
1. Wait for Unity to compile all scripts
2. In the menu bar, click **Commander Survival > 3. Full Setup (Data + Scene)**
3. This creates all ScriptableObject data assets and builds the game scene with every GameObject pre-configured

### Play
1. Open `Assets/_Project/Scenes/GameScene`
2. Press Play in the Unity Editor

## Project Structure

```
Assets/
  _Project/
    Data/            ScriptableObjects (UnitData definitions)
    Prefabs/         Entity prefabs (Commander, units, enemies, projectile)
    Scenes/          DraftScene, ArenaScene
    Scripts/
      Components/    HealthComponent, RangedAttackComponent, MovementComponent, etc.
      Systems/       WaveManager, CommandSystem, ObjectPool, CameraController
      Data/          UnitData ScriptableObject class definitions
      UI/            DraftUI, DebugOverlay, GameOverUI
      Input/         InputHandler
    Materials/       Shared materials (per unit type, GPU instanced)
```

## Controls

| Key | Action |
|-----|--------|
| WASD | Move Commander |
| Right Arrow | Engage (attack nearest enemy) |
| Up Arrow | Follow (form perimeter, fire when able) |
| Down Arrow | Retreat (regroup on Commander, no attacking) |
| Tab | Toggle mid-wave reinforcement panel |
| F3 | Toggle debug overlay |

## Troubleshooting

### Assets missing after clone/pull

If you synced the repo but the `Assets/` folder is empty or missing textures, scenes, or `.asset` files (Unity shows missing references, broken prefabs):

1. **Full pull**  
   `git pull origin main`

2. **Re-fetch and restore Assets from remote**  
   ```bash
   git fetch origin
   git checkout origin/main -- Assets/
   ```

3. **Fix shallow or partial clone** (if you used `--depth 1` or `--filter`):  
   ```bash
   git fetch --unshallow
   git checkout -- .
   ```

4. **Discard local changes and reset** (if you had conflicts or stale state):  
   ```bash
   git fetch origin
   git reset --hard origin/main
   ```

5. **Check branch** – ensure you're on `main`:  
   `git branch` then `git checkout main` if needed.

6. **Check global gitignore** – a custom global ignore can hide files:  
   `git config --global core.excludesfile`  
   If it points to a file that ignores `Assets/` or `*.png`, adjust or remove that rule.

7. **Fresh clone** – if all else fails, delete the folder and run a normal clone:  
   ```bash
   git clone https://github.com/oliveiratiano/skirmish-commander-survivor.git
   cd skirmish-commander-survivor
   ```  
   Avoid `--depth`, `--filter`, or sparse checkout so all assets are fetched.

### Unity Hub: nothing happens when adding project

If you select the project folder in Unity Hub and nothing happens (project is not added):

1. **Verify the folder is a valid Unity project** – The folder you select must contain `Assets/`, `ProjectSettings/`, and `Packages/` at the same level. Select the *root* folder (e.g. `skirmish-commander-survivor`), not a subfolder.

2. **Check the clone is complete** – If `ProjectSettings/` or `Packages/` is empty/missing, Unity won't recognize it. Run:
   ```bash
   dir ProjectSettings\ProjectVersion.txt
   dir Packages\manifest.json
   dir Assets\_Project
   ```
   All should exist. If not, fix the clone first (see "Assets missing" above), then try again.

3. **Unity version** – The project requires **Unity 6 (6000.3.11f1)**. Install it in Unity Hub via **Installs > Install Editor**. Hub may ignore projects whose version is not installed.

4. **Path requirements** (Windows):
   - Use a short path (e.g. `C:\Projects\skirmish-commander-survivor`)
   - Avoid paths over 260 characters
   - Avoid special characters, spaces, or non-ASCII letters
   - Don't place the project inside `Program Files`, OneDrive sync folder, or a network drive

5. **Use "Add project from disk"** – In Unity Hub, click the dropdown next to **Add**, choose **Add project from disk**, then select the project root folder.

6. **Permissions & antivirus** – Run Unity Hub as administrator, or temporarily disable antivirus. Ensure the project folder is readable (not read-only).

## Documentation

| Document | Description |
|----------|-------------|
| [SPEC](docs/SPEC.md) | Product specification and requirements |
| [BACKLOG](docs/BACKLOG.md) | Feature backlog with incremental slices and test criteria |
| [ADRs](docs/adr/) | Architecture Decision Records |
| [CHANGELOG](CHANGELOG.md) | Version history |
