Shader "Custom/shadow testing shader 4"
{
    Properties
    {
		_ShadowSoftness("Shadow Softness", Float) = 0.5
    }

	SubShader
	{
		Pass
		{
			Tags {
				"RenderType" = "Opaque"
				//"LightMode" = "ForwardAdd"
				//"PassFlags" = "OnlyDirectional"
			}
			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight

			#include "/Assets/Graphics/CGincFiles/NormalShading.cginc"
			
			float4 frag(v2f i) : SV_Target
			{
				float light = CalculateLight(i);

				return light;
			}
			ENDCG
		}
		UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
	}
}