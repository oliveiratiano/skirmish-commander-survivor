using UnityEngine;

public class ArenaSetup : MonoBehaviour
{
    [Tooltip("Optional: assign any texture for the floor. If unset, uses default from Resources (see GameConstants.ARENA_DEFAULT_FLOOR_TEXTURE_NAME), then grass_tile.")]
    public Texture2D floorTexture;

    [Tooltip("Tiling (repeats per arena side). Higher = more repeats (smaller tiles), lower = fewer repeats (bigger tiles). Vertical aspect is from GameConstants.ARENA_FLOOR_TILING_V_ASPECT.")]
    [Min(0.1f)]
    public float tiling = 1.25f;

    [Header("Border Decoration")]
    [Tooltip("Optional: right border texture. Must have transparency. If unset, loads default from Resources.")]
    public Texture2D borderTexture;

    [Tooltip("Optional: left border texture. Must have transparency. If unset, loads default from Resources.")]
    public Texture2D borderTextureLeft;

    [Tooltip("Optional: north border texture (horizontal). Must have transparency. If unset, loads default from Resources.")]
    public Texture2D borderTextureNorth;

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
#if UNITY_EDITOR
        if (!fromInspector)
            Debug.Log("[ArenaSetup] Floor Texture unset on '" + gameObject.name + "'. Using default from Resources: " + (tex != null ? tex.name : "none") + ".");
#endif

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
            Vector3 floorCenter = GameConstants.ARENA_FLOOR_CENTER;
            mat.SetVector("_FloorPlaneN", planeNormalTowardCamera);
            mat.SetFloat("_FloorPlaneD", -Vector3.Dot(floorCenter, planeNormalTowardCamera));
            mat.SetVector("_FloorCenter", new Vector4(floorCenter.x, floorCenter.y, floorCenter.z, 0f));
            mat.SetFloat("_FloorSize", GameConstants.ARENA_HALF_SIZE * 2f);
            mat.SetFloat("_Tiling", tiling);
            mat.SetFloat("_TilingVAspect", GameConstants.ARENA_FLOOR_TILING_V_ASPECT);

            // Border overlay (blended in the same shader pass)
            Texture2D borderTex = borderTexture != null ? borderTexture : Resources.Load<Texture2D>(GameConstants.ARENA_DEFAULT_RBORDER_TEXTURE_NAME);
            if (borderTex != null)
            {
                borderTex.wrapMode = TextureWrapMode.Repeat;
                float ppu = GameConstants.SPRITE_SHEET_PIXELS_PER_UNIT;
                float borderWidth = borderTex.width / ppu;
                float borderTileHeight = borderTex.height / ppu;
                float correctedTileHeight = borderTileHeight / GameConstants.ARENA_FLOOR_TILING_V_ASPECT;
                float borderScale = GameConstants.ARENA_BORDER_SCALE_RIGHT;

                mat.SetTexture("_BorderTex", borderTex);
                mat.SetFloat("_BorderEnabled", 1f);
                float startX = GameConstants.ARENA_HALF_SIZE + GameConstants.ARENA_BORDER_OFFSET_RIGHT;
                mat.SetFloat("_BorderStartX", startX);
                mat.SetFloat("_BorderWidth", borderWidth * borderScale);
                mat.SetFloat("_BorderTileHeight", correctedTileHeight * borderScale);
#if UNITY_EDITOR
                Debug.Log("[ArenaSetup] Border overlay: texture=" + borderTex.name +
                    ", width=" + (borderWidth * borderScale) + "u, tileHeight=" + (correctedTileHeight * borderScale) +
                    "u, startX=" + startX + ".");
#endif
            }
            else
            {
                mat.SetFloat("_BorderEnabled", 0f);
            }

            // Left border overlay
            Texture2D borderTexL = borderTextureLeft != null ? borderTextureLeft : Resources.Load<Texture2D>(GameConstants.ARENA_DEFAULT_LBORDER_TEXTURE_NAME);
            if (borderTexL != null)
            {
                borderTexL.wrapMode = TextureWrapMode.Repeat;
                float ppuL = GameConstants.SPRITE_SHEET_PIXELS_PER_UNIT;
                float borderWidthL = borderTexL.width / ppuL;
                float borderTileHeightL = borderTexL.height / ppuL;
                float correctedTileHeightL = borderTileHeightL / GameConstants.ARENA_FLOOR_TILING_V_ASPECT;
                float borderScaleL = GameConstants.ARENA_BORDER_SCALE_LEFT;
                float scaledWidthL = borderWidthL * borderScaleL;

                mat.SetTexture("_BorderTexL", borderTexL);
                mat.SetFloat("_BorderEnabledL", 1f);
                mat.SetFloat("_BorderStartXL", -GameConstants.ARENA_HALF_SIZE - scaledWidthL);
                mat.SetFloat("_BorderWidthL", scaledWidthL);
                mat.SetFloat("_BorderTileHeightL", correctedTileHeightL * borderScaleL);
#if UNITY_EDITOR
                Debug.Log("[ArenaSetup] Left border overlay: texture=" + borderTexL.name +
                    ", width=" + scaledWidthL + "u, startX=" + (-GameConstants.ARENA_HALF_SIZE - scaledWidthL) + ".");
#endif
            }
            else
            {
                mat.SetFloat("_BorderEnabledL", 0f);
            }

            // North border overlay (horizontal)
            // The V axis on the tilted floor plane is axisV = normalize(cross(planeNormal, right)).
            // Game positions use (x, y) with arena edge at y = ARENA_HALF_SIZE.
            // floorV = dot(arenaEdge - floorCenter, axisV) where arenaEdge = (0, ARENA_HALF_SIZE, 0).
            Vector3 axisVDir = Vector3.Cross(planeNormalTowardCamera, Vector3.right).normalized;
            Vector3 northEdgeToP = new Vector3(0f, GameConstants.ARENA_HALF_SIZE + GameConstants.ARENA_BORDER_OFFSET_NORTH, 0f) - floorCenter;
            float borderStartVN = Vector3.Dot(northEdgeToP, axisVDir);

            Texture2D borderTexN = borderTextureNorth != null ? borderTextureNorth : Resources.Load<Texture2D>(GameConstants.ARENA_DEFAULT_NBORDER_TEXTURE_NAME);
            if (borderTexN != null)
            {
                borderTexN.wrapMode = TextureWrapMode.Repeat;
                float ppuN = GameConstants.SPRITE_SHEET_PIXELS_PER_UNIT;
                float borderHeightN = borderTexN.height / ppuN;
                float borderTileWidthN = borderTexN.width / ppuN;
                float correctedHeightN = borderHeightN / GameConstants.ARENA_FLOOR_TILING_V_ASPECT;
                float borderScaleN = GameConstants.ARENA_BORDER_SCALE_NORTH;
                float scaledHeightN = correctedHeightN * borderScaleN;
                float scaledTileWidthN = borderTileWidthN * borderScaleN;

                mat.SetTexture("_BorderTexN", borderTexN);
                mat.SetFloat("_BorderEnabledN", 1f);
                mat.SetFloat("_BorderStartVN", borderStartVN);
                mat.SetFloat("_BorderHeightN", scaledHeightN);
                mat.SetFloat("_BorderTileWidthN", scaledTileWidthN);
#if UNITY_EDITOR
                Debug.Log("[ArenaSetup] North border overlay: texture=" + borderTexN.name +
                    ", height=" + scaledHeightN + "u, tileWidth=" + scaledTileWidthN + "u.");
#endif
            }
            else
            {
                mat.SetFloat("_BorderEnabledN", 0f);
            }

#if UNITY_EDITOR
            Debug.Log("[ArenaSetup] Fullscreen floor on '" + gameObject.name + "': texture=" + tex.name + " (" + (fromInspector ? "Inspector" : "Resources") + "), tiling=" + tiling + ".");
#endif
        }
        else
        {
            mat.SetColor("_Color", GameConstants.ARENA_COLOR);
            mat.SetFloat("_BorderEnabled", 0f);
            mat.SetFloat("_BorderEnabledL", 0f);
            mat.SetFloat("_BorderEnabledN", 0f);
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
        Material mat = _floorRenderer.material;
        Texture2D tex = floorTexture != null ? floorTexture : GetDefaultFloorTexture();
        if (tex != null)
        {
            mat.SetTexture("_MainTex", tex);
            mat.SetColor("_Color", Color.white);
            mat.SetFloat("_Tiling", tiling);
        }
        else
        {
            mat.SetColor("_Color", GameConstants.ARENA_COLOR);
        }

        // Refresh right border overlay
        Texture2D rTex = borderTexture != null ? borderTexture : Resources.Load<Texture2D>(GameConstants.ARENA_DEFAULT_RBORDER_TEXTURE_NAME);
        if (rTex != null)
        {
            float ppu = GameConstants.SPRITE_SHEET_PIXELS_PER_UNIT;
            float borderWidth = rTex.width / ppu;
            float borderTileHeight = rTex.height / ppu;
            float correctedTileHeight = borderTileHeight / GameConstants.ARENA_FLOOR_TILING_V_ASPECT;
            float borderScale = GameConstants.ARENA_BORDER_SCALE_RIGHT;

            mat.SetTexture("_BorderTex", rTex);
            mat.SetFloat("_BorderEnabled", 1f);
            mat.SetFloat("_BorderStartX", GameConstants.ARENA_HALF_SIZE + GameConstants.ARENA_BORDER_OFFSET_RIGHT);
            mat.SetFloat("_BorderWidth", borderWidth * borderScale);
            mat.SetFloat("_BorderTileHeight", correctedTileHeight * borderScale);
        }
        else
        {
            mat.SetFloat("_BorderEnabled", 0f);
        }

        // Refresh left border overlay
        Texture2D lTex = borderTextureLeft != null ? borderTextureLeft : Resources.Load<Texture2D>(GameConstants.ARENA_DEFAULT_LBORDER_TEXTURE_NAME);
        if (lTex != null)
        {
            float ppu = GameConstants.SPRITE_SHEET_PIXELS_PER_UNIT;
            float borderWidth = lTex.width / ppu;
            float borderTileHeight = lTex.height / ppu;
            float correctedTileHeight = borderTileHeight / GameConstants.ARENA_FLOOR_TILING_V_ASPECT;
            float borderScale = GameConstants.ARENA_BORDER_SCALE_LEFT;
            float scaledWidth = borderWidth * borderScale;

            mat.SetTexture("_BorderTexL", lTex);
            mat.SetFloat("_BorderEnabledL", 1f);
            mat.SetFloat("_BorderStartXL", -GameConstants.ARENA_HALF_SIZE - scaledWidth);
            mat.SetFloat("_BorderWidthL", scaledWidth);
            mat.SetFloat("_BorderTileHeightL", correctedTileHeight * borderScale);
        }
        else
        {
            mat.SetFloat("_BorderEnabledL", 0f);
        }

        // Refresh north border overlay
        Texture2D nTex = borderTextureNorth != null ? borderTextureNorth : Resources.Load<Texture2D>(GameConstants.ARENA_DEFAULT_NBORDER_TEXTURE_NAME);
        if (nTex != null)
        {
            float angleRad = Mathf.Deg2Rad * GameConstants.ISOMETRIC_CAMERA_ANGLE;
            Vector3 planeN = new Vector3(0f, -Mathf.Cos(angleRad), Mathf.Sin(angleRad));
            Vector3 axisVDir = Vector3.Cross(planeN, Vector3.right).normalized;
            Vector3 floorCtr = GameConstants.ARENA_FLOOR_CENTER;
            Vector3 northEdgeToP = new Vector3(0f, GameConstants.ARENA_HALF_SIZE + GameConstants.ARENA_BORDER_OFFSET_NORTH, 0f) - floorCtr;
            float startVN = Vector3.Dot(northEdgeToP, axisVDir);

            float ppu = GameConstants.SPRITE_SHEET_PIXELS_PER_UNIT;
            float borderHeight = nTex.height / ppu;
            float borderTileWidth = nTex.width / ppu;
            float correctedHeight = borderHeight / GameConstants.ARENA_FLOOR_TILING_V_ASPECT;
            float borderScale = GameConstants.ARENA_BORDER_SCALE_NORTH;

            mat.SetTexture("_BorderTexN", nTex);
            mat.SetFloat("_BorderEnabledN", 1f);
            mat.SetFloat("_BorderStartVN", startVN);
            mat.SetFloat("_BorderHeightN", correctedHeight * borderScale);
            mat.SetFloat("_BorderTileWidthN", borderTileWidth * borderScale);
        }
        else
        {
            mat.SetFloat("_BorderEnabledN", 0f);
        }
    }
}
