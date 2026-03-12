using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Follow")]
    public Transform target;
    public float followSmoothing = 8f;

    [Header("Dynamic Zoom")]
    public float baseSize = 12f;
    public float maxSize = 20f;
    public float zoomSmoothing = 2f;

    float _targetSize;
    Camera _cam;

    void Awake()
    {
        _cam = GetComponent<Camera>();
        _cam.orthographic = true;
        _cam.orthographicSize = baseSize;
        _targetSize = baseSize;
        _cam.backgroundColor = GameConstants.ARENA_COLOR;
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desired = target.position;
        desired.z = transform.position.z;
        transform.position = Vector3.Lerp(transform.position, desired, followSmoothing * Time.deltaTime);

        _cam.orthographicSize = Mathf.Lerp(_cam.orthographicSize, _targetSize, zoomSmoothing * Time.deltaTime);
    }

    public void SetZoomByEnemyCount(int enemyCount)
    {
        float t = Mathf.Clamp01(enemyCount / 100f);
        _targetSize = Mathf.Lerp(baseSize, maxSize, t);
    }
}
