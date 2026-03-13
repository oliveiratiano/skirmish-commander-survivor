using UnityEngine;

[RequireComponent(typeof(MovementComponent))]
[RequireComponent(typeof(HealthComponent))]
public class CommanderController : MonoBehaviour
{
    public static CommanderController Instance { get; private set; }

    MovementComponent _movement;
    HealthComponent _health;
    Vector2 _facing = new Vector2(0f, -1f);

    public Vector2 FacingDirection => _facing;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        _movement = GetComponent<MovementComponent>();
        _health = GetComponent<HealthComponent>();
    }

    void Update()
    {
        if (InputHandler.Instance == null) return;

        Vector2 input = InputHandler.Instance.MoveInput;
        if (input.sqrMagnitude > 0.01f)
            _facing = input.normalized;

        Vector3 dir = new Vector3(input.x, input.y, 0f);
        _movement.Move(dir);

        ClampToArena();

        if (DebugOverlay.Instance != null && DebugOverlay.Instance.IsVisible)
            Debug.DrawLine(transform.position, transform.position + (Vector3)(_facing * 3f), Color.cyan, 0f);
    }

    void ClampToArena()
    {
        Vector3 pos = transform.position;
        float half = GameConstants.ARENA_HALF_SIZE;
        pos.x = Mathf.Clamp(pos.x, -half, half);
        pos.y = Mathf.Clamp(pos.y, -half, half);
        transform.position = pos;
    }
}
