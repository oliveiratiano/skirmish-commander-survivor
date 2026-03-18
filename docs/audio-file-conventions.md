# Audio File Conventions

All audio files loaded at runtime must live under `Assets/Resources/` so Unity's
`Resources.Load<AudioClip>()` can find them. The `AudioManager` and `MusicManager`
systems load clips by name automatically — no Inspector wiring required.

---

## Folder Structure

```
Assets/Resources/Audio/
  Stage1_BGM.mp3              ← battle background music (MusicManager)
  SFX/
    Commands/                 ← Commander shout per command type
      command_attack_0.wav
      command_standground_0.wav
      command_regroup_0.wav
      command_follow_0.wav
    Responses/                ← Unit acknowledgement, tiered by group size
      response_tier1_0.wav    ← few units  (< RESPONSE_TIER_1_THRESHOLD = 4)
      response_tier2_0.wav    ← medium     (4 – 8 units)
      response_tier3_0.wav    ← large      (>= RESPONSE_TIER_2_THRESHOLD = 9)
    Weapons/                  ← Gunshot per unit type (keyed by UnitData.unitName)
      shot_commander_0.wav
      shot_close_quarters_0.wav
      shot_machine_gunner_0.wav
      shot_sharpshooter_0.wav
      shot_swarm_bug_0.wav
      shot_swarm_bug_boss_0.wav
      shot_swarm_bug_escort_0.wav
    Deaths/                   ← Unit death, split by side
      death_player_0.wav
      death_enemy_0.wav
    Hits/                     ← Unit taking damage, split by side
      hit_player_0.wav
      hit_enemy_0.wav
```

---

## Naming Rule

```
{category}_{variant}_{index}.wav
```

- **category** — matches the subfolder name (e.g. `command_attack`, `response_tier2`)
- **variant** — differentiates the clip within the category (see table below)
- **index** — zero-based integer; `AudioManager.LoadClips()` probes `_0`, `_1`, `_2`, …
  until `Resources.Load` returns null, so adding a new variant requires no code changes

**To add a variant:** drop `command_attack_1.wav` in the Commands folder. Done.

**Silent failure rule:** if no file is found for a given base path, `AudioManager`
returns an empty array and the event plays silently. This is intentional — missing
clips never cause errors, only missing sound.

---

## Category Reference

| Base path (relative to `Audio/`) | Trigger | Source | Notes |
|---|---|---|---|
| `SFX/Commands/command_attack` | Player issues Attack command | `CommandSystem.SetState` | 2D, played once per broadcast |
| `SFX/Commands/command_standground` | Player issues Stand Ground command | `CommandSystem.SetState` | 2D, played once per broadcast |
| `SFX/Commands/command_regroup` | Player issues Regroup command | `CommandSystem.SetState` | 2D, played once per broadcast |
| `SFX/Commands/command_follow` | Player issues Follow command | `CommandSystem.SetState` | 2D, played once per broadcast |
| `SFX/Responses/response_tier1` | 1–3 units received a command | `CommandSystem.SetState` | 2D, one clip per event |
| `SFX/Responses/response_tier2` | 4–8 units received a command | `CommandSystem.SetState` | 2D, one clip per event |
| `SFX/Responses/response_tier3` | 9+ units received a command | `CommandSystem.SetState` | 2D, one clip per event |
| `SFX/Weapons/shot_{unitname}` | Unit fires a shot | `RangedAttackComponent` | 3D spatial, distance-culled beyond `SHOT_AUDIO_MAX_DISTANCE`; `{unitname}` = `UnitData.unitName` lowercased with spaces and hyphens replaced by `_` |
| `SFX/Deaths/death_player` | A player unit dies | `UnitAIController.HandleDeath` | 3D spatial, always plays |
| `SFX/Deaths/death_enemy` | An enemy unit dies | `UnitAIController.HandleDeath` | 3D spatial, throttled (`ENEMY_DEATH_SOUND_COOLDOWN`) |
| `SFX/Hits/hit_player` | A player unit takes damage | `HealthComponent.TakeDamage` | 3D spatial, per-unit cooldown (`HIT_SOUND_COOLDOWN`) |
| `SFX/Hits/hit_enemy` | An enemy unit takes damage | `HealthComponent.TakeDamage` | 3D spatial, per-unit cooldown (`HIT_SOUND_COOLDOWN`) |
| `Audio/Stage1_BGM` | Battle / Overtime phase starts | `MusicManager` | Music, loops, no index suffix |

---

## Weapon Shot Files — Full Unit Reference

Filename derived from `UnitData.unitName`: lowercased, spaces and hyphens replaced by `_`.

| UnitData.unitName | Derived filename (index 0) | Side |
|---|---|---|
| `Commander` | `shot_commander_0.wav` | Player |
| `Close-Quarters` | `shot_close_quarters_0.wav` | Player |
| `Machine Gunner` | `shot_machine_gunner_0.wav` | Player |
| `Sharpshooter` | `shot_sharpshooter_0.wav` | Player |
| `Swarm Bug` | `shot_swarm_bug_0.wav` | Enemy |
| `Swarm Bug Boss` | `shot_swarm_bug_boss_0.wav` | Enemy |
| `Swarm Bug Escort` | `shot_swarm_bug_escort_0.wav` | Enemy |

To add a second variant for any unit, drop a `_1` file alongside the `_0` — e.g. `shot_commander_1.wav`.

---

## Audio Format Recommendations

- **Format:** WAV (16-bit PCM) for SFX; MP3 acceptable for music
- **Sample rate:** 44 100 Hz
- **Channels:** Mono for all SFX (Unity spatialises at runtime); Stereo for music
- **Normalisation:** Target –3 dBFS peak; leave headroom for pitch variation

---

## Runtime Constants (defined in `GameConstants.cs`)

| Constant | Value | Effect |
|---|---|---|
| `COMMAND_AUDIO_VOLUME` | 1.0 | Master volume for shout and response clips |
| `COMMAND_AUDIO_PITCH_VARIANCE` | ±0.05 | Pitch variation applied to shout/response playback |
| `SHOT_AUDIO_PITCH_VARIANCE` | ±0.08 | Pitch variation applied to death and hit clips |
| `SHOT_AUDIO_MAX_DISTANCE` | 25 units | Spatial source max rolloff distance |
| `ENEMY_DEATH_SOUND_COOLDOWN` | 0.08 s | Minimum gap between enemy death sounds (prevents swarm pile-up) |
| `HIT_SOUND_COOLDOWN` | 0.15 s | Minimum gap between hit sounds per unit |
| `RESPONSE_TIER_1_THRESHOLD` | 4 | Receiver count below which tier 1 response plays |
| `RESPONSE_TIER_2_THRESHOLD` | 9 | Receiver count below which tier 2 response plays |
