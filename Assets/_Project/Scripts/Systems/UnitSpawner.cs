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
        bool hasSprites = (data.spritesUp != null && data.spritesUp.Length > 0) || (data.spritesRight != null && data.spritesRight.Length > 0) || (data.spritesDown != null && data.spritesDown.Length > 0);
        if (hasSprites)
            return SpawnPlayerUnitWithSprites(data, position);

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

    GameObject SpawnPlayerUnitWithSprites(UnitData data, Vector3 position)
    {
        var go = new GameObject(data.unitName);
        go.transform.position = position;
        go.transform.localScale = Vector3.one * GameConstants.UNIT_SPRITE_SCALE;

        var sr = go.AddComponent<SpriteRenderer>();
        Sprite[] first = data.spritesDown ?? data.spritesUp ?? data.spritesRight;
        int idleIdx = GameConstants.SPRITE_SHEET_IDLE_FRAME_INDEX;
        sr.sprite = first != null && first.Length > idleIdx ? first[idleIdx] : (first != null && first.Length > 0 ? first[0] : null);
        sr.material = new Material(Shader.Find("Sprites/Default"));
        sr.material.color = Color.white;

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
        var anim = go.AddComponent<SpriteSheetAnimator>();
        anim.SetDirectionalSprites(data.spritesUp, data.spritesRight, data.spritesDown);
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
