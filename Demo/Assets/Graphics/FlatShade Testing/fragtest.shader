Shader "Custom/fragtest"
{
    Properties
    {
		_ShadowSoftness("Shadow Softness", Range(0, 1)) = 0.5
		_DarkestValue("Darkest Value", Range(0, 1)) = 0.5
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
            #pragma fragment frag
			#pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight

			#include "/Assets/Graphics/CGincFiles/FlatShadingSetup.cginc"

			struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD2;
				float light : TEXCOORD1;
            };

			v2f vert (appdata_base v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos (v.vertex);
                o.worldPos = mul (unity_ObjectToWorld, v.vertex);
                float3 worldNormal = UnityObjectToWorldNormal (v.normal);

				o.light = LightCalculation(v.vertex, worldNormal);

                return o;
            }
			
			float4 frag (v2f i) : SV_Target
			{
				return float4(i.light, i.light, i.light, 1);
			}
			ENDCG
		}
		UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
	}
}