using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(WaveManager))]
public class WaveManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var wm = (WaveManager)target;
        if (wm.enemyData == null)
        {
            EditorGUILayout.HelpBox("Assign Enemy Data above to enable sprite tools.", MessageType.Info);
            return;
        }

        EditorGUILayout.Space(4);
        EditorGUILayout.HelpBox("Enemy sprites (from Enemy Data): load 3 directional sheets (Up, Right, Down). 1280×1280, 5×5; idle = center frame.", MessageType.None);

        string startPath = System.IO.Path.Combine(Application.dataPath, "_Project", "Art");
        if (!System.IO.Directory.Exists(startPath))
            startPath = Application.dataPath;

        if (GUILayout.Button("Enemy Up..."))
            LoadEnemyDirection(wm.enemyData, startPath, "Up");
        if (GUILayout.Button("Enemy Right..."))
            LoadEnemyDirection(wm.enemyData, startPath, "Right");
        if (GUILayout.Button("Enemy Down..."))
            LoadEnemyDirection(wm.enemyData, startPath, "Down");
    }

    void LoadEnemyDirection(UnitData enemyData, string startPath, string direction)
    {
        string selectedFile = EditorUtility.OpenFilePanel("Select sprite sheet " + direction + " (1280×1280 PNG)", startPath, "png");
        if (string.IsNullOrEmpty(selectedFile)) return;
        try
        {
            var (sprites, error) = UnitDataSpriteLoader.LoadSpritesFromSelectedFile(selectedFile);
            if (error != null)
            {
                EditorUtility.DisplayDialog("Load Sprites", error, "OK");
                return;
            }
            UnitDataSpriteLoader.ApplySpritesToUnitDataDirection(enemyData, sprites, direction);
            EditorUtility.SetDirty(enemyData);
            AssetDatabase.SaveAssets();
            Repaint();
            EditorUtility.DisplayDialog("Load Sprites", "Loaded " + sprites.Count + " enemy sprites into " + direction + ".", "OK");
        }
        catch (System.Exception ex)
        {
            EditorUtility.DisplayDialog("Load Sprites", "Error: " + ex.Message, "OK");
            Debug.LogException(ex);
        }
    }
}

