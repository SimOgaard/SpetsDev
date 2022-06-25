Shader "Custom/Dirt Shader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

		_Colors ("Color Texture", 2D) = "white" {}
		_ColorShading ("Color Shading", 2D) = "white" {}
    }

    SubShader
    {
		Pass
		{
			Tags
			{
				"RenderType"= "Opaque"
				"Queue"="Geometry+2"
				"LightMode" = "ForwardBase"
				"PassFlags" = "OnlyDirectional"
			}
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "/Assets/Graphics/CGincFiles/ToonShading/ToonShading.cginc"

            float4 frag (v2f i) : SV_Target
            {
				fnl_state noise = fnlCreateState();
				noise.seed = 1337; //_Noise_Seed;
				noise.frequency = 0.5; //_Noise_Frequency;
				noise.noise_type = 2; //_Noise_NoiseType;
				noise.rotation_type_3d = 2; //_Noise_RotationType3D;

				noise.fractal_type = 0; //_Noise_FractalType;
				noise.octaves = 0; //_Noise_FractalOctaves;
				noise.lacunarity = 0.0; //_Noise_FractalLacunarity;
				noise.gain = 0.0; //_Noise_FractalGain;
				noise.weighted_strength = 0.0; //_Noise_FractalWeightedStrength;
				noise.ping_pong_strength = 0.0; //_Noise_FractalPingPongStrength;

				noise.cellular_distance_func = 1; //_Noise_CellularDistanceFunction;
				noise.cellular_return_type = 4; //_Noise_CellularReturnType;
				noise.cellular_jitter_mod = 1.0; //_Noise_CellularJitter;

				fnl_state warp = fnlCreateState();
				warp.domain_warp_type = 0; //_Warp_DomainWarpType;
				warp.rotation_type_3d = 0; //_Warp_RotationType3D;
				warp.domain_warp_amp = 0.0; //_Warp_DomainWarpAmplitude;
				warp.frequency = 0.0; //_Warp_Frequency;

				warp.fractal_type = 0; //_Warp_FractalType;
				warp.octaves = 0; //_Warp_FractalOctaves;
				warp.lacunarity = 0.0; //_Warp_FractalLacunarity;
				warp.gain = 0.0; //_Warp_FractalGain;

				float x = 1;//i.worldPos.x;
				float y = 2;//i.worldPos.y;
				float z = 3;//i.worldPos.z;
				fnlDomainWarp3D(warp, x, y, z);
				float noise_value = saturate(fnlGetNoise3D(noise, x, y, z) + 1);

				float4 light = ToonShade(i);

				float curve_value = tex2D(_ColorShading, noise_value).r;
				float4 color = tex2D(_Colors, curve_value);

				return float4(color.rgb, 1);
			}
			ENDCG
		}
		UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
    }
}
