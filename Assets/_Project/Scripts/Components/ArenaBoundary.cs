using UnityEngine;

public class ArenaBoundary : MonoBehaviour
{
    GameObject[] _dangerStrips;
    GameObject[] _edgeLines;

    void Start()
    {
        CreateVisuals();
    }

    void Update()
    {
        ApplyBoundaryDamage(UnitAIController.AllPlayerUnits);
        ApplyBoundaryDamage(UnitAIController.AllEnemyUnits);
        ApplyBoundaryToCommander();
        ClampAllUnits();
    }

    void ApplyBoundaryDamage(System.Collections.Generic.List<UnitAIController> units)
    {
        float safe = GameConstants.ARENA_SAFE_HALF_SIZE;
        float dps = GameConstants.DANGER_ZONE_DPS;

        for (int i = 0; i < units.Count; i++)
        {
            if (units[i] == null || !units[i].gameObject.activeInHierarchy) continue;
            var health = units[i].GetComponent<HealthComponent>();
            if (health == null || health.IsDead) continue;

            Vector3 pos = units[i].transform.position;
            if (Mathf.Abs(pos.x) > safe || Mathf.Abs(pos.y) > safe)
            {
                health.TakeDamage(dps * Time.deltaTime);
            }
        }
    }

    void ApplyBoundaryToCommander()
    {
        if (CommanderController.Instance == null) return;
        var health = CommanderController.Instance.GetComponent<HealthComponent>();
        if (health == null || health.IsDead) return;

        Vector3 pos = CommanderController.Instance.transform.position;
        float safe = GameConstants.ARENA_SAFE_HALF_SIZE;

        if (Mathf.Abs(pos.x) > safe || Mathf.Abs(pos.y) > safe)
        {
            health.TakeDamage(GameConstants.DANGER_ZONE_DPS * Time.deltaTime);
        }
    }

    void ClampAllUnits()
    {
        float half = GameConstants.ARENA_HALF_SIZE;

        for (int i = 0; i < UnitAIController.AllEnemyUnits.Count; i++)
        {
            if (UnitAIController.AllEnemyUnits[i] == null) continue;
            ClampPosition(UnitAIController.AllEnemyUnits[i].transform, half);
        }

        for (int i = 0; i < UnitAIController.AllPlayerUnits.Count; i++)
        {
            if (UnitAIController.AllPlayerUnits[i] == null) continue;
            ClampPosition(UnitAIController.AllPlayerUnits[i].transform, half);
        }
    }

    static void ClampPosition(Transform t, float half)
    {
        Vector3 p = t.position;
        p.x = Mathf.Clamp(p.x, -half, half);
        p.y = Mathf.Clamp(p.y, -half, half);
        t.position = p;
    }

    void CreateVisuals()
    {
        float half = GameConstants.ARENA_HALF_SIZE;
        float dangerW = GameConstants.ARENA_DANGER_ZONE_WIDTH;
        float arenaFull = half * 2f;

        _dangerStrips = new GameObject[4];
        _edgeLines = new GameObject[4];

        // Top strip
        _dangerStrips[0] = MakeQuad("DangerTop",
            new Vector3(0f, half - dangerW / 2f, 0f),
            new Vector3(arenaFull, dangerW, 1f),
            GameConstants.DANGER_ZONE_COLOR, -99);

        // Bottom strip
        _dangerStrips[1] = MakeQuad("DangerBottom",
            new Vector3(0f, -half + dangerW / 2f, 0f),
            new Vector3(arenaFull, dangerW, 1f),
            GameConstants.DANGER_ZONE_COLOR, -99);

        // Right strip
        _dangerStrips[2] = MakeQuad("DangerRight",
            new Vector3(half - dangerW / 2f, 0f, 0f),
            new Vector3(dangerW, arenaFull, 1f),
            GameConstants.DANGER_ZONE_COLOR, -99);

        // Left strip
        _dangerStrips[3] = MakeQuad("DangerLeft",
            new Vector3(-half + dangerW / 2f, 0f, 0f),
            new Vector3(dangerW, arenaFull, 1f),
            GameConstants.DANGER_ZONE_COLOR, -99);

        float edgeThickness = 0.15f;

        // Top edge
        _edgeLines[0] = MakeQuad("EdgeTop",
            new Vector3(0f, half, 0f),
            new Vector3(arenaFull + edgeThickness, edgeThickness, 1f),
            GameConstants.DANGER_EDGE_COLOR, -98);

        // Bottom edge
        _edgeLines[1] = MakeQuad("EdgeBottom",
            new Vector3(0f, -half, 0f),
            new Vector3(arenaFull + edgeThickness, edgeThickness, 1f),
            GameConstants.DANGER_EDGE_COLOR, -98);

        // Right edge
        _edgeLines[2] = MakeQuad("EdgeRight",
            new Vector3(half, 0f, 0f),
            new Vector3(edgeThickness, arenaFull + edgeThickness, 1f),
            GameConstants.DANGER_EDGE_COLOR, -98);

        // Left edge
        _edgeLines[3] = MakeQuad("EdgeLeft",
            new Vector3(-half, 0f, 0f),
            new Vector3(edgeThickness, arenaFull + edgeThickness, 1f),
            GameConstants.DANGER_EDGE_COLOR, -98);
    }

    static GameObject MakeQuad(string name, Vector3 position, Vector3 scale, Color color, int sortOrder)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Quad);
        go.name = name;
        go.transform.position = position;
        go.transform.localScale = scale;

        Renderer r = go.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = color;
        r.material = mat;
        r.sortingOrder = sortOrder;

        Collider col = go.GetComponent<Collider>();
        if (col != null) Object.Destroy(col);

        return go;
    }
}
