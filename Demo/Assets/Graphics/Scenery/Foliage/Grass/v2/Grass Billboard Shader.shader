Shader "Custom/Grass Billboard Shader"
{
    Properties
    {
		_MainTex ("Texture", 2D) = "white" {}
		_TileAmount("Amount of Tiles", Int) = 8
		_TilePixelSize("Tile Pixel Size", Int) = 16
		
		_YDisplacement ("Y Displacement", Float) = 0.5
		_XZDisplacementRandom ("X Z Displacement Random", Float) = 0.5
		_TessellationUniform("Tessellation Uniform", Range(1, 32)) = 1

		_Colors ("Texture", 2D) = "white" {}
		_CurveTexture ("Texture", 2D) = "white" {}
		
		_WindDistortionMap ("Texture", 2D) = "white" {}
		_WindFrequency("Wind Frequency", Vector) = (0.05, 0.05, 0, 0)
		_WindStrength("Wind Strength", Float) = 1
    }

	SubShader
	{
		Pass
		{
			Tags {
				"RenderType" = "Opaque"
				"LightMode" = "ForwardBase"
				"PassFlags" = "OnlyDirectional"
			}
			CGPROGRAM
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
			#include "AutoLight.cginc"

			#pragma vertex vert
			#pragma fragment frag

			struct v2f
			{
				float2 uv : TEXCOORD0;
				SHADOW_COORDS(1)
				fixed3 diff : COLOR0;
				fixed3 ambient : COLOR1;
				float4 pos : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			sampler2D _Colors;
			float4 _Colors_ST;

			sampler2D _CurveTexture;
			float4 _CurveTexture_ST;

			v2f vert(appdata_base v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord;
				half3 worldNormal = UnityObjectToWorldNormal(v.normal);
				half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
				o.diff = nl * _LightColor0.rgb;
				o.ambient = ShadeSH9(half4(worldNormal,1));
				TRANSFER_SHADOW(o)
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				fixed shadow = SHADOW_ATTENUATION(i);
				fixed3 lighting = i.diff * shadow + i.ambient;
				float curve_value = tex2D(_CurveTexture, saturate(lighting.x)).r;
				fixed4 color = tex2D(_Colors, curve_value);

				return color;
			}
			ENDCG
		}
		UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
		//UsePass "Legacy Shaders/Transparent/Cutout/VertexLit"

		Pass
		{
			Tags
			{
				"Queue" = "Transparent"
				"IgnoreProjector" = "True"
				"RenderType" = "Transparent"
			}

			CGPROGRAM
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
			#include "AutoLight.cginc"
			#include "/Assets/Graphics/CustomTessellation.cginc"

			#pragma vertex vert
			#pragma fragment frag
			#pragma geometry geo
			#pragma target 4.6
			#pragma multi_compile_fwdbase
			#pragma hull hull
			#pragma domain domain

			sampler2D _WindDistortionMap;
			float4 _WindDistortionMap_ST;
			float2 _WindFrequency;
			float _WindStrength;

			float _YDisplacement;
			float _XZDisplacementRandom;

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
			void geo(triangle vertexOutput IN[3], inout TriangleStream<g2f> outStream)
			{
				float3 center = IN[0].vertex + float3(0, _YDisplacement, 0);

				float3 up = float3(0, 1, 0);
				float3 look = mul(mul((float3x3)unity_CameraToWorld, float3(0,0,-1)), unity_ObjectToWorld);
				look = normalize(look);

				float3 right = normalize(cross(up, look));
				up = normalize(cross(look, right));

				float PixelSize = _TilePixelSize / (10.8); // 5.4 * 2
				float3 r = right * PixelSize;
				float3 u = up * PixelSize;

				float3 forward = float3(0, 0, 1);
				float3 right_displacement = right * rand(center) * _XZDisplacementRandom;
				float3 up_displacement = forward * rand(center.xzy) * _XZDisplacementRandom;
				center += right_displacement + up_displacement;
				float4 v[4];
				v[0] = float4(center + r - u, 1.0f);
				v[1] = float4(center + r + u, 1.0f);
				v[2] = float4(center - r - u, 1.0f);
				v[3] = float4(center - r + u, 1.0f);

				float3 vNormal = IN[0].normal;

				float2 center_world = mul(unity_ObjectToWorld, center).xz;
				
				float2 uv = center_world * _WindDistortionMap_ST.xy + _WindDistortionMap_ST.zw + _WindFrequency * _Time.y;
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
			}

			sampler2D _Colors;
			float4 _Colors_ST;

			sampler2D _MainTex;
			float4 _MainTex_ST;

			sampler2D _CurveTexture;
			float4 _CurveTexture_ST;

			fixed4 frag(g2f i, fixed facing : VFACE) : SV_Target
			{
				//return tex2D(_MainTex, i.uv * float2(0.125, 0.125) + i.wind);
				//return fixed4(i.wind.x, i.wind.y, 0, 1);
				float alpha = tex2D(_MainTex, i.uv * float2(0.125, 0.125) + i.wind).r;

				float mask_value = tex2D(_MainTex, i.uv * float2(0.25, 1)).r;
				if (alpha == 0)
				{
					discard;
				}

				fixed shadow = SHADOW_ATTENUATION(i);
				fixed3 lighting = i.diff * shadow + i.ambient;
				float curve_value = tex2D(_CurveTexture, saturate(lighting.x)).r;
				fixed4 color = tex2D(_Colors, curve_value);

				return color;
			}
			ENDCG
		}
    }
}