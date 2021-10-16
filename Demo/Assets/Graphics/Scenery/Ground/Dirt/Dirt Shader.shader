Shader "Custom/Dirt Shader"
{
    Properties
    {
		_Colors ("Color Texture", 2D) = "white" {}
		_CurveTexture ("Curve Texture", 2D) = "white" {}

		[Header(Noise settings)]
		_Noise_Seed("Seed", Int) = 1337
		_Noise_Frequency("Frequency", Float) = 0.02
		_Noise_NoiseType("Noise Type", Int) = 1
		_Noise_RotationType3D("Rotation Type 3D", Int) = 2
		
		_Noise_FractalType("Fractal Type", Int) = 1
        _Noise_FractalOctaves("Fractal Octaves", Int) = 5
        _Noise_FractalLacunarity("Fractal Lacunarity", Float) = 2
        _Noise_FractalGain("Fractal Gain", Float) = 0.5
        _Noise_FractalWeightedStrength("Fractal Weighted Strength", Float) = 0
        _Noise_FractalPingPongStrength("Fractal PingPong Strength", Float) = 2

        _Noise_CellularDistanceFunction("Cellular Distance Function", Int) = 1
        _Noise_CellularReturnType("Cellular Return Type", Int) = 1
        _Noise_CellularJitter("Cellular Jitter", Float) = 1

		[Header(Warp settings)]
        _Warp_DomainWarpType("Domain Warp Type", Int) = 0
        _Warp_RotationType3D("Rotation Type 3D", Int) = 0
        _Warp_DomainWarpAmplitude("Domain Warp Amplitude", Float) = 30
        _Warp_Frequency("Domain Warp Frequency", Float) = 0.005

        _Warp_FractalType("Rotation Type 3D", Int) = 0
        _Warp_FractalOctaves("Domain Warp Fractal Octaves", Int) = 5
        _Warp_FractalLacunarity("Domain Warp Fractal Lacunarity", Float) = 2
        _Warp_FractalGain("Domain Warp Fractal Gain", Float) = 0.5
    }
    SubShader
    {
        Pass
        {
			Tags
			{
				"RenderType"= "Opaque"
				"Queue"="Geometry+2"
				"LightMode" = "ForwardAdd"
				"PassFlags" = "OnlyDirectional"
			}

			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight

			#include "/Assets/Graphics/CGincFiles/NormalShading.cginc"
			#include "/Assets/Graphics/FastNoiseLite.cginc"

			sampler2D _CurveTexture;
			float4 _CurveTexture_ST;

			sampler2D _Colors;
			float4 _Colors_ST;

			int _Noise_Seed;
			float _Noise_Frequency;
			int _Noise_NoiseType;
			int _Noise_RotationType3D;
		
			int _Noise_FractalType;
			int _Noise_FractalOctaves;
			float _Noise_FractalLacunarity;
			float _Noise_FractalGain;
			float _Noise_FractalWeightedStrength;
			float _Noise_FractalPingPongStrength;

			int _Noise_CellularDistanceFunction;
			int _Noise_CellularReturnType;
			float _Noise_CellularJitter;

			int _Warp_DomainWarpType;
			int _Warp_RotationType3D;
			float _Warp_DomainWarpAmplitude;
			float _Warp_Frequency;

			int _Warp_FractalType;
			int _Warp_FractalOctaves;
			float _Warp_FractalLacunarity;
			float _Warp_FractalGain;

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

				float x = i.worldPos.x;
				float y = i.worldPos.y;
				float z = i.worldPos.z;
				fnlDomainWarp3D(warp, x, y, z);
				fixed noise_value = saturate(fnlGetNoise3D(noise, x, y, z) + 1);

				fixed light = CalculateLight(i);

				fixed curve_value = tex2D(_CurveTexture, noise_value).r;
				fixed4 color = tex2D(_Colors, curve_value);

				return fixed4(color.rgb, 1);
			}
            ENDCG
        }
		UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
    }
}
