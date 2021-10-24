Shader "Custom/Normals Texture"
{
    Properties
    {
    }
    SubShader
    {
        Tags 
		{ 
			"RenderType" = "Opaque" 
		}

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
				float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
				float3 viewNormal : NORMAL;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.viewNormal = COMPUTE_VIEW_NORMAL;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                return float4(i.viewNormal, 0);
            }
            ENDCG
        }
    }
	SubShader
    {
        Tags 
		{ 
			"RenderType" = "TransparentCutout" 
		}

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
            };

            struct v2f
            {
            };

            v2f vert (appdata v)
            {
                v2f o;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                return float4(0, 0, 0.866025403784439, 0); // cos(30)
            }
            ENDCG
        }
    }
}
