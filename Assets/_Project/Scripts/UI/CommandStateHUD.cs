using UnityEngine;

public class CommandStateHUD : MonoBehaviour
{
    GUIStyle _style;
    GUIStyle _activeStyle;

    CommandState? _highlightedState;
    float _highlightEndTime;

    // Layout matches arrow keys: Left=Follow, Up=StandGround, Down=Regroup, Right=Attack
    readonly (CommandState state, string label, Color color)[] _slots = {
        (CommandState.Follow, "FOLLOW [←]", new Color(0.2f, 0.7f, 0.9f)),
        (CommandState.StandGround, "STAND GROUND [↑]", new Color(0.2f, 0.8f, 0.3f)),
        (CommandState.Regroup, "REGROUP [↓]", new Color(0.9f, 0.8f, 0.2f)),
        (CommandState.Attack, "ATTACK [→]", new Color(0.9f, 0.3f, 0.2f))
    };

    const float ButtonWidth = 144f;
    const float ButtonHeight = 32f;
    const float CenterButtonWidth = 200f; // Wider for "STAND GROUND [↑]"
    const float HorizontalGap = 16f;
    const float VerticalGap = 8f;

    void OnEnable()
    {
        SubscribeToCommandSystem();
    }

    void Start()
    {
        SubscribeToCommandSystem();
    }

    void OnDisable()
    {
        if (CommandSystem.Instance != null)
            CommandSystem.Instance.OnStateChanged -= OnCommandIssued;
    }

    void SubscribeToCommandSystem()
    {
        if (CommandSystem.Instance != null)
        {
            CommandSystem.Instance.OnStateChanged -= OnCommandIssued;
            CommandSystem.Instance.OnStateChanged += OnCommandIssued;
        }
    }

    void OnCommandIssued(CommandState state)
    {
        _highlightedState = state;
        _highlightEndTime = Time.time + 1f;
    }

    void OnGUI()
    {
        if (CommandSystem.Instance == null) return;

        InitStyles();

        if (Time.time >= _highlightEndTime)
            _highlightedState = null;

        float centerX = Screen.width / 2f;
        float centerY = Screen.height - 60f;

        // Cross layout: Left, Top, Bottom, Right — no overlap, evenly spaced
        float leftX = centerX - ButtonWidth - HorizontalGap - CenterButtonWidth / 2f;
        float centerColX = centerX - CenterButtonWidth / 2f;
        float rightX = centerX + CenterButtonWidth / 2f + HorizontalGap;

        float[] xPos = { leftX, centerColX, centerColX, rightX };
        float[] width = { ButtonWidth, CenterButtonWidth, CenterButtonWidth, ButtonWidth };
        float[] yPos = {
            centerY - ButtonHeight / 2f,
            centerY - ButtonHeight - VerticalGap,
            centerY + VerticalGap,
            centerY - ButtonHeight / 2f
        };

        for (int i = 0; i < _slots.Length; i++)
        {
            var (state, label, color) = _slots[i];
            bool isHighlighted = _highlightedState == state;

            GUIStyle s = isHighlighted ? _activeStyle : _style;
            s.normal.textColor = isHighlighted ? color : new Color(0.5f, 0.5f, 0.5f);

            GUI.Label(new Rect(xPos[i], yPos[i], width[i], ButtonHeight), label, s);
        }
    }

    void InitStyles()
    {
        if (_style != null) return;

        _style = new GUIStyle(GUI.skin.box);
        _style.fontSize = 14;
        _style.alignment = TextAnchor.MiddleCenter;

        _activeStyle = new GUIStyle(_style);
        _activeStyle.fontStyle = FontStyle.Bold;
        _activeStyle.fontSize = 16;
    }
}
