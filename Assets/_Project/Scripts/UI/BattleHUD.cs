using UnityEngine;

public class BattleHUD : MonoBehaviour
{
    GUIStyle _timerStyle;
    GUIStyle _infoStyle;
    GUIStyle _ammoStyle;
    HealthComponent _commanderHealth;

    void OnEnable()
    {
        RefreshCommanderHealthCache();
    }

    void RefreshCommanderHealthCache()
    {
        _commanderHealth = CommanderController.Instance != null
            ? CommanderController.Instance.GetComponent<HealthComponent>()
            : null;
    }

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
        if (CommanderController.Instance == null)
            _commanderHealth = null;
        else
        {
            if (_commanderHealth == null)
                RefreshCommanderHealthCache();
            if (_commanderHealth != null)
            {
                string hpText = $"HP: {Mathf.CeilToInt(_commanderHealth.CurrentHP)} / {_commanderHealth.maxHP}";
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

        DrawAmmoIndicator();
    }

    void DrawAmmoIndicator()
    {
        if (GameManager.Instance == null || GameManager.Instance.CommanderObject == null) return;
        var burst = GameManager.Instance.CommanderObject.GetComponent<CommanderBurstAttack>();
        if (burst == null) return;

        int ammo = burst.CurrentAmmo;
        int max = burst.MaxAmmo;
        if (max <= 0) return;

        bool isReloading = ammo < max && burst.CanReloadNow;

        // Position: left of command HUD, vertically centered
        const float stickW = 6f;
        const float stickH = 20f;
        const float gap = 3f;
        const float pad = 8f;
        float sticksW = max * stickW + (max - 1) * gap;
        float boxW = sticksW + pad * 2f;
        float boxH = stickH + pad * 2f + 18f;
        float cmdLeftX = Screen.width / 2f - 260f;
        float x = cmdLeftX - boxW - 16f;
        float centerY = Screen.height - 60f;
        float y = centerY - boxH / 2f;

        // Label
        string label = isReloading ? "BURST [SPACE] ..." : "BURST [SPACE]";
        GUI.Label(new Rect(x, y, boxW, 18f), label, _ammoStyle);

        // Translucent border box (same style as command buttons)
        float borderY = y + 18f;
        float borderH = stickH + pad * 2f;
        GUI.Box(new Rect(x, borderY, boxW, borderH), "", GUI.skin.box);

        // Sticks inside the border
        float stickX = x + pad;
        float stickY = borderY + pad;
        Color oldColor = GUI.color;
        Color loaded = new Color(0.5f, 0.5f, 0.5f);
        Color empty = new Color(0.5f, 0.5f, 0.5f, 0.2f);

        for (int i = 0; i < max; i++)
        {
            float sx = stickX + i * (stickW + gap);

            if (i < ammo)
            {
                GUI.color = loaded;
                GUI.DrawTexture(new Rect(sx, stickY, stickW, stickH), Texture2D.whiteTexture);
            }
            else if (i == ammo && isReloading)
            {
                // Empty background
                GUI.color = empty;
                GUI.DrawTexture(new Rect(sx, stickY, stickW, stickH), Texture2D.whiteTexture);
                // Fill from bottom up based on reload progress
                float progress = burst.ReloadProgress;
                float fillH = stickH * progress;
                GUI.color = loaded;
                GUI.DrawTexture(new Rect(sx, stickY + stickH - fillH, stickW, fillH), Texture2D.whiteTexture);
            }
            else
            {
                GUI.color = empty;
                GUI.DrawTexture(new Rect(sx, stickY, stickW, stickH), Texture2D.whiteTexture);
            }
        }

        GUI.color = oldColor;
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

        _ammoStyle = new GUIStyle(GUI.skin.label);
        _ammoStyle.fontSize = 12;
        _ammoStyle.alignment = TextAnchor.UpperLeft;
        _ammoStyle.normal.textColor = new Color(0.6f, 0.6f, 0.6f);
    }
}
