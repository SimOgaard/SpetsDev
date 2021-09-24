Shader "Custom/shadow testing shader 3"
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
				"LightMode" = "ForwardAdd"
				"PassFlags" = "OnlyDirectional"
			}
			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma geometry geo
			#pragma fragment frag
			#pragma require geometry
			#pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight

			#include "/Assets/Graphics/CGincFiles/FlatShadingSetup.cginc"
			#include "/Assets/Graphics/CGincFiles/Geo/SimpleGeo.cginc"
			
			fixed4 frag(g2f i, fixed facing : VFACE) : SV_Target
			{
				return i.light;
			}
			ENDCG
		}
		UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
	}
}