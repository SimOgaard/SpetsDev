Shader "Custom/Colossal Plains Grass Shader"
{
    Properties
    {
		_BendRotationRandom("Bend Rotation Random", Range(0, 1)) = 0.2
		_BladeWidth("Blade Width", Float) = 0.05
		_BladeWidthRandom("Blade Width Random", Float) = 0.02
		_BladeHeight("Blade Height", Float) = 0.5
		_BladeHeightRandom("Blade Height Random", Float) = 0.3
		_TessellationUniform("Tessellation Uniform", Range(1, 64)) = 1

		_ColorLight ("ColorLight", Color) = (0.7607843, 0.8666667, 0.5960785, 1)
        _ColorMedium ("ColorMedium", Color) = (0.4666667, 0.654902, 0.4196078, 1)
        _ColorDark ("ColorDark", Color) = (0.1215686, 0.3411765, 0.3058824, 1)
        _ColorReallyDark ("ColorReallyDark", Color) = (0.1215686, 0.3411765, 0.3058824, 1)

		_FirstThreshold ("FirstThreshold", Range(0,1)) = 0.3
        _SecondThreshold ("SecondThreshold", Range(0,1)) = 0.5
        _ThirdThreshold ("ThirdThreshold", Range(0,1)) = 0.5
    }

	CGINCLUDE
	#include "UnityCG.cginc"

	#include "Lighting.cginc"
	#pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
	#include "AutoLight.cginc"

	#include "/Assets/Graphics/CustomTessellation.cginc"

	float _BendRotationRandom;
	float _BladeHeight;
	float _BladeHeightRandom;	
	float _BladeWidth;
	float _BladeWidthRandom;

	fixed4 _ColorLight;
	fixed4 _ColorMedium;
	fixed4 _ColorDark;
	fixed4 _ColorReallyDark;

	float _FirstThreshold;
	float _SecondThreshold;
	float _ThirdThreshold;

	struct geometryOutput
	{
		float4 pos : SV_POSITION;
		float2 uv : TEXCOORD0;
		SHADOW_COORDS(1)
		fixed3 diff : COLOR0;
		fixed3 ambient : COLOR1;
		float rand : TEXCOORD2;
	};

	geometryOutput VertexOutput(float3 pos, float2 uv, float3 norm, float rand)
	{
		geometryOutput o;
		o.pos = UnityObjectToClipPos(pos);
		o.uv = uv;
		half3 worldNormal = UnityObjectToWorldNormal(norm);
        half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
        o.diff = nl * _LightColor0.rgb;
        o.ambient = ShadeSH9(half4(worldNormal,1));
		o.rand = rand;
        // compute shadows data
        TRANSFER_SHADOW(o)
		return o;
	}

	float rand(float3 co)
	{
		return frac(sin(dot(co.xyz, float3(12.9898, 78.233, 53.539))) * 43758.5453);
	}

	float3x3 AngleAxis3x3(float angle, float3 axis)
	{
		float c, s;
		sincos(angle, s, c);

		float t = 1 - c;
		float x = axis.x;
		float y = axis.y;
		float z = axis.z;

		return float3x3(
			t * x * x + c, t * x * y - s * z, t * x * z + s * y,
			t * x * y + s * z, t * y * y + c, t * y * z - s * x,
			t * x * z - s * y, t * y * z + s * x, t * z * z + c
		);
	}

	[maxvertexcount(3)]
	void geo(triangle vertexOutput IN[3], inout TriangleStream<geometryOutput> triStream)
	{
		float3 pos = IN[0].vertex;

		float3 vNormal = IN[0].normal;
		float4 vTangent = IN[0].tangent;
		float3 vBinormal = cross(vNormal, vTangent) * vTangent.w;

		geometryOutput o;

		float3x3 tangentToLocal = float3x3(
			vTangent.x, vBinormal.x, vNormal.x,
			vTangent.y, vBinormal.y, vNormal.y,
			vTangent.z, vBinormal.z, vNormal.z
		);

		float randPosValue = rand(pos);

		float3x3 facingRotationMatrix = AngleAxis3x3(randPosValue * UNITY_TWO_PI, float3(0, 0, 1));
		float3x3 bendRotationMatrix = AngleAxis3x3(rand(pos.zzx) * _BendRotationRandom * UNITY_PI * 0.5, float3(-1, 0, 0));
		float3x3 transformationMatrix = mul(mul(tangentToLocal, facingRotationMatrix), bendRotationMatrix);

		float height = (rand(pos.zyx) * 2 - 1) * _BladeHeightRandom + _BladeHeight;
		float width = (rand(pos.xzy) * 2 - 1) * _BladeWidthRandom + _BladeWidth;

		triStream.Append(VertexOutput(pos + mul(transformationMatrix, float3(width, 0, 0)), float2(0, 0), vNormal, randPosValue));
		triStream.Append(VertexOutput(pos + mul(transformationMatrix, float3(-width, 0, 0)), float2(1, 0), vNormal, randPosValue));
		triStream.Append(VertexOutput(pos + mul(transformationMatrix, float3(0, 0, height)), float2(0.5, 1), vNormal, randPosValue));
	}

	ENDCG

    SubShader
    {
		Cull Off

        Pass
        {
			Tags
			{
				"RenderType" = "Opaque"
				"LightMode" = "ForwardBase"
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

			float4 _TopColor;
			float4 _BottomColor;
			float _TranslucentGain;

			float4 frag (geometryOutput i, fixed facing : VFACE) : SV_Target
            {	
				fixed shadow = SHADOW_ATTENUATION(i);
                fixed3 lighting = i.diff * shadow + i.ambient;

				float4 col = _ColorLight;
				if (lighting.x < _FirstThreshold)
				{
					col = _ColorReallyDark;
				}
				else if (lighting.x < _SecondThreshold)
				{
					col = _ColorDark;
				}
				else if (lighting.x < _ThirdThreshold)
				{
					col = _ColorMedium;				
				}

                return col;
            }
            ENDCG
        }
    }
}