Shader "Custom/Leaf Shader"
{
    Properties
    {
		_MainTex ("Texture", 2D) = "white" {}
		
		_TileAmount("Amount of Tiles", Int) = 8
		_TilePixelSize("Tile Pixel Size", Int) = 16

        _ExtrudeDistance ("Extrude Distance", Float) = 0.0
		_TessellationUniform ("Tessellation Uniform", Range(1, 32)) = 1

		_UniformDisplacementRandom ("Random Uniformed Displacement", Vector) = (0, 0, 0, 0)

		_DiscardValue ("Discard Value", Range(0, 1)) = 0.5

		_Colors ("Texture", 2D) = "white" {}
		_CurveTexture ("Texture", 2D) = "white" {}

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

	float3 _UniformDisplacementRandom;

	float _ExtrudeDistance;

	sampler2D _CurveTexture;
	float4 _CurveTexture_ST;
	
	#define y_scale 1 / cos(3.14159265 / 6);

	float _TileAmount;
	float _TilePixelSize;

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
		float3 center = IN[0].vertex;

		float3 up = float3(0, 1, 0);
		float3 look = mul(mul((float3x3)unity_CameraToWorld, float3(0,0,-1)), unity_ObjectToWorld);
		//look.y = 0;
		look = normalize(look);

		float3 right = normalize(cross(up, look));
		up = normalize(cross(look, right)) * y_scale;

		float PixelSize = _TilePixelSize / (10.8); // 5.4 * 2
		float3 r = right * PixelSize;
		float3 u = up * PixelSize;

		float3 uniform_displacement = float3(rand(center), rand(center.yzx), rand(center.xzy)) * _UniformDisplacementRandom;
		center += uniform_displacement + vNormal * _ExtrudeDistance;
		float4 v[4];
		v[0] = float4(center + r - u, 1.0f);
		v[1] = float4(center + r + u, 1.0f);
		v[2] = float4(center - r - u, 1.0f);
		v[3] = float4(center - r + u, 1.0f);

		float3 center_world = mul(unity_ObjectToWorld, center).xyz;
				
		float2 uv = center_world.xz * _WindDistortionMap_ST.xy + _WindDistortionMap_ST.zw + _WindFrequency * _Time.y;
		float2 windSample = tex2Dlod(_WindDistortionMap, float4(uv, 0, 0)).xy; // get wind value ranging (0.33 - 0.66)
		float2 windSample01 = saturate(windSample * 3 - 1); // remap to 0-1
		float2 windSamplenegpos = windSample01 * 2 - 1; // remap to -1 - 1
		float2 windSampleStrength = windSamplenegpos * _WindStrength; // multiply by windstrength
		float2 remap_01 = saturate(windSampleStrength * 0.5 + 0.5); // saturate between 01 to keep low/all/high values dependent on windstrength
		float2 windSampleGrid;
				
		windSampleGrid = floor(remap_01 * (_TileAmount - 0.0001)) * (1 / _TileAmount);

		outStream.Append(VertexOutput(v[0], float2(1, 0), vNormal, windSampleGrid));
		outStream.Append(VertexOutput(v[1], float2(1, 1), vNormal, windSampleGrid));
		outStream.Append(VertexOutput(v[2], float2(0, 0), vNormal, windSampleGrid));
		outStream.Append(VertexOutput(v[3], float2(0, 1), vNormal, windSampleGrid));

		/*
		float3 vNormal = IN[0].normal;
		float3 center = IN[0].vertex;
		
		center += _DisplacementRandomUniformed * float3(rand(center), rand(center.yxz), rand(center.yzx));

		float3 up = float3(0, 1, 0);
		float3 look = mul(mul((float3x3)unity_CameraToWorld, float3(0,0,-1)), unity_ObjectToWorld);
		look.y = 0;
		look = normalize(look);

		float3 right = normalize(cross(up, look));
		up = normalize(cross(look, right)) * y_scale;
                
		float PixelSize = _TilePixelSize / (10.8); // 5.4 * 2
		float3 r = right * PixelSize;
		float3 u = up * PixelSize;

		float3 forward = float3(0, 0, 1);
		
		float4 v[4];
		v[0] = float4(center + r - u + _DisplacementRandom * float3(rand(center.xzy), rand(center.yxz), rand(center.yzx)) + vNormal * _ExtrudeDistance, 1.0f);
		v[1] = float4(center + r + u + _DisplacementRandom * float3(rand(center.yyz), rand(center.xzz), rand(center.xyz)) + vNormal * _ExtrudeDistance, 1.0f);
		v[2] = float4(center - r - u + _DisplacementRandom * float3(rand(center.zzy), rand(center.zzx), rand(center.xzy)) + vNormal * _ExtrudeDistance, 1.0f);
		v[3] = float4(center - r + u + _DisplacementRandom * float3(rand(center.xyz), rand(center.xxz), rand(center.yyx)) + vNormal * _ExtrudeDistance, 1.0f);

		float2 center_world = (mul(unity_ObjectToWorld, float4(0.0,0.0,0.0,1.0)) + center).xz;
		float2 uv = center_world * _WindDistortionMap_ST.xy + _WindDistortionMap_ST.zw + _WindFrequency * _Time.y;
		float2 windSample = (tex2Dlod(_WindDistortionMap, float4(uv, 0, 0)).xy * 2 - 1) * _WindStrength;

		outStream.Append(VertexOutput(v[0], float2(1, 0), vNormal, windSample));
		outStream.Append(VertexOutput(v[1], float2(1, 1), vNormal, windSample));
		outStream.Append(VertexOutput(v[2], float2(0, 0), vNormal, windSample));
		outStream.Append(VertexOutput(v[3], float2(0, 1), vNormal, windSample));
		*/
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
				"LightMode" = "ForwardAdd"
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
			
			sampler2D _Colors;
			float4 _Colors_ST;

			sampler2D _LightTexture0;
			float4x4 unity_WorldToLight;

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
				float2 uvCookie = mul(unity_WorldToLight, float4(i.worldPos, 1)).xy;
				float attenuation = tex2D(_LightTexture0, uvCookie).w;
                fixed3 lighting = i.diff * shadow * attenuation + i.ambient;
				float curve_value = tex2D(_CurveTexture, saturate(lighting.x)).r;
				fixed4 color = tex2D(_Colors, curve_value);

				return color;
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
