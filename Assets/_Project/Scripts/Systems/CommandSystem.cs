using System;
using UnityEngine;

public enum CommandState
{
    Engage,
    Follow,
    Retreat
}

public class CommandSystem : MonoBehaviour
{
    public static CommandSystem Instance { get; private set; }

    public CommandState CurrentState { get; private set; } = CommandState.Follow;

    public event Action<CommandState> OnStateChanged;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void OnEnable()
    {
        SubscribeInput();
    }

    void Start()
    {
        SubscribeInput();
    }

    void OnDisable()
    {
        if (InputHandler.Instance != null)
            InputHandler.Instance.OnCommandStateChanged -= SetState;
    }

    void SubscribeInput()
    {
        if (InputHandler.Instance != null)
        {
            InputHandler.Instance.OnCommandStateChanged -= SetState;
            InputHandler.Instance.OnCommandStateChanged += SetState;
        }
    }

    public void SetState(CommandState newState)
    {
        if (CurrentState == newState) return;
        CurrentState = newState;
        OnStateChanged?.Invoke(newState);
    }
}
