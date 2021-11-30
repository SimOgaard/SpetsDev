Shader "Custom/Wood Shader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

        _Colors ("Color Texture", 2D) = "white" {}
		_CurveTexture ("Curve Texture", 2D) = "white" {}

		_ShadowSoftness("Shadow Softness", Range(0, 1)) = 0.5
		_DarkestValue("Darkest Value", Range(0, 1)) = 0.0
    }

    SubShader
    {
        Pass
        {
			Tags
			{
				"Queue" = "Geometry+2"
				"RenderType"= "Opaque"
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

			float4 frag(v2f i) : SV_Target
			{
				float light = CalculateLight(i);

				float curve_value = tex2D(_CurveTexture, light).r;
				float4 color = tex2D(_Colors, curve_value);

				return float4(color.rgb, 1);
			}
			ENDCG
		}
		UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
    }
}
