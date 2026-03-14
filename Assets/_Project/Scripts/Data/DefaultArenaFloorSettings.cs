using UnityEngine;

/// <summary>Stored in Resources. Overrides default floor tile and tiling when set. Updated by ArenaSetup inspector when you assign a tile or change tiling.</summary>
public class DefaultArenaFloorSettings : ScriptableObject
{
    public const string RESOURCES_NAME = "DefaultArenaFloorSettings";

    [Tooltip("Default floor texture name (loaded via Resources.Load). Empty = use GameConstants.ARENA_DEFAULT_FLOOR_TEXTURE_NAME.")]
    public string defaultFloorTextureName = "";

    [Tooltip("Default tiling (repeats per arena side). Updated when you change tiling in ArenaSetup inspector. Min 0.1.")]
    [Min(0.1f)]
    public float defaultTiling = 1.25f;
}
