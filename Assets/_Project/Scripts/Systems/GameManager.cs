using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("References")]
    public UnitData commanderData;
    [Tooltip("Ordered: index 0 = key 1, 1 = key 2, 2 = key 3. Same as DraftUI available units.")]
    public UnitData[] playerUnitTypes;

    public GameObject CommanderObject { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void SpawnCommander()
    {
        if (CommanderObject != null) return;

        CommanderObject = CreatePrimitive("Commander", Vector3.zero, commanderData.unitColor, 1f);
        var move = CommanderObject.AddComponent<MovementComponent>();
        move.moveSpeed = commanderData.moveSpeed;
        var health = CommanderObject.AddComponent<HealthComponent>();
        health.Initialize(commanderData.maxHP);
        CommanderObject.AddComponent<CommanderController>();

        var attack = CommanderObject.AddComponent<RangedAttackComponent>();
        attack.data = commanderData;
        attack.isPlayerUnit = true;

        CommanderObject.AddComponent<HitFlashComponent>();
        CommanderObject.AddComponent<ProceduralAnimator>();
        CommanderObject.AddComponent<ShoutOvalDisplay>();

        var cam = Camera.main.GetComponent<CameraController>();
        if (cam != null)
            cam.target = CommanderObject.transform;
    }

    public static GameObject CreatePrimitive(string name, Vector3 position, Color color, float scale)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Quad);
        go.name = name;
        go.transform.position = position;
        go.transform.localScale = Vector3.one * scale;

        Renderer r = go.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = color;
        mat.enableInstancing = true;
        r.material = mat;

        Collider col = go.GetComponent<Collider>();
        if (col != null) Object.Destroy(col);

        return go;
    }
}
