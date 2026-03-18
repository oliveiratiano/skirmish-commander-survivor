using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    // Loaded at startup from Resources/Audio/SFX/ by naming convention.
    // See docs/audio-file-conventions.md for folder layout and file naming rules.
    AudioClip[] _attackShoutClips;
    AudioClip[] _standGroundShoutClips;
    AudioClip[] _regroupShoutClips;
    AudioClip[] _followShoutClips;

    AudioClip[] _responseTier1Clips;
    AudioClip[] _responseTier2Clips;
    AudioClip[] _responseTier3Clips;

    AudioClip[] _deathPlayerClips;
    AudioClip[] _deathEnemyClips;
    AudioClip[] _hitPlayerClips;
    AudioClip[] _hitEnemyClips;

    // _uiSource: 2D, for command shouts and unit responses (not positional)
    AudioSource _uiSource;
    // _spatialSource: 3D, repositioned per call, for death and impact sounds
    AudioSource _spatialSource;

    float _lastEnemyDeathSoundTime = -999f;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        var sources = GetComponents<AudioSource>();
        _uiSource     = sources.Length > 0 ? sources[0] : gameObject.AddComponent<AudioSource>();
        _spatialSource = sources.Length > 1 ? sources[1] : gameObject.AddComponent<AudioSource>();

        _uiSource.spatialBlend = 0f;
        _uiSource.playOnAwake  = false;
        _uiSource.loop         = false;

        _spatialSource.spatialBlend  = 1f;
        _spatialSource.rolloffMode   = AudioRolloffMode.Linear;
        _spatialSource.minDistance   = 3f;
        _spatialSource.maxDistance   = GameConstants.SHOT_AUDIO_MAX_DISTANCE;
        _spatialSource.playOnAwake   = false;
        _spatialSource.loop          = false;

        LoadAllClips();
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
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
    public void PlayDeathSound(bool isEnemy, Vector3 position)
    {
        if (isEnemy && Time.time - _lastEnemyDeathSoundTime < GameConstants.ENEMY_DEATH_SOUND_COOLDOWN)
            return;

        AudioClip clip = PickRandom(isEnemy ? _deathEnemyClips : _deathPlayerClips);
        if (clip == null) return;

        PlaySpatial(clip, position, GameConstants.SHOT_AUDIO_PITCH_VARIANCE);

        if (isEnemy)
            _lastEnemyDeathSoundTime = Time.time;
    }

    public void PlayHitSound(bool isEnemy, Vector3 position)
    {
        AudioClip clip = PickRandom(isEnemy ? _hitEnemyClips : _hitPlayerClips);
        if (clip == null) return;
        PlaySpatial(clip, position, GameConstants.SHOT_AUDIO_PITCH_VARIANCE);
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

        _deathPlayerClips = LoadClips("Audio/SFX/Deaths/death_player");
        _deathEnemyClips  = LoadClips("Audio/SFX/Deaths/death_enemy");

        _hitPlayerClips = LoadClips("Audio/SFX/Hits/hit_player");
        _hitEnemyClips  = LoadClips("Audio/SFX/Hits/hit_enemy");
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

    void PlaySpatial(AudioClip clip, Vector3 position, float pitchVariance)
    {
        _spatialSource.transform.position = position;
        _spatialSource.pitch = 1f + Random.Range(-pitchVariance, pitchVariance);
        _spatialSource.PlayOneShot(clip);
    }

    static AudioClip PickRandom(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0) return null;
        return clips[Random.Range(0, clips.Length)];
    }
}
