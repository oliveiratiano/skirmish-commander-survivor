using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

/// <summary>Shared helper to load sprites from the single texture the user selected. Used by UnitData editor, GameManager editor, and menu.</summary>
public static class UnitDataSpriteLoader
{
    /// <summary>Load sprites from the selected image file (one texture, 5x5 = 25 parts for 1280x1280). Returns (sprites, null) on success, or (null, errorMessage) on failure.</summary>
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

        const int expectedCount = 25;
        if (allSprites.Count < expectedCount)
        {
            if (SpriteSheetAutoImport.ApplyGridSliceAndReimport(assetPath))
            {
                assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
                allSprites.Clear();
                foreach (var obj in assets)
                {
                    if (obj is Sprite s)
                        allSprites.Add(s);
                }
                Debug.Log($"[UnitDataSpriteLoader] After auto 5x5 slice: {assetPath} -> {allSprites.Count} sprite(s)");
            }
        }

        if (allSprites.Count == 0)
            return (null, "No sprite sub-assets. Texture must be exactly 1280×1280 px for 5×5 (25) slice. Use menu: Commander Survival > Slice Selected Texture 5x5 (25 sprites), then try again.");

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

    public static void ApplySpritesToUnitDataDirection(UnitData unitData, List<Sprite> allSprites, string direction)
    {
        string propName = direction == "Up" ? "spritesUp" : direction == "Right" ? "spritesRight" : "spritesDown";
        var so = new SerializedObject(unitData);
        var spritesProp = so.FindProperty(propName);
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
            var arr = allSprites.ToArray();
            if (direction == "Up") unitData.spritesUp = arr;
            else if (direction == "Right") unitData.spritesRight = arr;
            else unitData.spritesDown = arr;
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
    void OnEnable() { }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var unitData = (UnitData)target;
        EditorGUILayout.Space(4);
        EditorGUILayout.HelpBox("Three directional sprite sheets (1280×1280, 5×5 = 25 frames each). Idle = center frame (index 12). Load Up, Right, and Down.", MessageType.None);
        string startPath = System.IO.Path.Combine(Application.dataPath, "_Project", "Art");
        if (!System.IO.Directory.Exists(startPath))
            startPath = Application.dataPath;

        if (GUILayout.Button("Load Sprites (Up)..."))
            LoadAndApplyDirection(unitData, startPath, "Up");
        if (GUILayout.Button("Load Sprites (Right)..."))
            LoadAndApplyDirection(unitData, startPath, "Right");
        if (GUILayout.Button("Load Sprites (Down)..."))
            LoadAndApplyDirection(unitData, startPath, "Down");
    }

    void LoadAndApplyDirection(UnitData unitData, string startPath, string direction)
    {
        string selectedFile = EditorUtility.OpenFilePanel("Select sprite sheet " + direction + " (PNG, 1280×1280, 5×5)", startPath, "png");
        if (string.IsNullOrEmpty(selectedFile)) return;
        try
        {
            var (sprites, error) = UnitDataSpriteLoader.LoadSpritesFromSelectedFile(selectedFile);
            if (error != null) { EditorUtility.DisplayDialog("Load Sprites", error, "OK"); return; }
            UnitDataSpriteLoader.ApplySpritesToUnitDataDirection(unitData, sprites, direction);
            EditorUtility.SetDirty(unitData);
            AssetDatabase.SaveAssets();
            Repaint();
            EditorUtility.DisplayDialog("Load Sprites", "Loaded " + sprites.Count + " sprites into " + direction + ".", "OK");
        }
        catch (System.Exception ex) { EditorUtility.DisplayDialog("Load Sprites", "Error: " + ex.Message, "OK"); Debug.LogException(ex); }
    }

    [MenuItem("Commander Survival/Fill Sprites From Image (Up)", true)]
    [MenuItem("Commander Survival/Fill Sprites From Image (Right)", true)]
    [MenuItem("Commander Survival/Fill Sprites From Image (Down)", true)]
    static bool ValidateFillSpritesFromImage() => Selection.activeObject is UnitData;

    [MenuItem("Commander Survival/Fill Sprites From Image (Up)", false, 20)]
    static void FillSpritesUp() { FillSpritesDirection("Up"); }
    [MenuItem("Commander Survival/Fill Sprites From Image (Right)", false, 21)]
    static void FillSpritesRight() { FillSpritesDirection("Right"); }
    [MenuItem("Commander Survival/Fill Sprites From Image (Down)", false, 22)]
    static void FillSpritesDown() { FillSpritesDirection("Down"); }

    static void FillSpritesDirection(string direction)
    {
        var unitData = Selection.activeObject as UnitData;
        if (unitData == null) return;
        string startPath = System.IO.Path.Combine(Application.dataPath, "_Project", "Art");
        if (!System.IO.Directory.Exists(startPath)) startPath = Application.dataPath;
        string selectedFile = EditorUtility.OpenFilePanel("Select sprite sheet " + direction + " (PNG, 1280×1280)", startPath, "png");
        if (string.IsNullOrEmpty(selectedFile)) return;
        try
        {
            var (sprites, error) = UnitDataSpriteLoader.LoadSpritesFromSelectedFile(selectedFile);
            if (error != null) { EditorUtility.DisplayDialog("Load Sprites", error, "OK"); return; }
            UnitDataSpriteLoader.ApplySpritesToUnitDataDirection(unitData, sprites, direction);
            EditorUtility.DisplayDialog("Load Sprites", "Loaded " + sprites.Count + " into " + direction + ".", "OK");
        }
        catch (System.Exception ex) { EditorUtility.DisplayDialog("Load Sprites", "Error: " + ex.Message, "OK"); }
    }
}
