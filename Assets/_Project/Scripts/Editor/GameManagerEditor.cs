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
            EditorGUILayout.HelpBox("Assign Commander Data above, then use the button below to fill its Sprites from a folder.", MessageType.Info);
        }
        else
        {
            EditorGUILayout.Space(4);
            EditorGUILayout.HelpBox("Select the Commander sprite sheet (e.g. Commander.png). Texture must be exactly 1536×614 px (6×2 grid). See GameConstants and SPEC.", MessageType.None);
            if (GUILayout.Button("Fill Commander Sprites From Image..."))
            {
                string startPath = System.IO.Path.Combine(Application.dataPath, "_Project", "Art", "Commander");
                if (!System.IO.Directory.Exists(startPath))
                    startPath = System.IO.Path.Combine(Application.dataPath, "_Project", "Art");
                if (!System.IO.Directory.Exists(startPath))
                    startPath = Application.dataPath;
                string selectedFile = EditorUtility.OpenFilePanel("Select Commander sprite sheet (PNG, exactly 1536×614 px, 6×2 grid)", startPath, "png");
                if (string.IsNullOrEmpty(selectedFile)) return;

                try
                {
                    var (sprites, error) = UnitDataSpriteLoader.LoadSpritesFromSelectedFile(selectedFile);
                    if (error != null)
                    {
                        EditorUtility.DisplayDialog("Load Sprites", error, "OK");
                        return;
                    }
                    UnitDataSpriteLoader.ApplySpritesToUnitData(gm.commanderData, sprites);
                    serializedObject.Update();
                    var commanderSpritesProp = serializedObject.FindProperty("commanderSprites");
                    if (commanderSpritesProp != null)
                    {
                        commanderSpritesProp.ClearArray();
                        commanderSpritesProp.arraySize = sprites.Count;
                        for (int i = 0; i < sprites.Count; i++)
                            commanderSpritesProp.GetArrayElementAtIndex(i).objectReferenceValue = sprites[i];
                    }
                    serializedObject.ApplyModifiedProperties();
                    EditorUtility.SetDirty(gm);
                    if (gm.gameObject != null)
                        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gm.gameObject.scene);
                    AssetDatabase.SaveAssets();
                    Repaint();
                    string msg = "Loaded " + sprites.Count + " sprites. Commander Data and the array above are filled.\n\nSave the scene (Ctrl+S) so the game uses them.";
                    if (sprites.Count < 12)
                        msg += "\n\nOnly " + sprites.Count + " sprites were found. For full animation you need 12. Select the texture in the Project window, then use menu: Commander Survival > Slice Selected Texture 6x2 (12 sprites). Then run this again.";
                    EditorUtility.DisplayDialog("Load Sprites", msg, "OK");
                }
                catch (System.Exception ex)
                {
                    EditorUtility.DisplayDialog("Load Sprites", "Error: " + ex.Message, "OK");
                    Debug.LogException(ex);
                }
            }
        }
    }
}
