using UnityEngine;

public class CommanderBurstAttack : MonoBehaviour
{
    [SerializeField] public CommanderActionsData actionsData;

    int _currentAmmo;
    float _reloadTimer;
    float _fireIntervalTimer;
    bool _spaceWasPressed;

    UnitData _commanderData;
    static readonly Color BURST_COLOR = new Color(0.85f, 0.97f, 1f);

    public int CurrentAmmo => _currentAmmo;
    public int MaxAmmo => actionsData != null ? actionsData.burstMaxAmmo : 0;
    public bool CanReloadNow => _currentAmmo < MaxAmmo && _fireIntervalTimer <= 0f;
    public float ReloadProgress => actionsData != null ? _reloadTimer / CurrentReloadTime : 0f;

    void Start()
    {
        if (actionsData == null)
        {
            Debug.LogError("CommanderBurstAttack: No CommanderActionsData assigned!");
            enabled = false;
            return;
        }
        _currentAmmo = actionsData.burstMaxAmmo;

        if (GameManager.Instance != null)
            _commanderData = GameManager.Instance.commanderData;
    }

    void Update()
    {
        UpdateReload();
        UpdateFiring();
    }

    float CurrentReloadTime
    {
        get
        {
            bool isMoving = InputHandler.Instance != null &&
                            InputHandler.Instance.MoveInput.sqrMagnitude > 0.01f;
            return isMoving ? actionsData.burstReloadTime : actionsData.burstReloadTimeIdle;
        }
    }

    void UpdateReload()
    {
        if (_currentAmmo >= actionsData.burstMaxAmmo) return;
        if (_fireIntervalTimer > 0f) return;

        float reloadTime = CurrentReloadTime;
        _reloadTimer += Time.deltaTime;
        if (_reloadTimer >= reloadTime)
        {
            _reloadTimer -= reloadTime;
            _currentAmmo++;
            AudioManager.Instance.PlayBurstReload();
            Debug.Log($"[BurstAttack] Reloaded! Ammo: {_currentAmmo}/{actionsData.burstMaxAmmo}");
        }
    }

    void UpdateFiring()
    {
        if (_fireIntervalTimer > 0f)
            _fireIntervalTimer -= Time.deltaTime;

        bool spaceHeld = Input.GetKey(KeyCode.Space);
        bool spaceDown = !_spaceWasPressed && spaceHeld;
        _spaceWasPressed = spaceHeld;

        bool shouldFire = false;
        if (spaceDown)
            shouldFire = true;
        else if (spaceHeld && _fireIntervalTimer <= 0f)
            shouldFire = true;

        if (!shouldFire) return;
        if (_fireIntervalTimer > 0f) return;

        if (_currentAmmo <= 0)
        {
            if (spaceDown)
                AudioManager.Instance.PlayBurstNoTarget();
            return;
        }

        if (_commanderData == null) return;

        Transform target = FindClosestEnemy();
        if (target == null)
        {
            if (spaceDown)
                AudioManager.Instance.PlayBurstNoTarget();
            return;
        }

        _currentAmmo--;
        _fireIntervalTimer = actionsData.burstFireInterval;
        _reloadTimer = 0f;

        SpawnBurstProjectile(target);
        AudioManager.Instance.PlayBurstShot();
        Debug.Log($"[BurstAttack] Fired! Ammo: {_currentAmmo}/{actionsData.burstMaxAmmo}");
    }

    Transform FindClosestEnemy()
    {
        float range = _commanderData.range;
        float rangeSqr = range * range;
        Transform best = null;
        float bestDist = float.MaxValue;

        for (int i = 0; i < UnitAIController.AllEnemyUnits.Count; i++)
        {
            var enemy = UnitAIController.AllEnemyUnits[i];
            if (enemy == null || !enemy.gameObject.activeInHierarchy) continue;
            var h = enemy.GetComponent<HealthComponent>();
            if (h != null && h.IsDead) continue;

            float d = (enemy.transform.position - transform.position).sqrMagnitude;
            if (d < bestDist && d <= rangeSqr)
            {
                bestDist = d;
                best = enemy.transform;
            }
        }

        return best;
    }

    void SpawnBurstProjectile(Transform target)
    {
        Vector3 direction = (target.position - transform.position).normalized;
        float deviation = Random.Range(-_commanderData.accuracySpreadDegrees, _commanderData.accuracySpreadDegrees);
        Vector3 dir = Quaternion.Euler(0f, 0f, deviation) * direction;

        GameObject go = null;
        if (ObjectPool.Instance != null)
            go = ObjectPool.Instance.Get("Projectile", transform.position);

        if (go == null)
        {
            go = GameManager.CreatePrimitive("Projectile", transform.position, BURST_COLOR, 0.2f);
            go.AddComponent<ProjectileComponent>();
        }

        var proj = go.GetComponent<ProjectileComponent>();
        proj.Initialize(dir, _commanderData.projectileSpeed, _commanderData.projectileLifetime,
            _commanderData.damage, true, BURST_COLOR,
            _commanderData.explosionRadius, _commanderData.splashDamageMultiplier);
    }
}
