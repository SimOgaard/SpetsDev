Shader "Custom/Equipment Drop Shader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1, 1, 1, 1)
		//_DisplacementConstant("Displacement Constant", Vector) = (0, 1.25, 0, 0)
		//_BobDisplacement("Bob Displacement", Vector) = (0, 0.2, 0, 0)
		//_BobFrequency("Bob Frequency", Float) = 100
    }
    SubShader
    {
        Tags
		{
			"RenderType"="Opaque"
			"Queue"="Geometry+2"
		}
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
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
			float4 _Color;

			float4 _DisplacementConstant;
			float4 _BobDisplacement;
			float _BobFrequency;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex + _DisplacementConstant + sin(_Time[0] * _BobFrequency) * _BobDisplacement);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                return float4(_Color.rgb, 1);
            }
            ENDCG
        }
    }
}
