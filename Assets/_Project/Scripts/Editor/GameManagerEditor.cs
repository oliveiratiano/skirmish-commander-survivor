using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GameManager))]
public class GameManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var gm = (GameManager)target;
        if (gm.commanderData == null)
        {
            EditorGUILayout.Space(4);
            EditorGUILayout.HelpBox("Assign Commander Data above, then load three directional sprite sheets (Up, Right, Down).", MessageType.Info);
        }
        else
        {
            EditorGUILayout.Space(4);
            EditorGUILayout.HelpBox("Scene Commander sprites: load 3 directional sheets (Up, Right, Down). 1280×1280, 5×5; idle = center frame.", MessageType.None);
            string startPath = System.IO.Path.Combine(Application.dataPath, "_Project", "Art", "Commander");
            if (!System.IO.Directory.Exists(startPath))
                startPath = System.IO.Path.Combine(Application.dataPath, "_Project", "Art");
            if (!System.IO.Directory.Exists(startPath))
                startPath = Application.dataPath;

            if (GUILayout.Button("Commander Up..."))
                FillCommanderDirection(gm, startPath, "Up");
            if (GUILayout.Button("Commander Right..."))
                FillCommanderDirection(gm, startPath, "Right");
            if (GUILayout.Button("Commander Down..."))
                FillCommanderDirection(gm, startPath, "Down");
        }
    }

    void FillCommanderDirection(GameManager gm, string startPath, string direction)
    {
        string selectedFile = EditorUtility.OpenFilePanel("Select sprite sheet " + direction + " (1280×1280 PNG)", startPath, "png");
        if (string.IsNullOrEmpty(selectedFile)) return;
        try
        {
            var (sprites, error) = UnitDataSpriteLoader.LoadSpritesFromSelectedFile(selectedFile);
            if (error != null) { EditorUtility.DisplayDialog("Load Sprites", error, "OK"); return; }

            UnitDataSpriteLoader.ApplySpritesToUnitDataDirection(gm.commanderData, sprites, direction);

            serializedObject.Update();
            var prop = serializedObject.FindProperty(direction == "Up" ? "commanderSpritesUp" : direction == "Right" ? "commanderSpritesRight" : "commanderSpritesDown");
            if (prop != null)
            {
                prop.ClearArray();
                prop.arraySize = sprites.Count;
                for (int i = 0; i < sprites.Count; i++)
                    prop.GetArrayElementAtIndex(i).objectReferenceValue = sprites[i];
            }
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(gm);
            if (gm.gameObject != null)
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gm.gameObject.scene);
            AssetDatabase.SaveAssets();
            Repaint();
            EditorUtility.DisplayDialog("Load Sprites", "Loaded " + sprites.Count + " into Commander " + direction + ". Save scene (Ctrl+S).", "OK");
        }
        catch (System.Exception ex)
        {
            EditorUtility.DisplayDialog("Load Sprites", "Error: " + ex.Message, "OK");
            Debug.LogException(ex);
        }
    }
}
