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
    int _unitTypeIndex = -1;
    public int UnitTypeIndex => _unitTypeIndex;
    Transform _promptedIndicator;
    CommandState _relayPendingState;
    float _relayTimer = -1f;

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
    CommandState _currentCommand = CommandState.Follow;
    CommandState _effectiveCommand = CommandState.Follow;
    float _reactionTimer = -1f;
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

        _currentCommand = CommandState.Follow;

        if (isPlayer && GameManager.Instance != null && GameManager.Instance.playerUnitTypes != null)
        {
            var arr = GameManager.Instance.playerUnitTypes;
            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] == unitData) { _unitTypeIndex = i; break; }
            }
        }

        if (isPlayer)
            AllPlayerUnits.Add(this);
        else
            AllEnemyUnits.Add(this);
    }

    void Start()
    {
        if (IsPlayerUnit && _promptedIndicator == null)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            go.name = "PromptedIndicator";
            go.transform.SetParent(transform, worldPositionStays: false);
            go.transform.localPosition = new Vector3(0f, 0.6f, 0f);
            go.transform.localScale = Vector3.one * 0.35f;
            var r = go.GetComponent<Renderer>();
            if (r != null)
            {
                var mat = new Material(Shader.Find("Sprites/Default"));
                mat.color = new Color(1f, 0.9f, 0.3f, 0.85f);
                r.material = mat;
            }
            var col = go.GetComponent<Collider>();
            if (col != null) Object.Destroy(col);
            _promptedIndicator = go.transform;
            _promptedIndicator.gameObject.SetActive(false);
        }
    }

    public void ReceiveCommand(CommandState state, bool withPropagation = false)
    {
        if (state == _currentCommand) return;
        _currentCommand = state;
        _reactionTimer = GameConstants.AI_REACTION_DELAY;
        if (withPropagation)
        {
            _relayPendingState = state;
            _relayTimer = GameConstants.RELAY_HOP_DELAY;
        }
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
        EnsureAttackReference();

        if (_health != null && _health.IsDead) return;

        if (IsPlayerUnit)
        {
            if (_reactionTimer > 0f)
            {
                _reactionTimer -= Time.deltaTime;
                if (_reactionTimer <= 0f)
                {
                    CommandState previousEffective = _effectiveCommand;
                    _effectiveCommand = _currentCommand;
                    if (_effectiveCommand == CommandState.Retreat)
                        EnterRetreatMode();
                    else if (previousEffective == CommandState.Retreat)
                        ExitRetreatMode();
                    _reactionTimer = -1f;
                }
            }
            if (_relayTimer > 0f)
            {
                _relayTimer -= Time.deltaTime;
                if (_relayTimer <= 0f)
                {
                    float r2 = GameConstants.RELAY_RADIUS * GameConstants.RELAY_RADIUS;
                    Vector3 myPos = transform.position;
                    for (int i = 0; i < AllPlayerUnits.Count; i++)
                    {
                        var other = AllPlayerUnits[i];
                        if (other == null || other == this || !other.gameObject.activeInHierarchy) continue;
                        var otherHealth = other.GetComponent<HealthComponent>();
                        if (otherHealth != null && otherHealth.IsDead) continue;
                        if (_unitTypeIndex < 0 || other.UnitTypeIndex != _unitTypeIndex) continue;
                        float distSq = (other.transform.position - myPos).sqrMagnitude;
                        if (distSq <= r2)
                            other.ReceiveCommand(_relayPendingState, true);
                    }
                    _relayTimer = -1f;
                }
            }
            UpdatePlayerUnit();
            if (_promptedIndicator != null)
            {
                var prompted = InputHandler.Instance != null ? InputHandler.Instance.GetPromptedTypeIndices() : null;
                bool show = prompted != null && prompted.Count > 0 && _unitTypeIndex >= 0 && prompted.Contains(_unitTypeIndex);
                _promptedIndicator.gameObject.SetActive(show);
            }
        }
        else
            UpdateEnemyUnit();

        if (!_movementLocked)
            ApplyFriendlyAvoidance();
    }

    void EnsureAttackReference()
    {
        if (_attack == null)
            _attack = GetComponent<RangedAttackComponent>();
    }

    void UpdatePlayerUnit()
    {
        CommandState state = _effectiveCommand;

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
