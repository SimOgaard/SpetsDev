Shader "Custom/Leaf Shader"
{
    Properties
    {
		_MainTex ("Texture", 2D) = "white" {}
		
        _Size ("Size", Float) = 1.0
        _ExtrudeDistance ("Extrude Distance", Float) = 0.0
		_TessellationUniform ("Tessellation Uniform", Range(1, 32)) = 1

		_DisplacementRandomUniformed ("Random Uniformed Displacement", Vector) = (0, 0, 0, 0)
		_DisplacementRandom ("Random Displacement", Vector) = (0, 0, 0, 0)

		_DiscardValue ("Discard Value", Range(0, 1)) = 0.5

		_ColorLight ("ColorLight", Color) = (0.7607843, 0.8666667, 0.5960785, 1)
        _ColorMedium ("ColorMedium", Color) = (0.4666667, 0.654902, 0.4196078, 1)
        _ColorDark ("ColorDark", Color) = (0.1215686, 0.3411765, 0.3058824, 1)
		_FirstThreshold ("FirstThreshold", Range(0,1)) = 0.3
        _SecondThreshold ("SecondThreshold", Range(0,1)) = 0.5

		_WindDistortionMap ("Texture", 2D) = "white" {}
		_WindFrequency("Wind Frequency", Vector) = (0.05, 0.05, 0, 0)
		_WindStrength("Wind Strength", Float) = 1
    }

	CGINCLUDE
	#include "UnityCG.cginc"

	#include "Lighting.cginc"
	#pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
	#include "AutoLight.cginc"

	#include "/Assets/Graphics/CustomTessellation.cginc"

	sampler2D _WindDistortionMap;
	float4 _WindDistortionMap_ST;
	float2 _WindFrequency;
	float _WindStrength;

	float3 _DisplacementRandomUniformed;
	float3 _DisplacementRandom;

	float _Size;
	float _ExtrudeDistance;

	struct g2f {
		float4 pos : SV_POSITION;
		float2 uv : TEXCOORD0;
		SHADOW_COORDS(1)
		fixed3 diff : COLOR0;
		fixed3 ambient : COLOR1;
		float3 worldPos : TEXCOORD2;
		float2 wind : TEXCOORD3;
	};

	g2f VertexOutput(float3 pos, float2 uv, float3 norm, float2 wind)
	{
		g2f o;
		o.worldPos = pos;
		o.pos = UnityObjectToClipPos(pos);
		o.uv = uv;
		half3 worldNormal = UnityObjectToWorldNormal(norm);
		half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
		o.diff = nl * _LightColor0.rgb;
		o.ambient = ShadeSH9(half4(worldNormal,1));
		o.wind = wind;
		TRANSFER_SHADOW(o)
		return o;
	}

	float rand(float3 co)
	{
		return sin(dot(co.xyz, float3(12.9898, 78.233, 53.539)));
	}

	[maxvertexcount(4)]
	void geo(point vertexOutput IN[1], inout TriangleStream<g2f> outStream)
	{
		float3 vNormal = IN[0].normal;
		float3 center = IN[0].vertex + vNormal * _ExtrudeDistance;
		
		center += _DisplacementRandomUniformed * float3(rand(center), rand(center.yxz), rand(center.yzx));

		float3 up = float3(0, 1, 0);
		float3 look = mul(mul((float3x3)unity_CameraToWorld, float3(0,0,-1)), unity_ObjectToWorld);
		look = normalize(look);

		float3 right = normalize(cross(up, look));
		up = normalize(cross(look, right));
                
		float3 r = right * _Size * 0.5;
		float3 u = up * _Size * 0.5;

		float3 forward = float3(0, 0, 1);
		
		float4 v[4];
		v[0] = float4(center + r - u + _DisplacementRandom * float3(rand(center.xzy), rand(center.yxz), rand(center.yzx)), 1.0f);
		v[1] = float4(center + r + u + _DisplacementRandom * float3(rand(center.yyz), rand(center.xzz), rand(center.xyz)), 1.0f);
		v[2] = float4(center - r - u + _DisplacementRandom * float3(rand(center.zzy), rand(center.zzx), rand(center.xzy)), 1.0f);
		v[3] = float4(center - r + u + _DisplacementRandom * float3(rand(center.xyz), rand(center.xxz), rand(center.yyx)), 1.0f);

		float2 center_world = (mul(unity_ObjectToWorld, float4(0.0,0.0,0.0,1.0)) + center).xz;
		float2 uv = center_world * _WindDistortionMap_ST.xy + _WindDistortionMap_ST.zw + _WindFrequency * _Time.y;
		float2 windSample = (tex2Dlod(_WindDistortionMap, float4(uv, 0, 0)).xy * 2 - 1) * _WindStrength;

		outStream.Append(VertexOutput(v[0], float2(1, 0), vNormal, windSample));
		outStream.Append(VertexOutput(v[1], float2(1, 1), vNormal, windSample));
		outStream.Append(VertexOutput(v[2], float2(0, 0), vNormal, windSample));
		outStream.Append(VertexOutput(v[3], float2(0, 1), vNormal, windSample));
	}

	ENDCG

    SubShader
    {
        Pass
        {
			Tags
			{
				"Queue" = "Transparent"
				"IgnoreProjector"="True"
				"RenderType" = "Transparent"
			}

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma geometry geo
			#pragma target 4.6
			#pragma multi_compile_fwdbase
			#pragma hull hull
			#pragma domain domain

			#include "Lighting.cginc"
			
			fixed4 _ColorLight;
			fixed4 _ColorMedium;
			fixed4 _ColorDark;

			float _FirstThreshold;
			float _SecondThreshold;

			float _DiscardValue;

			sampler2D _MainTex;
			float4 _MainTex_ST;

            fixed4 frag (g2f i, fixed facing : VFACE) : SV_Target
            {
				float alpha_value = tex2D(_MainTex, i.uv).r;
				if (alpha_value < _DiscardValue)
				{
					discard;
				}

				//return fixed4(i.wind.x, i.wind.y, 0, 1);
				//return tex2D(_MainTex, i.uv * float2(0.125, 0.125) + floor((i.wind + 0.5) * 8) * 0.125);

				/*
				float mask_value = tex2D(_MainTex, i.uv * float2(0.25, 1)).r;
				if (mask_value != 0)
				{
					discard;
				}
                */
				fixed shadow = SHADOW_ATTENUATION(i);
                fixed3 lighting = i.diff * shadow + i.ambient;

				float4 col = _ColorLight;
				//return _ColorLight * lighting.x;
				
				if (lighting.x < _FirstThreshold)
				{
					col = _ColorDark;
				}
				else if (lighting.x < _SecondThreshold)
				{
					col = _ColorMedium;
				}

                return col;
				
            }
            ENDCG
        }

		Pass {
			Name "ShadowCaster"
			Tags { "LightMode" = "ShadowCaster" }

			ZWrite On ZTest LEqual

			CGPROGRAM
			#pragma target 2.0

			#pragma multi_compile_shadowcaster

			#pragma vertex vertShadowCaster
			#pragma fragment fragShadowCaster

			#include "UnityStandardShadow.cginc"

			ENDCG
		}
    }
}
