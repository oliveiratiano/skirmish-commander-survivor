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

    enum ShootPrepPhase
    {
        OutOfRange,
        Preparing,
        ReadyToFire
    }

    float _shootPrepTimer;
    const float SHOOT_PREP_DURATION = 0.5f;
    ShootPrepPhase _shootPrepPhase = ShootPrepPhase.OutOfRange;

    bool _movementLocked;
    CommandState _lastPlayerState = CommandState.Follow;
    bool _hasLastPlayerState;

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

        if (!_movementLocked)
            ApplyFriendlyAvoidance();
    }

    void UpdatePlayerUnit()
    {
        CommandState state = CommandSystem.Instance != null
            ? CommandSystem.Instance.CurrentState
            : CommandState.Follow;

        if (!_hasLastPlayerState)
        {
            _lastPlayerState = state;
            _hasLastPlayerState = true;
        }

        if (_lastPlayerState != state)
        {
            if (state == CommandState.Retreat)
                EnterRetreatMode();
            else if (_lastPlayerState == CommandState.Retreat)
                ExitRetreatMode();
        }

        _lastPlayerState = state;

        switch (state)
        {
            case CommandState.Engage:
                if (_attack != null)
                    _attack.enabled = true;
                HandleEngage();
                break;
            case CommandState.Follow:
                if (_attack != null)
                    _attack.enabled = true;
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
            ClearShootPrepState();
            _movementLocked = false;
            _movement.Stop();
            return;
        }

        float dist = (nearest.position - transform.position).magnitude;
        if (data != null && dist <= data.range)
        {
            _movementLocked = true;
            _movement.Stop();
            UpdateShootPrepInRange();
        }
        else
        {
            ClearShootPrepState();
            _movementLocked = false;
            Vector3 dir = (nearest.position - transform.position).normalized;
            _movement.Move(dir);
        }
    }

    void HandleFollow()
    {
        if (CommanderController.Instance == null)
        {
            ClearShootPrepState();
            _movementLocked = false;
            _movement.Stop();
            return;
        }

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
            _movementLocked = true;
            _movement.Stop();
            UpdateShootPrepInRange();
        }
        else
        {
            ClearShootPrepState();
            _movementLocked = false;
            MoveTowardCommander(0.85f);
        }
    }

    void HandleRetreat()
    {
        if (_attack != null)
            _attack.enabled = false;

        ClearShootPrepState();
        _movementLocked = false;
        MoveTowardCommander(1f);
    }

    void UpdateEnemyUnit()
    {
        if (_attack != null)
            _attack.enabled = true;

        Transform nearest = FindNearestAnyUnit();
        if (nearest == null)
        {
            ClearShootPrepState();
            _movementLocked = false;
            _movement.Stop();
            return;
        }

        float dist = (nearest.position - transform.position).magnitude;
        if (data != null && dist <= data.range)
        {
            _movementLocked = true;
            _movement.Stop();
            UpdateShootPrepInRange();
        }
        else
        {
            ClearShootPrepState();
            _movementLocked = false;
            Vector3 dir = (nearest.position - transform.position).normalized;
            _movement.Move(dir);
        }
    }

    void UpdateShootPrepInRange()
    {
        if (_attack == null) return;

        if (_shootPrepPhase == ShootPrepPhase.OutOfRange)
        {
            _shootPrepPhase = ShootPrepPhase.Preparing;
            _shootPrepTimer = SHOOT_PREP_DURATION;
            _attack.CanFire = false;
        }

        if (_shootPrepPhase == ShootPrepPhase.Preparing)
        {
            _shootPrepTimer -= Time.deltaTime;

            if (_shootPrepTimer <= 0f)
            {
                _shootPrepPhase = ShootPrepPhase.ReadyToFire;
                _attack.CanFire = true;
            }
            else
            {
                _attack.CanFire = false;
            }
            return;
        }

        if (_shootPrepPhase == ShootPrepPhase.ReadyToFire)
            _attack.CanFire = true;
    }

    void ClearShootPrepState()
    {
        _shootPrepPhase = ShootPrepPhase.OutOfRange;
        _shootPrepTimer = 0f;
        if (_attack != null)
            _attack.CanFire = false;
    }

    void EnterRetreatMode()
    {
        if (_attack != null)
        {
            _attack.enabled = false;
            _attack.CanFire = false;
        }
        ClearShootPrepState();
        _movementLocked = false;
    }

    void ExitRetreatMode()
    {
        if (_attack != null)
        {
            _attack.enabled = true;
            _attack.CanFire = false;
        }
        ClearShootPrepState();
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
