Shader "Custom/FireGrassShader"
{
    Properties
    {
		_Size("Size", Float) = 1
		_YDisplacement("Y Displacement", Float) = 1
		_XZDisplacementRandom("XZ Displacement Random", Float) = 1

		_Cutoff("Cutoff", Range(0, 1)) = 0.5
		_Speed("Speed", Float) = 1.5

		_Colors ("Color Texture", 2D) = "white" {}
		_CurveTexture ("Curve Texture", 2D) = "white" {}
    }

    SubShader
    {
        Pass
        {
			Tags {
				"RenderType"="TransparentCutout"
				"Queue"="AlphaTest"
			}
			
			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma geometry geo
			#pragma fragment frag
			#pragma require geometry

			#include "/Assets/Graphics/CGincFiles/CustomGeo.cginc"
			#include "/Assets/Graphics/CGincFiles/BillboardFire.cginc"
			#include "/Assets/Graphics/CGincFiles/FastNoiseLite.cginc"

			sampler2D _CurveTexture;
			float4 _CurveTexture_ST;

			sampler2D _Colors;
			float4 _Colors_ST;

			float _Cutoff;
			float _Speed;

			float4 frag (g2f i, float facing : VFACE) : SV_Target
            {
				fnl_state noise = fnlCreateState();
				noise.rotation_type_3d = 2;

				noise.fractal_type = 1;
				noise.octaves = 2;
				noise.lacunarity = 3;
				noise.gain = 1;
				noise.weighted_strength = 0.25;
				noise.frequency = 2;

				noise.domain_warp_amp = 0;

				float3 noise_pos;
				noise_pos.xy = i.uv + i.worldPos.xy;
				noise_pos.y -= 2 * _Time[0] * _Speed;
				noise_pos.z = i.worldPos.z;

				fnlDomainWarp3D(noise, noise_pos.x, noise_pos.y, noise_pos.z);
				float noise_value = saturate(fnlGetNoise3D(noise, noise_pos.x, noise_pos.y, noise_pos.z) + 1);
				
				float gradient = pow(i.uv.y+0.5, 1.5);
				noise_value *= gradient;

				if (noise_value > _Cutoff)
				{
					discard;
				}

				float curve_value = tex2D(_CurveTexture, noise_value).r;
				float4 color = tex2D(_Colors, curve_value);

				return float4(color.rgb, 1);
            }
            ENDCG
        }
    }
}