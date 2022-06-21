Shader "Custom/Cloud Shader"
{
    Properties
    {
		_MainTex ("Texture", 2D) = "white" {}

		_HorizonAngleThreshold ("Horizon Angle Threshold", Range(0, 90)) = 10
		_HorizonAngleFade ("Horizon Angle Fade", Range(0, 90)) = 10

		seed("seed", int) = 1337
		noise_type("noise_type", int) = 1
		weighted_strength("weighted_strength", float) = 0.0
		ping_pong_strength("ping_pong_strength", float) = 2.0
		cellular_distance_func("cellular_distance_func", int) = 1
		cellular_return_type("cellular_return_type", int) = 1
		cellular_jitter_mod("cellular_jitter_mod", float) = 1.0
		domain_warp_type("domain_warp_type", int) = 0
		domain_warp_amp("domain_warp_amp", float) = 30.0

		amplitude("amplitude", float) = 1.0
		min_value("min_value", float) = -1.0
		smoothing_min("smoothing_min", float) = 0.0
		max_value("max_value", float) = 1.0
		smoothing_max("smoothing_max", float) = 0.0

		invert("invert", int) = 0

		// warp specific
		frequency_warp("frequency_warp", float) = 0.005
		rotation_type_3d_warp("rotation_type_3d_warp", int) = 2
		fractal_type_warp("fractal_type_warp", int) = 0
		octaves_warp("octaves_warp", int) = 5
		lacunarity_warp("lacunarity_warp", float) = 2.0
		gain_warp("gain_warp", float) = 2.0

		// noise specific
		frequency_warp("frequency", float) = 6.0
		rotation_type_3d_warp("rotation_type_3d", int) = 2
		fractal_type_warp("fractal_type", int) = 1
		octaves_warp("octaves", int) = 5
		lacunarity_warp("lacunarity", float) = 2.0
		gain_warp("gain", float) = 0.5
		
		_ColorShading ("Texture", 2D) = "white" {}
    }

	CGINCLUDE
	#include "UnityCG.cginc"
	#include "/Assets/Graphics/CGincFiles/Noise/FastNoiseLite.cginc"
	 
	sampler2D _MainTex;

	float3 _WindScroll;

	float _HorizonAngleThreshold;
	float _HorizonAngleFade;

	float _AngleToHorizon;

	sampler2D _ColorShading;
	float4 _ColorShading_ST;

	float3 _LightPosition;
	float _CookieSize;
	float _Zoom;
	
	float3 _WorldOffset;
	float3 _CloudStrechOffset;

	/*
	CBUFFER_START(UnityPerMaterial)
	// non specific (the same for both noise)
	int seed;
	fnl_noise_type noise_type;
    float weighted_strength;
    float ping_pong_strength;
    fnl_cellular_return_type cellular_distance_func;
    fnl_cellular_return_type cellular_return_type;
    float cellular_jitter_mod;
    fnl_domain_warp_type domain_warp_type;
    float domain_warp_amp;

    float amplitude;
    float min_value;
    float smoothing_min;
    float max_value;
    float smoothing_max;

    int invert;

	// warp specific
	float frequency_warp;
	fnl_rotation_type_3d rotation_type_3d_warp;
	fnl_fractal_type fractal_type_warp;
	int octaves_warp;
	float lacunarity_warp;
	float gain_warp;

	// noise specific
	float frequency;
	fnl_rotation_type_3d rotation_type_3d;
	fnl_fractal_type fractal_type;
	int octaves;
	float lacunarity;
	float gain;
    CBUFFER_END

	StructuredBuffer<fnl_state> fnl_warp_state;
	StructuredBuffer<fnl_state> fnl_noise_state;
	*/

	struct appdata_t
	{
		float4 vertex : POSITION;
		float2 texcoord : TEXCOORD0;
	};

	struct VertexOut
	{
		float4 vertex : SV_POSITION;
		float2 texcoord : TEXCOORD0;
	};

	VertexOut Vert (appdata_t v)
	{
		VertexOut o;
		o.vertex = UnityObjectToClipPos(v.vertex);
		o.texcoord = v.texcoord.xy;
		return o;
	}

	fnl_state GetNonSpecificFNLState()
	{
		fnl_state state = fnlCreateState(1337/*seed*/);
		state.noise_type = 1;//noise_type;
		state.weighted_strength = 0.0;//weighted_strength;
		state.ping_pong_strength = 2.0;//ping_pong_strength;
		state.cellular_distance_func = 1;//cellular_distance_func;
		state.cellular_return_type = 1;//cellular_return_type;
		state.cellular_jitter_mod = 1.0;//cellular_jitter_mod;
		state.domain_warp_type = 0;//domain_warp_type;
		state.domain_warp_amp = 0;//domain_warp_amp;

		state.amplitude = 1.0;//amplitude;
		state.min_value = -1;//min_value;
		state.smoothing_min = 0;//smoothing_min;
		state.max_value = 1.0;//max_value;
		state.smoothing_max = 0.0;//smoothing_max;

		state.invert = 0;//invert;

		return state;
	}

	fnl_state GetWarpState()
	{
		fnl_state warp = GetNonSpecificFNLState();

		// warp specific
        warp.frequency = 0.005;//frequency_warp;
        warp.rotation_type_3d = 0;//rotation_type_3d_warp;
        warp.fractal_type = 0;//fractal_type_warp;
        warp.octaves = 5;//octaves_warp;
        warp.lacunarity = 2.0;//lacunarity_warp;
        warp.gain = 2.0;//gain_warp;

		return warp;
	}

	fnl_state GetNoiseState()
	{
		fnl_state noise = GetNonSpecificFNLState();

		// noise specific
        noise.frequency = 6.0;//frequency;
        noise.rotation_type_3d = 2;//rotation_type_3d;
        noise.fractal_type = 1;//fractal_type;
        noise.octaves = 5;//octaves;
        noise.lacunarity = 2.0;//lacunarity;
        noise.gain = 0.5;//gain;

		return noise;
	}

	float GetNoiseValue(VertexOut i)
	{
		// translate tex pixel coordinates to world local
		float length = 1. / (2./* * _Zoom*/);
		return length;
		float2 worldTexCoord = (((i.texcoord / (_CloudStrechOffset.xz)) * length) + ((1. - length) / 2.)) * _CookieSize;
		//float2 worldTexCoord = ((i.texcoord / (2. * _Zoom)) + (1. / (4. * _Zoom * _Zoom))) * (_CookieSize);

		// get xyz values
		float x = worldTexCoord.x + _WindScroll.x + ((_LightPosition.x * 0.5) - _WorldOffset.x) / _CloudStrechOffset.x;
		float y = _WindScroll.y;
		float z = worldTexCoord.y + _WindScroll.z + ((_LightPosition.y * 0.5) - _WorldOffset.z) / _CloudStrechOffset.z;

		// create warp and noise state
		fnl_state fnl_warp_state = GetWarpState();
		fnl_state fnl_noise_state = GetNoiseState();

		// warp xyz values
		fnlDomainWarp3D(fnl_warp_state, x, y, z);

		// get noise
		float noise_value = SampleNoise3D(fnl_noise_state, x, y, z);

		// remap to 0-amplitude and return it
		return remap01(noise_value, fnl_noise_state);
	}

	int resolution;

	float4 Frag (VertexOut i) : SV_Target
	{
		return 1;
		return (
			(
			round(i.texcoord.x * resolution) % 2 == 0 &&
			round(i.texcoord.y * resolution) % 2 == 0
			)
			||
			(
			(round(i.texcoord.x * resolution) + 1) % 2 == 0 &&
			(round(i.texcoord.y * resolution) + 1) % 2 == 0
			)
		);

		float alpha = GetNoiseValue(i);
		return alpha;

		float angle = _AngleToHorizon - _HorizonAngleThreshold;
		float angle_opacity = smoothstep(0, 1, angle / _HorizonAngleFade);
		
		float curve_value = tex2D(_ColorShading, alpha).r  * angle_opacity;

		float4 color = curve_value;

		return alpha;
	}

	ENDCG

	SubShader
    {
		Pass
		{
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
			#pragma vertex Vert
			#pragma fragment Frag
			ENDCG

		}

		/*
		Pass
        {
			CGPROGRAM
            #include "UnityCustomRenderTexture.cginc"
			#include "/Assets/Graphics/CGincFiles/Noise/FastNoiseLite.cginc"
            #pragma vertex CustomRenderTextureVertexShader
            #pragma fragment frag
            #pragma target 3.0

            uniform float _AngleToHorizon;
			sampler2D _MainTex;

            float4 frag(v2f_customrendertexture IN) : COLOR
            {
				return float4(1,1,1,1);
				
                return tex2D(_MainTex, IN.localTexcoord.xy);
            }
			
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
				return float4(1,1,1,1);
				discard;

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
        }
		*/
    }
	Fallback Off
}
