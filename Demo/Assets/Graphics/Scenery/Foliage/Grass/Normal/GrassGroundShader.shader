Shader "Unlit/GrassGroundShaderSplit"
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
				"Queue" = "Geometry"
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
				/*
				float3 light_col = _LightColor0.rgb;
				float light_value = CalculateLight(i);

				float color_value = tex2D(_ColorShading, light_value).r;
				float4 main_color = tex2D(_Colors, color_value);

				// for each light take highlight
				float highlight_value = tex2D(_HighlightShading, light_value).r * _LightColorValue;
				float4 highlight_color = float4(light_col, highlight_value);

				float4 color = float4(alphaBlend(highlight_color, main_color).rgb, 1);
				return color;
				*/
			}
			ENDCG
		}
		UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
    }
}
