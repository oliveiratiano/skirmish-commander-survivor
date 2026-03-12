using UnityEngine;

public class BattleHUD : MonoBehaviour
{
    GUIStyle _timerStyle;
    GUIStyle _infoStyle;

    void OnGUI()
    {
        if (GameFlowManager.Instance == null) return;

        var phase = GameFlowManager.Instance.CurrentPhase;
        if (phase != GamePhase.Battle && phase != GamePhase.Overtime) return;

        InitStyles();

        // Timer
        if (phase == GamePhase.Battle)
        {
            float t = GameFlowManager.Instance.Timer;
            int minutes = Mathf.FloorToInt(t / 60f);
            int seconds = Mathf.FloorToInt(t % 60f);
            string timerText = $"{minutes:00}:{seconds:00}";
            _timerStyle.normal.textColor = t < 30f ? Color.red : Color.white;
            GUI.Label(new Rect(Screen.width / 2f - 60f, 10f, 120f, 40f), timerText, _timerStyle);
        }
        else if (phase == GamePhase.Overtime)
        {
            float ot = GameFlowManager.Instance.OvertimeTimer;
            string otText = $"OVERTIME +{ot:F0}s";
            _timerStyle.normal.textColor = Color.red;
            GUI.Label(new Rect(Screen.width / 2f - 80f, 10f, 160f, 40f), otText, _timerStyle);
        }

        // Commander HP
        if (CommanderController.Instance != null)
        {
            var health = CommanderController.Instance.GetComponent<HealthComponent>();
            if (health != null)
            {
                string hpText = $"HP: {Mathf.CeilToInt(health.CurrentHP)} / {health.maxHP}";
                GUI.Label(new Rect(Screen.width / 2f - 60f, 55f, 120f, 25f), hpText, _infoStyle);
            }
        }

        // Enemy count
        if (WaveManager.Instance != null)
        {
            string enemyText = $"Enemies: {WaveManager.Instance.AliveCount} | Remaining: {WaveManager.Instance.RemainingToSpawn}";
            GUI.Label(new Rect(Screen.width / 2f - 120f, 80f, 240f, 25f), enemyText, _infoStyle);
        }
    }

    void InitStyles()
    {
        if (_timerStyle != null) return;

        _timerStyle = new GUIStyle(GUI.skin.label);
        _timerStyle.fontSize = 28;
        _timerStyle.fontStyle = FontStyle.Bold;
        _timerStyle.alignment = TextAnchor.MiddleCenter;
        _timerStyle.normal.textColor = Color.white;

        _infoStyle = new GUIStyle(GUI.skin.label);
        _infoStyle.fontSize = 14;
        _infoStyle.alignment = TextAnchor.MiddleCenter;
        _infoStyle.normal.textColor = Color.white;
    }
}
