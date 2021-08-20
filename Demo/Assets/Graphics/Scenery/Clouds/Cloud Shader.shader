Shader "Custom/Cloud Shader"
{
    Properties
    {
		_MainTex ("Texture", 2D) = "white" {}

		_CloudColorThin("Cloud Color Thin", Color) = (1,1,1,1)
		_ThresholdThin("Threshold Thin", Float) = 0.4
		_CloudColorDense("Cloud Color Dense", Color) = (1,1,1,1)
		_ThresholdDense("Threshold Dense", Float) = 0.6

		_NoiseScroll("Noise Scroll XYZ", Vector) = (0.03, 0.03, 0.03, 0)

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
		Tags
		{
			"Queue" = "Transparent"
		}

        Pass
        {
			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
			#include "/Assets/Graphics/FastNoiseLite.cginc"

            struct appdata
            {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
            };

            struct v2f
            {
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
            };

			sampler2D _MainTex;
			float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;

				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                return o;
            }

			float4 _CloudColorThin;
			float _ThresholdThin;
			float4 _CloudColorDense;
			float _ThresholdDense;
			float3 _NoiseScroll;

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

			float remap01(float v) {
				return saturate((v + 1) * 0.5);
			}

			float4 alphaBlend(float4 top, float4 bottom)
			{
				float3 color = (top.rgb * top.a) + (bottom.rgb * (1 - top.a));
				float alpha = top.a + bottom.a * (1 - top.a);

				return float4(color, alpha);
			}

            float4 frag (v2f i) : SV_Target
            {
				fnl_state noise = fnlCreateState();
				noise.seed = 1337; //_Noise_Seed;
				noise.frequency = 25; //_Noise_Frequency;
				noise.noise_type = 1; //_Noise_NoiseType;
				noise.rotation_type_3d = 2; //_Noise_RotationType3D;

				noise.fractal_type = 1; //_Noise_FractalType;
				noise.octaves = 5; //_Noise_FractalOctaves;
				noise.lacunarity = 2.0; //_Noise_FractalLacunarity;
				noise.gain = 0.5; //_Noise_FractalGain;
				noise.weighted_strength = 0.0; //_Noise_FractalWeightedStrength;
				noise.ping_pong_strength = 2.0; //_Noise_FractalPingPongStrength;

				noise.cellular_distance_func = 1; //_Noise_CellularDistanceFunction;
				noise.cellular_return_type = 1; //_Noise_CellularReturnType;
				noise.cellular_jitter_mod = 1.0; //_Noise_CellularJitter;

				fnl_state warp = fnlCreateState();
				warp.domain_warp_type = 0; //_Warp_DomainWarpType;
				warp.rotation_type_3d = 0; //_Warp_RotationType3D;
				warp.domain_warp_amp = 30.0; //_Warp_DomainWarpAmplitude;
				warp.frequency = 0.005; //_Warp_Frequency;

				warp.fractal_type = 0; //_Warp_FractalType;
				warp.octaves = 5; //_Warp_FractalOctaves;
				warp.lacunarity = 2.0; //_Warp_FractalLacunarity;
				warp.gain = 2.0; //_Warp_FractalGain;

				float x = i.uv.x + _Time[0] * _NoiseScroll.x;
				float y = _Time[0] * _NoiseScroll.y;
				float z = i.uv.y + _Time[0] * _NoiseScroll.z;
				fnlDomainWarp3D(warp, x, y, z);
				float noise_value = remap01(fnlGetNoise3D(noise, x, y, z));
				
				if (noise_value < _ThresholdThin)
				{
					discard;				
				}
				else if (noise_value < _ThresholdDense)
				{
					return _CloudColorThin;
				}
				return _CloudColorDense;
			}
            ENDCG
        }
    }
}
