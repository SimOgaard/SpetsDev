Shader "Custom/Dirt Shader"
{
    Properties
    {
		_CrackColor("Crack Color", Color) = (1,1,1,1)
		_StoneColor("Stone Color", Color) = (1,1,1,1)
		_Threshold("Threshold", Float) = 0.1

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
		Tags {
			"RenderType"= "Opaque"
			"LightMode" = "ForwardAdd"
			"PassFlags" = "OnlyDirectional"
		}

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
			#include "Lighting.cginc"
			#pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
            #include "AutoLight.cginc"

			#include "/Assets/Graphics/FastNoiseLite.cginc"

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
				SHADOW_COORDS(1)
				fixed3 diff : COLOR0;
                fixed3 ambient : COLOR1;
            };

            v2f vert (appdata_base v)
            {
                v2f o;

				o.worldPos = mul (unity_ObjectToWorld, v.vertex);
				o.pos = UnityObjectToClipPos(v.vertex);
                half3 worldNormal = UnityObjectToWorldNormal(v.normal);
                half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
                o.diff = nl * _LightColor0.rgb;
                o.ambient = ShadeSH9(half4(worldNormal,1));
                TRANSFER_SHADOW(o)
                
                return o;
            }

			float4 _CrackColor;
			float4 _StoneColor;
			float _Threshold;

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

			sampler2D _LightTexture0;
			float4x4 unity_WorldToLight;

			float remap01(float v) {
				return saturate((v + 1) * 0.5);
			}

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

				noise.cellular_distance_func = 0; //_Noise_CellularDistanceFunction;
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
				float noise_value = remap01(fnlGetNoise3D(noise, x, y, z));
				
				fixed shadow = SHADOW_ATTENUATION(i);
				float2 uvCookie = mul(unity_WorldToLight, float4(i.worldPos, 1)).xy;
				float attenuation = tex2D(_LightTexture0, uvCookie).w;
                fixed3 lighting = i.diff * shadow * attenuation + i.ambient;

				//noise_value = noise_value < _Threshold ? 0 : 1;
				//return noise_value;

				if (noise_value < _Threshold)
				{
					return _CrackColor;
				}
				return _StoneColor;
			}
            ENDCG
        }
		UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
    }
}
