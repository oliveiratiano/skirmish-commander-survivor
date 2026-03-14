using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Follow")]
    public Transform target;
    public float followSmoothing = 8f;

    [Header("Zoom")]
    public float baseSize = 12f;
    public float maxSize = 20f;
    public float zoomSmoothing = 2f;
    [Tooltip("Scroll wheel: min orthographic size (zoomed in).")]
    public float scrollMinSize = 6f;
    [Tooltip("Scroll wheel: max orthographic size (zoomed out).")]
    public float scrollMaxSize = 24f;
    [Tooltip("Orthographic size change per scroll tick.")]
    public float scrollStep = 1.5f;

    float _baseTargetSize; // from enemy count (SetZoomByEnemyCount) or default baseSize
    float _scrollOffset;   // user offset from scroll wheel
    Camera _cam;

    void Awake()
    {
        _cam = GetComponent<Camera>();
        _cam.orthographic = true;
        _cam.orthographicSize = baseSize;
        _baseTargetSize = baseSize;
        _scrollOffset = 0f;
        _cam.clearFlags = CameraClearFlags.SolidColor;
        _cam.backgroundColor = GameConstants.ARENA_COLOR;
        transform.rotation = Quaternion.Euler(GameConstants.ISOMETRIC_CAMERA_ANGLE, 0f, 0f);
        // Wide near/far so the arena floor is never clipped when the camera moves to the borders.
        _cam.nearClipPlane = 0.01f;
        _cam.farClipPlane = 5000f;
    }

    void Update()
    {
        float scroll = Input.mouseScrollDelta.y;
        if (scroll != 0f)
        {
            _scrollOffset -= scroll * scrollStep;
            float halfRange = (scrollMaxSize - scrollMinSize) * 0.5f;
            _scrollOffset = Mathf.Clamp(_scrollOffset, -halfRange, halfRange);
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desired = target.position + new Vector3(0f, GameConstants.ISOMETRIC_CAMERA_OFFSET_Y, GameConstants.ISOMETRIC_CAMERA_OFFSET_Z);
        transform.position = Vector3.Lerp(transform.position, desired, followSmoothing * Time.deltaTime);

        float targetSize = Mathf.Clamp(_baseTargetSize + _scrollOffset, scrollMinSize, scrollMaxSize);
        _cam.orthographicSize = Mathf.Lerp(_cam.orthographicSize, targetSize, zoomSmoothing * Time.deltaTime);
    }

    public void SetZoomByEnemyCount(int enemyCount)
    {
        float t = Mathf.Clamp01(enemyCount / 100f);
        _baseTargetSize = Mathf.Lerp(baseSize, maxSize, t);
    }
}
