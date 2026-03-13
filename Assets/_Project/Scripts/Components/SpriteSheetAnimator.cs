using UnityEngine;

/// <summary>
/// Directional 25-frame sprite animator. Three sheets (up, right, down); right flipped for left.
/// Idle = single center frame (index 12). Walk = all 25 frames. No shoot frames from sheet.
/// </summary>
public class SpriteSheetAnimator : MonoBehaviour
{
    const int WALK_FRAME_COUNT = 25;
    const int IDLE_FRAME_INDEX = 12; // center of 5x5 (row 3, col 3)

    SpriteRenderer _spriteRenderer;
    MovementComponent _movement;
    CommanderController _commander;
    Sprite[] _spritesUp;
    Sprite[] _spritesRight;
    Sprite[] _spritesDown;
    Vector2 _lastFacing = new Vector2(0f, -1f);

    void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _movement = GetComponent<MovementComponent>();
        _commander = GetComponent<CommanderController>();
    }

    public void SetDirectionalSprites(Sprite[] up, Sprite[] right, Sprite[] down)
    {
        _spritesUp = up;
        _spritesRight = right;
        _spritesDown = down;
    }

    void Update()
    {
        if (_spriteRenderer == null) return;

        transform.rotation = Quaternion.Euler(GameConstants.ISOMETRIC_CAMERA_ANGLE, 0f, 0f);

        Vector2 facing = GetFacing();
        if (facing.sqrMagnitude > 0.01f)
            _lastFacing = facing.normalized;

        float flip = _lastFacing.x >= 0f ? 1f : -1f;
        Vector3 s = transform.localScale;
        s.x = Mathf.Abs(s.x) * flip;
        transform.localScale = s;

        Sprite[] sheet = GetCurrentSheet();
        if (sheet == null || sheet.Length == 0) return;

        bool isMoving = _movement != null && _movement.Velocity.sqrMagnitude > 0.01f;
        int index = isMoving
            ? Mathf.FloorToInt(Time.time * GameConstants.SPRITE_SHEET_FRAMES_PER_SECOND) % WALK_FRAME_COUNT
            : GameConstants.SPRITE_SHEET_IDLE_FRAME_INDEX;

        index = Mathf.Clamp(index, 0, sheet.Length - 1);
        if (sheet[index] != null)
            _spriteRenderer.sprite = sheet[index];
    }

    Sprite[] GetCurrentSheet()
    {
        float x = _lastFacing.x;
        float y = _lastFacing.y;
        if (y > 0.01f && y >= Mathf.Abs(x))
            return _spritesUp;
        if (x > 0.01f)
            return _spritesRight;
        if (x < -0.01f)
            return _spritesRight;
        return _spritesDown;
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
