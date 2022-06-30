Shader "Custom/Toon Shader Snap"
{
    Properties
    {
		_Colors ("Color Texture", 2D) = "white" {}
		_ColorShading ("Color Shading", 2D) = "white" {}
    }

	SubShader
	{
		Pass
		{
			Tags
			{
				"RenderType" = "Opaque"
				"Queue" = "Geometry+2"
				"LightMode" = "ForwardBase"
				"PassFlags" = "OnlyDirectional"
			}
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			// Common things like unity lightning and functions import
			#include "/Assets/Graphics/CGincFiles/Common.cginc"
			// Geometry part of this shader
			#include "/Assets/Graphics/CGincFiles/Geometry/GeoSnap.cginc"
			// Shading part of this shader
			#include "/Assets/Graphics/CGincFiles/ToonShading/ToonShading.cginc"

			float4 frag(v2f i) : SV_Target
			{
				//return _ProjectionParams.z - _ProjectionParams.y > 0;
				//return i.worldPosition.z > -100;
				//return float4(i.pos.rgb, 1.0);
				//return float4(i.worldPosition, 1.0);
				//return tex2D(_Colors, 0);
				return ToonShade(i);
			}
			ENDCG
		}
		
		// shadow caster rendering pass, implemented manually
        // using macros from UnityCG.cginc
        Pass
        {
            Tags {"LightMode"="ShadowCaster"}

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_shadowcaster

			// Common things like unity lightning and functions import
			#include "/Assets/Graphics/CGincFiles/Common.cginc"
			// Geometry part of this shader
			#include "/Assets/Graphics/CGincFiles/Shadow/ShadowSnap.cginc"

            float4 frag(v2f i) : SV_Target
            {
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }
		
		//UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
    }
}
