using UnityEngine;

[CreateAssetMenu(fileName = "NewUnitData", menuName = "Game/Unit Data")]
public class UnitData : ScriptableObject
{
    [Header("Identity")]
    public string unitName;
    public Color unitColor = Color.white;

    [Header("Sprites (optional) — 3 directional sheets, 25 frames each (5×5). Idle = center frame (index 12).")]
    [Tooltip("25 sprites: movement up. Leave empty for colored quad.")]
    public Sprite[] spritesUp;
    [Tooltip("25 sprites: movement right (flipped for left).")]
    public Sprite[] spritesRight;
    [Tooltip("25 sprites: movement down.")]
    public Sprite[] spritesDown;

    [Header("Economy")]
    public int cost;

    [Header("Stats")]
    public float maxHP = 10f;
    public float moveSpeed = 3.5f;

    [Header("Combat")]
    public float damage = 5f;
    public float range = 5f;
    public float accuracySpreadDegrees = 15f;

    [Header("Fire Rate")]
    public float cooldown = 1f;
    public int burstCount = 1;
    public float burstInterval = 0.1f;
    [Tooltip("When > 0 and burstCount > 1, fire all bullets in a spray arc (total degrees).")]
    public float sprayArcDegrees;

    [Tooltip("How much to lead moving targets (0 = none, 1 = full prediction). Spread cone still applies on top.")]
    [Range(0f, 1f)]
    public float leadFactor = 0.6f;

    [Header("Projectile")]
    public float projectileSpeed = 15f;
    public float projectileLifetime = 2f;
    public Color projectileColor = Color.yellow;

    [Header("Audio")]
    [Tooltip("Playback volume for this unit's shot sound. 1.0 = default; 1.2 = 20% louder.")]
    public float shotVolume = 1.0f;

}
