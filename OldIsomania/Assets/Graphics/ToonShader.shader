Shader "Custom/Toon Shader"
{
    Properties
    {
		_Colors ("Color Texture", 2D) = "white" {}
		_ColorShading ("Color Shading", 2D) = "white" {}
    }

	SubShader
	{
		Pass
		{
			Tags
			{
				"RenderType" = "Opaque"
				"Queue" = "Geometry+2"
				"LightMode" = "ForwardBase"
				"PassFlags" = "OnlyDirectional"
			}
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			// Common things like unity lightning and functions import
			#include "/Assets/Graphics/CGincFiles/Common.cginc"
			// Geometry part of this shader
			#include "/Assets/Graphics/CGincFiles/Geometry/Geo.cginc"
			// Shading part of this shader
			#include "/Assets/Graphics/CGincFiles/ToonShading/ToonShading.cginc"

			float4 frag(v2f i) : SV_Target
			{
				//return _ProjectionParams.z - _ProjectionParams.y > 0;
				//return i.worldPosition.z < 0.5;
				//return float4(i.pos.rgb, 1.0);
				//return float4(i.worldPosition, 1.0);
				//return tex2D(_Colors, 0);
				return ToonShade(i);
			}
			ENDCG
		}
		UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
    }
}
