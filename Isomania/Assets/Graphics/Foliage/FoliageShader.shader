Shader "Custom/FoliageShader"
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
    }

	SubShader
	{
		Pass
		{
			Tags
			{
				"RenderType" = "TransparentCutout"
				"Queue" = "Geometry+1"
				"LightMode" = "ForwardBase"
				"PassFlags" = "OnlyDirectional"
			}

			CGPROGRAM
			#pragma vertex vert
			#pragma geometry geo
			#pragma fragment frag
			#pragma require geometry

			// For Tessellation
			//#pragma hull hull
			//#pragma domain domain
			//#pragma target 4.6
			//#include "/Assets/Graphics/CGincFiles/Geo/CustomTessellation.cginc"

			// For no tesselation
			#include "/Assets/Graphics/CGincFiles/Geo/CustomGeo.cginc"

			#include "/Assets/Graphics/CGincFiles/ToonShading/FlatToonShading.cginc"
			#include "/Assets/Graphics/CGincFiles/Billboard/BillboardQuad.cginc"

			float4 frag(g2f i, float facing : VFACE) : SV_Target
			{
				float uv_remap = 1 / _TileAmount;
				float alpha = tex2D(_MainTex, i.uv * uv_remap + i.wind).r;

				if (alpha == 0)
				{
					discard;
				}
				
				return ToonShade(i.toonUV);

				/*
				float3 light_col = _LightColor0.rgb;

				float color_value = tex2D(_ColorShading, i.light).r;
				float4 main_color = tex2D(_Colors, color_value);

				// for each light take highlight
				float highlight_value = tex2D(_HighlightShading, i.light).r * _LightColorValue;
				float4 highlight_color = float4(light_col, highlight_value);

				float4 color = float4(alphaBlend(highlight_color, main_color).rgb, 1);
				return color;
				*/
			}
			ENDCG
		}
    }
}