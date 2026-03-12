using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    GUIStyle _titleStyle;
    GUIStyle _labelStyle;
    GUIStyle _buttonStyle;
    GUIStyle _boxStyle;

    void OnEnable()
    {
        if (GameFlowManager.Instance != null)
            GameFlowManager.Instance.OnPhaseChanged += OnPhaseChanged;
    }

    void OnDisable()
    {
        if (GameFlowManager.Instance != null)
            GameFlowManager.Instance.OnPhaseChanged -= OnPhaseChanged;
    }

    void OnPhaseChanged(GamePhase phase) { }

    void OnGUI()
    {
        if (GameFlowManager.Instance == null) return;

        GamePhase phase = GameFlowManager.Instance.CurrentPhase;
        if (phase != GamePhase.Victory && phase != GamePhase.Defeat) return;

        InitStyles();

        float panelW = 400f;
        float panelH = 250f;
        float x = (Screen.width - panelW) / 2f;
        float y = (Screen.height - panelH) / 2f;

        GUI.Box(new Rect(x, y, panelW, panelH), "", _boxStyle);

        float cy = y + 20f;

        string title = phase == GamePhase.Victory ? "VICTORY" : "DEFEAT";
        Color titleColor = phase == GamePhase.Victory ? Color.green : Color.red;
        _titleStyle.normal.textColor = titleColor;
        GUI.Label(new Rect(x, cy, panelW, 40f), title, _titleStyle);
        cy += 50f;

        int kills = WaveManager.Instance != null ? WaveManager.Instance.KilledCount : 0;
        float score = GameFlowManager.Instance.Score;
        float overtime = GameFlowManager.Instance.OvertimeTimer;

        GUI.Label(new Rect(x + 40f, cy, panelW - 80f, 25f), $"Enemies Killed: {kills}", _labelStyle);
        cy += 30f;
        GUI.Label(new Rect(x + 40f, cy, panelW - 80f, 25f), $"Score: {Mathf.Max(0, Mathf.RoundToInt(score))}", _labelStyle);
        cy += 30f;
        if (overtime > 0f)
        {
            GUI.Label(new Rect(x + 40f, cy, panelW - 80f, 25f), $"Overtime: {overtime:F1}s", _labelStyle);
            cy += 30f;
        }

        cy += 10f;

        if (GUI.Button(new Rect(x + 100f, cy, 200f, 40f), "RESTART", _buttonStyle))
        {
            Restart();
        }
    }

    void Restart()
    {
        UnitAIController.AllPlayerUnits.Clear();
        UnitAIController.AllEnemyUnits.Clear();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void InitStyles()
    {
        if (_titleStyle != null) return;

        _boxStyle = new GUIStyle(GUI.skin.box);
        _boxStyle.normal.background = MakeTex(2, 2, new Color(0.05f, 0.05f, 0.05f, 0.95f));

        _titleStyle = new GUIStyle(GUI.skin.label);
        _titleStyle.fontSize = 32;
        _titleStyle.fontStyle = FontStyle.Bold;
        _titleStyle.alignment = TextAnchor.MiddleCenter;

        _labelStyle = new GUIStyle(GUI.skin.label);
        _labelStyle.fontSize = 18;
        _labelStyle.normal.textColor = Color.white;

        _buttonStyle = new GUIStyle(GUI.skin.button);
        _buttonStyle.fontSize = 18;
        _buttonStyle.normal.textColor = Color.white;
    }

    Texture2D MakeTex(int w, int h, Color col)
    {
        Color[] pix = new Color[w * h];
        for (int i = 0; i < pix.Length; i++) pix[i] = col;
        Texture2D tex = new Texture2D(w, h);
        tex.SetPixels(pix);
        tex.Apply();
        return tex;
    }
}
