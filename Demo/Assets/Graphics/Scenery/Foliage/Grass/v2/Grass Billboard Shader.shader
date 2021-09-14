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

		_Colors ("Color Texture", 2D) = "white" {}
		_CurveTexture ("Curve Texture", 2D) = "white" {}
		
		_WindDistortionMap ("Distortion Map Texture", 2D) = "white" {}
		_WindFrequency("Wind Frequency", Vector) = (0.05, 0.05, 0, 0)
		_WindStrength("Wind Strength", Float) = 1
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
			#include "Lighting.cginc"
			#pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
			#pragma target 3.0
 
			#if defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED)
				#define UNITY_SAMPLE_SCREEN_SHADOW(tex, uv) UNITY_SAMPLE_TEX2DARRAY_LOD( tex, float3((uv).x/(uv).w, (uv).y/(uv).w, (float)unity_StereoEyeIndex), 0 ).r
			#else
				#define UNITY_SAMPLE_SCREEN_SHADOW(tex, uv) tex2Dlod( tex, float4 (uv.xy / uv.w, 0, 0) ).r
			#endif
 
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
				float3 worldPos : TEXCOORD2;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			sampler2D _LightTexture0;
			float4x4 unity_WorldToLight;

			sampler2D _Colors;
			float4 _Colors_ST;

			sampler2D _CurveTexture;
			float4 _CurveTexture_ST;

			v2f vert(appdata_base v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.worldPos = mul (unity_ObjectToWorld, v.vertex);
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
				float2 uvCookie = mul(unity_WorldToLight, float4(i.worldPos, 1)).xy;
				float attenuation = tex2D(_LightTexture0, uvCookie).w;
                fixed3 lighting = i.diff * shadow * attenuation + i.ambient;

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
				"LightMode" = "ForwardAdd"
				"PassFlags" = "OnlyDirectional"
			}

			CGPROGRAM
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
		    #include "HLSLSupport.cginc"
			#include "UnityShadowLibrary.cginc"
			#include "/Assets/Graphics/CustomTessellation.cginc"

			#pragma vertex vert
			#pragma fragment frag
			#pragma geometry geo
			#pragma target 4.6
			#pragma multi_compile_fwdbase
			#pragma hull hull
			#pragma domain domain

			#if defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED)
				#define UNITY_SAMPLE_SCREEN_SHADOW(tex, uv) UNITY_SAMPLE_TEX2DARRAY_LOD( tex, float3((uv).x/(uv).w, (uv).y/(uv).w, (float)unity_StereoEyeIndex), 0 ).r
			#else
				#define UNITY_SAMPLE_SCREEN_SHADOW(tex, uv) tex2Dlod( tex, float4 (uv.xy / uv.w, 0, 0) ).r
			#endif

			#include "AutoLight.cginc"

			sampler2D _WindDistortionMap;
			float4 _WindDistortionMap_ST;
			float2 _WindFrequency;
			float _WindStrength;

			float _YDisplacement;
			float _XZDisplacementRandom;

			float _TileAmount;
			float _TilePixelSize;

			#define y_scale 1 / cos(3.14159265 / 6);

			struct g2f {
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				SHADOW_COORDS(1)
				float3 worldPos : TEXCOORD2;
				float2 wind : TEXCOORD3;
				float3 color : COLOR0;
			};

		  	sampler2D _LightTexture0;
			float4x4 unity_WorldToLight;

			sampler2D _Colors;
			float4 _Colors_ST;

			sampler2D _MainTex;
			float4 _MainTex_ST;

			sampler2D _CurveTexture;
			float4 _CurveTexture_ST;

			g2f VertexOutput(float3 pos, float2 uv, float3 norm, float2 wind, float attenuation)
			{
				g2f o;
				o.worldPos = pos;
				o.pos = UnityObjectToClipPos(pos);
				o.uv = uv;
				o.wind = wind;
				TRANSFER_SHADOW(o)

				half3 worldNormal = UnityObjectToWorldNormal(norm);
				half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
				fixed3 diff = nl * _LightColor0.rgb;
				fixed3 ambient = ShadeSH9(half4(worldNormal,1));

				fixed shadow = SHADOW_ATTENUATION(o);
                fixed3 lighting = diff * shadow * attenuation + ambient;
				float curve_value = tex2Dlod(_CurveTexture, float4(saturate(lighting.x),0,0,0)).r;
				fixed4 color = tex2Dlod(_Colors, float4(curve_value,0,0,0));
				o.color = color.rgb;

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
				look.y = 0;
				look = normalize(look);

				float3 right = normalize(cross(up, look));
				//up = normalize(cross(look, right)) * y_scale;

				float PixelSize = _TilePixelSize / (5.4 * 2);
				float3 r = right * PixelSize;
				float3 u = up * PixelSize * 2;

				float3 right_displacement = float3(1, 0, 0) * rand(center) * _XZDisplacementRandom;
				float3 up_displacement = float3(0, 0, 1) * rand(center.xzy) * _XZDisplacementRandom;
				center += right_displacement + up_displacement;
				float4 v[4];
				v[0] = float4(center + r, 1.0f);
				v[1] = float4(center + r + u, 1.0f);
				v[2] = float4(center - r, 1.0f);
				v[3] = float4(center - r + u, 1.0f);

				float3 vNormal = IN[0].normal;

				float3 center_world = mul(unity_ObjectToWorld, center).xyz;
				
				float2 uv = center_world.xz * _WindDistortionMap_ST.xy + _WindDistortionMap_ST.zw + _WindFrequency * _Time.y;
				float2 windSample = tex2Dlod(_WindDistortionMap, float4(uv, 0, 0)).xy; // get wind value ranging (0.33 - 0.66)
				float2 windSample01 = saturate(windSample * 3 - 1); // remap to 0-1
				float2 windSamplenegpos = windSample01 * 2 - 1; // remap to -1 - 1
				float2 windSampleStrength = windSamplenegpos * _WindStrength; // multiply by windstrength
				float2 remap_01 = saturate(windSampleStrength * 0.5 + 0.5); // saturate between 01 to keep low/all/high values dependent on windstrength
				float2 windSampleGrid;
				
				windSampleGrid = floor(remap_01 * (_TileAmount - 0.0001)) * (1 / _TileAmount);

				float2 uvCookie = mul(unity_WorldToLight, float4(center_world, 1)).xy;
				float attenuation = tex2Dlod(_LightTexture0, float4(uvCookie,0,0)).w;

				outStream.Append(VertexOutput(v[0], float2(1, 0), vNormal, windSampleGrid, attenuation ));
				outStream.Append(VertexOutput(v[1], float2(1, 1), vNormal, windSampleGrid, attenuation ));
				outStream.Append(VertexOutput(v[2], float2(0, 0), vNormal, windSampleGrid, attenuation ));
				outStream.Append(VertexOutput(v[3], float2(0, 1), vNormal, windSampleGrid, attenuation ));
			}

			fixed4 frag(g2f i, fixed facing : VFACE) : SV_Target
			{
				float uv_remap = 1 / _TileAmount;
				float alpha = tex2D(_MainTex, i.uv * uv_remap + i.wind).r;

				if (alpha == 0)
				{
					discard;
				}

				return float4(i.color,1);
			}
			ENDCG
		}
    }
}