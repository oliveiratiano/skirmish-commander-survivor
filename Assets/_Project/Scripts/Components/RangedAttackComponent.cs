using UnityEngine;

public class RangedAttackComponent : MonoBehaviour
{
    public UnitData data;
    public bool isPlayerUnit;

    float _cooldownTimer;
    int _burstRemaining;
    float _burstTimer;
    bool _isBursting;

    static GameObject _projectilePrefab;

    public bool IsShooting => _isBursting;

    void Update()
    {
        if (data == null) return;

        _cooldownTimer -= Time.deltaTime;

        if (_isBursting)
        {
            _burstTimer -= Time.deltaTime;
            if (_burstTimer <= 0f)
            {
                FireOnce();
                _burstRemaining--;
                if (_burstRemaining <= 0)
                {
                    _isBursting = false;
                    _cooldownTimer = data.cooldown;
                }
                else
                {
                    _burstTimer = data.burstInterval;
                }
            }
            return;
        }

        if (_cooldownTimer > 0f) return;

        Transform target = FindTarget();
        if (target == null) return;

        float dist = (target.position - transform.position).magnitude;
        if (dist > data.range) return;

        if (data.burstCount > 1)
        {
            _isBursting = true;
            _burstRemaining = data.burstCount;
            _burstTimer = 0f;
        }
        else
        {
            FireOnce();
            _cooldownTimer = data.cooldown;
        }
    }

    void FireOnce()
    {
        Transform target = FindTarget();
        if (target == null) return;

        Vector3 toTarget = (target.position - transform.position).normalized;
        float deviation = Random.Range(-data.accuracySpreadDegrees, data.accuracySpreadDegrees);
        Vector3 direction = Quaternion.Euler(0f, 0f, deviation) * toTarget;

        SpawnProjectile(direction);
    }

    void SpawnProjectile(Vector3 direction)
    {
        EnsurePrefab();

        GameObject go = null;
        if (ObjectPool.Instance != null)
            go = ObjectPool.Instance.Get("Projectile", transform.position);

        if (go == null)
        {
            go = GameManager.CreatePrimitive("Projectile", transform.position, data.projectileColor, 0.2f);
            go.AddComponent<ProjectileComponent>();
        }

        var proj = go.GetComponent<ProjectileComponent>();
        proj.Initialize(direction, data.projectileSpeed, data.projectileLifetime,
            data.damage, isPlayerUnit, data.projectileColor);
    }

    static void EnsurePrefab()
    {
        if (_projectilePrefab != null) return;
        _projectilePrefab = GameManager.CreatePrimitive("ProjectilePrefab", Vector3.zero, Color.yellow, 0.2f);
        _projectilePrefab.AddComponent<ProjectileComponent>();
        _projectilePrefab.SetActive(false);

        if (ObjectPool.Instance != null)
            ObjectPool.Instance.Prewarm("Projectile", _projectilePrefab, 200);
    }

    Transform FindTarget()
    {
        if (isPlayerUnit)
            return FindNearestIn(UnitAIController.AllEnemyUnits);
        else
            return FindNearestIncludingCommander();
    }

    Transform FindNearestIn(System.Collections.Generic.List<UnitAIController> list)
    {
        Transform best = null;
        float bestDist = float.MaxValue;

        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] == null || !list[i].gameObject.activeInHierarchy) continue;
            var h = list[i].GetComponent<HealthComponent>();
            if (h != null && h.IsDead) continue;

            float d = (list[i].transform.position - transform.position).sqrMagnitude;
            if (d < bestDist) { bestDist = d; best = list[i].transform; }
        }

        return best;
    }

    Transform FindNearestIncludingCommander()
    {
        Transform best = FindNearestIn(UnitAIController.AllPlayerUnits);
        float bestDist = best != null
            ? (best.position - transform.position).sqrMagnitude
            : float.MaxValue;

        if (CommanderController.Instance != null)
        {
            var h = CommanderController.Instance.GetComponent<HealthComponent>();
            if (h != null && !h.IsDead)
            {
                float d = (CommanderController.Instance.transform.position - transform.position).sqrMagnitude;
                if (d < bestDist) best = CommanderController.Instance.transform;
            }
        }

        return best;
    }
}
