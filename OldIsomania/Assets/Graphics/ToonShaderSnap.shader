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

			#pragma vertex vertSnap
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
				//return i.worldPosition.x < 5.1000001;
				//return float4(i.pos.rgb, 1.0);
				//return float4(i.worldPosition, 1.0);
				//return tex2D(_Colors, 0);
				return ToonShade(i);
			}
			ENDCG
		}
		/*
		// shadow caster rendering pass, implemented manually
        Pass
        {
            Tags {"LightMode"="ShadowCaster"}

            CGPROGRAM
            #pragma vertex vertShadowSnap
            #pragma fragment frag
            #pragma multi_compile_shadowcaster

			// Common things like unity lightning and functions import
			#include "/Assets/Graphics/CGincFiles/Common.cginc"
			// Geometry part of this shader
			#include "/Assets/Graphics/CGincFiles/Geometry/GeoSnap.cginc"

            float4 frag(v2f_shadow i) : SV_Target
            {
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }
		*/
		//UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
		
		
		
		
		// shadow caster rendering pass, implemented manually
        Pass
        {
            Tags {"LightMode"="ShadowCaster"}

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_shadowcaster

			#include "UnityCG.cginc"

			#include "/Assets/Graphics/CGincFiles/GenericShaderFunctions.cginc"
			#include "/Assets/Graphics/CGincFiles/PixelPerfectShaderFunctions.cginc"
			#include "/Assets/Graphics/CGincFiles/Geometry/SnapSetup.cginc"

			struct appdata
			{
				float4 vertex : POSITION;				
				float4 uv : TEXCOORD0;
				float3 normal : NORMAL;
				float3 worldPosition : TEXCOORD1;
				float4 screenPosition : TEXCOORD3;
			};

			struct v2f
			{
				V2F_SHADOW_CASTER;
			};

			v2f vert (appdata v)
			{

				if (unity_OrthoParams.w == 0.0)
				{
					v2f o;
					TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
					return o;
				}

				// create v2f_shadow
				v2f o;

				// get vertex object snapped difference
				float4 m_vertexClipSnappedDiff = MainCameraClipSnappedDifference();
				float4 vertexWorldSnappedDiff = SnappedDifferenceWorld(m_vertexClipSnappedDiff);
				float4 vertexObjectSnappedDiff = SnappedDifferenceObject(vertexWorldSnappedDiff);

				// snap current vertex
				v.vertex += vertexObjectSnappedDiff;

				// get vertex clip
				o.pos = UnityObjectToClipPos(v.vertex);

				// Defined in Autolight.cginc.
				TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)

				return o;
			}

            float4 frag(v2f i) : SV_Target
            {
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }
		
    }
}
