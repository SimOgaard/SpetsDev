Shader "Custom/Wood Shader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

        _Colors ("Color Texture", 2D) = "white" {}
		_CurveTexture ("Curve Texture", 2D) = "white" {}

		_ShadowSoftness("Shadow Softness", Float) = 0.5
    }

    SubShader
    {
        Pass
        {
			Tags {
				"Queue" = "Transparent-1"
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

			fixed4 frag(v2f i) : SV_Target
			{
				fixed light = CalculateLight(i);

				fixed curve_value = tex2D(_CurveTexture, light).r;
				fixed4 color = tex2D(_Colors, curve_value);

				return color;
			}
			ENDCG
		}
		UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
    }
}
