# Commander Survival (Working Title)

A real-time wave survival / auto-battler hybrid. Control a vulnerable Commander, draft an automated army, and issue real-time behavioral commands to survive endless swarms.

## Quick Start

### Prerequisites
- Unity 6 (6000.3.11f1)
- Git

### Open the Project
1. Clone the repository
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

## Documentation

| Document | Description |
|----------|-------------|
| [SPEC](docs/SPEC.md) | Product specification and requirements |
| [BACKLOG](docs/BACKLOG.md) | Feature backlog with incremental slices and test criteria |
| [ADRs](docs/adr/) | Architecture Decision Records |
| [CHANGELOG](CHANGELOG.md) | Version history |
