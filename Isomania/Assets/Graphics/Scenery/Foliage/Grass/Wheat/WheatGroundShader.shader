Shader "Unlit/GrassGroundShaderSplit"
{
    Properties
    {
		_Colors ("Color Texture", 2D) = "white" {}
		_ColorShading ("Color Shading", 2D) = "white" {}

		_ShadowSoftness("Shadow Softness", Range(0, 1)) = 0.5
    }

	SubShader
	{
		Pass
		{
			Tags
			{
				"RenderType" = "Opaque"
				"Queue" = "Geometry"
				//"LightMode" = "ForwardAdd"
				//"PassFlags" = "OnlyDirectional"
			}
			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight

			#include "/Assets/Graphics/CGincFiles/NormalShading.cginc"
			#include "/Assets/Graphics/CGincFiles/GenericShaderFunctions.cginc"

			sampler2D _ColorShading;
			float4 _ColorShading_ST;

			sampler2D _Colors;
			float4 _Colors_ST;

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
