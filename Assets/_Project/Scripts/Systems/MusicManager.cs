using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    // Loaded from Resources/Audio/Stage1_BGM. See docs/audio-file-conventions.md.
    const string STAGE1_BGM_RESOURCES_PATH = "Audio/Stage1_BGM";

    AudioSource _source;
    bool _subscribed;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        _source = GetComponent<AudioSource>();
        if (_source == null)
            _source = gameObject.AddComponent<AudioSource>();

        _source.loop = true;
        _source.playOnAwake = false;
    }

    void OnEnable()
    {
        TrySubscribe();
    }

    void OnDisable()
    {
        if (GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.OnPhaseChanged -= OnPhaseChanged;
            _subscribed = false;
        }
    }

    void Start()
    {
        TrySubscribe();
    }

    void Update()
    {
        TrySubscribe();

        if (_subscribed && GameFlowManager.Instance != null)
        {
            var phase = GameFlowManager.Instance.CurrentPhase;
            if ((phase == GamePhase.Battle || phase == GamePhase.Overtime) && !_source.isPlaying)
                PlayStage1BGM();
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    void TrySubscribe()
    {
        if (_subscribed || GameFlowManager.Instance == null) return;

        GameFlowManager.Instance.OnPhaseChanged -= OnPhaseChanged;
        GameFlowManager.Instance.OnPhaseChanged += OnPhaseChanged;
        _subscribed = true;

        if (GameFlowManager.Instance.CurrentPhase == GamePhase.Battle ||
            GameFlowManager.Instance.CurrentPhase == GamePhase.Overtime)
            PlayStage1BGM();
    }

    void OnPhaseChanged(GamePhase phase)
    {
        if (phase == GamePhase.Battle || phase == GamePhase.Overtime)
            PlayStage1BGM();
        else
            StopBGM();
    }

    public void PlayStage1BGM()
    {
        if (_source == null) return;

        AudioClip clip = Resources.Load<AudioClip>(STAGE1_BGM_RESOURCES_PATH);

        if (clip == null) return;
        if (_source.clip == clip && _source.isPlaying) return;

        _source.clip = clip;
        _source.Play();
    }

    public void StopBGM()
    {
        if (_source != null && _source.isPlaying)
            _source.Stop();
    }
}
