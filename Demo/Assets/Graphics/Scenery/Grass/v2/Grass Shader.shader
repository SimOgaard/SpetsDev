Shader "Custom/Grass Shader"
{
    Properties
    {
		_MainTex ("Texture", 2D) = "white" {}

        _Size ("Size", Float) = 1.0
		_YDisplacement ("Y Displacement", Float) = 0.5
		_XYDisplacementRandom ("X Y Displacement Random", Float) = 0.5
		_TessellationUniform("Tessellation Uniform", Range(1, 32)) = 1

		_ColorLight ("ColorLight", Color) = (0.7607843, 0.8666667, 0.5960785, 1)
        _ColorMedium ("ColorMedium", Color) = (0.4666667, 0.654902, 0.4196078, 1)
        _ColorDark ("ColorDark", Color) = (0.1215686, 0.3411765, 0.3058824, 1)

		_FirstThreshold ("FirstThreshold", Range(0,1)) = 0.3
        _SecondThreshold ("SecondThreshold", Range(0,1)) = 0.5
    }

	CGINCLUDE
	#include "UnityCG.cginc"

	#include "Lighting.cginc"
	#pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
	#include "AutoLight.cginc"

	#include "/Assets/Graphics/CustomTessellation.cginc"

    float _Size;
	float _YDisplacement;
	float _XYDisplacementRandom;

	struct geometryOutput
	{
		float4 pos : SV_POSITION;
		float2 uv : TEXCOORD0;
		SHADOW_COORDS(1)
		fixed3 diff : COLOR0;
		fixed3 ambient : COLOR1;
        float3 worldPos : TEXCOORD2;
	};

	geometryOutput VertexOutput(float3 pos, float2 uv, float3 norm)
	{
		geometryOutput o;
		o.worldPos = pos;
		o.pos = UnityObjectToClipPos(pos);
		o.uv = uv;
		half3 worldNormal = UnityObjectToWorldNormal(norm);
        half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
        o.diff = nl * _LightColor0.rgb;
        o.ambient = ShadeSH9(half4(worldNormal,1));
        // compute shadows data
        TRANSFER_SHADOW(o)
		return o;
	}

	float rand(float3 co)
	{
		return sin(dot(co.xyz, float3(12.9898, 78.233, 53.539)));
	}

	/*
	[maxvertexcount(4)]
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

		float3 randPosValue = (rand(pos.yxz), rand(pos.yzx), rand(pos.yyz)) * _BladePositionRandom;

		
		//float3x3 facingRotationMatrix = AngleAxis3x3(rand(pos) * UNITY_TWO_PI, float3(0, 0, 1));
		//float3x3 bendRotationMatrix = AngleAxis3x3(rand(pos.zzx) * _BendRotationRandom * UNITY_PI * 0.5, float3(-1, 0, 0));
		//float3x3 transformationMatrix = mul(mul(tangentToLocal, facingRotationMatrix), bendRotationMatrix);
		

		float height = (rand(pos.zyx) * 2 - 1) * _BladeHeightRandom + _BladeHeight;
		float width = (rand(pos.xzy) * 2 - 1) * _BladeWidthRandom + _BladeWidth;

		
		//triStream.Append(VertexOutput(pos + mul(transformationMatrix, float3(-width, 0, 0)), float2(0, 0), vNormal, randPosValue));
		//triStream.Append(VertexOutput(pos + mul(transformationMatrix, float3(width, 0, 0)), float2(1, 0), vNormal, randPosValue));
		//triStream.Append(VertexOutput(pos + mul(transformationMatrix, float3(-width, 0, height * 2)), float2(0, 1), vNormal, randPosValue));
		//triStream.Append(VertexOutput(pos + mul(transformationMatrix, float3(width, 0, height * 2)), float2(1, 1), vNormal, randPosValue));
		

		triStream.Append(VertexOutput(pos + float3(-width + randPosValue.x, 0, randPosValue.z), float2(0, 0), vNormal));
		triStream.Append(VertexOutput(pos + float3(width + randPosValue.x, 0, randPosValue.z), float2(1, 0), vNormal));
		triStream.Append(VertexOutput(pos + float3(-width + randPosValue.x, height * 2.309402, randPosValue.z), float2(0, 1), vNormal));
		triStream.Append(VertexOutput(pos + float3(width + randPosValue.x, height * 2.309402, randPosValue.z), float2(1, 1), vNormal));
	}
	*/

	[maxvertexcount(4)]
    void geo(point float4 p[1] : POSITION, inout TriangleStream<geometryOutput> triStream)
    {
        float3 center = p[0] + float3(0, _YDisplacement, 0);
		float3 vNormal = float3(0,1,0);

		geometryOutput o;

        float3 up = float3(0, 1, 0);
        float3 look = mul((float3x3)unity_CameraToWorld, float3(0,0,-1));;
        look = normalize(look);

        float3 right = cross(up, look);
        up = cross(look, right);
                
        float3 r = right * _Size * 0.5;
        float3 u = up * _Size * 0.5;

		float3 forward = float3(0, 0, 1);
		float3 right_displacement = right * rand(center) * _XYDisplacementRandom;
		float3 up_displacement = forward * rand(center.xzy) * _XYDisplacementRandom;

        float4 v[4];
        v[0] = float4(center + r - u + right_displacement + up_displacement, 1.0f);
        v[1] = float4(center + r + u + right_displacement + up_displacement, 1.0f);
        v[2] = float4(center - r - u + right_displacement + up_displacement, 1.0f);
        v[3] = float4(center - r + u + right_displacement + up_displacement, 1.0f);

        triStream.Append(VertexOutput(UnityObjectToClipPos(v[0]), float2(0, 0), vNormal));

        triStream.Append(VertexOutput(UnityObjectToClipPos(v[1]), float2(1, 0), vNormal));

        triStream.Append(VertexOutput(UnityObjectToClipPos(v[2]), float2(0, 1), vNormal));

        triStream.Append(VertexOutput(UnityObjectToClipPos(v[3]), float2(1, 1), vNormal));
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

			fixed4 _ColorLight;
			fixed4 _ColorMedium;
			fixed4 _ColorDark;

			float _FirstThreshold;
			float _SecondThreshold;

			sampler2D _MainTex;

			float4 frag (geometryOutput i, fixed facing : VFACE) : SV_Target
            {
				float mask_value = tex2D(_MainTex, i.uv).r;
				if (mask_value != 0)
				{
					discard;
				}

				fixed shadow = SHADOW_ATTENUATION(i);
                fixed3 lighting = i.diff * shadow + i.ambient;

				float4 col = _ColorLight;
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
    }
}