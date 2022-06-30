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
            #pragma vertex vert
            #pragma fragment frag

			// Common things like unity lightning and functions import
			#include "/Assets/Graphics/CGincFiles/Common.cginc"
			// Geometry part of this shader
			#include "/Assets/Graphics/CGincFiles/Normal/NormalSnap.cginc"
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
            #pragma vertex vert
            #pragma fragment frag

            // Common things like unity lightning and functions import
			#include "/Assets/Graphics/CGincFiles/Common.cginc"
			// Geometry part of this shader
			#include "/Assets/Graphics/CGincFiles/Normal/Normal.cginc"
			// Frag part
			#include "/Assets/Graphics/CGincFiles/Normal/NormalFrag.cginc"

            ENDCG
        }
    }
}
