public static class BuildVersion
{
    public const string VERSION = "0.2.0";
    public const string DATE = "2026-03-12";
    public const string LABEL = "Arena Bounds + Difficulty Tuning";

    public static string FullString => $"v{VERSION} ({DATE}) - {LABEL}";

    // Version History (newest first)
    // v0.2.0  2026-03-12  Arena Bounds + Difficulty Tuning
    //   - Bounded arena (120x120) with danger zone boundary
    //   - Swarm Bug retuned: HP 20, speed 4.0, damage 7
    //   - 100 enemies, faster spawn ramp (1.5s base, 0.2s min)
    //
    // v0.1.0  2026-03-12  MVP Prototype
    //   - M1-M6: Commander movement, army drafting, combat, command system,
    //     win/loss/scoring, procedural animation, object pooling, GPU instancing
}
