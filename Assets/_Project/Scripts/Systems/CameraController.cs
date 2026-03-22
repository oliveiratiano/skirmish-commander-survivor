using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Follow")]
    public Transform target;
    public float followSmoothing = 8f;

    [Header("Zoom")]
    public float zoomSmoothing = 2f;
    [Tooltip("Orthographic size change per scroll tick.")]
    public float scrollStep = 1.5f;

    float _baseTargetSize;
    float _scrollOffset;
    Camera _cam;

    void Awake()
    {
        _cam = GetComponent<Camera>();
        _cam.orthographic = true;
        _cam.orthographicSize = GameConstants.CAMERA_BASE_SIZE;
        _baseTargetSize = GameConstants.CAMERA_BASE_SIZE;
        _scrollOffset = 0f;
        _cam.clearFlags = CameraClearFlags.SolidColor;
        _cam.backgroundColor = GameConstants.ARENA_COLOR;
        transform.rotation = Quaternion.Euler(GameConstants.ISOMETRIC_CAMERA_ANGLE, 0f, 0f);
        _cam.nearClipPlane = 0.01f;
        _cam.farClipPlane = 5000f;
    }

    void Update()
    {
        float scroll = Input.mouseScrollDelta.y;
        if (scroll != 0f)
        {
            _scrollOffset -= scroll * scrollStep;
            // Only allow zooming in (negative offset), never beyond base size
            _scrollOffset = Mathf.Clamp(_scrollOffset, GameConstants.CAMERA_SCROLL_MIN_SIZE - GameConstants.CAMERA_BASE_SIZE, 0f);
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desired = target.position + new Vector3(0f, GameConstants.ISOMETRIC_CAMERA_OFFSET_Y, GameConstants.ISOMETRIC_CAMERA_OFFSET_Z);
        transform.position = Vector3.Lerp(transform.position, desired, followSmoothing * Time.deltaTime);

        float targetSize = Mathf.Clamp(_baseTargetSize + _scrollOffset, GameConstants.CAMERA_SCROLL_MIN_SIZE, GameConstants.CAMERA_BASE_SIZE);
        _cam.orthographicSize = Mathf.Lerp(_cam.orthographicSize, targetSize, zoomSmoothing * Time.deltaTime);
    }

    public void SetZoomByEnemyCount(int enemyCount)
    {
        _baseTargetSize = GameConstants.CAMERA_BASE_SIZE;
    }
}
