Shader "Custom/Leaf Shader"
{
    Properties
    {
		_MainTex ("Texture", 2D) = "white" {}
		
		_TileAmount("Amount of Tiles", Int) = 8
		_TilePixelSize("Tile Pixel Size", Int) = 16

        _ExtrudeDistance ("Extrude Distance", Float) = 0.0
		_TessellationUniform ("Tessellation Uniform", Range(1, 32)) = 1

		_UniformDisplacementRandom ("Random Uniformed Displacement", Vector) = (0, 0, 0, 0)

		_DiscardValue ("Discard Value", Range(0, 1)) = 0.5

		_Colors ("Texture", 2D) = "white" {}
		_ColorShading ("Texture", 2D) = "white" {}
		_HighlightShading ("Highlight Shading", 2D) = "white" {}
		
		_WindDistortionMap ("Texture", 2D) = "white" {}
		_WindFrequency("Wind Frequency", Vector) = (0.05, 0.05, 0, 0)
		_WindStrength("Wind Strength", Float) = 1

		_ShadowSoftness("Shadow Softness", Range(0, 1)) = 0.5
		_DarkestValue("Darkest Value", Range(0, 1)) = 0.0
		_LightColorValue("Light Color Value", Range(0, 1)) = 0
    }

    SubShader
    {
		Pass
		{
			Tags
			{
				"RenderType"="TransparentCutout"
				"Queue"="Geometry+2"
			}

			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma geometry geo
			#pragma fragment frag
			#pragma require geometry

			#pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
			
			// For Tessellation
			#pragma hull hull
			#pragma domain domain
			#pragma target 4.6
			#include "/Assets/Graphics/CGincFiles/CustomTessellation.cginc"

			// For no tesselation
			//#include "/Assets/Graphics/CGincFiles/CustomGeo.cginc"
			
			#include "/Assets/Graphics/CGincFiles/FlatShadingSetup.cginc"
			#include "/Assets/Graphics/CGincFiles/BillboardLeaf.cginc"
			#include "/Assets/Graphics/CGincFiles/GenericShaderFunctions.cginc"

			sampler2D _MainTex;
			float4 _MainTex_ST;

			sampler2D _ColorShading;
			float4 _ColorShading_ST;

			sampler2D _Colors;
			float4 _Colors_ST;

			sampler2D _HighlightShading;
			float4 _HighlightShading_ST;

			float _DiscardValue;

			float4 frag(g2f i, float facing : VFACE) : SV_Target
			{
				float uv_remap = 1 / _TileAmount;
				float alpha = tex2D(_MainTex, i.uv * uv_remap + i.wind).r;

				if (alpha < _DiscardValue)
				{
					discard;
				}

				float3 light_col = _LightColor0.rgb;

				float color_value = tex2D(_ColorShading, i.light).r;
				float4 main_color = tex2D(_Colors, color_value);

				// for each light take highlight
				float highlight_value = tex2D(_HighlightShading, i.light).r * _LightColorValue;
				float4 highlight_color = float4(light_col, highlight_value);

				float4 color = float4(alphaBlend(highlight_color, main_color).rgb, 1);
				return color;
			}
			ENDCG
		}

		Pass {
			Name "ShadowCaster"
			Tags { "LightMode" = "ShadowCaster" }

			ZWrite On ZTest LEqual

			CGPROGRAM
			#pragma target 2.0

			#pragma multi_compile_shadowcaster

			#pragma vertex vertShadowCaster
			#pragma fragment fragShadowCaster

			#include "UnityStandardShadow.cginc"

			ENDCG
		}
    }
}
