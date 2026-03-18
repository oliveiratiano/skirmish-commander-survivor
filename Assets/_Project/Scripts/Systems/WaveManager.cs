using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }
    public static GameObject ActiveBoss { get; private set; }

    [Header("Wave Config")]
    public UnitData enemyData;
    public UnitData bossData;
    [Tooltip("Escort swarm bugs that form up around the boss. Fallback to enemyData with 30% speed if null.")]
    public UnitData escortData;
    public int totalEnemies = 180;
    public float baseSpawnInterval = 1.5f;
    public float minSpawnInterval = 0.2f;

    int _spawnedCount;
    int _killedCount;
    float _spawnTimer;
    float _elapsedTime;
    float _currentSpawnInterval;
    bool _active;

    static GameObject _enemyPrefab;
    static GameObject _bossPrefab;
    const int BOSS_COUNT = 3;
    int _bossesSpawned;
    int _bossesKilled;
    const int ESCORT_COUNT = 10;
    int _escortKilled;
    Dictionary<GameObject, Action> _bossDeathHandlers = new Dictionary<GameObject, Action>();

    public int SpawnedCount => _spawnedCount;
    public int KilledCount => _killedCount + _bossesKilled;
    public int RemainingToSpawn => totalEnemies - _spawnedCount;
    public int AliveCount => _spawnedCount - _killedCount + (BOSS_COUNT - _bossesKilled) + (BOSS_COUNT * ESCORT_COUNT - _escortKilled);
    public bool AllSpawned => _spawnedCount >= totalEnemies;
    public bool AllDead => AllSpawned && _killedCount >= totalEnemies && _bossesKilled >= BOSS_COUNT && _escortKilled >= BOSS_COUNT * ESCORT_COUNT;
    public float CurrentSpawnRate => _active && _currentSpawnInterval > 0 ? 1f / _currentSpawnInterval : 0f;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void StartWave()
    {
        _spawnedCount = 0;
        _killedCount = 0;
        _bossesSpawned = 0;
        _bossesKilled = 0;
        _escortKilled = 0;
        _bossDeathHandlers.Clear();
        _elapsedTime = 0f;
        _currentSpawnInterval = baseSpawnInterval;
        _spawnTimer = 0f;
        _active = true;

        EnsureEnemyPrefab();
        if (bossData != null) EnsureBossPrefab();
    }

    public void StopWave()
    {
        _active = false;
    }

    void Update()
    {
        if (!_active) return;

        _elapsedTime += Time.deltaTime;

        if (!AllSpawned)
        {
            int intervals = Mathf.FloorToInt(_elapsedTime / GameConstants.SPAWN_RATE_INCREASE_INTERVAL);
            _currentSpawnInterval = Mathf.Max(minSpawnInterval,
                baseSpawnInterval - intervals * 0.25f);

            _spawnTimer -= Time.deltaTime;
            if (_spawnTimer <= 0f)
            {
                SpawnEnemy();
                _spawnTimer = _currentSpawnInterval;
            }
        }

        if (bossData != null && _bossesSpawned < BOSS_COUNT)
        {
            float nextTime = _bossesSpawned == 0 ? GameConstants.BOSS_SPAWN_TIME_1
                : _bossesSpawned == 1 ? GameConstants.BOSS_SPAWN_TIME_2
                : GameConstants.BOSS_SPAWN_TIME_3;
            if (_elapsedTime >= nextTime)
            {
                SpawnBoss();
                _bossesSpawned++;
            }
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
            ai.SetFormUpTarget(null);
            ai.SetImmuneToBoundaryDamage(true);
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

        bool hasSprites = (enemyData.spritesUp != null && enemyData.spritesUp.Length > 0) || (enemyData.spritesRight != null && enemyData.spritesRight.Length > 0) || (enemyData.spritesDown != null && enemyData.spritesDown.Length > 0);
        if (hasSprites)
            _enemyPrefab = CreateEnemyPrefabWithSprites();
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
            ConfigureShotAudio(_enemyPrefab);
            _enemyPrefab.AddComponent<HitFlashComponent>();
            _enemyPrefab.AddComponent<ProceduralAnimator>();
            _enemyPrefab.AddComponent<IsometricSorting>();
        }

        _enemyPrefab.SetActive(false);

        if (ObjectPool.Instance != null)
            ObjectPool.Instance.Prewarm("Enemy", _enemyPrefab, 180);
    }

    GameObject CreateEnemyPrefabWithSprites()
    {
        var go = new GameObject("EnemyPrefab");
        go.transform.position = Vector3.zero;
        go.transform.localScale = Vector3.one * GameConstants.ENEMY_SPRITE_SCALE;

        var sr = go.AddComponent<SpriteRenderer>();
        Sprite[] first = enemyData.spritesDown ?? enemyData.spritesUp ?? enemyData.spritesRight;
        int idleIdx = GameConstants.SPRITE_SHEET_IDLE_FRAME_INDEX;
        sr.sprite = first != null && first.Length > idleIdx ? first[idleIdx] : (first != null && first.Length > 0 ? first[0] : null);
        sr.material = new Material(Shader.Find("Sprites/Default"));
        sr.material.color = Color.white;

        var move = go.AddComponent<MovementComponent>();
        move.moveSpeed = enemyData.moveSpeed;
        go.AddComponent<HealthComponent>();
        go.AddComponent<UnitAIController>();
        var attack = go.AddComponent<RangedAttackComponent>();
        attack.data = enemyData;
        attack.isPlayerUnit = false;
        ConfigureShotAudio(go);
        go.AddComponent<HitFlashComponent>();
        go.AddComponent<ProceduralAnimator>();
        var anim = go.AddComponent<SpriteSheetAnimator>();
        anim.SetDirectionalSprites(enemyData.spritesUp, enemyData.spritesRight, enemyData.spritesDown);
        go.AddComponent<IsometricSorting>();

        return go;
    }

    void EnsureBossPrefab()
    {
        if (_bossPrefab != null || bossData == null) return;

        bool hasSprites = (bossData.spritesUp != null && bossData.spritesUp.Length > 0) || (bossData.spritesRight != null && bossData.spritesRight.Length > 0) || (bossData.spritesDown != null && bossData.spritesDown.Length > 0);
        if (hasSprites)
            _bossPrefab = CreateBossPrefabWithSprites();
        else
        {
            _bossPrefab = GameManager.CreatePrimitive("BossPrefab", Vector3.zero, bossData.unitColor, 0.85f);
            _bossPrefab.transform.localScale = Vector3.one * GameConstants.BOSS_SPRITE_SCALE;
            var move = _bossPrefab.AddComponent<MovementComponent>();
            move.moveSpeed = bossData.moveSpeed;
            _bossPrefab.AddComponent<HealthComponent>();
            _bossPrefab.AddComponent<UnitAIController>();
            var attack = _bossPrefab.AddComponent<RangedAttackComponent>();
            attack.data = bossData;
            attack.isPlayerUnit = false;
            ConfigureShotAudio(_bossPrefab);
            _bossPrefab.AddComponent<HitFlashComponent>();
            _bossPrefab.AddComponent<ProceduralAnimator>();
            _bossPrefab.AddComponent<IsometricSorting>();
        }

        _bossPrefab.SetActive(false);

        if (ObjectPool.Instance != null)
            ObjectPool.Instance.Prewarm("Boss", _bossPrefab, 5);
    }

    GameObject CreateBossPrefabWithSprites()
    {
        var go = new GameObject("BossPrefab");
        go.transform.position = Vector3.zero;
        go.transform.localScale = Vector3.one * GameConstants.BOSS_SPRITE_SCALE;

        var sr = go.AddComponent<SpriteRenderer>();
        Sprite[] first = bossData.spritesDown ?? bossData.spritesUp ?? bossData.spritesRight;
        int idleIdx = GameConstants.SPRITE_SHEET_IDLE_FRAME_INDEX;
        sr.sprite = first != null && first.Length > idleIdx ? first[idleIdx] : (first != null && first.Length > 0 ? first[0] : null);
        sr.material = new Material(Shader.Find("Sprites/Default"));
        sr.material.color = Color.white;

        var move = go.AddComponent<MovementComponent>();
        move.moveSpeed = bossData.moveSpeed;
        go.AddComponent<HealthComponent>();
        go.AddComponent<UnitAIController>();
        var attack = go.AddComponent<RangedAttackComponent>();
        attack.data = bossData;
        attack.isPlayerUnit = false;
        ConfigureShotAudio(go);
        go.AddComponent<HitFlashComponent>();
        go.AddComponent<ProceduralAnimator>();
        var anim = go.AddComponent<SpriteSheetAnimator>();
        anim.SetDirectionalSprites(bossData.spritesUp, bossData.spritesRight, bossData.spritesDown);
        go.AddComponent<IsometricSorting>();

        return go;
    }

    void SpawnBoss()
    {
        if (bossData == null) return;

        Vector3 pos = GetSpawnPosition();

        GameObject go = null;
        if (ObjectPool.Instance != null)
            go = ObjectPool.Instance.Get("Boss", pos);

        if (go == null)
        {
            EnsureBossPrefab();
            go = Object.Instantiate(_bossPrefab);
            go.transform.position = pos;
            go.SetActive(true);
        }

        ResetBoss(go, pos);
        SpawnBossEscorts(go);
    }

    void ResetBoss(GameObject go, Vector3 pos)
    {
        ActiveBoss = go;
        go.transform.position = pos;
        go.name = "SwarmBugBoss";

        var health = go.GetComponent<HealthComponent>();
        if (health != null)
        {
            health.Initialize(bossData.maxHP);
            if (_bossDeathHandlers.TryGetValue(go, out Action oldHandler))
            {
                health.OnDied -= oldHandler;
                _bossDeathHandlers.Remove(go);
            }
            Action handler = () => OnBossDied(go);
            _bossDeathHandlers[go] = handler;
            health.OnDied += handler;
        }

        var move = go.GetComponent<MovementComponent>();
        if (move != null)
            move.moveSpeed = bossData.moveSpeed;

        var ai = go.GetComponent<UnitAIController>();
        if (ai != null)
        {
            if (ai.data == null)
                ai.Initialize(bossData, isPlayer: false);
            else if (!UnitAIController.AllEnemyUnits.Contains(ai))
                UnitAIController.AllEnemyUnits.Add(ai);
            ai.SetImmuneToBoundaryDamage(true);
        }

        var attack = go.GetComponent<RangedAttackComponent>();
        if (attack != null)
        {
            attack.data = bossData;
            attack.isPlayerUnit = false;
            attack.forceCommanderTarget = true;
        }

        var flash = go.GetComponent<HitFlashComponent>();
        if (flash != null)
            flash.ResetFlash();

        var anim = go.GetComponent<ProceduralAnimator>();
        if (anim != null)
            anim.ResetAnimation();

        var spriteAnim = go.GetComponent<SpriteSheetAnimator>();
        if (spriteAnim != null)
            spriteAnim.SetDirectionalSprites(bossData.spritesUp, bossData.spritesRight, bossData.spritesDown);
    }

    void OnBossDied(GameObject boss)
    {
        if (_bossDeathHandlers.TryGetValue(boss, out Action h))
        {
            var health = boss.GetComponent<HealthComponent>();
            if (health != null) health.OnDied -= h;
            _bossDeathHandlers.Remove(boss);
        }
        if (ActiveBoss == boss)
        {
            ActiveBoss = null;
            for (int i = 0; i < UnitAIController.AllEnemyUnits.Count; i++)
            {
                var u = UnitAIController.AllEnemyUnits[i];
                if (u != null && u.gameObject.activeInHierarchy && u.data != null
                    && u.data.unitName != null && u.data.unitName.Contains("Boss"))
                {
                    ActiveBoss = u.gameObject;
                    break;
                }
            }
        }
        _bossesKilled++;
    }

    void SpawnBossEscorts(GameObject boss)
    {
        UnitData escort = escortData != null ? escortData : enemyData;
        float speedOverride = escortData == null ? enemyData.moveSpeed * 1.3f : 0f; // 30% faster when using fallback
        if (escort == null) return;

        Vector3 bossPos = boss.transform.position;
        for (int i = 0; i < ESCORT_COUNT; i++)
        {
            Vector2 offset = Random.insideUnitCircle * 3f;
            Vector3 pos = bossPos + new Vector3(offset.x, offset.y, 0f);

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

            ResetBossEscort(go, pos, escort, boss.transform, speedOverride);
        }
    }

    void ResetBossEscort(GameObject go, Vector3 pos, UnitData escort, Transform bossTransform, float speedOverride = -1f)
    {
        go.transform.position = pos;
        go.name = "SwarmBugEscort";

        var health = go.GetComponent<HealthComponent>();
        if (health != null)
        {
            health.Initialize(escort.maxHP);
            health.OnDied -= OnEnemyDied;
            health.OnDied -= OnBossEscortDied;
            health.OnDied += OnBossEscortDied;
        }

        var move = go.GetComponent<MovementComponent>();
        if (move != null)
            move.moveSpeed = speedOverride > 0 ? speedOverride : escort.moveSpeed;

        var ai = go.GetComponent<UnitAIController>();
        if (ai != null)
        {
            if (ai.data == null)
                ai.Initialize(escort, isPlayer: false);
            else if (!UnitAIController.AllEnemyUnits.Contains(ai))
                UnitAIController.AllEnemyUnits.Add(ai);
            ai.SetImmuneToBoundaryDamage(true);
            ai.SetFormUpTarget(bossTransform);
        }

        var attack = go.GetComponent<RangedAttackComponent>();
        if (attack != null)
        {
            attack.data = escort;
            attack.isPlayerUnit = false;
        }

        var flash = go.GetComponent<HitFlashComponent>();
        if (flash != null)
            flash.ResetFlash();

        var anim = go.GetComponent<ProceduralAnimator>();
        if (anim != null)
            anim.ResetAnimation();

        var spriteAnim = go.GetComponent<SpriteSheetAnimator>();
        if (spriteAnim != null)
            spriteAnim.SetDirectionalSprites(escort.spritesUp, escort.spritesRight, escort.spritesDown);
    }

    void OnBossEscortDied()
    {
        _escortKilled++;
    }

    static void ConfigureShotAudio(GameObject go)
    {
        var sfx = go.AddComponent<AudioSource>();
        sfx.spatialBlend = 1f;
        sfx.rolloffMode = AudioRolloffMode.Linear;
        sfx.minDistance = 3f;
        sfx.maxDistance = 30f;
        sfx.playOnAwake = false;
        sfx.loop = false;
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
