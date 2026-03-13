using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("References")]
    public UnitData commanderData;
    [Tooltip("Ordered: index 0 = key 1, 1 = key 2, 2 = key 3. Same as DraftUI available units.")]
    public UnitData[] playerUnitTypes;

    [Header("Sprites (optional) — 3 directional sheets, 25 frames each. Prefer Commander Data arrays when set.")]
    public Sprite[] commanderSpritesUp;
    public Sprite[] commanderSpritesRight;
    public Sprite[] commanderSpritesDown;

    public GameObject CommanderObject { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void SpawnCommander()
    {
        if (CommanderObject != null) return;

        Sprite[] up = commanderData != null && commanderData.spritesUp != null && commanderData.spritesUp.Length > 0 ? commanderData.spritesUp : commanderSpritesUp;
        Sprite[] right = commanderData != null && commanderData.spritesRight != null && commanderData.spritesRight.Length > 0 ? commanderData.spritesRight : commanderSpritesRight;
        Sprite[] down = commanderData != null && commanderData.spritesDown != null && commanderData.spritesDown.Length > 0 ? commanderData.spritesDown : commanderSpritesDown;
        if ((up != null && up.Length > 0) || (right != null && right.Length > 0) || (down != null && down.Length > 0))
        {
            SpawnCommanderWithSprites(up, right, down);
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

    void SpawnCommanderWithSprites(Sprite[] up, Sprite[] right, Sprite[] down)
    {
        var go = new GameObject("Commander");
        go.transform.position = Vector3.zero;
        go.transform.localScale = Vector3.one * GameConstants.COMMANDER_SPRITE_SCALE;

        var sr = go.AddComponent<SpriteRenderer>();
        Sprite[] first = up != null && up.Length > 0 ? up : (right != null && right.Length > 0 ? right : down);
        sr.sprite = first != null && first.Length > GameConstants.SPRITE_SHEET_IDLE_FRAME_INDEX ? first[GameConstants.SPRITE_SHEET_IDLE_FRAME_INDEX] : (first != null && first.Length > 0 ? first[0] : null);
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
        anim.SetDirectionalSprites(up, right, down);
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
