Shader "Custom/Golem Shader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

        _Color ("Color", Color) = (1, 1, 1, 1)
        _Colors ("Color Texture", 2D) = "white" {}
		_CurveTexture ("Curve Texture", 2D) = "white" {}

		_Distort("Distort", 2D) = "white" {}
		_Radius("Radius", Range(0, 1)) = 0.75
		_DistortionAmount("Distortion Amount", Float) = 0.1

		_ShadowSoftness("Shadow Softness", Range(0, 1)) = 0.5
		_DarkestValue("Darkest Value", Range(0, 1)) = 0.0
    }

    SubShader
    {
		Pass
		{
			Tags
			{
				"RenderType"= "Opaque"
				"Queue"="Geometry+2"
				"LightMode" = "ForwardAdd"
				"PassFlags" = "OnlyDirectional"
			}
			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight

			#include "/Assets/Graphics/CGincFiles/NormalShading.cginc"

			sampler2D _CurveTexture;
			float4 _CurveTexture_ST;

			sampler2D _Colors;
			float4 _Colors_ST;

			float4 _Color;

			sampler2D _Distort;
			float _DistortionAmount;
			float _Radius;

			float circle(in float2 _st, in float _radius){
				fixed2 dist = _st-fixed2(0.5, 0.5);
				return 1.-smoothstep(_radius-(_radius*0.01), _radius+(_radius*0.01), dot(dist,dist)*4.0);
			}

			fixed4 frag(v2f i) : SV_Target
			{
				fixed light = CalculateLight(i);
				
				fixed2 distortSample = (tex2D(_Distort, i.uv).xy * 2 - 1) * _DistortionAmount;
				fixed value = circle(i.uv + distortSample, _Radius);

				if (value < 0.5){
					fixed4 light_color = _LightColor0.rgba * light;
					return _Color * light_color;
				}

				fixed curve_value = tex2D(_CurveTexture, light).r;
				fixed4 color = tex2D(_Colors, curve_value);

				return fixed4(color.rgb, 1);
			}
			ENDCG
		}
		UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
    }
}
