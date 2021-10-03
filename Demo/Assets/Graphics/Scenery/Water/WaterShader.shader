﻿Shader "Custom/Water Shader"
{
    Properties
    {
		// What color the water will sample when the surface below is shallow.
		_DepthGradientShallow("Depth Gradient Shallow", Color) = (0.325, 0.807, 0.971, 0.725)

		// What color the water will sample when the surface below is at its deepest.
		_DepthGradientDeep("Depth Gradient Deep", Color) = (0.086, 0.407, 1, 0.749)

		// Maximum distance the surface below the water will affect the color gradient.
		_DepthMaxDistance("Depth Maximum Distance", Float) = 1

		// Color to render the foam generated by objects intersecting the surface.
		_FoamColor("Foam Color", Color) = (1,1,1,1)

		// Noise texture used to generate waves.
		_SurfaceNoise("Surface Noise", 2D) = "white" {}

		// Speed, in UVs per second the noise will scroll. Only the xy components are used.
		_SurfaceNoiseScroll("Surface Noise Scroll Amount", Vector) = (0.03, 0.03, 0, 0)

		// Values in the noise texture above this cutoff are rendered on the surface.
		_SurfaceNoiseCutoff("Surface Noise Cutoff", Range(0, 1)) = 0.777

		// Red and green channels of this texture are used to offset the noise texture to create distortion in the waves.
		_SurfaceDistortion("Surface Distortion", 2D) = "white" {}	

		// Multiplies the distortion by this value.
		_SurfaceDistortionAmount("Surface Distortion Amount", Range(0, 1)) = 0.27

		// Control the distance that surfaces below the water will contribute to foam being rendered.
		_FoamMaxDistance("Foam Maximum Distance", Float) = 0.4
		_FoamMinDistance("Foam Minimum Distance", Float) = 0.04

		// Alpha value of water reflection.
		_WaterReflectionAmount("Water Reflection Amount", Range(0, 1)) = 0.35
		// Color blend of water reflection. // NOT SHURE IF I SHOULD USE
		_WaterReflectionColor("Water Reflection Color", Color) = (1, 1, 1, 1)
	
		// Speed, in UVs per second the noise will scroll. Only the xy components are used.
		_UnderWaterDistortSpeed("Under Water Noise Speed", Vector) = (0.03, 0.03, 0, 0)

		// Speed, in UVs per second the noise will scroll. Only the xy components are used.
		_UnderWaterDistortAmount("Under Water Noise Amount", Vector) = (0.03, 0.03, 0, 0)
    }
    SubShader
    {
		Tags
		{
			"RenderType" = "Transparent"
			"Queue" = "Transparent"
		}

		GrabPass { "_GrabTexture" }

        Pass
        {
			// Transparent "normal" blending.
			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite Off

            CGPROGRAM
			//#define SMOOTHSTEP_AA 0.01
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

			// Blends two colors using the same algorithm that our shader is using
			// to blend with the screen. This is usually called "normal blending",
			// and is similar to how software like Photoshop blends two layers.
			float4 alphaBlend(float4 top, float4 bottom)
			{
				float3 color = (top.rgb * top.a) + (bottom.rgb * (1 - top.a));
				float alpha = top.a + bottom.a * (1 - top.a);

				return float4(color, alpha);
			}

            struct appdata
            {
                float4 vertex : POSITION;
				float4 uv : TEXCOORD0;
				float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;	
				float2 noiseUV : TEXCOORD0;
				float2 distortUV : TEXCOORD1;
				float4 screenPosition : TEXCOORD2;
				float3 viewNormal : NORMAL;
            };

			sampler2D _SurfaceNoise;
			float4 _SurfaceNoise_ST;

			sampler2D _SurfaceDistortion;
			float4 _SurfaceDistortion_ST;

            v2f vert (appdata v)
            {
                v2f o;

                o.vertex = UnityObjectToClipPos(v.vertex);
				o.screenPosition = ComputeScreenPos(o.vertex);
				o.distortUV = TRANSFORM_TEX(v.uv, _SurfaceDistortion);
				o.noiseUV = TRANSFORM_TEX(v.uv, _SurfaceNoise);
				o.viewNormal = COMPUTE_VIEW_NORMAL;

                return o;
            }

			float4 _DepthGradientShallow;
			float4 _DepthGradientDeep;
			float4 _FoamColor;

			float _DepthMaxDistance;
			float _FoamMaxDistance;
			float _FoamMinDistance;
			float _SurfaceNoiseCutoff;
			float _SurfaceDistortionAmount;

			float2 _SurfaceNoiseScroll;

			sampler2D _CameraDepthTexture;
			sampler2D _CameraNormalsTexture;

			sampler2D _GrabTexture;

			float _WaterReflectionAmount;
			float4 _WaterReflectionColor;
		
			float2 _UnderWaterDistortSpeed;
			float2 _UnderWaterDistortAmount;
			uniform float4 _CameraOffset;

			sampler2D _WaterReflectionTexture;
			
            float4 frag (v2f i) : SV_Target
            {
				// Retrieve the current linear depth value of the surface behind the pixel we are currently rendering.
				float rawDepth = tex2D(_CameraDepthTexture, i.screenPosition).r;
				// Flip orthographic projection.
				float orthoLinearDepth = _ProjectionParams.x > 0 ? rawDepth : 1 - rawDepth;
				// Recast surface 01-linear depth value to unity units.
				float orthoEyeDepth = lerp(_ProjectionParams.y, _ProjectionParams.z, orthoLinearDepth);
				// Retrieve depth value to shader plane.
				float orthoPlainLinearDepth = 1 - i.screenPosition.z;
				// Recast to unity units.
				float orthoPlainDepth = lerp(_ProjectionParams.y, _ProjectionParams.z, orthoPlainLinearDepth);
				// Water depth for current pixel in unity units.
				float depthDifference = (orthoEyeDepth - orthoPlainDepth) / 10.0;

				// Calculate the color of the water based on the depth using our two gradient colors.
				float waterDepthDifference01 = saturate(depthDifference / _DepthMaxDistance);
				float4 waterColor = lerp(_DepthGradientShallow, _DepthGradientDeep, waterDepthDifference01);

				// Retrieve the view-space normal of the surface behind the
				// pixel we are currently rendering.
				float3 existingNormal = tex2Dproj(_CameraNormalsTexture, UNITY_PROJ_COORD(i.screenPosition)).rgb;

				// Modulate the amount of foam we display based on the difference
				// between the normals of our water surface and the object behind it.
				// Larger differences allow for extra foam to attempt to keep the overall
				// amount consistent.
				fixed3 WTF = fixed3(0.0, 0.866, 0.5); // 0 cos 30 sin 30

				float3 normalDot = saturate(dot(existingNormal, WTF));

				//float3 forward = mul((float3x3)unity_CameraToWorld, float3(0,0,1)); 

				//return float4(normalDot,1);
				float foamDistance = lerp(_FoamMaxDistance, _FoamMinDistance, normalDot);
				float foamDepthDifference01 = saturate(depthDifference / foamDistance);

				float surfaceNoiseCutoff = foamDepthDifference01 * _SurfaceNoiseCutoff;

				float2 distortTex = (tex2D(_SurfaceDistortion, i.distortUV).xy * 2 - 1); 
				float2 distortSample = distortTex * _SurfaceDistortionAmount;

				// Distort the noise UV based off the RG channels (using xy here) of the distortion texture.
				// Also offset it by time, scaled by the scroll speed.
				float2 noiseUV = float2((i.noiseUV.x + _Time.y * _SurfaceNoiseScroll.x) + distortSample.x, 
				(i.noiseUV.y + _Time.y * _SurfaceNoiseScroll.y) + distortSample.y);
				float surfaceNoiseSample = tex2D(_SurfaceNoise, noiseUV).r;

				// Use smoothstep to ensure we get some anti-aliasing in the transition from foam to surface.
				// Uncomment the line below to see how it looks without AA.
				float surfaceNoise = surfaceNoiseSample > surfaceNoiseCutoff ? 1 : 0;
				// float surfaceNoise = smoothstep(surfaceNoiseCutoff - SMOOTHSTEP_AA, surfaceNoiseCutoff + SMOOTHSTEP_AA, surfaceNoiseSample);

				float4 surfaceNoiseColor = _FoamColor;
				surfaceNoiseColor.a = surfaceNoise;
			
				// Get pixel perfect distort value for water.
				float2 under_water_distort_time = _Time.yy * _UnderWaterDistortSpeed.xy;
				float2 under_water_distort = (tex2D(_SurfaceDistortion, i.distortUV + under_water_distort_time).xy * 2 - 1) * _UnderWaterDistortAmount.xy;
				float2 px = float2(384, 216);
				float2 distort_value = round(px * under_water_distort) / px;

				// Screen uv coords for under water pixel.
				float2 screen_uv = i.screenPosition.xy;
				float2 screen_uv_distort = screen_uv + distort_value;
				
				// Get raw depth for new uv.
				rawDepth = tex2D(_CameraDepthTexture, screen_uv_distort).r;
				// Flip orthographic projection.
				orthoLinearDepth = _ProjectionParams.x > 0 ? rawDepth : 1 - rawDepth;
				// Recast surface 01-linear depth value to unity units.
				orthoEyeDepth = lerp(_ProjectionParams.y, _ProjectionParams.z, orthoLinearDepth);
				// Retrieve depth value to shader plane.
				orthoPlainLinearDepth = 1 - i.screenPosition.z;
				// Recast to unity units.
				orthoPlainDepth = lerp(_ProjectionParams.y, _ProjectionParams.z, orthoPlainLinearDepth);
				// Water depth for current pixel in unity units.
				depthDifference = orthoEyeDepth - orthoPlainDepth;				
				
				// Relfection uv.
				float2 screen_uv_reflection = float2(screen_uv.x, 1-screen_uv.y) + distort_value;

				// Sample reflection camera color and depth value.
				float4 waterReflection = tex2D(_WaterReflectionTexture, screen_uv_reflection);
				waterReflection.a *= _WaterReflectionAmount;

				// If we are under water.
				if (depthDifference > 0)
				{
					float4 under_color = float4(tex2D(_GrabTexture, screen_uv_distort).rgb, 1);
					waterColor = alphaBlend(waterColor, under_color);
				}

				waterReflection = alphaBlend(_WaterReflectionColor, waterReflection);
				waterColor = alphaBlend(waterReflection, waterColor);

				return alphaBlend(surfaceNoiseColor, waterColor);
            }
            ENDCG
        }
    }
}