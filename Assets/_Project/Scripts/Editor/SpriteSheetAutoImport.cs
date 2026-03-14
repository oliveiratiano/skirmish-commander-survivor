using UnityEngine;
using UnityEditor;

/// <summary>
/// Automatically configures sprite sheets only under Assets/_Project/Art/.../Sprites/ (or below).
/// Uses GameConstants for grid and dimensions (5x5, 1280x1280). Other textures (e.g. floor tiles) are left to default import.
/// </summary>
public class SpriteSheetAutoImport : AssetPostprocessor
{
    const string ART_BASE = "Assets/_Project/Art";
    /// <summary>Only textures under a folder named "Sprites" (under Art) get 1280x1280 sprite sheet treatment.</summary>
    const string SPRITES_FOLDER = "/Sprites/";

    static int GRID_COLS => GameConstants.SPRITE_SHEET_GRID_COLS;
    static int GRID_ROWS => GameConstants.SPRITE_SHEET_GRID_ROWS;

    void OnPreprocessTexture()
    {
        var importer = (TextureImporter)assetImporter;

        if (!assetPath.StartsWith(ART_BASE + "/") || !assetPath.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase))
            return;
        if (assetPath.IndexOf(SPRITES_FOLDER, System.StringComparison.OrdinalIgnoreCase) < 0)
            return;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.spritePixelsPerUnit = GameConstants.SPRITE_SHEET_PIXELS_PER_UNIT;
        importer.filterMode = FilterMode.Bilinear;
        importer.textureCompression = TextureImporterCompression.Compressed;

        int width, height;
        if (!TryGetPngDimensions(assetPath, out width, out height))
        {
            Debug.LogError($"[SpriteSheetAutoImport] Could not read dimensions for {assetPath}. Required: {GameConstants.SPRITE_SHEET_WIDTH}x{GameConstants.SPRITE_SHEET_HEIGHT}. Skipping auto-slice.");
            return;
        }

        if (width != GameConstants.SPRITE_SHEET_WIDTH || height != GameConstants.SPRITE_SHEET_HEIGHT)
        {
            Debug.LogError($"[SpriteSheetAutoImport] {assetPath} is {width}x{height}. Required exactly {GameConstants.SPRITE_SHEET_WIDTH}x{GameConstants.SPRITE_SHEET_HEIGHT} (see SPEC and GameConstants). Skipping auto-slice to avoid rect-outside-texture.");
            return;
        }

        int cellW = GameConstants.SPRITE_SHEET_CELL_WIDTH;
        int cellH = GameConstants.SPRITE_SHEET_CELL_HEIGHT;
        string baseName = System.IO.Path.GetFileNameWithoutExtension(assetPath);

        var sheet = new SpriteMetaData[GRID_COLS * GRID_ROWS];
        for (int row = 0; row < GRID_ROWS; row++)
        {
            for (int col = 0; col < GRID_COLS; col++)
            {
                int i = row * GRID_COLS + col;
                sheet[i] = new SpriteMetaData
                {
                    name = $"{baseName}_{i}",
                    rect = new Rect(col * cellW, (GRID_ROWS - 1 - row) * cellH, cellW, cellH),
                    alignment = (int)SpriteAlignment.Center,
                    pivot = new Vector2(0.5f, 0.5f)
                };
            }
        }

        SetSpriteSheetViaSerializedObject(importer, sheet);
    }

    /// <summary>Force 5x5 (25) slice on the selected texture and reimport. Texture must be 1280x1280.</summary>
    [MenuItem("Commander Survival/Slice Selected Texture 5x5 (25 sprites)", true)]
    static bool ValidateSliceSelectedTexture5x5()
    {
        var o = Selection.activeObject;
        if (o == null) return false;
        string path = AssetDatabase.GetAssetPath(o);
        return !string.IsNullOrEmpty(path) && path.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase);
    }

    [MenuItem("Commander Survival/Slice Selected Texture 5x5 (25 sprites)", false, 25)]
    static void SliceSelectedTexture5x5()
    {
        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (string.IsNullOrEmpty(path) || !path.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase))
            return;
        bool ok = ApplyGridSliceAndReimport(path);
        EditorUtility.DisplayDialog("Slice Texture", ok ? "Applied 5x5 (25) slices. Use Fill Commander Sprites / Load Sprites for each direction." : "Could not apply 25 slices. Texture must be 1280x1280. Check Console.", "OK");
    }

    /// <summary>Apply grid from GameConstants (5x5, 1280x1280) and reimport. Returns true only if texture matches.</summary>
    public static bool ApplyGridSliceAndReimport(string assetPath)
    {
        var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null)
        {
            Debug.LogWarning("[SpriteSheetAutoImport] No TextureImporter for " + assetPath);
            return false;
        }

        int width, height;
        string fullPath = System.IO.Path.Combine(Application.dataPath, "..", assetPath).Replace('\\', '/');
        if (!TryGetPngDimensions(fullPath, out width, out height))
        {
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            if (tex != null) { width = tex.width; height = tex.height; }
            else
            {
                Debug.LogError($"[SpriteSheetAutoImport] Could not read dimensions. Texture must be exactly {GameConstants.SPRITE_SHEET_WIDTH}x{GameConstants.SPRITE_SHEET_HEIGHT} (5x5 grid). See SPEC and GameConstants.");
                return false;
            }
        }

        if (width != GameConstants.SPRITE_SHEET_WIDTH || height != GameConstants.SPRITE_SHEET_HEIGHT)
        {
            Debug.LogError($"[SpriteSheetAutoImport] Texture is {width}x{height}. Required exactly {GameConstants.SPRITE_SHEET_WIDTH}x{GameConstants.SPRITE_SHEET_HEIGHT}. Resize the PNG and try again. See docs/SPEC.md.");
            return false;
        }

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.spritePixelsPerUnit = GameConstants.SPRITE_SHEET_PIXELS_PER_UNIT;

        int cellW = GameConstants.SPRITE_SHEET_CELL_WIDTH;
        int cellH = GameConstants.SPRITE_SHEET_CELL_HEIGHT;
        string baseName = System.IO.Path.GetFileNameWithoutExtension(assetPath);

        var sheet = new SpriteMetaData[GRID_COLS * GRID_ROWS];
        for (int row = 0; row < GRID_ROWS; row++)
        {
            for (int col = 0; col < GRID_COLS; col++)
            {
                int i = row * GRID_COLS + col;
                sheet[i] = new SpriteMetaData
                {
                    name = $"{baseName}_{i}",
                    rect = new Rect(col * cellW, (GRID_ROWS - 1 - row) * cellH, cellW, cellH),
                    alignment = (int)SpriteAlignment.Center,
                    pivot = new Vector2(0.5f, 0.5f)
                };
            }
        }

        SetSpriteSheetViaSerializedObject(importer, sheet);
        EditorUtility.SetDirty(importer);
        importer.SaveAndReimport();
        return true;
    }

    static void SetSpriteSheetViaSerializedObject(TextureImporter importer, SpriteMetaData[] sheet)
    {
        var so = new SerializedObject(importer);
        var spritesheetProp = so.FindProperty("m_SpriteSheet.m_Sprites");
        if (spritesheetProp == null)
            spritesheetProp = so.FindProperty("m_SpriteSheet");
        if (spritesheetProp == null)
        {
            Debug.LogWarning("[SpriteSheetAutoImport] Could not find sprite sheet property on TextureImporter; sprite layout may need to be set manually.");
            return;
        }
        spritesheetProp.arraySize = sheet.Length;
        for (int i = 0; i < sheet.Length; i++)
        {
            var el = spritesheetProp.GetArrayElementAtIndex(i);
            var nameProp = el.FindPropertyRelative("name") ?? el.FindPropertyRelative("m_Name");
            if (nameProp != null) nameProp.stringValue = sheet[i].name;
            var rectProp = el.FindPropertyRelative("rect") ?? el.FindPropertyRelative("m_Rect");
            if (rectProp != null) rectProp.rectValue = sheet[i].rect;
            var alignProp = el.FindPropertyRelative("alignment") ?? el.FindPropertyRelative("m_Alignment");
            if (alignProp != null) alignProp.intValue = sheet[i].alignment;
            var pivotProp = el.FindPropertyRelative("pivot") ?? el.FindPropertyRelative("m_Pivot");
            if (pivotProp != null) pivotProp.vector2Value = sheet[i].pivot;
            var borderProp = el.FindPropertyRelative("border") ?? el.FindPropertyRelative("m_Border");
            if (borderProp != null) borderProp.vector4Value = sheet[i].border;
        }
        so.ApplyModifiedProperties();
    }

    static bool TryGetPngDimensions(string path, out int width, out int height)
    {
        width = height = 0;
        try
        {
            byte[] bytes = System.IO.File.ReadAllBytes(path);
            if (bytes.Length < 24) return false;
            // PNG: 8-byte signature then IHDR chunk (width at 16, height at 20, big-endian)
            if (bytes[12] != 'I' || bytes[13] != 'H' || bytes[14] != 'D' || bytes[15] != 'R')
                return false;
            width = (bytes[16] << 24) | (bytes[17] << 16) | (bytes[18] << 8) | bytes[19];
            height = (bytes[20] << 24) | (bytes[21] << 16) | (bytes[22] << 8) | bytes[23];
            return width > 0 && height > 0;
        }
        catch
        {
            return false;
        }
    }
}
