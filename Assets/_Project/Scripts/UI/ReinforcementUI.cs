using UnityEngine;

public class ReinforcementUI : MonoBehaviour
{
    [Header("Available Units")]
    public UnitData[] availableUnits;

    bool _visible;
    int _remainingBudget;

    GUIStyle _boxStyle;
    GUIStyle _btnStyle;
    GUIStyle _lblStyle;

    public void SetBudget(int budget)
    {
        _remainingBudget = budget;
    }

    void Update()
    {
        if (GameFlowManager.Instance == null) return;
        var phase = GameFlowManager.Instance.CurrentPhase;
        if (phase != GamePhase.Battle && phase != GamePhase.Overtime)
        {
            _visible = false;
            return;
        }

        if (Input.GetKeyDown(KeyCode.Tab))
            _visible = !_visible;
    }

    void OnGUI()
    {
        if (!_visible || availableUnits == null || availableUnits.Length == 0) return;

        InitStyles();

        float panelW = 300f;
        float panelH = 40f + availableUnits.Length * 40f;
        float x = 10f;
        float y = Screen.height / 2f - panelH / 2f;

        GUI.Box(new Rect(x, y, panelW, panelH), "", _boxStyle);

        float cy = y + 10f;
        GUI.Label(new Rect(x + 10f, cy, panelW - 20f, 25f),
            $"Reinforce (Budget: {_remainingBudget})", _lblStyle);
        cy += 30f;

        for (int i = 0; i < availableUnits.Length; i++)
        {
            UnitData u = availableUnits[i];
            string label = $"{u.unitName} ({u.cost}pts)";

            if (_remainingBudget >= u.cost)
            {
                if (GUI.Button(new Rect(x + 10f, cy, panelW - 20f, 30f), label, _btnStyle))
                {
                    SpawnReinforcement(u);
                }
            }
            else
            {
                GUI.Label(new Rect(x + 10f, cy, panelW - 20f, 30f), label + " [insufficient]", _lblStyle);
            }

            cy += 35f;
        }
    }

    void SpawnReinforcement(UnitData data)
    {
        if (_remainingBudget < data.cost) return;
        _remainingBudget -= data.cost;

        if (CommanderController.Instance == null) return;

        Vector3 pos = CommanderController.Instance.transform.position;
        pos += new Vector3(Random.Range(-2f, 2f), Random.Range(-2f, 2f), 0f);

        if (UnitSpawner.Instance != null)
            UnitSpawner.Instance.SpawnPlayerUnit(data, pos);
    }

    void InitStyles()
    {
        if (_boxStyle != null) return;

        _boxStyle = new GUIStyle(GUI.skin.box);
        _boxStyle.normal.background = MakeTex(2, 2, new Color(0.1f, 0.1f, 0.1f, 0.9f));

        _btnStyle = new GUIStyle(GUI.skin.button);
        _btnStyle.fontSize = 14;

        _lblStyle = new GUIStyle(GUI.skin.label);
        _lblStyle.fontSize = 14;
        _lblStyle.normal.textColor = Color.white;
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
