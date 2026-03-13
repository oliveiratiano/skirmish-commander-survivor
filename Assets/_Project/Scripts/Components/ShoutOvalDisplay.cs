using UnityEngine;

[RequireComponent(typeof(CommanderController))]
public class ShoutOvalDisplay : MonoBehaviour
{
    const int SEGMENT_COUNT = 256;
    const float LINE_WIDTH = 0.18f;
    const float Z_POS = 0f;

    LineRenderer _line;
    Vector3[] _positions;

    void Awake()
    {
        GameObject child = new GameObject("ShoutOval");
        child.transform.SetParent(transform, worldPositionStays: false);

        _line = child.AddComponent<LineRenderer>();
        _line.useWorldSpace = true;
        _line.loop = true;
        _line.positionCount = SEGMENT_COUNT + 1;
        _line.startWidth = LINE_WIDTH;
        _line.endWidth = LINE_WIDTH;
        _line.numCapVertices = 8;
        _line.numCornerVertices = 8;
        _line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        _line.receiveShadows = false;

        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = new Color(0.45f, 0.75f, 1f, 0.32f);
        _line.material = mat;
        _line.startColor = new Color(0.45f, 0.75f, 1f, 0.35f);
        _line.endColor = new Color(0.45f, 0.75f, 1f, 0.35f);

        _positions = new Vector3[SEGMENT_COUNT + 1];
    }

    void LateUpdate()
    {
        var commander = GetComponent<CommanderController>();
        if (commander == null || _line == null) return;

        Vector2 pos = transform.position;
        Vector2 facing = commander.FacingDirection;
        Vector2 center = pos + facing * GameConstants.SHOUT_OVAL_OFFSET;
        Vector2 perp = new Vector2(facing.y, -facing.x);
        float a = GameConstants.SHOUT_OVAL_FORWARD_RADIUS;
        float b = GameConstants.SHOUT_OVAL_SIDE_RADIUS;

        for (int i = 0; i <= SEGMENT_COUNT; i++)
        {
            float t = (float)i / SEGMENT_COUNT * 2f * Mathf.PI;
            Vector2 p = center + facing * (a * Mathf.Cos(t)) + perp * (b * Mathf.Sin(t));
            _positions[i] = new Vector3(p.x, p.y, Z_POS);
        }

        _line.SetPositions(_positions);
    }
}
