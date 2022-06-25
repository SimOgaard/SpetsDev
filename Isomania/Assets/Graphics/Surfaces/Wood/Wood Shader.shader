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
				"Queue" = "Geometry+2"
				"RenderType"= "Opaque"
				"LightMode" = "ForwardBase"
				"PassFlags" = "OnlyDirectional"
			}

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

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
