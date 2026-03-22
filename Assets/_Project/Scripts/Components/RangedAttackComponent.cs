using System.Collections.Generic;
using UnityEngine;

public class RangedAttackComponent : MonoBehaviour
{
    public UnitData data;
    public bool isPlayerUnit;
    /// <summary>When true, always target Commander (enemies only).</summary>
    public bool forceCommanderTarget;

    float _cooldownTimer;
    int _burstRemaining;
    float _burstTimer;
    bool _isBursting;

    AudioSource _audioSource;
    AudioClip[] _shotClips;

    static GameObject _projectilePrefab;

    public bool IsShooting => _isBursting;
    public bool CanFire { get; set; } = true;

    void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        _shotClips = LoadShotClips();
    }

    // Loads Audio/SFX/Weapons/shot_{unitname}_0, _1, ... from Resources.
    // See docs/audio-file-conventions.md for naming rules.
    AudioClip[] LoadShotClips()
    {
        if (data == null || string.IsNullOrEmpty(data.unitName)) return null;
        string basePath = $"Audio/SFX/Weapons/shot_{data.unitName.ToLower().Replace(" ", "_").Replace("-", "_")}";
        var list = new List<AudioClip>();
        for (int i = 0; ; i++)
        {
            var clip = Resources.Load<AudioClip>($"{basePath}_{i}");
            if (clip == null) break;
            list.Add(clip);
        }
        return list.Count > 0 ? list.ToArray() : null;
    }

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

        if (!CanFire) return;
        if (_cooldownTimer > 0f) return;

        Transform target = FindTarget();
        if (target == null) return;

        float dist = (target.position - transform.position).magnitude;
        if (dist > data.range) return;

        bool useSpray = data.sprayArcDegrees > 0f && data.burstCount > 1;
        if (useSpray)
        {
            FireSpray(data.burstCount, data.sprayArcDegrees);
            _cooldownTimer = data.cooldown;
        }
        else if (data.burstCount > 1)
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

        Vector3 toTarget = GetLeadDirection(target);
        float deviation = UnityEngine.Random.Range(-data.accuracySpreadDegrees, data.accuracySpreadDegrees);
        Vector3 direction = Quaternion.Euler(0f, 0f, deviation) * toTarget;

        SpawnProjectile(direction);
    }

    void FireSpray(int count, float arcDegrees)
    {
        Transform target = FindTarget();
        if (target == null) return;

        Vector3 toTarget = GetLeadDirection(target);
        float halfArc = arcDegrees * 0.5f;
        float step = count > 1 ? arcDegrees / (count - 1) : 0f;

        for (int i = 0; i < count; i++)
        {
            float angle = count > 1 ? -halfArc + step * i : 0f;
            float deviation = UnityEngine.Random.Range(-data.accuracySpreadDegrees, data.accuracySpreadDegrees);
            Vector3 direction = Quaternion.Euler(0f, 0f, angle + deviation) * toTarget;
            SpawnProjectile(direction);
        }
    }

    Vector3 GetLeadDirection(Transform target)
    {
        Vector3 toTarget = target.position - transform.position;
        float dist = toTarget.magnitude;
        if (dist < 0.01f) return toTarget.normalized;

        var movement = target.GetComponent<MovementComponent>();
        if (movement == null || data.leadFactor <= 0f || data.projectileSpeed <= 0f)
            return toTarget.normalized;

        float timeToHit = dist / data.projectileSpeed;
        Vector3 predicted = target.position + movement.Velocity * timeToHit;
        Vector3 aimPoint = Vector3.Lerp(target.position, predicted, data.leadFactor);
        return (aimPoint - transform.position).normalized;
    }

    void PlayShotSound()
    {
        if (_audioSource == null || _shotClips == null || _shotClips.Length == 0) return;

        // Distance cull: skip audio if too far from Commander to keep voice count manageable
        if (CommanderController.Instance != null)
        {
            float sqrDist = (transform.position - CommanderController.Instance.transform.position).sqrMagnitude;
            if (sqrDist > GameConstants.SHOT_AUDIO_MAX_DISTANCE * GameConstants.SHOT_AUDIO_MAX_DISTANCE) return;
        }

        AudioClip clip = _shotClips[UnityEngine.Random.Range(0, _shotClips.Length)];
        _audioSource.pitch = 1f + UnityEngine.Random.Range(-GameConstants.SHOT_AUDIO_PITCH_VARIANCE, GameConstants.SHOT_AUDIO_PITCH_VARIANCE);
        _audioSource.PlayOneShot(clip, data.shotVolume);
    }

    void SpawnProjectile(Vector3 direction)
    {
        EnsurePrefab();
        PlayShotSound();

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
            data.damage, isPlayerUnit, data.projectileColor,
            data.explosionRadius, data.splashDamageMultiplier);
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
        if (forceCommanderTarget && CommanderController.Instance != null)
        {
            var h = CommanderController.Instance.GetComponent<HealthComponent>();
            if (h != null && !h.IsDead)
                return CommanderController.Instance.transform;
            return null;
        }
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
