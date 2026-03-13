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

    // Shout oval (initial/baseline size; tune here, not from a previous progression)
    public const float SHOUT_OVAL_FORWARD_RADIUS = 10f;
    public const float SHOUT_OVAL_SIDE_RADIUS = 6f;
    public const float SHOUT_OVAL_OFFSET = 2f;
    public const float ATTENTION_PROMPT_TIMEOUT = 5f;
    public const float AI_REACTION_DELAY = 0.3f;
    public const float RELAY_HOP_DELAY = 0.5f;
    public const float RELAY_RADIUS = 13f;
    public const float COMMAND_FEEDBACK_DELAY = 1f;
    public const float COMMAND_FEEDBACK_DURATION = 1f;
    public const float COMMANDER_RADIUS = 2.5f;

    // Isometric view: camera tilt and offset (shared with ProceduralAnimator quad tilt). Stronger angle = more obvious isometric look.
    public const float ISOMETRIC_CAMERA_ANGLE = 45f;
    public const float ISOMETRIC_CAMERA_OFFSET_Y = 16f;   // 20% closer than previous 20
    public const float ISOMETRIC_CAMERA_OFFSET_Z = -19.2f; // 20% closer than previous -24
    public const int ISOMETRIC_SORT_BASE_ORDER = 1000;
    public const int ISOMETRIC_SORT_SCALE = 10;
    public const int ISOMETRIC_SORT_PROJECTILE_ORDER = 10000;

    // Sprite sheet dimensions (single source of truth — must match SPEC and art pipeline). 5x5 = 25 frames per direction.
    public const int SPRITE_SHEET_GRID_COLS = 5;
    public const int SPRITE_SHEET_GRID_ROWS = 5;
    public const int SPRITE_SHEET_CELL_WIDTH = 256;
    public const int SPRITE_SHEET_CELL_HEIGHT = 256;
    public const int SPRITE_SHEET_WIDTH = SPRITE_SHEET_GRID_COLS * SPRITE_SHEET_CELL_WIDTH;  // 1280
    public const int SPRITE_SHEET_HEIGHT = SPRITE_SHEET_GRID_ROWS * SPRITE_SHEET_CELL_HEIGHT; // 1280
    public const int SPRITE_SHEET_IDLE_FRAME_INDEX = 12; // center cell (row 3, col 3 in 1-based; row 2, col 2 in 0-based)
    public const float SPRITE_SHEET_PIXELS_PER_UNIT = 64f;

    // Commander sprite sheet: scale so 256px @ 64 PPU fits ~1 unit; animation cycle speed
    public const float COMMANDER_SPRITE_SCALE = 0.56f; // ~50% larger again (was 0.375)
    public const float UNIT_SPRITE_SCALE = 0.2f;
    public const float ENEMY_SPRITE_SCALE = 0.175f;
    public const float SPRITE_SHEET_FRAMES_PER_SECOND = 10f;
    public const float SHOOT_ANIM_DURATION = 0.2f;

    public static readonly Color ARENA_COLOR = new Color(0.184f, 0.176f, 0.165f); // #2F2D2A
    public static readonly Color DANGER_ZONE_COLOR = new Color(0.6f, 0.05f, 0.05f, 0.25f);
    public static readonly Color DANGER_EDGE_COLOR = new Color(0.9f, 0.1f, 0.1f, 0.8f);

    public static Color CommandIconColor(CommandState state)
    {
        switch (state)
        {
            case CommandState.Attack: return new Color(0.9f, 0.25f, 0.2f, 0.9f);
            case CommandState.FormUp: return new Color(0.2f, 0.8f, 0.3f, 0.9f);
            case CommandState.Regroup: return new Color(0.2f, 0.4f, 0.9f, 0.9f);
            default: return Color.white;
        }
    }
}
