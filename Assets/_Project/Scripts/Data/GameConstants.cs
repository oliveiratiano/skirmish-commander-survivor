using UnityEngine;

public static class GameConstants
{
    public const float ARENA_HALF_SIZE = 60f;
    public const float ARENA_DANGER_ZONE_WIDTH = 5f;
    public const float ARENA_SAFE_HALF_SIZE = ARENA_HALF_SIZE - ARENA_DANGER_ZONE_WIDTH;
    public const float DANGER_ZONE_DPS = 15f;

    public const int DRAFT_BUDGET = 100;
    public const float WAVE_DURATION = 180f;
    public const float SPAWN_RATE_INCREASE_INTERVAL = 30f;
    public const float HIT_FLASH_DURATION = 0.1f;
    public const float RECOIL_DURATION = 0.1f;

    public static readonly Color ARENA_COLOR = new Color(0.184f, 0.176f, 0.165f); // #2F2D2A
    public static readonly Color DANGER_ZONE_COLOR = new Color(0.6f, 0.05f, 0.05f, 0.25f);
    public static readonly Color DANGER_EDGE_COLOR = new Color(0.9f, 0.1f, 0.1f, 0.8f);
}
