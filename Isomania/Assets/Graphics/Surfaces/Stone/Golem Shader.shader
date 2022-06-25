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
    }

    SubShader
    {
		Pass
		{
			Tags
			{
				"RenderType"= "Opaque"
				"Queue"="Geometry+2"
				"LightMode" = "ForwardBase"
				"PassFlags" = "OnlyDirectional"
			}
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "/Assets/Graphics/CGincFiles/ToonShading/ToonShading.cginc"

			sampler2D _ColorsMoss;
			float4 _ColorsMoss_ST;

			sampler2D _ColorShadingMoss;
			float4 _ColorShadingMoss_ST;

			sampler2D _Distort;
			float _DistortionAmount;
			float _Radius;

			float circle(in float2 _st, in float _radius)
			{
				float2 dist = _st-float2(0.5, 0.5);
				return 1.-smoothstep(_radius-(_radius*0.01), _radius+(_radius*0.01), dot(dist,dist)*4.0);
			}

			float4 frag(v2f i) : SV_Target
			{
				float2 mossDistortSample = (tex2D(_Distort, i.uv).xy * 2 - 1) * _DistortionAmount;
				float mossValue = circle(i.uv + mossDistortSample, _Radius);

				float2 toonUV = ToonUV(i);

				if (mossValue < 0.5)
				{
					// stone
					float shade = tex2D(_ColorShading, toonUV).r;
					return tex2D(_Colors, shade);
				}
				else
				{
					// moss
					float shade = tex2D(_ColorShadingMoss, toonUV).r;
					return tex2D(_ColorsMoss, shade);				
				}
			}
			ENDCG
		}
		UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
    }
}
