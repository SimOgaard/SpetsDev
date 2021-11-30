Shader "Custom/shadow testing shader"
{
    Properties
    {
		_ShadowSoftness("Shadow Softness", Float) = 0.5
    }

	SubShader
	{
		Pass
		{
			Tags {
				"RenderType" = "Opaque"
				"LightMode" = "ForwardAdd"
				"PassFlags" = "OnlyDirectional"
			}
			CGPROGRAM
			#include "UnityCG.cginc"
			#include "AutoLight.cginc"
			#include "Lighting.cginc"
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
			#pragma geometry geo
			#pragma require geometry
			#pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
			
			sampler2D _LightTexture0;
			float4x4 unity_WorldToLight;

			float _ShadowSoftness;

			struct vertexInput
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float4 tangent : TANGENT;
			};

			struct vertexOutput
			{
				float4 vertex : SV_POSITION;
				float3 normal : NORMAL;
				float4 tangent : TANGENT;
			};

			vertexInput vert(vertexInput v)
			{
				return v;
			}

			vertexOutput tessVert(vertexInput v)
			{
				vertexOutput o;
				o.vertex = v.vertex; // Note that the vertex is NOT transformed to clip space here;
				o.normal = v.normal;
				o.tangent = v.tangent;
				return o;
			}

			struct g2f
			{
				float4 pos : SV_POSITION;
				float3 color : COLOR0;
				SHADOW_COORDS(0)
			};

			g2f VertexOutput(float3 pos, float directionalLightValue)
			{
				g2f o;
				o.pos = UnityObjectToClipPos(pos);
				o.color = _LightColor0 * directionalLightValue;
                TRANSFER_SHADOW(o)
				return o;
			}

			[maxvertexcount(3)]
			void geo(triangle vertexOutput IN[3], inout TriangleStream<g2f> outStream)
			{
				float3 flatNormal = normalize(cross(IN[1].vertex - IN[0].vertex, IN[2].vertex - IN[0].vertex));
				float3 center = (IN[0].vertex + IN[1].vertex + IN[1].vertex) / 3;

				float2 lightUVCookie = mul(unity_WorldToLight, float4(center, 1)).xy;
				float lightMap = tex2Dlod(_LightTexture0, float4(lightUVCookie,0,0)).w;
				float3 worldFlatNormal = UnityObjectToWorldNormal(flatNormal);
				float directionalLightValue = max(0, dot(worldFlatNormal, _WorldSpaceLightPos0.xyz)) * lightMap;

				// This results in inconsistant shadows:
				// Create vertex output so that shadow sampeling are the same for the 3 vertices and therefore the whole face
				g2f idealVertexOutput = VertexOutput(center, directionalLightValue);

				// Only change SV_POSITION and append to outStream
				idealVertexOutput.pos = UnityObjectToClipPos(IN[0].vertex);
				outStream.Append(idealVertexOutput);
				idealVertexOutput.pos = UnityObjectToClipPos(IN[1].vertex);
				outStream.Append(idealVertexOutput);
				idealVertexOutput.pos = UnityObjectToClipPos(IN[2].vertex);
				outStream.Append(idealVertexOutput);
			}

			float4 frag(g2f i, float facing : VFACE) : SV_Target
			{
				float shadow = SHADOW_ATTENUATION(i);
				shadow = saturate(shadow + _ShadowSoftness);
				return float4(i.color * shadow,1);
			}
			ENDCG
		}
		UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
	}
}