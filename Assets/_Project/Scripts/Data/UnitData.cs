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

    [Header("Projectile")]
    public float projectileSpeed = 15f;
    public float projectileLifetime = 2f;
    public Color projectileColor = Color.yellow;
}
