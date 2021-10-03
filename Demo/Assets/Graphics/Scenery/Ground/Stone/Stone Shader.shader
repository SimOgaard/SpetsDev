Shader "Custom/Stone Shader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_Color ("Color", Color) = (1, 1, 1, 1)
    }

    SubShader
    {
		Pass
		{
			Tags
			{
				"RenderType" = "Opaque"
				"Queue"="Transparent-1"
				"LightMode" = "ForwardAdd"
				"PassFlags" = "OnlyDirectional"
			}
			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight

			#include "/Assets/Graphics/CGincFiles/NormalShading.cginc"

			float4 _Color;

			fixed4 frag(v2f i) : SV_Target
			{
				fixed light = CalculateLight(i);
				fixed4 light_color = _LightColor0.rgba * light;
				return fixed4(_Color.rgb * light_color.rgb, 1);
			}
			ENDCG
		}
		UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
    }
}
