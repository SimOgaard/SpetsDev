Shader "Custom/Cloud Shader"
{
    Properties
    {
		_MainTex ("Texture", 2D) = "white" {}

		_HorizonAngleThreshold ("Horizon Angle Threshold", Range(0, 90)) = 10
		_HorizonAngleFade ("Horizon Angle Fade", Range(0, 90)) = 10

		_ColorShading ("Texture", 2D) = "white" {}
    }

	CGINCLUDE
	#include "UnityCG.cginc"
	#include "/Assets/Graphics/CGincFiles/FastNoiseLite.cginc"

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

	StructuredBuffer<fnl_state> fnl_warp_state;
	StructuredBuffer<fnl_state> fnl_noise_state;	

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

	float GetNoiseValue(VertexOut i)
	{
		// translate tex pixel coordinates to world local
		float length = 1. / (2. * _Zoom);
		float2 worldTexCoord = (((i.texcoord / (_CloudStrechOffset.xz)) * length) + ((1. - length) / 2.)) * _CookieSize;
		//float2 worldTexCoord = ((i.texcoord / (2. * _Zoom)) + (1. / (4. * _Zoom * _Zoom))) * (_CookieSize);

		// get xyz values
		float x = worldTexCoord.x + _WindScroll.x + ((_LightPosition.x * 0.5) - _WorldOffset.x) / _CloudStrechOffset.x;
		float y = _WindScroll.y;
		float z = worldTexCoord.y + _WindScroll.z + ((_LightPosition.y * 0.5) - _WorldOffset.z) / _CloudStrechOffset.z;

		// warp xyz values
		fnlDomainWarp3D(fnl_warp_state[0], x, y, z);

		// get noise
		float noise_value = SampleNoise3D(fnl_noise_state[0], x, y, z);

		// remap to 0-amplitude and return it
		return remap01(noise_value, fnl_noise_state[0]);
	}

	float4 Frag (VertexOut i) : SV_Target
	{
		float angle = _AngleToHorizon - _HorizonAngleThreshold;
		float angle_opacity = smoothstep(0, 1, angle / _HorizonAngleFade);

		float alpha = GetNoiseValue(i);
		
		float curve_value = tex2D(_ColorShading, alpha).r  * angle_opacity;

		float4 color = curve_value;
		return color;
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
			#include "/Assets/Graphics/CGincFiles/FastNoiseLite.cginc"
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
