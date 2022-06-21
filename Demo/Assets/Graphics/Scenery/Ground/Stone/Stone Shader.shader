Shader "Custom/Stone Shader"
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

			#include "/Assets/Graphics/CGincFiles/ToonShading/ToonShading.cginc"
			#include "/Assets/Graphics/CGincFiles/GenericShaderFunctions.cginc"

			float4 frag(v2f i) : SV_Target
			{
				return CalculateLight(i);
			}
			ENDCG
		}
		UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
    }
}
