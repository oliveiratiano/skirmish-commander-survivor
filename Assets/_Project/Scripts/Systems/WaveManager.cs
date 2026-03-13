using UnityEngine;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }

    [Header("Wave Config")]
    public UnitData enemyData;
    public int totalEnemies = 100;
    public float baseSpawnInterval = 1.5f;
    public float minSpawnInterval = 0.2f;

    int _spawnedCount;
    int _killedCount;
    float _spawnTimer;
    float _elapsedTime;
    float _currentSpawnInterval;
    bool _active;

    static GameObject _enemyPrefab;

    public int SpawnedCount => _spawnedCount;
    public int KilledCount => _killedCount;
    public int RemainingToSpawn => totalEnemies - _spawnedCount;
    public int AliveCount => _spawnedCount - _killedCount;
    public bool AllSpawned => _spawnedCount >= totalEnemies;
    public bool AllDead => AllSpawned && _killedCount >= totalEnemies;
    public float CurrentSpawnRate => _active && _currentSpawnInterval > 0 ? 1f / _currentSpawnInterval : 0f;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void StartWave()
    {
        _spawnedCount = 0;
        _killedCount = 0;
        _elapsedTime = 0f;
        _currentSpawnInterval = baseSpawnInterval;
        _spawnTimer = 0f;
        _active = true;

        EnsureEnemyPrefab();
    }

    public void StopWave()
    {
        _active = false;
    }

    void Update()
    {
        if (!_active) return;
        if (AllSpawned) return;

        _elapsedTime += Time.deltaTime;

        int intervals = Mathf.FloorToInt(_elapsedTime / GameConstants.SPAWN_RATE_INCREASE_INTERVAL);
        _currentSpawnInterval = Mathf.Max(minSpawnInterval,
            baseSpawnInterval - intervals * 0.25f);

        _spawnTimer -= Time.deltaTime;
        if (_spawnTimer <= 0f)
        {
            SpawnEnemy();
            _spawnTimer = _currentSpawnInterval;
        }

        UpdateDebugOverlay();
    }

    void SpawnEnemy()
    {
        if (AllSpawned) return;

        Vector3 pos = GetSpawnPosition();

        GameObject go = null;
        if (ObjectPool.Instance != null)
            go = ObjectPool.Instance.Get("Enemy", pos);

        if (go == null)
        {
            EnsureEnemyPrefab();
            go = Object.Instantiate(_enemyPrefab);
            go.transform.position = pos;
            go.SetActive(true);
        }

        ResetEnemy(go, pos);
        _spawnedCount++;
    }

    void ResetEnemy(GameObject go, Vector3 pos)
    {
        go.transform.position = pos;
        go.name = "SwarmBug";

        var health = go.GetComponent<HealthComponent>();
        if (health != null)
        {
            health.Initialize(enemyData.maxHP);
            health.OnDied -= OnEnemyDied;
            health.OnDied += OnEnemyDied;
        }

        var move = go.GetComponent<MovementComponent>();
        if (move != null)
            move.moveSpeed = enemyData.moveSpeed;

        var ai = go.GetComponent<UnitAIController>();
        if (ai != null)
        {
            if (ai.data == null)
                ai.Initialize(enemyData, isPlayer: false);
            else if (!UnitAIController.AllEnemyUnits.Contains(ai))
                UnitAIController.AllEnemyUnits.Add(ai);
        }

        var attack = go.GetComponent<RangedAttackComponent>();
        if (attack != null)
        {
            attack.data = enemyData;
            attack.isPlayerUnit = false;
        }

        var flash = go.GetComponent<HitFlashComponent>();
        if (flash != null)
            flash.ResetFlash();

        var anim = go.GetComponent<ProceduralAnimator>();
        if (anim != null)
            anim.ResetAnimation();
    }

    void OnEnemyDied()
    {
        _killedCount++;
    }

    Vector3 GetSpawnPosition()
    {
        Camera cam = Camera.main;
        float camHeight = cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;
        Vector3 camPos = cam.transform.position;
        float margin = 3f;

        int side = Random.Range(0, 4);
        float x, y;

        switch (side)
        {
            case 0: // top
                x = Random.Range(camPos.x - camWidth - margin, camPos.x + camWidth + margin);
                y = camPos.y + camHeight + margin;
                break;
            case 1: // bottom
                x = Random.Range(camPos.x - camWidth - margin, camPos.x + camWidth + margin);
                y = camPos.y - camHeight - margin;
                break;
            case 2: // right
                x = camPos.x + camWidth + margin;
                y = Random.Range(camPos.y - camHeight - margin, camPos.y + camHeight + margin);
                break;
            default: // left
                x = camPos.x - camWidth - margin;
                y = Random.Range(camPos.y - camHeight - margin, camPos.y + camHeight + margin);
                break;
        }

        float half = GameConstants.ARENA_HALF_SIZE;
        x = Mathf.Clamp(x, -half, half);
        y = Mathf.Clamp(y, -half, half);

        return new Vector3(x, y, 0f);
    }

    void EnsureEnemyPrefab()
    {
        if (_enemyPrefab != null) return;

        if (enemyData.sprites != null && enemyData.sprites.Length > 0)
        {
            _enemyPrefab = CreateEnemyPrefabWithSprites();
        }
        else
        {
            _enemyPrefab = GameManager.CreatePrimitive("EnemyPrefab", Vector3.zero, enemyData.unitColor, 0.7f);
            var move = _enemyPrefab.AddComponent<MovementComponent>();
            move.moveSpeed = enemyData.moveSpeed;
            _enemyPrefab.AddComponent<HealthComponent>();
            _enemyPrefab.AddComponent<UnitAIController>();
            var attack = _enemyPrefab.AddComponent<RangedAttackComponent>();
            attack.data = enemyData;
            attack.isPlayerUnit = false;
            _enemyPrefab.AddComponent<HitFlashComponent>();
            _enemyPrefab.AddComponent<ProceduralAnimator>();
            _enemyPrefab.AddComponent<IsometricSorting>();
        }

        _enemyPrefab.SetActive(false);

        if (ObjectPool.Instance != null)
            ObjectPool.Instance.Prewarm("Enemy", _enemyPrefab, 100);
    }

    GameObject CreateEnemyPrefabWithSprites()
    {
        var go = new GameObject("EnemyPrefab");
        go.transform.position = Vector3.zero;
        go.transform.localScale = Vector3.one * GameConstants.ENEMY_SPRITE_SCALE;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = enemyData.sprites[0];
        sr.material = new Material(Shader.Find("Sprites/Default"));
        sr.material.color = Color.white;

        var move = go.AddComponent<MovementComponent>();
        move.moveSpeed = enemyData.moveSpeed;
        go.AddComponent<HealthComponent>();
        go.AddComponent<UnitAIController>();
        var attack = go.AddComponent<RangedAttackComponent>();
        attack.data = enemyData;
        attack.isPlayerUnit = false;
        go.AddComponent<HitFlashComponent>();
        go.AddComponent<ProceduralAnimator>();
        var anim = go.AddComponent<SpriteSheetAnimator>();
        anim.SetSprites(enemyData.sprites);
        go.AddComponent<IsometricSorting>();

        return go;
    }

    void UpdateDebugOverlay()
    {
        if (DebugOverlay.Instance != null)
        {
            DebugOverlay.Instance.SetEnemyCount(AliveCount);
            DebugOverlay.Instance.SetSpawnRate(CurrentSpawnRate);
            if (ObjectPool.Instance != null)
            {
                DebugOverlay.Instance.SetPooledEnemies(ObjectPool.Instance.GetAvailableCount("Enemy"));
                DebugOverlay.Instance.SetPooledProjectiles(ObjectPool.Instance.GetAvailableCount("Projectile"));
            }
        }

        // Dynamic camera zoom
        var cam = Camera.main != null ? Camera.main.GetComponent<CameraController>() : null;
        if (cam != null)
            cam.SetZoomByEnemyCount(AliveCount);
    }
}
