using UnityEngine;

[CreateAssetMenu(fileName = "NewUnitData", menuName = "Game/Unit Data")]
public class UnitData : ScriptableObject
{
    [Header("Identity")]
    public string unitName;
    public Color unitColor = Color.white;

    [Header("Sprites (optional)")]
    [Tooltip("12 sprites: idle 0-2, walk 3-8, shoot 9-10. Same layout as Commander. Leave empty for colored quad.")]
    public Sprite[] sprites;

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
