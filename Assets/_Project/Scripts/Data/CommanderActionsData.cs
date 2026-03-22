using UnityEngine;

[CreateAssetMenu(fileName = "CommanderActionsData", menuName = "Game/Commander Actions Data")]
public class CommanderActionsData : ScriptableObject
{
    [Header("Burst Attack")]
    [Tooltip("Maximum shots the Commander can store.")]
    public int burstMaxAmmo = 10;

    [Tooltip("Time in seconds to reload one shot.")]
    public float burstReloadTime = 2f;

    [Tooltip("Minimum interval between consecutive burst shots (hold or tap).")]
    public float burstFireInterval = 0.3f;
}
