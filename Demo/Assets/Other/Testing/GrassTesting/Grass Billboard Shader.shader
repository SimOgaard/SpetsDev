﻿Shader "Custom/Grass Billboard Shader"
{
    Properties
    {
		_MainTex ("Texture", 2D) = "white" {}
		_TileAmount("Amount of Tiles", Int) = 8
		_TilePixelSize("Tile Pixel Size", Int) = 16
		
		_YDisplacement ("Y Displacement", Float) = 0.5
		_XZDisplacementRandom ("X Z Displacement Random", Float) = 0.5
		_TessellationUniform("Tessellation Uniform", Range(1, 32)) = 1

		_Colors ("Color Texture", 2D) = "white" {}
		_ColorShading ("Color Shading", 2D) = "white" {}
		
		/*
		_WindDistortionMap ("Distortion Map Texture", 2D) = "white" {}
		_WindFrequency("Wind Frequency", Vector) = (0.05, 0.05, 0, 0)
		_WindStrength("Wind Strength", Float) = 1
		*/

		_ShadowSoftness("Shadow Softness", Range(0, 1)) = 0.5
    }

	SubShader
	{
		Pass
		{
			Tags
			{
				"RenderType" = "Opaque"
				"LightMode" = "ForwardAdd"
				"PassFlags" = "OnlyDirectional"
			}
			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight

			#include "/Assets/Graphics/CGincFiles/NormalShading.cginc"

			sampler2D _ColorShading;
			float4 _ColorShading_ST;

			sampler2D _Colors;
			float4 _Colors_ST;

			float4 frag(v2f i) : SV_Target
			{
				float light = CalculateLight(i);

				float curve_value = tex2D(_ColorShading, light).r;
				float4 color = tex2D(_Colors, curve_value);

				return color;
			}
			ENDCG
		}
		UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"

		Pass
		{
			Tags
			{
				"RenderType"="TransparentCutout"
				"Queue"="Geometry+1"
			}

			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma geometry geo
			#pragma fragment frag
			#pragma require geometry

			// For Tessellation
			//#pragma hull hull
			//#pragma domain domain
			//#pragma target 4.6

			#pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight

			#include "/Assets/Graphics/CGincFiles/CustomGeo.cginc"
			//#include "/Assets/Graphics/CGincFiles/CustomTessellation.cginc"
			#include "/Assets/Graphics/CGincFiles/FlatShadingSetup.cginc"
			#include "/Assets/Graphics/CGincFiles/BillboardGrass.cginc"

			sampler2D _MainTex;
			float4 _MainTex_ST;

			sampler2D _ColorShading;
			float4 _ColorShading_ST;

			sampler2D _Colors;
			float4 _Colors_ST;

			float4 frag(g2f i, float facing : VFACE) : SV_Target
			{
				float uv_remap = 1 / _TileAmount;
				float alpha = tex2D(_MainTex, i.uv * uv_remap + i.wind).r;

				if (alpha == 0)
				{
					discard;
				}

				float curve_value = tex2D(_ColorShading, i.light).r;
				float4 color = tex2D(_Colors, curve_value);

				return float4(color.rgb, 1);
			}
			ENDCG
		}
    }
}