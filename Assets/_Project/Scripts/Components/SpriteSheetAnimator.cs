using UnityEngine;

/// <summary>
/// Drives which sprite is shown from a 12-frame sheet (idle 0-2, walk 3-8, shoot 9-10) based on movement and shooting state.
/// Keeps sprite facing the camera (isometric tilt only) and flips scale.x for left/right so it never goes edge-on and vanishes.
/// </summary>
public class SpriteSheetAnimator : MonoBehaviour
{
    const int IDLE_START = 0;
    const int IDLE_COUNT = 3;
    const int WALK_START = 3;
    const int WALK_COUNT = 6;
    const int SHOOT_START = 9;
    const int SHOOT_COUNT = 2;

    SpriteRenderer _spriteRenderer;
    MovementComponent _movement;
    RangedAttackComponent _attack;
    CommanderController _commander;
    Sprite[] _sprites;
    float _shootAnimTimer = -1f;
    Vector2 _lastFacing = new Vector2(0f, -1f);

    void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _movement = GetComponent<MovementComponent>();
        _attack = GetComponent<RangedAttackComponent>();
        _commander = GetComponent<CommanderController>();
    }

    public void SetSprites(Sprite[] sprites)
    {
        _sprites = sprites;
    }

    void Update()
    {
        if (_spriteRenderer == null) return;

        // Rotation: only isometric tilt so sprite always faces camera (no Y rotation = no vanishing when moving A/D)
        transform.rotation = Quaternion.Euler(GameConstants.ISOMETRIC_CAMERA_ANGLE, 0f, 0f);

        // Left/right: flip scale.x so sprite faces left/right without rotating (never edge-on / vanishing)
        Vector2 facing = GetFacing();
        if (facing.sqrMagnitude > 0.01f)
            _lastFacing = facing.normalized;
        float flip = _lastFacing.x >= 0f ? 1f : -1f;
        Vector3 s = transform.localScale;
        s.x = Mathf.Abs(s.x) * flip;
        transform.localScale = s;

        if (_sprites == null || _sprites.Length == 0) return;

        bool isMoving = _movement != null && _movement.Velocity.sqrMagnitude > 0.01f;
        bool isShooting = _attack != null && _attack.IsShooting;

        if (isShooting)
            _shootAnimTimer = GameConstants.SHOOT_ANIM_DURATION;

        int index;
        if (_shootAnimTimer > 0f)
        {
            _shootAnimTimer -= Time.deltaTime;
            float t = 1f - (_shootAnimTimer / GameConstants.SHOOT_ANIM_DURATION);
            index = t < 0.5f ? SHOOT_START : SHOOT_START + 1;
        }
        else if (isMoving)
        {
            int frame = Mathf.FloorToInt(Time.time * GameConstants.SPRITE_SHEET_FRAMES_PER_SECOND) % WALK_COUNT;
            index = WALK_START + frame;
        }
        else
        {
            int frame = Mathf.FloorToInt(Time.time * GameConstants.SPRITE_SHEET_FRAMES_PER_SECOND) % IDLE_COUNT;
            index = IDLE_START + frame;
        }

        index = Mathf.Clamp(index, 0, _sprites.Length - 1);
        if (_sprites[index] != null)
            _spriteRenderer.sprite = _sprites[index];
    }

    Vector2 GetFacing()
    {
        if (_commander != null && CommanderController.Instance == _commander)
            return _commander.FacingDirection;
        if (_movement != null && _movement.Velocity.sqrMagnitude > 0.01f)
            return _movement.Velocity.normalized;
        return _lastFacing;
    }
}
