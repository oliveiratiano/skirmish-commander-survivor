using System;
using UnityEngine;

public class HealthComponent : MonoBehaviour
{
    public float maxHP = 100f;
    public float CurrentHP { get; private set; }
    public bool IsDead => CurrentHP <= 0f;

    public event Action<float, float> OnDamaged; // currentHP, damage
    public event Action OnDied;

    void Awake()
    {
        CurrentHP = maxHP;
    }

    public void Initialize(float max)
    {
        maxHP = max;
        CurrentHP = max;
    }

    public void TakeDamage(float damage)
    {
        if (IsDead) return;

        CurrentHP = Mathf.Max(0f, CurrentHP - damage);
        OnDamaged?.Invoke(CurrentHP, damage);

        if (IsDead)
            OnDied?.Invoke();
    }

    public void ResetHealth()
    {
        CurrentHP = maxHP;
    }
}
