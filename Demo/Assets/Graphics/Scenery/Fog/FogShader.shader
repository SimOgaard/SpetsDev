Shader "Custom/Fog Shader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

		_Colors ("Color Texture", 2D) = "white" {}
		_ColorShading ("Color Shading", 2D) = "white" {}
		_HighlightShading ("Highlight Shading", 2D) = "white" {}

		_ShadowSoftness("_ShadowSoftness", Range(0,1)) = 0.35
		_LightColorValue("Light Color Value", Range(0, 1)) = 0
    }

    SubShader
    {
		Pass
		{
			Tags
			{
				"RenderType" = "Transparent"
				"Queue"="Transparent+10"
				//"LightMode" = "ForwardAdd"
				//"PassFlags" = "OnlyDirectional"
			}
			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight


			#include "UnityCG.cginc"
			#include "AutoLight.cginc"
			#include "Lighting.cginc"

			sampler2D _LightTexture0;
			float4x4 unity_WorldToLight;

			float _ShadowSoftness;
			float _Ambient;
			float _Darkest;

			float _LightColorValue;

			struct v2f
			{
				float2 uv : TEXCOORD0;
				SHADOW_COORDS(1)
				float light_val : COLOR0;
				float4 pos : SV_POSITION;
				float3 worldPos : TEXCOORD2;
			};

			v2f vert(appdata_base v)
			{
				v2f o;
				v.vertex.y += sin(_Time*10) * 10;

				o.pos = UnityObjectToClipPos(v.vertex);
				o.worldPos = mul (unity_ObjectToWorld, v.vertex);
				o.uv = v.texcoord;
				float3 worldNormal = UnityObjectToWorldNormal(v.normal);
				o.light_val = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
				TRANSFER_SHADOW(o)
				return o;
			}

			float CalculateLight(v2f i)
			{
				float shadow = SHADOW_ATTENUATION(i);
				shadow = saturate(shadow + _ShadowSoftness);

				float2 uvCookie = mul(unity_WorldToLight, float4(i.worldPos, 1)).xy;
				float attenuation = tex2D(_LightTexture0, uvCookie).w;

				return max(saturate(i.light_val * shadow * attenuation + _Ambient), _Darkest);
			}


			#include "/Assets/Graphics/CGincFiles/GenericShaderFunctions.cginc"

			sampler2D _Colors;
			float4 _Colors_ST;

			sampler2D _ColorShading;
			float4 _ColorShading_ST;

			sampler2D _HighlightShading;
			float4 _HighlightShading_ST;

			float4 frag(v2f i) : SV_Target
			{
				float3 light_col = _LightColor0.rgb;
				float light_value = CalculateLight(i);

				float color_value = tex2D(_ColorShading, light_value).r;
				float4 main_color = tex2D(_Colors, color_value);

				// for each light take highlight
				float highlight_value = tex2D(_HighlightShading, light_value).r * _LightColorValue;
				float4 highlight_color = float4(light_col, highlight_value);

				float4 color = float4(alphaBlend(highlight_color, main_color).rgb, 1);
				return color;
			}
			ENDCG
		}
		UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
    }
}
