Shader "Unlit/Leaf Shader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_BillboardBlend ("Billboard Blend", Range(0, 1)) = 0
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

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
				float3 worldSpace : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

			float _BillboardBlend;

            v2f vert (appdata v)
            {
                v2f o;
				float3 remap_uv_to_coords = float3(v.uv * 2 - 1, 0);
				float3 to_camera_matrix = normalize(mul(mul(remap_uv_to_coords, UNITY_MATRIX_V), unity_ObjectToWorld));
				float3 lerped_value = lerp(float3(0,0,0), to_camera_matrix, _BillboardBlend);
				v.vertex.xyz += lerped_value;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.worldSpace = mul(unity_ObjectToWorld, v.vertex);

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return float4(i.uv,0,1);
            }
            ENDCG
        }
    }
}
