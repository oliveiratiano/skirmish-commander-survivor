using UnityEngine;

public static class ShoutGeometry
{
    public static bool IsInShoutOval(Vector2 commanderPosition, Vector2 facing, Vector2 unitPosition)
    {
        Vector2 center = commanderPosition + facing * GameConstants.SHOUT_OVAL_OFFSET;
        Vector2 d = unitPosition - center;
        float u = Vector2.Dot(d, facing);
        Vector2 perp = new Vector2(facing.y, -facing.x);
        float v = Vector2.Dot(d, perp);
        float forwardRadius = GameConstants.SHOUT_OVAL_FORWARD_RADIUS;
        float sideRadius = GameConstants.SHOUT_OVAL_SIDE_RADIUS;
        return (u / forwardRadius) * (u / forwardRadius) + (v / sideRadius) * (v / sideRadius) <= 1f;
    }
}
