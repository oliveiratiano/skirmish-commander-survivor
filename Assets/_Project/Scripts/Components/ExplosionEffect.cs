using UnityEngine;

public class ExplosionEffect : MonoBehaviour
{
    const float DURATION = 0.3f;

    float _targetDiameter;
    float _timer;
    Renderer _renderer;
    Material _mat;

    static readonly Color PlayerColor = new Color(0.4f, 0.8f, 1f, 0.45f);
    static readonly Color EnemyColor = new Color(1f, 0.35f, 0.2f, 0.45f);

    public void Initialize(float radius, bool isPlayer)
    {
        _targetDiameter = radius * 2f;
        _timer = 0f;

        var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
        go.transform.SetParent(transform, false);
        go.transform.localRotation = Quaternion.Euler(GameConstants.ISOMETRIC_CAMERA_ANGLE, 0f, 0f);

        var col = go.GetComponent<Collider>();
        if (col != null) Object.Destroy(col);

        _renderer = go.GetComponent<Renderer>();
        _mat = new Material(Shader.Find("Sprites/Default"));
        _mat.mainTexture = Texture2D.whiteTexture;
        _mat.color = isPlayer ? PlayerColor : EnemyColor;
        _renderer.material = _mat;
        _renderer.sortingOrder = GameConstants.ISOMETRIC_SORT_PROJECTILE_ORDER - 1;

        transform.localScale = Vector3.zero;
    }

    void Update()
    {
        _timer += Time.deltaTime;
        float t = _timer / DURATION;

        if (t >= 1f)
        {
            Destroy(gameObject);
            return;
        }

        // Fast expand, then ease out
        float scale = Mathf.Lerp(0f, _targetDiameter, Mathf.Sqrt(t));
        transform.localScale = new Vector3(scale, scale, scale);

        // Fade out alpha
        Color c = _mat.color;
        c.a = Mathf.Lerp(c.a, 0f, t * t);
        _mat.color = c;
    }
}
