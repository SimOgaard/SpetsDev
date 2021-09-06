Shader "Custom/Colossal Plains Grass Plain Shader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ColorLight ("ColorLight", Color) = (0.7607843, 0.8666667, 0.5960785, 1)
        _ColorMedium ("ColorMedium", Color) = (0.4666667, 0.654902, 0.4196078, 1)
        _ColorDark ("ColorDark", Color) = (0.1215686, 0.3411765, 0.3058824, 1)
		_ColorReallyDark ("ColorReallyDark", Color) = (0.1215686, 0.3411765, 0.3058824, 1)

        _FirstThreshold ("FirstThreshold", Range(0,1)) = 0.3
        _SecondThreshold ("SecondThreshold", Range(0,1)) = 0.5
        _ThirdThreshold ("ThirdThreshold", Range(0,1)) = 0.5
    }

    SubShader
    {
        Pass
        {
			Tags {
				"RenderType"= "Opaque"
				"LightMode" = "ForwardBase"
				"PassFlags" = "OnlyDirectional"
			}
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
			#include "Lighting.cginc"

			#pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
            #include "AutoLight.cginc"

            struct v2f
            {
                float2 uv : TEXCOORD0;
				SHADOW_COORDS(1)
				fixed3 diff : COLOR0;
                fixed3 ambient : COLOR1;
                float4 pos : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

	        fixed4 _ColorLight;
			fixed4 _ColorMedium;
			fixed4 _ColorDark;
			fixed4 _ColorReallyDark;

			float _FirstThreshold;
			float _SecondThreshold;
			float _ThirdThreshold;

            v2f vert (appdata_base v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
				half3 worldNormal = UnityObjectToWorldNormal(v.normal);
                half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
                o.diff = nl * _LightColor0.rgb;
                o.ambient = ShadeSH9(half4(worldNormal,1));
                TRANSFER_SHADOW(o)
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				fixed shadow = SHADOW_ATTENUATION(i);
                fixed3 lighting = i.diff * shadow + i.ambient;

				float4 col = _ColorLight;
				if (lighting.x < _FirstThreshold)
				{
					col = _ColorReallyDark;
				}
				else if (lighting.x < _SecondThreshold)
				{
					col = _ColorDark;
				}
				else if (lighting.x < _ThirdThreshold)
				{
					col = _ColorMedium;				
				}

                return col;
            }
            ENDCG
        }
		UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
    }
}
