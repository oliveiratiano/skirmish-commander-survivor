# BGM Integration for Swarm Swamp Arena (Stage 1)

## Summary

Background music plays when the battle starts (Drafting → Battle) and stops when the game ends (Victory or Defeat). The BGM loops seamlessly.

## What Was Done

### 1. Folder structure

```
Assets/_Project/Audio/
├── .gitkeep
└── Music/
    └── Stage1/
        └── .gitkeep
```

Place your BGM file (e.g. `Stage1_BGM.wav`) in `Assets/_Project/Audio/Music/Stage1/`.

### 2. MusicManager component

- **Location:** `Assets/_Project/Scripts/Systems/MusicManager.cs`
- **Behavior:** Subscribes to `GameFlowManager.OnPhaseChanged`. Plays BGM when phase is Battle or Overtime; stops on Victory or Defeat.
- **Inspector:** Assign the BGM clip to the `Stage1 BGM` field.

### 3. Scene setup

- **New scenes:** `ProjectSetup` menu "Commander Survival > 3. Full Setup" creates a MusicManager GameObject.
- **Existing scenes:** Use "Commander Survival > 4. Add MusicManager to Scene" to add it.

## Steps to Use BGM

1. **BGM file**  
   - Primary: Assign in Inspector on MusicManager (drag clip to **Stage 1 BGM**).  
   - Fallback: Place `Stage1_BGM.mp3` in `Assets/Resources/Audio/` — MusicManager auto-loads it if Inspector field is empty.

2. **Add MusicManager to the scene** (if needed)  
   In Unity: **Commander Survival > 4. Add MusicManager to Scene**.

3. **Import settings for BGM** (recommended)  
   Select the BGM asset → Inspector:
   - Load Type: **Streaming**
   - Loop: **true**
   - Compression: **Vorbis**

## Flow

```
Drafting (no BGM)
    ↓ Start Battle
Battle (BGM plays, loops)
    ↓ Timer expires
Overtime (BGM continues)
    ↓ All dead OR Commander dead
Victory / Defeat (BGM stops)
```

## Future Extensions

- Volume control (e.g. via GameConstants or settings)
- Fade in/out on phase change
- Different BGM per stage (Hive, etc.)
- SFX hooks (commands, combat, death) in `Assets/_Project/Audio/SFX/`
