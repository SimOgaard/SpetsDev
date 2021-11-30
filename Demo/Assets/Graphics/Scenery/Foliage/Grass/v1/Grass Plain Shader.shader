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
				"LightMode" = "ForwardAdd"
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
				float3 diff : COLOR0;
                float3 ambient : COLOR1;
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD2;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

	        float4 _ColorLight;
			float4 _ColorMedium;
			float4 _ColorDark;
			float4 _ColorReallyDark;

			float _FirstThreshold;
			float _SecondThreshold;
			float _ThirdThreshold;

			sampler2D _LightTexture0;
			float4x4 unity_WorldToLight;

            v2f vert (appdata_base v)
            {
                v2f o;
				o.worldPos = mul (unity_ObjectToWorld, v.vertex);
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
				float3 worldNormal = UnityObjectToWorldNormal(v.normal);
                float nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
                o.diff = nl * _LightColor0.rgb;
                o.ambient = ShadeSH9(float4(worldNormal,1));
                TRANSFER_SHADOW(o)
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
				float shadow = SHADOW_ATTENUATION(i);
				float2 uvCookie = mul(unity_WorldToLight, float4(i.worldPos, 1)).xy;
				float attenuation = tex2D(_LightTexture0, uvCookie).w;
                float3 lighting = i.diff * shadow * attenuation + i.ambient;

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
