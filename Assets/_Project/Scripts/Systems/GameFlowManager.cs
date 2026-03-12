using System;
using UnityEngine;

public enum GamePhase
{
    Drafting,
    Battle,
    Overtime,
    Victory,
    Defeat
}

public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager Instance { get; private set; }

    public GamePhase CurrentPhase { get; private set; } = GamePhase.Drafting;

    [Header("Timing")]
    public float waveDuration = GameConstants.WAVE_DURATION;

    float _timer;
    float _overtimeTimer;
    float _score;
    float _overtimePenaltyPerSecond = 10f;
    float _baseScorePerKill = 100f;

    public float Timer => _timer;
    public float OvertimeTimer => _overtimeTimer;
    public float Score => _score;

    public event Action<GamePhase> OnPhaseChanged;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void StartBattle()
    {
        CurrentPhase = GamePhase.Battle;
        _timer = waveDuration;
        _overtimeTimer = 0f;
        _score = 0f;
        OnPhaseChanged?.Invoke(CurrentPhase);

        if (WaveManager.Instance != null)
            WaveManager.Instance.StartWave();
    }

    void Update()
    {
        if (CurrentPhase == GamePhase.Battle)
            UpdateBattle();
        else if (CurrentPhase == GamePhase.Overtime)
            UpdateOvertime();
    }

    void UpdateBattle()
    {
        _timer -= Time.deltaTime;

        CheckCommanderDeath();
        TrackKillScore();

        if (WaveManager.Instance != null && WaveManager.Instance.AllDead)
        {
            EndWithVictory();
            return;
        }

        if (_timer <= 0f)
        {
            _timer = 0f;
            if (WaveManager.Instance != null && !WaveManager.Instance.AllDead)
            {
                CurrentPhase = GamePhase.Overtime;
                OnPhaseChanged?.Invoke(CurrentPhase);
            }
            else
            {
                EndWithVictory();
            }
        }
    }

    void UpdateOvertime()
    {
        _overtimeTimer += Time.deltaTime;
        _score -= _overtimePenaltyPerSecond * Time.deltaTime;

        CheckCommanderDeath();
        TrackKillScore();

        if (WaveManager.Instance != null && WaveManager.Instance.AllDead)
            EndWithVictory();
    }

    int _lastTrackedKills;

    void TrackKillScore()
    {
        if (WaveManager.Instance == null) return;

        int kills = WaveManager.Instance.KilledCount;
        int newKills = kills - _lastTrackedKills;
        if (newKills > 0)
        {
            _score += newKills * _baseScorePerKill;
            _lastTrackedKills = kills;
        }
    }

    void CheckCommanderDeath()
    {
        if (CommanderController.Instance == null) return;
        var health = CommanderController.Instance.GetComponent<HealthComponent>();
        if (health != null && health.IsDead)
            EndWithDefeat();
    }

    void EndWithVictory()
    {
        CurrentPhase = GamePhase.Victory;
        _score = Mathf.Max(0f, _score);
        if (WaveManager.Instance != null)
            WaveManager.Instance.StopWave();
        OnPhaseChanged?.Invoke(CurrentPhase);
    }

    void EndWithDefeat()
    {
        CurrentPhase = GamePhase.Defeat;
        if (WaveManager.Instance != null)
            WaveManager.Instance.StopWave();
        OnPhaseChanged?.Invoke(CurrentPhase);
    }
}
