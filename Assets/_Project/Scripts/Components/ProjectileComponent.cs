using UnityEngine;

public class ProjectileComponent : MonoBehaviour
{
    public float speed;
    public float lifetime;
    public float damage;
    public bool isPlayerProjectile;

    Vector3 _direction;
    float _timer;

    public void Initialize(Vector3 direction, float speed, float lifetime, float damage, bool isPlayer, Color color)
    {
        _direction = direction.normalized;
        this.speed = speed;
        this.lifetime = lifetime;
        this.damage = damage;
        this.isPlayerProjectile = isPlayer;
        _timer = 0f;

        Renderer r = GetComponent<Renderer>();
        if (r != null)
        {
            r.material.color = color;
            r.sortingOrder = GameConstants.ISOMETRIC_SORT_PROJECTILE_ORDER;
        }
    }

    void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= lifetime)
        {
            ReturnToPool();
            return;
        }

        transform.position += _direction * speed * Time.deltaTime;

        CheckHit();
    }

    void CheckHit()
    {
        float hitRadius = 0.4f;

        if (isPlayerProjectile)
        {
            for (int i = UnitAIController.AllEnemyUnits.Count - 1; i >= 0; i--)
            {
                var enemy = UnitAIController.AllEnemyUnits[i];
                if (enemy == null || !enemy.gameObject.activeInHierarchy) continue;

                float dist = (enemy.transform.position - transform.position).sqrMagnitude;
                if (dist < hitRadius * hitRadius)
                {
                    var health = enemy.GetComponent<HealthComponent>();
                    if (health != null && !health.IsDead)
                    {
                        health.TakeDamage(damage);
                        ReturnToPool();
                        return;
                    }
                }
            }
        }
        else
        {
            // Enemy projectiles hit player units and Commander
            for (int i = UnitAIController.AllPlayerUnits.Count - 1; i >= 0; i--)
            {
                var unit = UnitAIController.AllPlayerUnits[i];
                if (unit == null || !unit.gameObject.activeInHierarchy) continue;

                float dist = (unit.transform.position - transform.position).sqrMagnitude;
                if (dist < hitRadius * hitRadius)
                {
                    var health = unit.GetComponent<HealthComponent>();
                    if (health != null && !health.IsDead)
                    {
                        health.TakeDamage(damage);
                        ReturnToPool();
                        return;
                    }
                }
            }

            // Check Commander
            if (CommanderController.Instance != null)
            {
                float dist = (CommanderController.Instance.transform.position - transform.position).sqrMagnitude;
                if (dist < hitRadius * hitRadius)
                {
                    var health = CommanderController.Instance.GetComponent<HealthComponent>();
                    if (health != null && !health.IsDead)
                    {
                        health.TakeDamage(damage);
                        ReturnToPool();
                        return;
                    }
                }
            }
        }
    }

    void ReturnToPool()
    {
        if (ObjectPool.Instance != null)
            ObjectPool.Instance.Return("Projectile", gameObject);
        else
            gameObject.SetActive(false);
    }
}
