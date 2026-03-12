using UnityEngine;

public class DebugOverlay : MonoBehaviour
{
    public static DebugOverlay Instance { get; private set; }

    bool _visible = false;

    float _deltaTime;
    int _playerUnitCount;
    int _enemyCount;
    int _pooledProjectiles;
    int _pooledEnemies;
    float _spawnRate;

    GUIStyle _style;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void OnEnable() { SubscribeInput(); }
    void Start() { SubscribeInput(); }

    void OnDisable()
    {
        if (InputHandler.Instance != null)
            InputHandler.Instance.OnDebugToggle -= ToggleVisibility;
    }

    void SubscribeInput()
    {
        if (InputHandler.Instance != null)
        {
            InputHandler.Instance.OnDebugToggle -= ToggleVisibility;
            InputHandler.Instance.OnDebugToggle += ToggleVisibility;
        }
    }

    void Update()
    {
        _deltaTime += (Time.unscaledDeltaTime - _deltaTime) * 0.1f;
    }

    void ToggleVisibility()
    {
        _visible = !_visible;
    }

    public void SetPlayerUnitCount(int count) => _playerUnitCount = count;
    public void SetEnemyCount(int count) => _enemyCount = count;
    public void SetPooledProjectiles(int count) => _pooledProjectiles = count;
    public void SetPooledEnemies(int count) => _pooledEnemies = count;
    public void SetSpawnRate(float rate) => _spawnRate = rate;

    void OnGUI()
    {
        if (!_visible) return;

        if (_style == null)
        {
            _style = new GUIStyle(GUI.skin.box);
            _style.fontSize = 14;
            _style.alignment = TextAnchor.UpperLeft;
            _style.normal.textColor = Color.white;
        }

        float fps = 1f / _deltaTime;
        float ms = _deltaTime * 1000f;

        string commandState = CommandSystem.Instance != null
            ? CommandSystem.Instance.CurrentState.ToString()
            : "N/A";

        string phase = GameFlowManager.Instance != null
            ? GameFlowManager.Instance.CurrentPhase.ToString()
            : "N/A";

        string text =
            $"FPS: {fps:F0}  ({ms:F1} ms)\n" +
            $"Phase: {phase}\n" +
            $"Command: {commandState}\n" +
            $"Player Units: {_playerUnitCount}\n" +
            $"Enemies: {_enemyCount}\n" +
            $"Pooled Projectiles: {_pooledProjectiles}\n" +
            $"Pooled Enemies: {_pooledEnemies}\n" +
            $"Spawn Rate: {_spawnRate:F1}/s";

        GUI.Box(new Rect(10, 10, 260, 180), text, _style);
    }
}
