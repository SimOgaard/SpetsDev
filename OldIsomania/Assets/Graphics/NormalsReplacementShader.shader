Shader "Custom/Normals Replacement Shader"
{
    Properties
    {
    }
    SubShader
    {
        Tags 
		{ 
			"RenderType" = "VertexSnap"
		}

        Pass
        {
            ZWrite On
            ZTest LEqual

            CGPROGRAM
            #pragma vertex vertNormalSnap
            #pragma fragment frag

			// Common things like unity lightning and functions import
			#include "/Assets/Graphics/CGincFiles/Common.cginc"
			// Geometry part of this shader
			#include "/Assets/Graphics/CGincFiles/Geometry/GeoSnap.cginc"
			// Frag part
			#include "/Assets/Graphics/CGincFiles/Normal/NormalFrag.cginc"

            ENDCG
        }
    }
	SubShader
    {
        Tags 
		{ 
			"RenderType" = "Vertex"
		}

        Pass
        {
            ZWrite On
            ZTest Less

            CGPROGRAM
            #pragma vertex vertNormal
            #pragma fragment frag

            // Common things like unity lightning and functions import
			#include "/Assets/Graphics/CGincFiles/Common.cginc"
			// Geometry part of this shader
			#include "/Assets/Graphics/CGincFiles/Geometry/Geo.cginc"
			// Frag part
			#include "/Assets/Graphics/CGincFiles/Normal/NormalFrag.cginc"

            ENDCG
        }
    }
}
