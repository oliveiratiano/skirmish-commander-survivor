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

        const float leftMargin = 20f;
        const float lineHeight = 28f;
        float y = 10f;

        // Timer
        if (phase == GamePhase.Battle)
        {
            float t = GameFlowManager.Instance.Timer;
            int minutes = Mathf.FloorToInt(t / 60f);
            int seconds = Mathf.FloorToInt(t % 60f);
            string timerText = $"{minutes:00}:{seconds:00}";
            _timerStyle.normal.textColor = t < 30f ? Color.red : Color.white;
            GUI.Label(new Rect(leftMargin, y, 120f, 40f), timerText, _timerStyle);
            y += lineHeight + 8f;
        }
        else if (phase == GamePhase.Overtime)
        {
            float ot = GameFlowManager.Instance.OvertimeTimer;
            string otText = $"OVERTIME +{ot:F0}s";
            _timerStyle.normal.textColor = Color.red;
            GUI.Label(new Rect(leftMargin, y, 160f, 40f), otText, _timerStyle);
            y += lineHeight + 8f;
        }

        // Commander HP
        if (CommanderController.Instance != null)
        {
            var health = CommanderController.Instance.GetComponent<HealthComponent>();
            if (health != null)
            {
                string hpText = $"HP: {Mathf.CeilToInt(health.CurrentHP)} / {health.maxHP}";
                GUI.Label(new Rect(leftMargin, y, 200f, 25f), hpText, _infoStyle);
                y += lineHeight;
            }
        }

        // Enemy count
        if (WaveManager.Instance != null)
        {
            string enemyText = $"Enemies: {WaveManager.Instance.AliveCount} | Remaining: {WaveManager.Instance.RemainingToSpawn}";
            GUI.Label(new Rect(leftMargin, y, 280f, 25f), enemyText, _infoStyle);
        }
    }

    void InitStyles()
    {
        if (_timerStyle != null) return;

        _timerStyle = new GUIStyle(GUI.skin.label);
        _timerStyle.fontSize = 28;
        _timerStyle.fontStyle = FontStyle.Bold;
        _timerStyle.alignment = TextAnchor.UpperLeft;
        _timerStyle.normal.textColor = Color.white;

        _infoStyle = new GUIStyle(GUI.skin.label);
        _infoStyle.fontSize = 14;
        _infoStyle.alignment = TextAnchor.UpperLeft;
        _infoStyle.normal.textColor = Color.white;
    }
}
