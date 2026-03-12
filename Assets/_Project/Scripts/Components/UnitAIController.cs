using UnityEngine;

[RequireComponent(typeof(MovementComponent))]
[RequireComponent(typeof(HealthComponent))]
public class UnitAIController : MonoBehaviour
{
    public UnitData data;
    public bool IsPlayerUnit { get; private set; }

    MovementComponent _movement;
    HealthComponent _health;
    RangedAttackComponent _attack;

    float _shootPrepTimer;
    bool _isPreparingToShoot;
    bool _prepComplete;
    const float SHOOT_PREP_DURATION = 0.5f;

    public static readonly System.Collections.Generic.List<UnitAIController> AllPlayerUnits
        = new System.Collections.Generic.List<UnitAIController>();
    public static readonly System.Collections.Generic.List<UnitAIController> AllEnemyUnits
        = new System.Collections.Generic.List<UnitAIController>();

    public void Initialize(UnitData unitData, bool isPlayer)
    {
        data = unitData;
        IsPlayerUnit = isPlayer;

        _movement = GetComponent<MovementComponent>();
        _health = GetComponent<HealthComponent>();
        _attack = GetComponent<RangedAttackComponent>();
        _movement.moveSpeed = unitData.moveSpeed;
        _health.Initialize(unitData.maxHP);

        _health.OnDied += HandleDeath;

        if (isPlayer)
            AllPlayerUnits.Add(this);
        else
            AllEnemyUnits.Add(this);
    }

    void OnDestroy()
    {
        AllPlayerUnits.Remove(this);
        AllEnemyUnits.Remove(this);
        if (_health != null)
            _health.OnDied -= HandleDeath;
    }

    void Update()
    {
        if (_health != null && _health.IsDead) return;

        if (IsPlayerUnit)
            UpdatePlayerUnit();
        else
            UpdateEnemyUnit();

        ApplyFriendlyAvoidance();
    }

    void UpdatePlayerUnit()
    {
        CommandState state = CommandSystem.Instance != null
            ? CommandSystem.Instance.CurrentState
            : CommandState.Follow;

        bool canAttack = state != CommandState.Retreat;
        if (_attack != null)
            _attack.enabled = canAttack;

        switch (state)
        {
            case CommandState.Engage:
                HandleEngage();
                break;
            case CommandState.Follow:
                HandleFollow();
                break;
            case CommandState.Retreat:
                HandleRetreat();
                break;
        }
    }

    void HandleEngage()
    {
        Transform nearest = FindNearest(AllEnemyUnits);
        if (nearest == null)
        {
            ResetPrep();
            _movement.Stop();
            return;
        }

        float dist = (nearest.position - transform.position).magnitude;
        if (data != null && dist <= data.range)
        {
            _movement.Stop();
            RunShootPrep();
        }
        else
        {
            ResetPrep();
            Vector3 dir = (nearest.position - transform.position).normalized;
            _movement.Move(dir);
        }
    }

    void HandleFollow()
    {
        if (CommanderController.Instance == null) return;

        bool enemyInRange = false;
        if (_attack != null && data != null)
        {
            Transform nearest = FindNearest(AllEnemyUnits);
            if (nearest != null)
            {
                float dist = (nearest.position - transform.position).magnitude;
                enemyInRange = dist <= data.range;
            }
        }

        if (enemyInRange)
        {
            _movement.Stop();
            RunShootPrep();

            if (_prepComplete && _attack != null && !_attack.IsShooting)
                ResetPrep();
        }
        else
        {
            ResetPrep();
            MoveTowardCommander(0.85f);
        }
    }

    void HandleRetreat()
    {
        ResetPrep();
        if (_attack != null)
            _attack.CanFire = false;
        MoveTowardCommander(1f);
    }

    void UpdateEnemyUnit()
    {
        Transform nearest = FindNearestAnyUnit();
        if (nearest == null)
        {
            ResetPrep();
            _movement.Stop();
            return;
        }

        float dist = (nearest.position - transform.position).magnitude;
        if (data != null && dist <= data.range)
        {
            _movement.Stop();
            RunShootPrep();
        }
        else
        {
            ResetPrep();
            Vector3 dir = (nearest.position - transform.position).normalized;
            _movement.Move(dir);
        }
    }

    void RunShootPrep()
    {
        if (_attack == null) return;

        if (!_isPreparingToShoot)
        {
            _isPreparingToShoot = true;
            _prepComplete = false;
            _shootPrepTimer = SHOOT_PREP_DURATION;
            _attack.CanFire = false;
        }

        _shootPrepTimer -= Time.deltaTime;

        if (_shootPrepTimer <= 0f && !_prepComplete)
        {
            _prepComplete = true;
            _attack.CanFire = true;
        }
    }

    void ResetPrep()
    {
        _isPreparingToShoot = false;
        _prepComplete = false;
        if (_attack != null)
            _attack.CanFire = false;
    }

    void MoveTowardCommander(float urgency)
    {
        if (CommanderController.Instance == null) return;

        Vector3 commanderPos = CommanderController.Instance.transform.position;
        Vector3 toCommander = commanderPos - transform.position;
        float perimeterRadius = 2.5f;

        if (toCommander.magnitude > perimeterRadius)
        {
            _movement.Move(toCommander.normalized * urgency);
        }
        else
        {
            _movement.Stop();
        }
    }

    Transform FindNearest(System.Collections.Generic.List<UnitAIController> list)
    {
        Transform best = null;
        float bestDist = float.MaxValue;

        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] == null || list[i] == this) continue;
            if (!list[i].gameObject.activeInHierarchy) continue;
            var h = list[i].GetComponent<HealthComponent>();
            if (h != null && h.IsDead) continue;

            float dist = (list[i].transform.position - transform.position).sqrMagnitude;
            if (dist < bestDist)
            {
                bestDist = dist;
                best = list[i].transform;
            }
        }

        return best;
    }

    Transform FindNearestAnyUnit()
    {
        Transform best = null;
        float bestDist = float.MaxValue;

        for (int i = 0; i < AllPlayerUnits.Count; i++)
        {
            if (AllPlayerUnits[i] == null || !AllPlayerUnits[i].gameObject.activeInHierarchy) continue;
            var h = AllPlayerUnits[i].GetComponent<HealthComponent>();
            if (h != null && h.IsDead) continue;

            float dist = (AllPlayerUnits[i].transform.position - transform.position).sqrMagnitude;
            if (dist < bestDist) { bestDist = dist; best = AllPlayerUnits[i].transform; }
        }

        if (CommanderController.Instance != null)
        {
            var cmdHealth = CommanderController.Instance.GetComponent<HealthComponent>();
            if (cmdHealth != null && !cmdHealth.IsDead)
            {
                float dist = (CommanderController.Instance.transform.position - transform.position).sqrMagnitude;
                if (dist < bestDist) { bestDist = dist; best = CommanderController.Instance.transform; }
            }
        }

        return best;
    }

    void ApplyFriendlyAvoidance()
    {
        var list = IsPlayerUnit ? AllPlayerUnits : AllEnemyUnits;
        Vector3 avoidance = Vector3.zero;
        float avoidRadius = 1.2f;
        int neighbors = 0;

        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] == null || list[i] == this) continue;
            Vector3 diff = transform.position - list[i].transform.position;
            float dist = diff.magnitude;
            if (dist < avoidRadius && dist > 0.001f)
            {
                avoidance += diff.normalized * (1f - dist / avoidRadius);
                neighbors++;
            }
        }

        if (neighbors > 0)
        {
            avoidance /= neighbors;
            transform.position += avoidance * _movement.moveSpeed * 0.5f * Time.deltaTime;
        }
    }

    void HandleDeath()
    {
        if (!IsPlayerUnit && ObjectPool.Instance != null)
        {
            AllEnemyUnits.Remove(this);
            ObjectPool.Instance.Return("Enemy", gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
