using UnityEngine;

public class HitFlashComponent : MonoBehaviour
{
    Color _originalColor;
    Renderer _renderer;
    float _flashTimer;
    bool _isFlashing;

    void Awake()
    {
        _renderer = GetComponent<Renderer>();
        if (_renderer != null)
            _originalColor = _renderer.material.color;
    }

    void OnEnable()
    {
        var health = GetComponent<HealthComponent>();
        if (health != null)
            health.OnDamaged += OnDamaged;
    }

    void OnDisable()
    {
        var health = GetComponent<HealthComponent>();
        if (health != null)
            health.OnDamaged -= OnDamaged;

        if (_isFlashing)
            RestoreColor();
    }

    void OnDamaged(float currentHP, float damage)
    {
        if (_renderer == null) return;
        _originalColor = _renderer.material.color;
        _renderer.material.color = Color.white;
        _flashTimer = GameConstants.HIT_FLASH_DURATION;
        _isFlashing = true;
    }

    void Update()
    {
        if (!_isFlashing) return;

        _flashTimer -= Time.deltaTime;
        if (_flashTimer <= 0f)
            RestoreColor();
    }

    void RestoreColor()
    {
        if (_renderer != null)
            _renderer.material.color = _originalColor;
        _isFlashing = false;
    }

    public void ResetFlash()
    {
        _isFlashing = false;
        if (_renderer != null)
            _renderer.material.color = _renderer.material.color;
    }
}
