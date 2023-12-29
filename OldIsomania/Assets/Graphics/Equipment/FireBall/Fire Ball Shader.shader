Shader "Custom/FireBallShader"
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
				"Queue"="Geometry+2"
			}
			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag

			#include "/Assets/Graphics/CGincFiles/ToonShading/NoShading.cginc"
			#include "/Assets/Graphics/CGincFiles/Noise/FastNoiseLite.cginc"

			sampler2D _ColorShading;
			float4 _ColorShading_ST;

			sampler2D _Colors;
			float4 _Colors_ST;

			float4 _FireBallCentre;
			float4 _FireDirection;

			float remap01(float v) {
				return saturate(0.5+v);
			}

			float4 frag(v2f i) : SV_Target
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
				float noise_value = remap01(fnlGetNoise3D(noise, noise_pos.x, noise_pos.y, noise_pos.z));

				float curve_value = tex2D(_ColorShading, noise_value).r;
				float4 color = tex2D(_Colors, curve_value);

				return float4(color.rgb, 1);
			}
			ENDCG
		}
		UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
    }
}
