using UnityEngine;

public class CommandFeedbackDisplay : MonoBehaviour
{
    Transform _icon;
    Renderer _iconRenderer;
    float _hideAt = -1f;

    void Start()
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
        go.name = "CommandFeedbackIcon";
        go.transform.SetParent(transform, worldPositionStays: false);
        go.transform.localPosition = new Vector3(0f, 0.8f, 0f);
        go.transform.localScale = Vector3.one * 0.4f;
        _icon = go.transform;
        _iconRenderer = go.GetComponent<Renderer>();
        if (_iconRenderer != null)
        {
            _iconRenderer.material = new Material(Shader.Find("Sprites/Default"));
            _iconRenderer.material.color = Color.white;
        }
        var col = go.GetComponent<Collider>();
        if (col != null) Object.Destroy(col);
        _icon.gameObject.SetActive(false);
    }

    void Update()
    {
        if (_hideAt > 0f && Time.time >= _hideAt)
        {
            _hideAt = -1f;
            if (_icon != null)
                _icon.gameObject.SetActive(false);
        }
    }

    public void ShowCommand(CommandState state)
    {
        if (_icon == null || _iconRenderer == null) return;
        _iconRenderer.material.color = GameConstants.CommandIconColor(state);
        _icon.gameObject.SetActive(true);
        _hideAt = Time.time + GameConstants.COMMAND_FEEDBACK_DURATION;
    }
}
