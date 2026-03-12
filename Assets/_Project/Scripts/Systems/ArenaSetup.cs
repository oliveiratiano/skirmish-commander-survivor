using UnityEngine;

public class ArenaSetup : MonoBehaviour
{
    void Awake()
    {
        CreateArenaPlane();
    }

    void CreateArenaPlane()
    {
        GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Quad);
        plane.name = "ArenaFloor";
        plane.transform.position = Vector3.zero;
        float size = GameConstants.ARENA_HALF_SIZE * 2f;
        plane.transform.localScale = new Vector3(size, size, 1f);
        plane.transform.rotation = Quaternion.identity;

        Renderer r = plane.GetComponent<Renderer>();
        r.material = new Material(Shader.Find("Sprites/Default"));
        r.material.color = GameConstants.ARENA_COLOR;
        r.sortingOrder = -100;

        Collider col = plane.GetComponent<Collider>();
        if (col != null) Destroy(col);
    }
}
