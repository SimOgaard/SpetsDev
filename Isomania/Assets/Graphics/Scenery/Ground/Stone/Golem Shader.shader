Shader "Custom/Golem Shader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

		_Colors ("Color Texture", 2D) = "white" {}
		_ColorShading ("Color Shading", 2D) = "white" {}
		
		_ColorsMoss ("Color Texture Moss", 2D) = "white" {}
		_ColorShadingMoss ("Color Shading Moss", 2D) = "white" {}
		_HighlightShading ("Highlight Shading", 2D) = "white" {}

		_Distort("Distort", 2D) = "white" {}
		_Radius("Radius", Range(0, 1)) = 0.75
		_DistortionAmount("Distortion Amount", Float) = 0.1

		_ShadowSoftness("Shadow Softness", Range(0, 1)) = 0.5
		_LightColorValue("Light Color Value", Range(0, 1)) = 0
    }

    SubShader
    {
		Pass
		{
			Tags
			{
				"RenderType"= "Opaque"
				"Queue"="Geometry+2"
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

			sampler2D _Colors;
			float4 _Colors_ST;

			sampler2D _ColorShading;
			float4 _ColorShading_ST;

			sampler2D _ColorsMoss;
			float4 _ColorsMoss_ST;

			sampler2D _ColorShadingMoss;
			float4 _ColorShadingMoss_ST;

			sampler2D _HighlightShading;
			float4 _HighlightShading_ST;

			sampler2D _Distort;
			float _DistortionAmount;
			float _Radius;

			float circle(in float2 _st, in float _radius){
				float2 dist = _st-float2(0.5, 0.5);
				return 1.-smoothstep(_radius-(_radius*0.01), _radius+(_radius*0.01), dot(dist,dist)*4.0);
			}

			float4 frag(v2f i) : SV_Target
			{
				float3 light_col = _LightColor0.rgb;
				float light_value = CalculateLight(i);
				
				float2 distortSample = (tex2D(_Distort, i.uv).xy * 2 - 1) * _DistortionAmount;
				float value = circle(i.uv + distortSample, _Radius);

				float4 main_color;
				if (value < 0.5){
					float curve_value = tex2D(_ColorShading, light_value).r;
					main_color = tex2D(_Colors, curve_value);
				}
				else
				{
					float curve_value = tex2D(_ColorShadingMoss, light_value).r;
					main_color = tex2D(_ColorsMoss, curve_value);				
				}

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
