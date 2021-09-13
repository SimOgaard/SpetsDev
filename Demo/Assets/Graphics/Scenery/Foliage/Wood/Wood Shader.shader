﻿Shader "Custom/Wood Shader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

        _Colors ("Color Texture", 2D) = "white" {}
		_CurveTexture ("Curve Texture", 2D) = "white" {}
    }

    SubShader
    {
        Pass
        {
			Tags {
				"RenderType"= "Opaque"
				"LightMode" = "ForwardAdd"
				"PassFlags" = "OnlyDirectional"
			}
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
                float3 worldPos : TEXCOORD0;
				SHADOW_COORDS(1)
				fixed3 diff : COLOR0;
                fixed3 ambient : COLOR1;
                float4 pos : SV_POSITION;
				float2 uv : TEXCOORD2; 
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

			sampler2D _Colors;
			float4 _Colors_ST;

			sampler2D _CurveTexture;
			float4 _CurveTexture_ST;

            v2f vert (appdata_base v)
            {
                v2f o;
				o.uv = v.texcoord;
				o.worldPos = mul (unity_ObjectToWorld, v.vertex);
                o.pos = UnityObjectToClipPos(v.vertex);
				half3 worldNormal = UnityObjectToWorldNormal(v.normal);
                half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
                o.diff = nl * _LightColor0.rgb;
                o.ambient = ShadeSH9(half4(worldNormal,1));
                TRANSFER_SHADOW(o)
                return o;
            }

			float4 _Color;
			sampler2D _NormalTex;
			float4 _NormalTex_ST;

			sampler2D _LightTexture0;
			float4x4 unity_WorldToLight;

            fixed4 frag (v2f i) : SV_Target
            {
				fixed shadow = SHADOW_ATTENUATION(i);
				float2 uvCookie = mul(unity_WorldToLight, float4(i.worldPos, 1)).xy;
				float attenuation = tex2D(_LightTexture0, uvCookie).w;
                fixed3 lighting = i.diff * shadow * attenuation + i.ambient;

				float curve_value = tex2D(_CurveTexture, saturate(lighting.x)).r;
				fixed4 color = tex2D(_Colors, curve_value);

				return color;
				/*
				fnl_state noise = fnlCreateState();
				noise.seed = 1337; //_Noise_Seed;
				noise.frequency = 0.1; //_Noise_Frequency;
				noise.noise_type = 1; //_Noise_NoiseType;
				noise.rotation_type_3d = 2; //_Noise_RotationType3D;

				noise.fractal_type = 1; //_Noise_FractalType;
				noise.octaves = 3; //_Noise_FractalOctaves;
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

				float x = i.worldPos.x;
				float y = i.worldPos.y;
				float z = i.worldPos.z;
				fnlDomainWarp3D(warp, x, y, z);
				float noise_value = remap01(fnlGetNoise3D(noise, x, y, z));

				float4 col = noise_value < _MossThreshold ? _MossColor : material_color;
                return col * light_color;
				*/
            }
            ENDCG
        }
		UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
    }
}
