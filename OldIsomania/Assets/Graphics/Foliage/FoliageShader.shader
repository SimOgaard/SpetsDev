Shader "Custom/FoliageShader"
{
    Properties
    {
		_MainTex ("Texture", 2D) = "white" {}
		_TileAmount("Amount of Tiles", Int) = 8
		_TilePixelSize("Tile Pixel Size", Int) = 16
		
		_BillboardDisplacement ("Billboard Displacement", Vector) = (.5, .25, .5, 0)
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
			// Common things like unity lightning and functions import
			#include "/Assets/Graphics/CGincFiles/Common.cginc"
			// Shading part of this shader
			#include "/Assets/Graphics/CGincFiles/ToonShading/FlatToonShading.cginc"
			// Geometry part of this shader, should be billboard quads for each triangle
			#include "/Assets/Graphics/CGincFiles/Geometry/Billboard/BillboardQuadFlat.cginc"
			// We want to get wind value for each pixel
			#include "/Assets/Graphics/CGincFiles/Wind.cginc"

			float4 frag(v2f i, float facing : VFACE) : SV_Target
			{
				// gets wind values
				float3 wind = GetWind(i.worldCenter);
				float2 windUV = WindToUVWindCoords(wind);
				float2 windTile = UVWindCoordsToTile(windUV);
				//return float4(windTile / _TileAmount, 0, 1);
				// remaps uv 0-1 to a single tile of maintexture
				float uvRemap = 1.0 / _TileAmount;

				float2 uv = (i.uv + windTile) * uvRemap; // i.uv * uvRemap + windTile * uvRemap

				float alpha = tex2D(_MainTex, uv).r;

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