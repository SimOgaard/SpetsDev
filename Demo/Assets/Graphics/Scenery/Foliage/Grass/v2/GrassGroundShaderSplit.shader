Shader "Unlit/GrassGroundShaderSplit"
{
    Properties
    {
		_MainTex ("Texture", 2D) = "white" {}
		_TileAmount("Amount of Tiles", Int) = 8
		_TilePixelSize("Tile Pixel Size", Int) = 16
		
		_YDisplacement ("Y Displacement", Float) = 0.5
		_XZDisplacementRandom ("X Z Displacement Random", Float) = 0.5
		_TessellationUniform("Tessellation Uniform", Range(1, 32)) = 1

		_Colors ("Color Texture", 2D) = "white" {}
		_CurveTexture ("Curve Texture", 2D) = "white" {}
		
		_WindDistortionMap ("Distortion Map Texture", 2D) = "white" {}
		_WindFrequency("Wind Frequency", Vector) = (0.05, 0.05, 0, 0)
		_WindStrength("Wind Strength", Float) = 1

		_ShadowSoftness("Shadow Softness", Range(0, 1)) = 0.5
		_DarkestValue("Darkest Value", Range(0, 1)) = 0.0
    }

	SubShader
	{
		Pass
		{
			Tags
			{
				"RenderType" = "Opaque"
				"Queue" = "Geometry"
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

			float4 frag(v2f i) : SV_Target
			{
				float light = CalculateLight(i);

				float curve_value = tex2D(_CurveTexture, light).r;
				float4 color = tex2D(_Colors, curve_value);

				return color;
			}
			ENDCG
		}
		UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
    }
}
