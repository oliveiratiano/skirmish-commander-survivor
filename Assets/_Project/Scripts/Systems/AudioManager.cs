using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    static AudioManager _instance;
    public static AudioManager Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("AudioManager");
                _instance = go.AddComponent<AudioManager>();
            }
            return _instance;
        }
    }

    // Loaded at startup from Resources/Audio/SFX/ by naming convention.
    // See docs/audio-file-conventions.md for folder layout and file naming rules.
    AudioClip[] _attackShoutClips;
    AudioClip[] _standGroundShoutClips;
    AudioClip[] _regroupShoutClips;
    AudioClip[] _followShoutClips;

    AudioClip[] _responseTier1Clips;
    AudioClip[] _responseTier2Clips;
    AudioClip[] _responseTier3Clips;

    Dictionary<string, AudioClip[]> _deathClipsByUnit = new Dictionary<string, AudioClip[]>();
    AudioClip[] _hitPlayerClips;
    AudioClip[] _hitEnemyClips;
    AudioClip[] _burstNoTargetClips;
    AudioClip[] _burstReloadClips;
    AudioClip[] _commanderShotClips;

    // _uiSource: 2D, for command shouts and unit responses (not positional)
    AudioSource _uiSource;
    // _spatialSource: 2D, for death and impact sounds (kept separate so pitch changes don't affect UI)
    AudioSource _spatialSource;

    float _lastEnemyDeathSoundTime = -999f;
    float _lastHitSoundTime = -999f;

    void Awake()
    {
        if (_instance != null && _instance != this) { Destroy(gameObject); return; }
        _instance = this;

        var sources = GetComponents<AudioSource>();
        _uiSource     = sources.Length > 0 ? sources[0] : gameObject.AddComponent<AudioSource>();
        _spatialSource = sources.Length > 1 ? sources[1] : gameObject.AddComponent<AudioSource>();

        _uiSource.spatialBlend = 0f;
        _uiSource.playOnAwake  = false;
        _uiSource.loop         = false;

        _spatialSource.spatialBlend  = 0f;
        _spatialSource.playOnAwake   = false;
        _spatialSource.loop          = false;

        LoadAllClips();
    }

    void OnDestroy()
    {
        if (_instance == this)
            _instance = null;
    }

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    public void PlayCommandShout(CommandState state)
    {
        Play2D(ClipsForCommand(state), GameConstants.COMMAND_AUDIO_PITCH_VARIANCE, GameConstants.COMMAND_AUDIO_VOLUME);
    }

    public void PlayUnitResponse(int receiverCount)
    {
        Play2D(ResponseTierClips(receiverCount), GameConstants.COMMAND_AUDIO_PITCH_VARIANCE, GameConstants.COMMAND_AUDIO_VOLUME);
    }

    // Called by UnitAIController.HandleDeath() — plays from AudioManager's own source
    // so the clip is not cut off when the unit's GameObject is deactivated.
    // Each unit type gets its own death clips loaded by naming convention:
    //   Audio/SFX/Deaths/death_{sanitized_unit_name}_{0,1,...}
    public void PlayDeathSound(bool isEnemy, string unitName, Vector3 position)
    {
        if (isEnemy && Time.time - _lastEnemyDeathSoundTime < GameConstants.ENEMY_DEATH_SOUND_COOLDOWN)
            return;

        AudioClip clip = PickRandom(GetDeathClips(unitName));
        if (clip == null) return;

        PlaySpatial(clip, position, GameConstants.SHOT_AUDIO_PITCH_VARIANCE);

        if (isEnemy)
            _lastEnemyDeathSoundTime = Time.time;
    }

    public void PlayBurstNoTarget()
    {
        Play2D(_burstNoTargetClips, 0.05f, 0.8f);
    }

    public void PlayBurstShot()
    {
        Play2D(_commanderShotClips, GameConstants.SHOT_AUDIO_PITCH_VARIANCE, 0.9f);
    }

    public void PlayBurstReload()
    {
        Play2D(_burstReloadClips, 0.06f, 0.5f);
    }

    public void PlayHitSound(bool isEnemy, Vector3 position)
    {
        if (Time.time - _lastHitSoundTime < GameConstants.HIT_SOUND_COOLDOWN)
            return;

        AudioClip clip = PickRandom(isEnemy ? _hitEnemyClips : _hitPlayerClips);
        if (clip == null) return;
        float volume = isEnemy ? 0.7f : 1f;
        PlaySpatial(clip, position, GameConstants.SHOT_AUDIO_PITCH_VARIANCE, volume);
        _lastHitSoundTime = Time.time;
    }

    // -------------------------------------------------------------------------
    // Internal helpers
    // -------------------------------------------------------------------------

    void LoadAllClips()
    {
        _attackShoutClips      = LoadClips("Audio/SFX/Commands/command_attack");
        _standGroundShoutClips = LoadClips("Audio/SFX/Commands/command_standground");
        _regroupShoutClips     = LoadClips("Audio/SFX/Commands/command_regroup");
        _followShoutClips      = LoadClips("Audio/SFX/Commands/command_follow");

        _responseTier1Clips = LoadClips("Audio/SFX/Responses/response_tier1");
        _responseTier2Clips = LoadClips("Audio/SFX/Responses/response_tier2");
        _responseTier3Clips = LoadClips("Audio/SFX/Responses/response_tier3");

        _hitPlayerClips = LoadClips("Audio/SFX/Hits/hit_player");
        _hitEnemyClips  = LoadClips("Audio/SFX/Hits/hit_enemy");

        _burstNoTargetClips = LoadClips("Audio/SFX/Skills/skill_burst_notarget");
        _burstReloadClips = LoadClips("Audio/SFX/Skills/skill_burst_reload");
        _commanderShotClips = LoadClips("Audio/SFX/Weapons/shot_commander");
    }

    // Loads Audio/SFX/.../name_0, name_1, ... until Resources.Load returns null.
    static AudioClip[] LoadClips(string basePath)
    {
        var list = new List<AudioClip>();
        for (int i = 0; ; i++)
        {
            var clip = Resources.Load<AudioClip>($"{basePath}_{i}");
            if (clip == null) break;
            list.Add(clip);
        }
        return list.ToArray();
    }

    AudioClip[] GetDeathClips(string unitName)
    {
        if (string.IsNullOrEmpty(unitName)) return null;
        string key = SanitizeName(unitName);
        if (_deathClipsByUnit.TryGetValue(key, out var cached)) return cached;
        var clips = LoadClips($"Audio/SFX/Deaths/death_{key}");
        _deathClipsByUnit[key] = clips;
        return clips;
    }

    static string SanitizeName(string name)
    {
        return name.ToLower().Replace(" ", "_").Replace("-", "_");
    }

    AudioClip[] ClipsForCommand(CommandState state)
    {
        switch (state)
        {
            case CommandState.Attack:      return _attackShoutClips;
            case CommandState.StandGround: return _standGroundShoutClips;
            case CommandState.Regroup:     return _regroupShoutClips;
            case CommandState.Follow:      return _followShoutClips;
            default:                       return null;
        }
    }

    AudioClip[] ResponseTierClips(int receiverCount)
    {
        if (receiverCount < GameConstants.RESPONSE_TIER_1_THRESHOLD) return _responseTier1Clips;
        if (receiverCount < GameConstants.RESPONSE_TIER_2_THRESHOLD) return _responseTier2Clips;
        return _responseTier3Clips;
    }

    void Play2D(AudioClip[] clips, float pitchVariance, float volume)
    {
        AudioClip clip = PickRandom(clips);
        if (clip == null) return;
        _uiSource.pitch = 1f + Random.Range(-pitchVariance, pitchVariance);
        _uiSource.PlayOneShot(clip, volume);
    }

    void PlaySpatial(AudioClip clip, Vector3 position, float pitchVariance, float volume = 1f)
    {
        if (CommanderController.Instance != null)
        {
            float sqrDist = (position - CommanderController.Instance.transform.position).sqrMagnitude;
            if (sqrDist > GameConstants.SHOT_AUDIO_MAX_DISTANCE * GameConstants.SHOT_AUDIO_MAX_DISTANCE) return;
        }

        _spatialSource.pitch = 1f + Random.Range(-pitchVariance, pitchVariance);
        _spatialSource.PlayOneShot(clip, volume);
    }

    static AudioClip PickRandom(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0) return null;
        return clips[Random.Range(0, clips.Length)];
    }
}
