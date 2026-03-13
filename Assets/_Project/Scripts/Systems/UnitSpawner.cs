using UnityEngine;

public class UnitSpawner : MonoBehaviour
{
    public static UnitSpawner Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public GameObject SpawnPlayerUnit(UnitData data, Vector3 position)
    {
        GameObject go = GameManager.CreatePrimitive(data.unitName, position, data.unitColor, 0.8f);
        var move = go.AddComponent<MovementComponent>();
        move.moveSpeed = data.moveSpeed;
        go.AddComponent<HealthComponent>();
        var attack = go.AddComponent<RangedAttackComponent>();
        attack.data = data;
        attack.isPlayerUnit = true;
        var ai = go.AddComponent<UnitAIController>();
        ai.Initialize(data, isPlayer: true);

        go.AddComponent<HitFlashComponent>();
        go.AddComponent<ProceduralAnimator>();
        go.AddComponent<IsometricSorting>();
        go.AddComponent<CommandFeedbackDisplay>();

        if (DebugOverlay.Instance != null)
            DebugOverlay.Instance.SetPlayerUnitCount(UnitAIController.AllPlayerUnits.Count);

        return go;
    }

    public void SpawnDraftedArmy(UnitData[] units, int[] counts, Vector3 center)
    {
        int total = 0;
        for (int i = 0; i < counts.Length; i++) total += counts[i];

        int spawned = 0;
        for (int i = 0; i < units.Length; i++)
        {
            for (int j = 0; j < counts[i]; j++)
            {
                float angle = (360f / total) * spawned * Mathf.Deg2Rad;
                float radius = 2f + (spawned / 8f);
                Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius;
                SpawnPlayerUnit(units[i], center + offset);
                spawned++;
            }
        }
    }
}
