using UnityEngine;

public class ProceduralAnimator : MonoBehaviour
{
    [Header("Idle Breathing")]
    public float breathAmount = 0.02f;
    public float breathSpeed = 2f;

    [Header("Movement Waddle")]
    public float waddleAngle = 5f;
    public float waddleSpeed = 10f;

    [Header("Recoil")]
    public float recoilScale = 0.1f;

    MovementComponent _movement;
    RangedAttackComponent _attack;
    Vector3 _baseScale;
    float _recoilTimer;
    bool _wasShootingLastFrame;

    void Awake()
    {
        _baseScale = transform.localScale;
        _movement = GetComponent<MovementComponent>();
        _attack = GetComponent<RangedAttackComponent>();
    }

    void Update()
    {
        Vector3 scale = _baseScale;
        float rotation = 0f;

        // Idle breathing — always active
        float breathOffset = Mathf.Sin(Time.time * breathSpeed + GetInstanceID()) * breathAmount;
        scale.y = _baseScale.y * (1f + breathOffset);

        // Movement waddle
        bool isMoving = _movement != null && _movement.Velocity.sqrMagnitude > 0.01f;
        if (isMoving)
        {
            rotation = Mathf.Sin(Time.time * waddleSpeed) * waddleAngle;
        }

        // Recoil on shoot
        if (_attack != null && _attack.IsShooting && !_wasShootingLastFrame)
        {
            _recoilTimer = GameConstants.RECOIL_DURATION;
        }
        _wasShootingLastFrame = _attack != null && _attack.IsShooting;

        if (_recoilTimer > 0f)
        {
            float t = _recoilTimer / GameConstants.RECOIL_DURATION;
            float recoilFactor = 1f - recoilScale * t;
            scale *= recoilFactor;
            _recoilTimer -= Time.deltaTime;
        }

        transform.localScale = scale;
        transform.rotation = Quaternion.Euler(0f, 0f, rotation);
    }

    public void ResetAnimation()
    {
        transform.localScale = _baseScale;
        transform.rotation = Quaternion.identity;
        _recoilTimer = 0f;
    }
}
