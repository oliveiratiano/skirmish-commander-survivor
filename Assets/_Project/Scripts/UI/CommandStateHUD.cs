using UnityEngine;

public class CommandStateHUD : MonoBehaviour
{
    GUIStyle _style;
    GUIStyle _activeStyle;

    readonly string[] _stateNames = { "ATTACK [→]", "FORM UP [↑]", "REGROUP [↓]" };
    readonly Color[] _stateColors = { new Color(0.9f, 0.3f, 0.2f), new Color(0.2f, 0.7f, 0.9f), new Color(0.9f, 0.8f, 0.2f) };

    void OnGUI()
    {
        if (CommandSystem.Instance == null) return;

        InitStyles();

        CommandState current = CommandSystem.Instance.CurrentState;

        float y = Screen.height - 50f;
        float totalWidth = 180f * 3f + 20f;
        float startX = (Screen.width - totalWidth) / 2f;

        for (int i = 0; i < 3; i++)
        {
            CommandState state = (CommandState)i;
            bool isActive = current == state;

            GUIStyle s = isActive ? _activeStyle : _style;
            if (isActive)
                s.normal.textColor = _stateColors[i];
            else
                s.normal.textColor = new Color(0.5f, 0.5f, 0.5f);

            GUI.Label(new Rect(startX + i * 190f, y, 180f, 40f), _stateNames[i], isActive ? _activeStyle : _style);
        }
    }

    void InitStyles()
    {
        if (_style != null) return;

        _style = new GUIStyle(GUI.skin.box);
        _style.fontSize = 18;
        _style.alignment = TextAnchor.MiddleCenter;

        _activeStyle = new GUIStyle(_style);
        _activeStyle.fontStyle = FontStyle.Bold;
        _activeStyle.fontSize = 20;
    }
}
