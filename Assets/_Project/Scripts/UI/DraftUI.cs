using UnityEngine;

public class DraftUI : MonoBehaviour
{
    [Header("Available Units")]
    public UnitData[] availableUnits;

    int[] _counts;
    int _remainingBudget;
    bool _draftActive = true;

    GUIStyle _boxStyle;
    GUIStyle _buttonStyle;
    GUIStyle _labelStyle;

    void Awake()
    {
        _remainingBudget = GameConstants.DRAFT_BUDGET;
        if (availableUnits != null)
            _counts = new int[availableUnits.Length];
        else
            _counts = new int[0];
    }

    public int RemainingBudget => _remainingBudget;
    public bool IsDraftActive => _draftActive;

    void OnGUI()
    {
        if (!_draftActive) return;

        InitStyles();

        float panelW = 400f;
        float panelH = 80f + availableUnits.Length * 50f + 60f;
        float x = (Screen.width - panelW) / 2f;
        float y = (Screen.height - panelH) / 2f;

        GUI.Box(new Rect(x, y, panelW, panelH), "", _boxStyle);

        float cy = y + 15f;

        GUI.Label(new Rect(x + 20f, cy, panelW - 40f, 30f),
            $"DRAFT YOUR ARMY   |   Budget: {_remainingBudget}", _labelStyle);
        cy += 40f;

        for (int i = 0; i < availableUnits.Length; i++)
        {
            UnitData u = availableUnits[i];
            string label = $"{u.unitName} (Cost: {u.cost})";

            GUI.Label(new Rect(x + 20f, cy, 200f, 30f), label, _labelStyle);

            if (GUI.Button(new Rect(x + 230f, cy, 30f, 30f), "-", _buttonStyle))
            {
                if (_counts[i] > 0)
                {
                    _counts[i]--;
                    _remainingBudget += u.cost;
                }
            }

            GUI.Label(new Rect(x + 270f, cy, 40f, 30f), _counts[i].ToString(), _labelStyle);

            if (GUI.Button(new Rect(x + 310f, cy, 30f, 30f), "+", _buttonStyle))
            {
                if (_remainingBudget >= u.cost)
                {
                    _counts[i]++;
                    _remainingBudget -= u.cost;
                }
            }

            cy += 45f;
        }

        cy += 10f;

        if (GUI.Button(new Rect(x + 100f, cy, 200f, 40f), "START BATTLE", _buttonStyle))
        {
            StartBattle();
        }
    }

    void StartBattle()
    {
        _draftActive = false;

        GameManager.Instance.SpawnCommander();

        if (UnitSpawner.Instance != null)
        {
            UnitSpawner.Instance.SpawnDraftedArmy(
                availableUnits, _counts, Vector3.zero);
        }

        // Pass remaining budget to reinforcement UI
        var reinforcement = FindObjectOfType<ReinforcementUI>();
        if (reinforcement != null)
            reinforcement.SetBudget(_remainingBudget);

        // Start the battle via GameFlowManager
        if (GameFlowManager.Instance != null)
            GameFlowManager.Instance.StartBattle();
    }

    void InitStyles()
    {
        if (_boxStyle != null) return;

        _boxStyle = new GUIStyle(GUI.skin.box);
        _boxStyle.normal.background = MakeTex(2, 2, new Color(0.1f, 0.1f, 0.1f, 0.92f));

        _buttonStyle = new GUIStyle(GUI.skin.button);
        _buttonStyle.fontSize = 16;
        _buttonStyle.normal.textColor = Color.white;

        _labelStyle = new GUIStyle(GUI.skin.label);
        _labelStyle.fontSize = 16;
        _labelStyle.normal.textColor = Color.white;
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
