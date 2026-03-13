using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("References")]
    public UnitData commanderData;
    [Tooltip("Ordered: index 0 = key 1, 1 = key 2, 2 = key 3. Same as DraftUI available units.")]
    public UnitData[] playerUnitTypes;

    [Header("Sprites (optional)")]
    [Tooltip("12 sprites: Commander_0..11 (idle 0-2, walk 3-8, shoot 9-10). Leave empty to use colored quad.")]
    public Sprite[] commanderSprites;

    public GameObject CommanderObject { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void SpawnCommander()
    {
        if (CommanderObject != null) return;

        Sprite[] sprites = (commanderData != null && commanderData.sprites != null && commanderData.sprites.Length > 0)
            ? commanderData.sprites
            : commanderSprites;
        if (sprites != null && sprites.Length > 0)
        {
            SpawnCommanderWithSprites(sprites);
            return;
        }

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
        CommanderObject.AddComponent<IsometricSorting>();
        CommanderObject.AddComponent<ShoutOvalDisplay>();
        CommanderObject.AddComponent<CommandFeedbackDisplay>();

        var cam = Camera.main.GetComponent<CameraController>();
        if (cam != null)
            cam.target = CommanderObject.transform;
    }

    void SpawnCommanderWithSprites(Sprite[] sprites)
    {
        var go = new GameObject("Commander");
        go.transform.position = Vector3.zero;
        go.transform.localScale = Vector3.one * GameConstants.COMMANDER_SPRITE_SCALE;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprites[0];
        sr.material = new Material(Shader.Find("Sprites/Default"));
        sr.material.color = Color.white;

        var move = go.AddComponent<MovementComponent>();
        move.moveSpeed = commanderData.moveSpeed;
        var health = go.AddComponent<HealthComponent>();
        health.Initialize(commanderData.maxHP);
        go.AddComponent<CommanderController>();

        var attack = go.AddComponent<RangedAttackComponent>();
        attack.data = commanderData;
        attack.isPlayerUnit = true;

        go.AddComponent<HitFlashComponent>();
        go.AddComponent<ProceduralAnimator>();
        var anim = go.AddComponent<SpriteSheetAnimator>();
        anim.SetSprites(sprites);
        go.AddComponent<IsometricSorting>();
        go.AddComponent<ShoutOvalDisplay>();
        go.AddComponent<CommandFeedbackDisplay>();

        CommanderObject = go;

        var cam = Camera.main.GetComponent<CameraController>();
        if (cam != null)
            cam.target = go.transform;
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
