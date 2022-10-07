Shader "Hidden/CustomShadows/Main"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            // Shadow Map info
            sampler2D _ShadowTex;
            float4x4 _LightMatrix;
            float4 _LightDirection;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 shadow : TEXCOORD0;
                float3 normal : TEXCOORD1;
            };

            v2f vert (appdata v)
            {
                float4x4 bias = float4x4(
                    0.5, 0.0, 0.0, 0.0,
                    0.0, 0.5, 0.0, 0.0,
                    0.0, 0.0, 0.5, 0.0,
                    0.5, 0.5, 0.5, 1.0
                );

                float4x4 light_mvp = mul(_LightMatrix, unity_ObjectToWorld);

                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.shadow = mul(transpose(bias), mul(light_mvp, v.vertex));
                o.normal = mul(UNITY_MATRIX_M,v.normal);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float3 normal = normalize(i.normal);
                float3 lightdir = normalize(_LightDirection);
                float ambient = 0.25;
                float acne_bias = 0.01;
                float occluder_distance = tex2D(_ShadowTex, i.shadow.xy).r;

                float visibility = (occluder_distance  <  i.shadow.z-acne_bias) ? ambient : 1.0;

                return visibility * dot(normalize(_LightDirection), normal);
            }
            ENDCG
        }
    }
}
