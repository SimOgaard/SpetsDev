Shader "Custom/FireBallShader"
{
    Properties
    {
		_Colors ("Color Texture", 2D) = "white" {}
		_CurveTexture ("Curve Texture", 2D) = "white" {}
    }
    SubShader
    {
		Pass
		{
			Tags
			{
				"RenderType" = "Opaque"
				"Queue"="Geometry+2"
			}
			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag

			#include "/Assets/Graphics/CGincFiles/NoShading.cginc"
			#include "/Assets/Graphics/FastNoiseLite.cginc"

			sampler2D _CurveTexture;
			float4 _CurveTexture_ST;

			sampler2D _Colors;
			float4 _Colors_ST;

			float4 _FireBallCentre;
			float4 _FireDirection;

			float remap01(float v) {
				return saturate(0.5+v);
			}

			fixed4 frag(v2f i) : SV_Target
			{
				fnl_state noise = fnlCreateState();
				noise.rotation_type_3d = 2;

				noise.fractal_type = 1;
				noise.octaves = 3;
				noise.lacunarity = 3;
				noise.gain = 1;
				noise.weighted_strength = 0.25;
				noise.frequency = 0.15;

				noise.domain_warp_amp = 3;

				float3 noise_pos = i.worldPos;
				noise_pos += _FireDirection.xyz * _Time[0];

				fnlDomainWarp3D(noise, noise_pos.x, noise_pos.y, noise_pos.z);
				fixed noise_value = remap01(fnlGetNoise3D(noise, noise_pos.x, noise_pos.y, noise_pos.z));

				fixed curve_value = tex2D(_CurveTexture, noise_value).r;
				fixed4 color = tex2D(_Colors, curve_value);

				return fixed4(color.rgb, 1);
			}
			ENDCG
		}
		UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
    }
}
