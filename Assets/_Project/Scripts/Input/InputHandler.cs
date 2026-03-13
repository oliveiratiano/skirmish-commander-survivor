using System;
using System.Collections.Generic;
using UnityEngine;

public class InputHandler : MonoBehaviour
{
    public static InputHandler Instance { get; private set; }

    public Vector2 MoveInput { get; private set; }

    public event Action<CommandState, bool> OnCommandStateChanged;
    public event Action OnDebugToggle;

    HashSet<int> _promptedTypeIndices = new HashSet<int>();
    float _promptTimeout;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Update()
    {
        if (_promptTimeout > 0f)
        {
            _promptTimeout -= Time.deltaTime;
            if (_promptTimeout <= 0f)
                _promptedTypeIndices.Clear();
        }

        float h = 0f, v = 0f;
        if (Input.GetKey(KeyCode.W)) v += 1f;
        if (Input.GetKey(KeyCode.S)) v -= 1f;
        if (Input.GetKey(KeyCode.D)) h += 1f;
        if (Input.GetKey(KeyCode.A)) h -= 1f;
        MoveInput = new Vector2(h, v).normalized;

        if (Input.GetKeyDown(KeyCode.Alpha1)) { _promptedTypeIndices.Add(0); _promptTimeout = GameConstants.ATTENTION_PROMPT_TIMEOUT; }
        if (Input.GetKeyDown(KeyCode.Alpha2)) { _promptedTypeIndices.Add(1); _promptTimeout = GameConstants.ATTENTION_PROMPT_TIMEOUT; }
        if (Input.GetKeyDown(KeyCode.Alpha3)) { _promptedTypeIndices.Add(2); _promptTimeout = GameConstants.ATTENTION_PROMPT_TIMEOUT; }
        if (Input.GetKeyDown(KeyCode.Alpha4)) { _promptedTypeIndices.Add(3); _promptTimeout = GameConstants.ATTENTION_PROMPT_TIMEOUT; }

        bool shiftHeld = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        if (Input.GetKeyDown(KeyCode.RightArrow))
            OnCommandStateChanged?.Invoke(CommandState.Engage, shiftHeld);
        if (Input.GetKeyDown(KeyCode.UpArrow))
            OnCommandStateChanged?.Invoke(CommandState.Follow, shiftHeld);
        if (Input.GetKeyDown(KeyCode.DownArrow))
            OnCommandStateChanged?.Invoke(CommandState.Retreat, shiftHeld);

        if (Input.GetKeyDown(KeyCode.F3))
            OnDebugToggle?.Invoke();
    }

    public ICollection<int> GetPromptedTypeIndices()
    {
        return _promptedTypeIndices;
    }
}
