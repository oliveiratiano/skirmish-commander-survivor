public static class BuildVersion
{
    public const string VERSION = "0.2.2";
    public const string DATE = "2026-03-12";
    public const string LABEL = "Retreat/Follow Behavior Fix";

    public static string FullString => $"v{VERSION} ({DATE}) - {LABEL}";

    // Version History (newest first)
    // v0.2.2  2026-03-12  Retreat/Follow Behavior Fix
    //   - Prevented units from drifting while stopped to shoot
    //   - Replaced prep booleans with deterministic shoot-prep phases
    //   - Centralized retreat state entry to hard-disable firing
    //
    // v0.2.1  2026-03-12  Shoot Prep Fix
    //   - All units (player + enemy) pause 0.5s before firing
    //   - CanFire gate on RangedAttackComponent wired to AI shoot-prep
    //   - Commander exempt — fires instantly
    //
    // v0.2.0  2026-03-12  Arena Bounds + Difficulty Tuning
    //   - Bounded arena (120x120) with danger zone boundary
    //   - Swarm Bug retuned: HP 20, speed 4.0, damage 7
    //   - 100 enemies, faster spawn ramp (1.5s base, 0.2s min)
    //
    // v0.1.0  2026-03-12  MVP Prototype
    //   - M1-M6: Commander movement, army drafting, combat, command system,
    //     win/loss/scoring, procedural animation, object pooling, GPU instancing
}
