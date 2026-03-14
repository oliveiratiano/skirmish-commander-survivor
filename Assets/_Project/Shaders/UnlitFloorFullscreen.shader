// Fullscreen floor: ray-plane intersection so the floor is never culled or clipped.
// Draw with a fullscreen quad; fragment world position on the floor plane is computed from the camera ray.
Shader "Unlit/Floor Fullscreen"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "Queue" = "Background" "RenderType" = "Opaque" "IgnoreProjector" = "True" }
        LOD 100
        Cull Off

        Pass
        {
            ZWrite Off
            Blend Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float3 _FloorPlaneN;
            float _FloorPlaneD;
            float3 _FloorCenter;
            float _FloorSize;
            float _Tiling;
            float _TilingVAspect;
            float3 _CameraForward;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Orthographic: cast from fragment along camera forward to floor plane (no perspective fan).
                // Ray P(t) = i.worldPos + t*viewDir; solve dot(P(t),N)+D=0 => t = -(dot(i.worldPos,N)+D)/dot(viewDir,N).
                float3 viewDir = normalize(_CameraForward);
                float denom = dot(_FloorPlaneN, viewDir);
                if (abs(denom) < 1e-5) discard;
                float t = -(dot(i.worldPos, _FloorPlaneN) + _FloorPlaneD) / denom;
                // Allow t < 0 (floor between camera and quad) so tiles don't vanish when camera moves.
                float3 worldPos = i.worldPos + t * viewDir;
                float3 toP = worldPos - _FloorCenter;
                float u = dot(toP, float3(1, 0, 0)) / _FloorSize * _Tiling;
                float3 axisV = normalize(cross(_FloorPlaneN, float3(1, 0, 0)));
                float v = dot(toP, axisV) / _FloorSize * _Tiling * _TilingVAspect;
                float2 uv = float2(u, v);
                fixed4 c = tex2D(_MainTex, uv) * _Color;
                c.a = 1.0;
                return c;
            }
            ENDCG
        }
    }
}
