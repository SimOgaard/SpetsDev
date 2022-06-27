Shader "Custom/Wood Shader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

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
				return ToonShade(i);
			}
			ENDCG
		}
		UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
    }
}
