using UnityEngine;

public class ArenaSetup : MonoBehaviour
{
    [Tooltip("Tiling count for grass texture (repeats per arena side).")]
    public float grassTiling = 15f;

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
        Material mat = new Material(Shader.Find("Sprites/Default"));
        Texture2D grassTex = Resources.Load<Texture2D>("grass_tile");
        if (grassTex != null)
        {
            mat.mainTexture = grassTex;
            mat.mainTextureScale = new Vector2(grassTiling, grassTiling);
            mat.color = Color.white;
        }
        else
        {
            mat.color = GameConstants.ARENA_COLOR;
        }
        r.material = mat;
        r.sortingOrder = -100;

        Collider col = plane.GetComponent<Collider>();
        if (col != null) Destroy(col);
    }
}
