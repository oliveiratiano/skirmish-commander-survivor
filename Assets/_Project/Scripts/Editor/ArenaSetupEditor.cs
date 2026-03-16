using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ArenaSetup))]
public class ArenaSetupEditor : Editor
{
    const string AssetPath = "Assets/Resources/DefaultArenaFloorSettings.asset";

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var floorTextureProp = serializedObject.FindProperty("floorTexture");
        var tilingProp = serializedObject.FindProperty("tiling");

        // After recompile, scene reloads from disk; component may have floorTexture == null.
        // Restore from saved default so tile configuration is not lost.
        var settings = LoadOrCreateSettings();
        if (settings != null && floorTextureProp != null && floorTextureProp.objectReferenceValue == null)
        {
            var restored = LoadDefaultFloorTexture(settings);
            if (restored != null)
            {
                Undo.RecordObject(target, "Restore default floor from project settings");
                floorTextureProp.objectReferenceValue = restored;
                if (tilingProp != null && tilingProp.floatValue != settings.defaultTiling)
                    tilingProp.floatValue = settings.defaultTiling;
            }
        }

        DrawDefaultInspector();

        // Whenever a tile or tiling is set in the inspector, make it the project default.
        serializedObject.ApplyModifiedProperties();
        var currentTexture = floorTextureProp?.objectReferenceValue as Texture2D;
        var currentTiling = tilingProp != null ? tilingProp.floatValue : 1.25f;
        SyncDefaultsToAsset(currentTexture, currentTiling);
    }

    static Texture2D LoadDefaultFloorTexture(DefaultArenaFloorSettings settings)
    {
        if (settings == null) return null;
        if (!string.IsNullOrEmpty(settings.defaultFloorTexturePath))
        {
            var byPath = AssetDatabase.LoadAssetAtPath<Texture2D>(settings.defaultFloorTexturePath);
            if (byPath != null) return byPath;
        }
        if (!string.IsNullOrEmpty(settings.defaultFloorTextureName))
            return Resources.Load<Texture2D>(settings.defaultFloorTextureName);
        return null;
    }

    static void SyncDefaultsToAsset(Texture2D texture, float tiling)
    {
        var settings = LoadOrCreateSettings();
        if (settings == null) return;
        bool dirty = false;
        string textureName = texture != null ? texture.name : "";
        string texturePath = texture != null ? AssetDatabase.GetAssetPath(texture) : "";
        if (!string.IsNullOrEmpty(textureName) && (settings.defaultFloorTextureName != textureName || settings.defaultFloorTexturePath != texturePath))
        {
            Undo.RecordObject(settings, "Set default floor tile");
            settings.defaultFloorTextureName = textureName;
            settings.defaultFloorTexturePath = texturePath ?? "";
            dirty = true;
        }
        else if (string.IsNullOrEmpty(textureName) && (!string.IsNullOrEmpty(settings.defaultFloorTextureName) || !string.IsNullOrEmpty(settings.defaultFloorTexturePath)))
        {
            Undo.RecordObject(settings, "Clear default floor tile");
            settings.defaultFloorTextureName = "";
            settings.defaultFloorTexturePath = "";
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
