using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ArenaSetup))]
public class ArenaSetupEditor : Editor
{
    const string AssetPath = "Assets/Resources/DefaultArenaFloorSettings.asset";

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        // Whenever a tile or tiling is set in the inspector, make it the project default.
        serializedObject.ApplyModifiedProperties();
        var floorTextureProp = serializedObject.FindProperty("floorTexture");
        var tilingProp = serializedObject.FindProperty("tiling");
        var currentTexture = floorTextureProp?.objectReferenceValue as Texture2D;
        var currentTiling = tilingProp != null ? tilingProp.floatValue : 1.25f;
        SyncDefaultsToAsset(currentTexture?.name, currentTiling);
    }

    static void SyncDefaultsToAsset(string textureName, float tiling)
    {
        var settings = LoadOrCreateSettings();
        if (settings == null) return;
        bool dirty = false;
        if (!string.IsNullOrEmpty(textureName) && settings.defaultFloorTextureName != textureName)
        {
            Undo.RecordObject(settings, "Set default floor tile");
            settings.defaultFloorTextureName = textureName;
            dirty = true;
        }
        if (tiling >= 0.1f && Mathf.Abs(settings.defaultTiling - tiling) > 0.001f)
        {
            if (!dirty) Undo.RecordObject(settings, "Set default floor tiling");
            settings.defaultTiling = tiling;
            dirty = true;
        }
        if (dirty)
        {
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
        }
    }

    static DefaultArenaFloorSettings LoadOrCreateSettings()
    {
        var settings = AssetDatabase.LoadAssetAtPath<DefaultArenaFloorSettings>(AssetPath);
        if (settings == null)
        {
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                AssetDatabase.CreateFolder("Assets", "Resources");
            settings = ScriptableObject.CreateInstance<DefaultArenaFloorSettings>();
            AssetDatabase.CreateAsset(settings, AssetPath);
        }
        return settings;
    }
}
