Shader "Custom/shadow testing shader 3"
{
    Properties
    {
		_ShadowSoftness("Shadow Softness", Range(0, 1)) = 0.5
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
			#pragma geometry geo
			#pragma fragment frag
			#pragma require geometry
			#pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight

			#include "/Assets/Graphics/CGincFiles/Geo/CustomGeo.cginc"
			#include "/Assets/Graphics/CGincFiles/FlatShadingSetup.cginc"
			#include "/Assets/Graphics/CGincFiles/Geo/FlatGeo.cginc"
			
			float4 frag(g2f i, float facing : VFACE) : SV_Target
			{
				return float4(i.light, i.light, i.light, 1);
			}
			ENDCG
		}
		UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
	}
}