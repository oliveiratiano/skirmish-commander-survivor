using System;
using UnityEngine;

public class InputHandler : MonoBehaviour
{
    public static InputHandler Instance { get; private set; }

    public Vector2 MoveInput { get; private set; }

    public event Action<CommandState> OnCommandStateChanged;
    public event Action OnDebugToggle;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Update()
    {
        float h = 0f, v = 0f;
        if (Input.GetKey(KeyCode.W)) v += 1f;
        if (Input.GetKey(KeyCode.S)) v -= 1f;
        if (Input.GetKey(KeyCode.D)) h += 1f;
        if (Input.GetKey(KeyCode.A)) h -= 1f;
        MoveInput = new Vector2(h, v).normalized;

        if (Input.GetKeyDown(KeyCode.RightArrow))
            OnCommandStateChanged?.Invoke(CommandState.Engage);
        if (Input.GetKeyDown(KeyCode.UpArrow))
            OnCommandStateChanged?.Invoke(CommandState.Follow);
        if (Input.GetKeyDown(KeyCode.DownArrow))
            OnCommandStateChanged?.Invoke(CommandState.Retreat);

        if (Input.GetKeyDown(KeyCode.F3))
            OnDebugToggle?.Invoke();
    }
}
