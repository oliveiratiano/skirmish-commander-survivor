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
    float _feedbackDelay = -1f;
    CommandState _feedbackCommand;

    enum ShootPrepPhase
    {
        OutOfRange,
        Preparing,
        ReadyToFire
    }

    float _shootPrepTimer;
    const float SHOOT_PREP_DURATION = 0.5f;
    ShootPrepPhase _shootPrepPhase = ShootPrepPhase.OutOfRange;

    float _bossMoveTimer;
    Vector3 _bossMoveDir;

    Transform _formUpTarget;

    bool _immuneToBoundaryDamage;
    public bool IsImmuneToBoundaryDamage => _immuneToBoundaryDamage;

    int _swarmThresholdOffset;
    float _swarmRadiusOffset;

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
        {
            AllEnemyUnits.Add(this);
            _swarmThresholdOffset = Random.Range(-1, 2);
            _swarmRadiusOffset = Random.Range(-0.1f, 0.1f);
        }
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

    public void SetFormUpTarget(Transform target)
    {
        _formUpTarget = target;
    }

    public void SetImmuneToBoundaryDamage(bool immune)
    {
        _immuneToBoundaryDamage = immune;
    }

    public void ReceiveCommand(CommandState state, bool withPropagation = false)
    {
        if (state == _currentCommand) return;
        _currentCommand = state;
        _reactionTimer = GameConstants.AI_REACTION_DELAY;
        _feedbackDelay = GameConstants.COMMAND_FEEDBACK_DELAY;
        _feedbackCommand = state;
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
                    if (_effectiveCommand == CommandState.Regroup)
                        EnterRegroupMode();
                    else if (previousEffective == CommandState.Regroup)
                        ExitRegroupMode();
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
            if (_feedbackDelay > 0f)
            {
                _feedbackDelay -= Time.deltaTime;
                if (_feedbackDelay <= 0f)
                {
                    var display = GetComponent<CommandFeedbackDisplay>();
                    if (display != null) display.ShowCommand(_feedbackCommand);
                    _feedbackDelay = -1f;
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
        {
            if (_immuneToBoundaryDamage)
            {
                Vector3 pos = transform.position;
                float safe = GameConstants.ARENA_SAFE_HALF_SIZE;
                if (Mathf.Abs(pos.x) <= safe && Mathf.Abs(pos.y) <= safe)
                    _immuneToBoundaryDamage = false;
            }
            if (data != null && !string.IsNullOrEmpty(data.unitName) && data.unitName.Contains("Boss"))
                UpdateBossUnit();
            else if (_formUpTarget != null && _formUpTarget.gameObject.activeInHierarchy)
                UpdateEscortUnit();
            else
                UpdateEnemyUnit();
        }

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
            case CommandState.Attack:
                if (_attack != null)
                    _attack.enabled = true;
                HandleAttack();
                break;
            case CommandState.StandGround:
                if (_attack != null)
                    _attack.enabled = true;
                HandleStandGround();
                break;
            case CommandState.Regroup:
                HandleRegroup();
                break;
            case CommandState.Follow:
                if (_attack != null)
                    _attack.enabled = true;
                HandleFollow();
                break;
        }
    }

    void HandleStandGround()
    {
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
            _movement.Stop();
        }
    }

    void HandleAttack()
    {
        Transform nearest = FindNearest(AllEnemyUnits);
        if (nearest == null)
        {
            ClearShootPrepState();
            _movementLocked = false;
            MoveTowardCommander(0.85f);
            return;
        }

        float dist = (nearest.position - transform.position).magnitude;
        float minShoot = data != null ? data.range * 0.5f : 2.5f;
        if (data != null && minShoot <= dist && dist <= data.range)
        {
            _movementLocked = true;
            _movement.Stop();
            UpdateShootPrepInRange();
        }
        else if (dist < minShoot)
        {
            ClearShootPrepState();
            _movementLocked = false;
            Vector3 awayFromEnemy = (transform.position - nearest.position).normalized;
            Vector3 dir = GetRetreatDirectionAvoidingDangerZone(transform.position, awayFromEnemy);
            _movement.Move(dir * GameConstants.SWARM_RETREAT_URGENCY);
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

    void HandleRegroup()
    {
        if (_attack != null)
            _attack.enabled = false;

        ClearShootPrepState();
        _movementLocked = false;

        if (CommanderController.Instance != null)
        {
            float dist = (CommanderController.Instance.transform.position - transform.position).magnitude;
            if (dist <= GameConstants.COMMANDER_RADIUS)
            {
                _currentCommand = CommandState.Follow;
                _effectiveCommand = CommandState.Follow;
                ExitRegroupMode();
                return;
            }
        }
        MoveTowardCommander(1f);
    }

    void UpdateBossUnit()
    {
        if (_attack != null)
            _attack.enabled = true;

        if (CommanderController.Instance == null)
        {
            ClearShootPrepState();
            _movementLocked = false;
            _movement.Stop();
            return;
        }

        var cmdHealth = CommanderController.Instance.GetComponent<HealthComponent>();
        if (cmdHealth != null && cmdHealth.IsDead)
        {
            ClearShootPrepState();
            _movementLocked = false;
            _movement.Stop();
            return;
        }

        Vector3 commanderPos = CommanderController.Instance.transform.position;
        float dist = (commanderPos - transform.position).magnitude;

        if (data != null && dist <= data.range)
        {
            _movementLocked = true;
            _movement.Stop();
            UpdateShootPrepInRange();
            return;
        }

        ClearShootPrepState();
        _movementLocked = false;

        _bossMoveTimer -= Time.deltaTime;
        if (_bossMoveTimer <= 0f)
        {
            Vector3 toCommander = (commanderPos - transform.position).normalized;
            float r = Random.value;
            if (r < GameConstants.BOSS_APPROACH_WEIGHT)
            {
                _bossMoveDir = toCommander;
            }
            else if (r < 0.75f)
            {
                _bossMoveDir = new Vector3(-toCommander.y, toCommander.x, 0f).normalized; // strafe left
            }
            else if (r < 0.9f)
            {
                _bossMoveDir = new Vector3(toCommander.y, -toCommander.x, 0f).normalized; // strafe right
            }
            else
            {
                _bossMoveDir = -toCommander; // short retreat
            }
            _bossMoveTimer = Random.Range(GameConstants.BOSS_MOVE_CHANGE_INTERVAL_MIN, GameConstants.BOSS_MOVE_CHANGE_INTERVAL_MAX);
        }

        _movement.Move(_bossMoveDir);
    }

    void UpdateEscortUnit()
    {
        if (_attack != null)
            _attack.enabled = true;

        if (_formUpTarget == null || !_formUpTarget.gameObject.activeInHierarchy)
        {
            _formUpTarget = null;
            UpdateEnemyUnit();
            return;
        }

        bool playerInRange = false;
        if (_attack != null && data != null)
        {
            Transform nearest = FindNearestAnyUnit();
            if (nearest != null)
            {
                float dist = (nearest.position - transform.position).magnitude;
                playerInRange = dist <= data.range;
            }
        }

        if (playerInRange)
        {
            _movementLocked = true;
            _movement.Stop();
            UpdateShootPrepInRange();
        }
        else
        {
            ClearShootPrepState();
            _movementLocked = false;
            MoveTowardTarget(_formUpTarget, GameConstants.BOSS_ESCORT_RADIUS, 0.85f);
        }
    }

    void UpdateEnemyUnit()
    {
        if (_attack != null)
            _attack.enabled = true;

        Transform nearestThreat = FindNearestAnyUnit();
        if (nearestThreat == null)
        {
            ClearShootPrepState();
            _movementLocked = false;
            _movement.Stop();
            return;
        }

        float radius = GameConstants.SWARM_COUNT_RADIUS * (1f + _swarmRadiusOffset);
        radius = Mathf.Max(radius, 2f);
        int playerCount = CountPlayerUnitsInRadius(radius);
        int swarmCount = CountSwarmInRadius(radius);
        int engageThreshold = Mathf.Max(1, GameConstants.SWARM_ENGAGE_THRESHOLD + _swarmThresholdOffset);

        bool shouldRetreat = playerCount > swarmCount || swarmCount < engageThreshold;

        if (shouldRetreat)
        {
            float dist = (nearestThreat.position - transform.position).magnitude;
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
                Transform nearestAlly = FindNearest(AllEnemyUnits);
                Vector3 dir;
                if (nearestAlly != null)
                {
                    dir = (nearestAlly.position - transform.position).normalized;
                }
                else
                {
                    Vector3 awayFromThreat = (transform.position - nearestThreat.position).normalized;
                    dir = awayFromThreat;
                }
                if (dir.sqrMagnitude >= 0.01f)
                {
                    float jitter = (Random.value - 0.5f) * 10f * Mathf.Deg2Rad;
                    dir = new Vector3(dir.x * Mathf.Cos(jitter) - dir.y * Mathf.Sin(jitter),
                        dir.x * Mathf.Sin(jitter) + dir.y * Mathf.Cos(jitter), 0f);
                    _movement.Move(dir * GameConstants.SWARM_RETREAT_URGENCY);
                }
                else
                    _movement.Stop();
            }
        }
        else
        {
            float dist = (nearestThreat.position - transform.position).magnitude;
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
                Vector3 dir = (nearestThreat.position - transform.position).normalized;
                _movement.Move(dir);
            }
        }
    }

    Transform FindNearestInRadius(System.Collections.Generic.List<UnitAIController> list, float radius)
    {
        float r2 = radius * radius;
        Transform best = null;
        float bestDist = float.MaxValue;

        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] == null || list[i] == this) continue;
            if (!list[i].gameObject.activeInHierarchy) continue;
            var h = list[i].GetComponent<HealthComponent>();
            if (h != null && h.IsDead) continue;

            float distSq = (list[i].transform.position - transform.position).sqrMagnitude;
            if (distSq <= r2 && distSq < bestDist)
            {
                bestDist = distSq;
                best = list[i].transform;
            }
        }

        return best;
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

    void EnterRegroupMode()
    {
        if (_attack != null)
        {
            _attack.enabled = false;
            _attack.CanFire = false;
        }
        ClearShootPrepState();
        _movementLocked = false;
    }

    void ExitRegroupMode()
    {
        if (_attack != null)
        {
            _attack.enabled = true;
            _attack.CanFire = false;
        }
        ClearShootPrepState();
    }

    Vector3 GetRetreatDirectionAvoidingDangerZone(Vector3 from, Vector3 awayFromEnemy)
    {
        float safe = GameConstants.ARENA_SAFE_HALF_SIZE;
        float step = 1f;
        Vector3 candidate = from + awayFromEnemy * step;
        bool wouldEnterDanger = Mathf.Abs(candidate.x) > safe || Mathf.Abs(candidate.y) > safe;
        if (!wouldEnterDanger)
            return awayFromEnemy;

        Vector3 towardSafe = Vector3.zero;
        if (Mathf.Abs(from.x) > 0.01f || Mathf.Abs(from.y) > 0.01f)
            towardSafe = new Vector3(-from.x, -from.y, 0f).normalized;

        Vector3 blended = (awayFromEnemy + towardSafe * 2f).normalized;
        Vector3 blendedCandidate = from + blended * step;
        if (Mathf.Abs(blendedCandidate.x) <= safe && Mathf.Abs(blendedCandidate.y) <= safe)
            return blended;

        if (towardSafe.sqrMagnitude > 0.01f)
            return towardSafe;
        return awayFromEnemy;
    }

    void MoveTowardCommander(float urgency)
    {
        if (CommanderController.Instance == null) return;

        Vector3 commanderPos = CommanderController.Instance.transform.position;
        Vector3 toCommander = commanderPos - transform.position;
        float perimeterRadius = GameConstants.COMMANDER_RADIUS;

        if (toCommander.magnitude > perimeterRadius)
        {
            _movement.Move(toCommander.normalized * urgency);
        }
        else
        {
            _movement.Stop();
        }
    }

    void MoveTowardTarget(Transform target, float perimeterRadius, float urgency)
    {
        if (target == null) return;

        Vector3 toTarget = target.position - transform.position;
        if (toTarget.magnitude > perimeterRadius)
            _movement.Move(toTarget.normalized * urgency);
        else
            _movement.Stop();
    }

    int CountPlayerUnitsInRadius(float radius)
    {
        float r2 = radius * radius;
        Vector3 pos = transform.position;
        int count = 0;

        for (int i = 0; i < AllPlayerUnits.Count; i++)
        {
            if (AllPlayerUnits[i] == null || !AllPlayerUnits[i].gameObject.activeInHierarchy) continue;
            var h = AllPlayerUnits[i].GetComponent<HealthComponent>();
            if (h != null && h.IsDead) continue;
            if ((AllPlayerUnits[i].transform.position - pos).sqrMagnitude <= r2)
                count++;
        }

        if (CommanderController.Instance != null)
        {
            var cmdHealth = CommanderController.Instance.GetComponent<HealthComponent>();
            if (cmdHealth != null && !cmdHealth.IsDead)
            {
                if ((CommanderController.Instance.transform.position - pos).sqrMagnitude <= r2)
                    count++;
            }
        }

        return count;
    }

    int CountSwarmInRadius(float radius)
    {
        float r2 = radius * radius;
        Vector3 pos = transform.position;
        int count = 0;

        for (int i = 0; i < AllEnemyUnits.Count; i++)
        {
            if (AllEnemyUnits[i] == null || AllEnemyUnits[i] == this) continue;
            if (!AllEnemyUnits[i].gameObject.activeInHierarchy) continue;
            var h = AllEnemyUnits[i].GetComponent<HealthComponent>();
            if (h != null && h.IsDead) continue;
            if ((AllEnemyUnits[i].transform.position - pos).sqrMagnitude <= r2)
                count++;
        }

        return count;
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
            string poolKey = (data != null && data.unitName != null && data.unitName.Contains("Boss")) ? "Boss" : "Enemy";
            ObjectPool.Instance.Return(poolKey, gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
