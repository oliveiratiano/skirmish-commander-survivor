using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

/// <summary>Shared helper to load sprites from the single texture the user selected. Used by UnitData editor, GameManager editor, and menu.</summary>
public static class UnitDataSpriteLoader
{
    /// <summary>Load sprites only from the selected image file (one texture, sliced into 12 or N parts). Returns (sprites, null) on success, or (null, errorMessage) on failure.</summary>
    public static (List<Sprite> sprites, string error) LoadSpritesFromSelectedFile(string absoluteFilePath)
    {
        string assetPath = FileUtil.GetProjectRelativePath(absoluteFilePath);
        if (string.IsNullOrEmpty(assetPath))
        {
            assetPath = GetAssetsRelativePathForFile(absoluteFilePath);
            if (string.IsNullOrEmpty(assetPath))
                return (null, "Please choose an image file inside the Unity project (under Assets).");
        }

        assetPath = assetPath.Replace('\\', '/');

        if (!assetPath.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase) && !assetPath.EndsWith(".jpg", System.StringComparison.OrdinalIgnoreCase))
            return (null, "Selected file is not a texture. Choose a PNG or JPG in the project.");

        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
        var allSprites = new List<Sprite>();
        foreach (var obj in assets)
        {
            if (obj is Sprite s)
                allSprites.Add(s);
        }

        Debug.Log($"[UnitDataSpriteLoader] Loading from asset: {assetPath} -> {allSprites.Count} sprite(s): [{string.Join(", ", allSprites.Select(x => x.name))}]");

        if (allSprites.Count < 12)
        {
            if (SpriteSheetAutoImport.Apply6x2SliceAndReimport(assetPath))
            {
                assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
                allSprites.Clear();
                foreach (var obj in assets)
                {
                    if (obj is Sprite s)
                        allSprites.Add(s);
                }
                Debug.Log($"[UnitDataSpriteLoader] After auto 6x2 slice: {assetPath} -> {allSprites.Count} sprite(s): [{string.Join(", ", allSprites.Select(x => x.name))}]");
            }
        }

        if (allSprites.Count == 0)
            return (null, "No sprite sub-assets. Texture must be exactly 1536×614 px so it can be auto-sliced. Resize the PNG and try again, or use menu: Commander Survival > Slice Selected Texture 6x2 (12 sprites) on the texture first.");

        allSprites = SortSpritesByNumericSuffix(allSprites);
        return (allSprites, null);
    }

    /// <summary>Fallback when GetProjectRelativePath fails (e.g. Windows path format). Converts absolute file path to Assets/... path.</summary>
    static string GetAssetsRelativePathForFile(string absoluteFilePath)
    {
        if (string.IsNullOrEmpty(absoluteFilePath)) return null;
        string dataPath = Application.dataPath.Replace('\\', '/');
        string file = absoluteFilePath.Replace('\\', '/');
        if (file.Length <= dataPath.Length) return null;
        if (file.IndexOf(dataPath, System.StringComparison.OrdinalIgnoreCase) != 0) return null;
        string sub = file.Substring(dataPath.Length).TrimStart('/');
        return string.IsNullOrEmpty(sub) ? null : "Assets/" + sub;
    }

    /// <summary>Sort by numeric suffix (e.g. Commander_0, Commander_1, ..., Commander_11) so 10 and 11 don't sort before 2.</summary>
    static List<Sprite> SortSpritesByNumericSuffix(List<Sprite> sprites)
    {
        return sprites.OrderBy(s => ParseTrailingNumber(s.name)).ThenBy(s => s.name).ToList();
    }

    static int ParseTrailingNumber(string name)
    {
        if (string.IsNullOrEmpty(name)) return -1;
        int lastUnderscore = name.LastIndexOf('_');
        if (lastUnderscore < 0 || lastUnderscore == name.Length - 1) return -1;
        string suffix = name.Substring(lastUnderscore + 1);
        int n;
        return int.TryParse(suffix, out n) ? n : -1;
    }

    public static void ApplySpritesToUnitData(UnitData unitData, List<Sprite> allSprites)
    {
        var so = new SerializedObject(unitData);
        var spritesProp = so.FindProperty("sprites");
        if (spritesProp != null)
        {
            so.Update();
            spritesProp.ClearArray();
            spritesProp.arraySize = allSprites.Count;
            for (int i = 0; i < allSprites.Count; i++)
                spritesProp.GetArrayElementAtIndex(i).objectReferenceValue = allSprites[i];
            so.ApplyModifiedProperties();
        }
        else
        {
            unitData.sprites = allSprites.ToArray();
        }
        EditorUtility.SetDirty(unitData);
        AssetDatabase.SaveAssets();
    }

    public static void ApplySpritesToSerializedProperty(SerializedProperty spritesProp, List<Sprite> allSprites, SerializedObject so)
    {
        if (spritesProp == null || so == null) return;
        so.Update();
        spritesProp.ClearArray();
        spritesProp.arraySize = allSprites.Count;
        for (int i = 0; i < allSprites.Count; i++)
            spritesProp.GetArrayElementAtIndex(i).objectReferenceValue = allSprites[i];
        so.ApplyModifiedProperties();
    }
}

[CustomEditor(typeof(UnitData))]
public class UnitDataEditor : Editor
{
    SerializedProperty _spritesProp;

    void OnEnable()
    {
        _spritesProp = serializedObject.FindProperty("sprites");
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var unitData = (UnitData)target;
        EditorGUILayout.Space(4);
        EditorGUILayout.HelpBox("Select the sprite sheet PNG for this unit. Texture must be exactly 1536×614 px (6×2 grid, 12 frames). See GameConstants and SPEC.", MessageType.None);
        if (GUILayout.Button("Load Sprites From Image..."))
        {
            string startPath = System.IO.Path.Combine(Application.dataPath, "_Project", "Art");
            if (!System.IO.Directory.Exists(startPath))
                startPath = Application.dataPath;
            string selectedFile = EditorUtility.OpenFilePanel("Select sprite sheet (PNG, exactly 1536×614 px, 6×2 grid)", startPath, "png");
            if (string.IsNullOrEmpty(selectedFile)) return;

            try
            {
                var (sprites, error) = UnitDataSpriteLoader.LoadSpritesFromSelectedFile(selectedFile);
                if (error != null)
                {
                    EditorUtility.DisplayDialog("Load Sprites", error, "OK");
                    return;
                }
                if (_spritesProp != null)
                {
                    UnitDataSpriteLoader.ApplySpritesToSerializedProperty(_spritesProp, sprites, serializedObject);
                }
                else
                {
                    UnitDataSpriteLoader.ApplySpritesToUnitData(unitData, sprites);
                }
                EditorUtility.SetDirty(unitData);
                AssetDatabase.SaveAssets();
                Repaint();
                string msg = "Loaded " + sprites.Count + " sprites into " + unitData.name + ".\n\nOrder: by name (idle 0-2, walk 3-8, shoot 9-10).";
                if (sprites.Count < 12)
                    msg += "\n\nOnly " + sprites.Count + " sprites were found. For full animation you need 12. Put the PNG in Assets/_Project/Art (e.g. Art/Sprites or Art/Commander) and reimport for auto 6x2 slice, then run this again.";
                EditorUtility.DisplayDialog("Load Sprites", msg, "OK");
            }
            catch (System.Exception ex)
            {
                EditorUtility.DisplayDialog("Load Sprites", "Error: " + ex.Message, "OK");
                Debug.LogException(ex);
            }
        }
    }

    [MenuItem("Commander Survival/Fill Sprites From Image", true)]
    static bool ValidateFillSpritesFromImage()
    {
        return Selection.activeObject is UnitData;
    }

    [MenuItem("Commander Survival/Fill Sprites From Image", false, 20)]
    static void FillSpritesFromImage()
    {
        var unitData = Selection.activeObject as UnitData;
        if (unitData == null) return;

        string startPath = System.IO.Path.Combine(Application.dataPath, "_Project", "Art");
        if (!System.IO.Directory.Exists(startPath))
            startPath = Application.dataPath;
        string selectedFile = EditorUtility.OpenFilePanel("Select sprite sheet (PNG, exactly 1536×614 px, 6×2 grid)", startPath, "png");
        if (string.IsNullOrEmpty(selectedFile)) return;

        try
        {
            var (sprites, error) = UnitDataSpriteLoader.LoadSpritesFromSelectedFile(selectedFile);
            if (error != null)
            {
                EditorUtility.DisplayDialog("Load Sprites", error, "OK");
                return;
            }
            UnitDataSpriteLoader.ApplySpritesToUnitData(unitData, sprites);
            string msg = "Loaded " + sprites.Count + " sprites into " + unitData.name + ".";
            if (sprites.Count < 12)
                msg += "\n\nFor full animation you need 12. Put the PNG in Assets/_Project/Art (e.g. Art/Sprites or Art/Commander) and reimport for auto 6x2 slice.";
            EditorUtility.DisplayDialog("Load Sprites", msg, "OK");
        }
        catch (System.Exception ex)
        {
            EditorUtility.DisplayDialog("Load Sprites", "Error: " + ex.Message, "OK");
            Debug.LogException(ex);
        }
    }
}
