Shader "Custom/FireBallShader"
{
    Properties
    {
		_Color1("Color 1", Color) = (1,0.8431373, 0.2117647, 1)
		_Color2("Color 2", Color) = (0.9411765, 0.3921569, 0.2431373, 1)
		_Color3("Color 3", Color) = (0.764706, 0.1490196, 0.1058824, 1)
		_Threshold1 ("Threshold1", Range(0,1)) = 0.4
		_Threshold2 ("Threshold2", Range(0,1)) = 0.65
    }
    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
			#include "/Assets/Resources/Graphics/FastNoiseLite.cginc"

			float4 _Color1;
			float4 _Color2;
			float4 _Color3;
			float _Threshold1;
			float _Threshold2;

			float4 _FireBallCentre;
			float4 _FireDirection;

            struct output
            {
                float3 worldPos : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

			float remap01(float v) {
				return saturate(0.5+v);
			}

            output vert(appdata_base v)
            {
                output o;
				o.worldPos = mul (unity_ObjectToWorld, v.vertex) - _FireBallCentre.xyz;
				o.vertex = UnityObjectToClipPos(v.vertex);

				return o;
            }

            fixed4 frag (output o) : SV_Target
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

				float3 noise_pos = o.worldPos;
				noise_pos += _FireDirection.xyz * _Time[0];

				fnlDomainWarp3D(noise, noise_pos.x, noise_pos.y, noise_pos.z);
				float noiseLight = remap01(fnlGetNoise3D(noise, noise_pos.x, noise_pos.y, noise_pos.z));
				
				// blend
				//float4 col = lerp(_ColorUndertone, _ColorFire, noiseLight);

				// cartoon stripes
				float4 col;
				if (noiseLight < _Threshold1)
				{
					col = _Color1;
				}
				else if (noiseLight < _Threshold2)
				{
					col = _Color2;
				}
				else
				{
					col = _Color3;
				}

				return col;
            }
            ENDCG
        }
    }
}
