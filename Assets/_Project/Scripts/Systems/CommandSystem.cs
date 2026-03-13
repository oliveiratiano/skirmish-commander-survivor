using System;
using System.Collections.Generic;
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

    void SetState(CommandState newState, bool withRelay)
    {
        CurrentState = newState;

        Vector2 commanderPosition = default;
        Vector2 facing = default;
        bool useOval = CommanderController.Instance != null;
        if (useOval)
        {
            commanderPosition = (Vector2)CommanderController.Instance.transform.position;
            facing = CommanderController.Instance.FacingDirection;
        }

        ICollection<int> prompted = InputHandler.Instance != null ? InputHandler.Instance.GetPromptedTypeIndices() : null;
        bool filterByPrompt = prompted != null && prompted.Count > 0;
        UnitData[] playerTypes = GameManager.Instance != null ? GameManager.Instance.playerUnitTypes : null;

        for (int i = 0; i < UnitAIController.AllPlayerUnits.Count; i++)
        {
            var u = UnitAIController.AllPlayerUnits[i];
            if (u == null || !u.gameObject.activeInHierarchy) continue;
            if (useOval && !ShoutGeometry.IsInShoutOval(commanderPosition, facing, (Vector2)u.transform.position))
                continue;
            if (filterByPrompt && playerTypes != null)
            {
                int typeIndex = IndexOf(playerTypes, u.data);
                if (typeIndex < 0 || !prompted.Contains(typeIndex))
                    continue;
            }
            u.ReceiveCommand(newState, withRelay);
        }

        OnStateChanged?.Invoke(newState);
    }

    static int IndexOf(UnitData[] arr, UnitData d)
    {
        if (arr == null || d == null) return -1;
        for (int i = 0; i < arr.Length; i++)
            if (arr[i] == d) return i;
        return -1;
    }
}
