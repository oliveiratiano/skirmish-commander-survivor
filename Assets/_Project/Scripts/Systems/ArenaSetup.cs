using UnityEngine;

public class ArenaSetup : MonoBehaviour
{
    [Tooltip("Optional: assign any texture for the floor. If unset, uses default from Resources (see GameConstants.ARENA_DEFAULT_FLOOR_TEXTURE_NAME), then grass_tile.")]
    public Texture2D floorTexture;

    [Tooltip("Tiling (repeats per arena side). Higher = more repeats (smaller tiles), lower = fewer repeats (bigger tiles). Vertical aspect is from GameConstants.ARENA_FLOOR_TILING_V_ASPECT.")]
    [Min(0.1f)]
    public float tiling = 1.25f;

    Renderer _floorRenderer;

    void Start()
    {
        CreateFullscreenFloor();
    }

    void OnValidate()
    {
        if (_floorRenderer != null)
            ApplyFloorMaterial();
    }

    void LateUpdate()
    {
        if (_floorRenderer == null || _floorRenderer.material == null) return;
        Camera cam = Camera.main;
        if (cam == null) return;
        _floorRenderer.material.SetVector("_CameraForward", cam.transform.forward);
    }

    void CreateFullscreenFloor()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("[ArenaSetup] No main camera. Cannot create fullscreen floor.");
            return;
        }

        Texture2D tex = floorTexture != null ? floorTexture : GetDefaultFloorTexture();
        bool fromInspector = floorTexture != null;
        if (!fromInspector)
            Debug.Log("[ArenaSetup] Floor Texture unset on '" + gameObject.name + "'. Using default from Resources: " + (tex != null ? tex.name : "none") + ".");

        Shader fsShader = Shader.Find("Unlit/Floor Fullscreen");
        if (fsShader == null)
        {
            Debug.LogError("[ArenaSetup] Unlit/Floor Fullscreen shader not found. Cannot create floor.");
            return;
        }

        GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.name = "ArenaFloor";
        quad.transform.SetParent(cam.transform, worldPositionStays: false);
        quad.transform.localPosition = new Vector3(0f, 0f, 10f);
        // Fixed large scale so the quad always fills the view regardless of zoom; no zoom coupling with orthographicSize.
        quad.transform.localScale = new Vector3(1000f, 1000f, 1f);
        quad.transform.localRotation = Quaternion.identity;

        Material mat = new Material(fsShader);
        mat.renderQueue = 1000;
        mat.SetVector("_CameraForward", cam.transform.forward);
        if (tex != null)
        {
            mat.SetTexture("_MainTex", tex);
            mat.SetColor("_Color", Color.white);
            float angleRad = Mathf.Deg2Rad * GameConstants.ISOMETRIC_CAMERA_ANGLE;
            Vector3 planeNormalTowardCamera = new Vector3(0f, -Mathf.Cos(angleRad), Mathf.Sin(angleRad));
            Vector3 floorCenter = new Vector3(0f, 0f, -5f);
            mat.SetVector("_FloorPlaneN", planeNormalTowardCamera);
            mat.SetFloat("_FloorPlaneD", -Vector3.Dot(floorCenter, planeNormalTowardCamera));
            mat.SetVector("_FloorCenter", new Vector4(0f, 0f, -5f, 0f));
            mat.SetFloat("_FloorSize", GameConstants.ARENA_HALF_SIZE * 2f);
            mat.SetFloat("_Tiling", tiling);
            mat.SetFloat("_TilingVAspect", GameConstants.ARENA_FLOOR_TILING_V_ASPECT);
            Debug.Log("[ArenaSetup] Fullscreen floor on '" + gameObject.name + "': texture=" + tex.name + " (" + (fromInspector ? "Inspector" : "Resources") + "), tiling=" + tiling + ".");
        }
        else
        {
            mat.SetColor("_Color", GameConstants.ARENA_COLOR);
            Debug.LogWarning("[ArenaSetup] No floor texture. Using solid color.");
        }

        Renderer r = quad.GetComponent<Renderer>();
        if (r == null) { Destroy(quad); return; }
        r.material = mat;
        _floorRenderer = r;

        Collider col = quad.GetComponent<Collider>();
        if (col != null) Destroy(col);
    }

    static Texture2D GetDefaultFloorTexture()
    {
        var settings = Resources.Load<DefaultArenaFloorSettings>(DefaultArenaFloorSettings.RESOURCES_NAME);
        if (settings != null && !string.IsNullOrEmpty(settings.defaultFloorTextureName))
        {
            var r = Resources.Load<Texture2D>(settings.defaultFloorTextureName);
            if (r != null) return r;
        }
        var fallback = Resources.Load<Texture2D>(GameConstants.ARENA_DEFAULT_FLOOR_TEXTURE_NAME);
        if (fallback != null) return fallback;
        return Resources.Load<Texture2D>("grass_tile");
    }

    void ApplyFloorMaterial()
    {
        if (_floorRenderer == null || _floorRenderer.material == null) return;
        Texture2D tex = floorTexture != null ? floorTexture : GetDefaultFloorTexture();
        if (tex != null)
        {
            _floorRenderer.material.SetTexture("_MainTex", tex);
            _floorRenderer.material.SetColor("_Color", Color.white);
            _floorRenderer.material.SetFloat("_Tiling", tiling);
        }
        else
        {
            _floorRenderer.material.SetColor("_Color", GameConstants.ARENA_COLOR);
        }
    }
}
