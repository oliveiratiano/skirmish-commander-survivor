using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;

public static class ProjectSetup
{
    const string DATA_PATH = "Assets/_Project/Data";
    const string SCENE_PATH = "Assets/_Project/Scenes";

    [MenuItem("Commander Survival/1. Create All Data Assets", priority = 1)]
    static void CreateDataAssets()
    {
        EnsureDirectory(DATA_PATH);

        CreateUnitData("CommanderData", new UnitDataConfig
        {
            unitName = "Commander",
            color = new Color(0.2f, 0.6f, 1f),
            cost = 0,
            maxHP = 100f,
            moveSpeed = 5f,
            damage = 12f,
            range = 8f,
            accuracySpread = 5f,
            cooldown = 0.8f,
            burstCount = 1,
            burstInterval = 0f,
            projectileSpeed = 18f,
            projectileLifetime = 1.5f,
            projectileColor = new Color(0.4f, 0.8f, 1f)
        });

        CreateUnitData("CloseQuartersData", new UnitDataConfig
        {
            unitName = "Close-Quarters",
            color = new Color(0.1f, 0.8f, 0.3f),
            cost = 10,
            maxHP = 30f,
            moveSpeed = 4.5f,
            damage = 8f,
            range = 4f,
            accuracySpread = 25f,
            cooldown = 0.4f,
            burstCount = 1,
            burstInterval = 0f,
            projectileSpeed = 14f,
            projectileLifetime = 1f,
            projectileColor = new Color(0.3f, 1f, 0.5f)
        });

        CreateUnitData("MachineGunnerData", new UnitDataConfig
        {
            unitName = "Machine Gunner",
            color = new Color(0.9f, 0.6f, 0.1f),
            cost = 15,
            maxHP = 20f,
            moveSpeed = 3.5f,
            damage = 4f,
            range = 8f,
            accuracySpread = 15f,
            cooldown = 1.5f,
            burstCount = 5,
            burstInterval = 0.1f,
            projectileSpeed = 16f,
            projectileLifetime = 1.5f,
            projectileColor = new Color(1f, 0.8f, 0.2f)
        });

        CreateUnitData("SharpshooterData", new UnitDataConfig
        {
            unitName = "Sharpshooter",
            color = new Color(0.8f, 0.2f, 0.9f),
            cost = 20,
            maxHP = 10f,
            moveSpeed = 4f,
            damage = 25f,
            range = 12f,
            accuracySpread = 2f,
            cooldown = 2f,
            burstCount = 1,
            burstInterval = 0f,
            projectileSpeed = 25f,
            projectileLifetime = 2f,
            projectileColor = new Color(1f, 0.4f, 1f)
        });

        CreateUnitData("SwarmBugData", new UnitDataConfig
        {
            unitName = "Swarm Bug",
            color = new Color(0.7f, 0.2f, 0.1f),
            cost = 0,
            maxHP = 20f,
            moveSpeed = 4f,
            damage = 7f,
            range = 3f,
            accuracySpread = 15f,
            cooldown = 1.2f,
            burstCount = 1,
            burstInterval = 0f,
            projectileSpeed = 10f,
            projectileLifetime = 1f,
            projectileColor = new Color(0.5f, 1f, 0.1f)
        });

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[ProjectSetup] All UnitData assets created in " + DATA_PATH);
    }

    [MenuItem("Commander Survival/2. Build Game Scene", priority = 2)]
    static void BuildGameScene()
    {
        EnsureDirectory(SCENE_PATH);

        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // Load data assets (must exist — run step 1 first)
        var commanderData = AssetDatabase.LoadAssetAtPath<UnitData>(DATA_PATH + "/CommanderData.asset");
        var closeQuarters = AssetDatabase.LoadAssetAtPath<UnitData>(DATA_PATH + "/CloseQuartersData.asset");
        var machineGunner = AssetDatabase.LoadAssetAtPath<UnitData>(DATA_PATH + "/MachineGunnerData.asset");
        var sharpshooter = AssetDatabase.LoadAssetAtPath<UnitData>(DATA_PATH + "/SharpshooterData.asset");
        var swarmBug = AssetDatabase.LoadAssetAtPath<UnitData>(DATA_PATH + "/SwarmBugData.asset");

        if (commanderData == null || swarmBug == null)
        {
            Debug.LogError("[ProjectSetup] UnitData assets not found. Run '1. Create All Data Assets' first.");
            return;
        }

        // Configure Main Camera
        var cam = Camera.main;
        cam.orthographic = true;
        cam.orthographicSize = 12f;
        cam.transform.position = new Vector3(0f, 0f, -10f);
        cam.backgroundColor = GameConstants.ARENA_COLOR;
        cam.gameObject.AddComponent<CameraController>();

        // Systems root
        GameObject systems = CreateEmpty("--- SYSTEMS ---");

        GameObject inputGO = CreateEmpty("InputHandler");
        inputGO.AddComponent<InputHandler>();

        GameObject commandGO = CreateEmpty("CommandSystem");
        commandGO.AddComponent<CommandSystem>();

        GameObject poolGO = CreateEmpty("ObjectPool");
        poolGO.AddComponent<ObjectPool>();

        GameObject gmGO = CreateEmpty("GameManager");
        var gm = gmGO.AddComponent<GameManager>();
        gm.commanderData = commanderData;

        GameObject spawnerGO = CreateEmpty("UnitSpawner");
        spawnerGO.AddComponent<UnitSpawner>();

        GameObject waveGO = CreateEmpty("WaveManager");
        var wm = waveGO.AddComponent<WaveManager>();
        wm.enemyData = swarmBug;
        wm.totalEnemies = 100;
        wm.baseSpawnInterval = 1.5f;
        wm.minSpawnInterval = 0.2f;

        GameObject flowGO = CreateEmpty("GameFlowManager");
        flowGO.AddComponent<GameFlowManager>();

        GameObject arenaGO = CreateEmpty("ArenaSetup");
        arenaGO.AddComponent<ArenaSetup>();

        GameObject boundaryGO = CreateEmpty("ArenaBoundary");
        boundaryGO.AddComponent<ArenaBoundary>();

        // UI root
        GameObject uiRoot = CreateEmpty("--- UI ---");

        UnitData[] draftableUnits = new UnitData[] { closeQuarters, machineGunner, sharpshooter };

        GameObject draftGO = CreateEmpty("DraftUI");
        var draftUI = draftGO.AddComponent<DraftUI>();
        draftUI.availableUnits = draftableUnits;

        GameObject reinforceGO = CreateEmpty("ReinforcementUI");
        var reinforceUI = reinforceGO.AddComponent<ReinforcementUI>();
        reinforceUI.availableUnits = draftableUnits;

        GameObject battleHudGO = CreateEmpty("BattleHUD");
        battleHudGO.AddComponent<BattleHUD>();

        GameObject cmdHudGO = CreateEmpty("CommandStateHUD");
        cmdHudGO.AddComponent<CommandStateHUD>();

        GameObject debugGO = CreateEmpty("DebugOverlay");
        debugGO.AddComponent<DebugOverlay>();

        GameObject gameOverGO = CreateEmpty("GameOverUI");
        gameOverGO.AddComponent<GameOverUI>();

        // Save the scene
        string scenePath = SCENE_PATH + "/GameScene.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        Debug.Log("[ProjectSetup] Game scene created at " + scenePath);

        // Add scene to build settings
        var buildScenes = EditorBuildSettings.scenes;
        bool alreadyAdded = false;
        foreach (var s in buildScenes)
        {
            if (s.path == scenePath) { alreadyAdded = true; break; }
        }
        if (!alreadyAdded)
        {
            var newScenes = new EditorBuildSettingsScene[buildScenes.Length + 1];
            buildScenes.CopyTo(newScenes, 0);
            newScenes[newScenes.Length - 1] = new EditorBuildSettingsScene(scenePath, true);
            EditorBuildSettings.scenes = newScenes;
        }

        Debug.Log("[ProjectSetup] Scene setup complete. Press Play to test!");
    }

    [MenuItem("Commander Survival/3. Full Setup (Data + Scene)", priority = 3)]
    static void FullSetup()
    {
        CreateDataAssets();
        BuildGameScene();
    }

    struct UnitDataConfig
    {
        public string unitName;
        public Color color;
        public int cost;
        public float maxHP, moveSpeed, damage, range, accuracySpread;
        public float cooldown;
        public int burstCount;
        public float burstInterval;
        public float projectileSpeed, projectileLifetime;
        public Color projectileColor;
    }

    static void CreateUnitData(string fileName, UnitDataConfig cfg)
    {
        string path = DATA_PATH + "/" + fileName + ".asset";
        var existing = AssetDatabase.LoadAssetAtPath<UnitData>(path);
        if (existing != null)
        {
            Debug.Log("[ProjectSetup] " + fileName + " already exists, updating values.");
            ApplyConfig(existing, cfg);
            EditorUtility.SetDirty(existing);
            return;
        }

        var asset = ScriptableObject.CreateInstance<UnitData>();
        ApplyConfig(asset, cfg);
        AssetDatabase.CreateAsset(asset, path);
    }

    static void ApplyConfig(UnitData asset, UnitDataConfig cfg)
    {
        asset.unitName = cfg.unitName;
        asset.unitColor = cfg.color;
        asset.cost = cfg.cost;
        asset.maxHP = cfg.maxHP;
        asset.moveSpeed = cfg.moveSpeed;
        asset.damage = cfg.damage;
        asset.range = cfg.range;
        asset.accuracySpreadDegrees = cfg.accuracySpread;
        asset.cooldown = cfg.cooldown;
        asset.burstCount = cfg.burstCount;
        asset.burstInterval = cfg.burstInterval;
        asset.projectileSpeed = cfg.projectileSpeed;
        asset.projectileLifetime = cfg.projectileLifetime;
        asset.projectileColor = cfg.projectileColor;
    }

    static GameObject CreateEmpty(string name)
    {
        var go = new GameObject(name);
        return go;
    }

    static void EnsureDirectory(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            string[] parts = path.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }
}
