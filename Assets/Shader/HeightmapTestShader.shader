Shader "Unlit/HeightmapTestShader"
{
    Properties
    {
        _HeightMap ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags
        {
            "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"
        }
        LOD 200
        Cull Off
        
        Pass
        {
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
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _HeightMap;
            float4 _HeightMap_ST;

            v2f vert (appdata v)
            {
                v2f o;
                float4 h = tex2Dlod(_HeightMap, float4(v.uv.xy, 0, 0));
                v.vertex.y = h.r * 50;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _HeightMap);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_HeightMap, i.uv);
                col.rgb = sin(_Time.y + i.uv * col.r * 2 + 18).x * 0.5 + 0.5;
                col.rgb *= cos(i.uv.y * i.uv.x) * 0.1;
                col.rgb = min(col.rgb, 1.0);
                col.rgb = max(col.rgb, 0.0);
                return fixed4(tex2D(_HeightMap, i.uv).rgb, 1);
            }
            ENDCG
        }
    }
}
