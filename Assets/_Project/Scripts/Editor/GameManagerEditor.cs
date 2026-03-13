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
            EditorGUILayout.HelpBox("Commander: 3 sheets (1280×1280, 5×5 = 25 frames). Load Up, Right, Down. Idle = center frame.", MessageType.None);
            string startPath = System.IO.Path.Combine(Application.dataPath, "_Project", "Art", "Commander");
            if (!System.IO.Directory.Exists(startPath))
                startPath = System.IO.Path.Combine(Application.dataPath, "_Project", "Art");
            if (!System.IO.Directory.Exists(startPath))
                startPath = Application.dataPath;

            if (GUILayout.Button("Fill Commander Sprites (Up)..."))
                FillCommanderDirection(gm, startPath, "Up");
            if (GUILayout.Button("Fill Commander Sprites (Right)..."))
                FillCommanderDirection(gm, startPath, "Right");
            if (GUILayout.Button("Fill Commander Sprites (Down)..."))
                FillCommanderDirection(gm, startPath, "Down");
        }
    }

    void FillCommanderDirection(GameManager gm, string startPath, string direction)
    {
        string selectedFile = EditorUtility.OpenFilePanel("Select Commander sprite sheet " + direction + " (PNG, 1280×1280)", startPath, "png");
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
