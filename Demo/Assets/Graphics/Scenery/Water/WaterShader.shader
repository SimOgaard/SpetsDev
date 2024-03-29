﻿Shader "Custom/Water Shader"
{
    Properties
    {
		_WaterColors ("Color Texture", 2D) = "white" {}
		_ColorShading ("Color Shading", 2D) = "white" {}
		_AlphaShading ("Curve Alpha", 2D) = "white" {}

		// Maximum distance the surface below the water will affect the color gradient.
		_DepthMaximumDistance("Depth Maximum Distance", Float) = 1

		// Color to render the foam generated by objects intersecting the surface.
		_FoamColor("Foam Color", Color) = (1,1,1,1)

		// Values in the noise texture above this cutoff are rendered on the surface.
		_SurfaceNoiseCutoff("Surface Noise Cutoff", Range(0, 1)) = 0.777

		// Control the distance that surfaces below the water will contribute to foam being rendered.
		_FoamMaxDistance("Foam Maximum Distance", Float) = 0.4
		_FoamMinDistance("Foam Minimum Distance", Float) = 0.04

		// Alpha value of water reflection.
		_WaterReflectionAmount("Water Reflection Amount", Range(0, 1)) = 0.35
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
			#include "/Assets/Graphics/CGincFiles/FastNoiseLite.cginc"
			#include "/Assets/Graphics/CGincFiles/GenericShaderFunctions.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
				float4 uv : TEXCOORD0;
				float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
				float4 screenPosition : TEXCOORD0;
				float3 viewNormal : NORMAL;
				float3 worldPos : TEXCOORD1;
            };

            v2f vert (appdata v)
            {
                v2f o;

                o.vertex = UnityObjectToClipPos(v.vertex);
				o.screenPosition = ComputeScreenPos(o.vertex);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				o.viewNormal = COMPUTE_VIEW_NORMAL;

                return o;
            }
			sampler2D _ColorShading;
			sampler2D _AlphaShading;
			sampler2D _WaterColors;
			float _DepthMaximumDistance;

			float4 _FoamColor;

			float _FoamMaxDistance;
			float _FoamMinDistance;
			float _SurfaceNoiseCutoff;

			sampler2D _CameraDepthTexture;
			sampler2D _CameraNormalsTexture;
			sampler2D _GrabTexture;
			sampler2D _WaterReflectionTexture;

			float _WaterReflectionAmount;
			float4 _WaterReflectionColor;
		
			float3 _WorldOffset;

			float _WaterColOffset;

			float remap01(float v) {
				return saturate((v + 1) * 0.5);
			}

			float3 GetWarpValue(float3 worldPos)
			{
				fnl_state warp = fnlCreateState();
				warp.domain_warp_type = 1; //_Warp_DomainWarpType;
				warp.rotation_type_3d = 2; //_Warp_RotationType3D;
				warp.domain_warp_amp = 0.01; //_Warp_DomainWarpAmplitude;
				warp.frequency = 0.4; //_Warp_Frequency;

				warp.fractal_type = 0; //_Warp_FractalType;
				warp.octaves = 0; //_Warp_FractalOctaves;
				warp.lacunarity = 0.0; //_Warp_FractalLacunarity;
				warp.gain = 0.0; //_Warp_FractalGain;

				fnlDomainWarp3D(warp, worldPos.x, worldPos.y, worldPos.z);
				return worldPos;
			}

			float GetNoiseValue(float3 worldPos)
			{
				fnl_state noise = fnlCreateState();
				noise.seed = 1337; //_Noise_Seed;
				noise.frequency = 0.15; //_Noise_Frequency;
				noise.noise_type = 1; //_Noise_NoiseType;
				noise.rotation_type_3d = 2; //_Noise_RotationType3D;

				noise.fractal_type = 1; //_Noise_FractalType;
				noise.octaves = 5; //_Noise_FractalOctaves;
				noise.lacunarity = 2.0; //_Noise_FractalLacunarity;
				noise.gain = 0.5; //_Noise_FractalGain;
				noise.weighted_strength = 0.0; //_Noise_FractalWeightedStrength;
				noise.ping_pong_strength = 2.0; //_Noise_FractalPingPongStrength;

				noise.cellular_distance_func = 1; //_Noise_CellularDistanceFunction;
				noise.cellular_return_type = 1; //_Noise_CellularReturnType;
				noise.cellular_jitter_mod = 1.0; //_Noise_CellularJitter;

				return remap01(fnlGetNoise3D(noise, worldPos.x, worldPos.y, worldPos.z));
			}

            float4 frag (v2f i) : SV_Target
            {
				// Get all positions neccesary
				float3 worldPos = (i.worldPos - _WorldOffset) * float3(0.35, 0.75, 0.75);
				float3 worldPosTime = worldPos + _Time[0] * float3(17.5, 12.5, 17.5);
				float3 worldPosWarped = GetWarpValue(worldPosTime);
				float3 worldPosWarpedOffsetOnly = worldPosWarped - worldPosTime;
				float2 px = float2(480, 270);
				float2 worldPosWarpedOffsetOnlyPixelPerfect = round(px * worldPosWarpedOffsetOnly.xz) / px;
				float2 screen_uv = i.screenPosition.xy;
				float2 screen_uv_distort = screen_uv + worldPosWarpedOffsetOnlyPixelPerfect;
				float2 screen_uv_reflection = float2(screen_uv.x, 1-screen_uv.y) + worldPosWarpedOffsetOnlyPixelPerfect;

				// Retrieve depth value to shader plane.
				float orthoPlainLinearDepth = 1 - i.screenPosition.z;
				// Recast to unity units.
				float orthoPlainDepth = lerp(_ProjectionParams.y, _ProjectionParams.z, orthoPlainLinearDepth);

				// Retrieve the current linear depth value of the surface behind the pixel we are currently rendering.
				float rawDepth = tex2D(_CameraDepthTexture, screen_uv).r;
				// Flip orthographic projection.
				float orthoLinearDepth = _ProjectionParams.x > 0 ? rawDepth : 1 - rawDepth;
				// Recast surface 01-linear depth value to unity units.
				float orthoEyeDepth = lerp(_ProjectionParams.y, _ProjectionParams.z, orthoLinearDepth);
				// Water depth for current pixel in unity units.
				float depthDifference = (orthoEyeDepth - orthoPlainDepth);
				// Depth scaled to our need
				float depthDifference01 = saturate(depthDifference / _DepthMaximumDistance);

				// Retrieve the current linear depth value of the surface behind the pixel we are currently rendering.
				rawDepth = tex2D(_CameraDepthTexture, screen_uv_distort).r;
				// Flip orthographic projection.
				orthoLinearDepth = _ProjectionParams.x > 0 ? rawDepth : 1 - rawDepth;
				// Recast surface 01-linear depth value to unity units.
				orthoEyeDepth = lerp(_ProjectionParams.y, _ProjectionParams.z, orthoLinearDepth);
				// Water depth for current pixel in unity units.
				float depthDifferenceDistort = (orthoEyeDepth - orthoPlainDepth);
				// Depth scaled to our need
				float depthDifferenceDistort01 = saturate(depthDifferenceDistort / _DepthMaximumDistance);

				/*
				// Retrieve the current linear depth value of the surface behind the pixel we are currently rendering.
				rawDepth = tex2D(_CameraDepthTexture, screen_uv - worldPosWarpedOffsetOnlyPixelPerfect).r;
				// Flip orthographic projection.
				orthoLinearDepth = _ProjectionParams.x > 0 ? rawDepth : 1 - rawDepth;
				// Recast surface 01-linear depth value to unity units.
				orthoEyeDepth = lerp(_ProjectionParams.y, _ProjectionParams.z, orthoLinearDepth);
				
				
				if ((orthoEyeDepth - orthoPlainDepth) < 0)
				{
					depthDifferenceDistort01 = depthDifference01;
				}
				*/

				// Calculate the color of the water based on the depth using our two gradient colors.
				float curve_value = tex2D(_ColorShading, depthDifference01 - _WaterColOffset).r;
				float alpha = tex2D(_AlphaShading, depthDifference01).r;
				float4 waterColor = float4(tex2D(_WaterColors, curve_value).rgb, alpha);

				// Retrieve the view-space normal of the surface behind the
				// pixel we are currently rendering.
				float3 existingNormal = tex2Dproj(_CameraNormalsTexture, UNITY_PROJ_COORD(i.screenPosition)).rgb;

				// Modulate the amount of foam we display based on the difference
				// between the normals of our water surface and the object behind it.
				// Larger differences allow for extra foam to attempt to keep the overall
				// amount consistent.
				float3 WTF = float3(0.0, 0.866, 0.5); // 0 cos 30 sin 30

				float3 normalDot = saturate(dot(existingNormal, WTF));

				//float3 forward = mul((float3x3)unity_CameraToWorld, float3(0,0,1)); 

				//return float4(normalDot,1);
				float foamDistance = lerp(_FoamMaxDistance, _FoamMinDistance, normalDot);
				float foamDepthDifference01 = saturate(depthDifference / foamDistance);

				float surfaceNoiseCutoff = foamDepthDifference01 * _SurfaceNoiseCutoff;



				/*
				float2 distortTex = (tex2D(_SurfaceDistortion, i.distortUV).xy * 2 - 1); 
				float2 distortSample = distortTex * _SurfaceDistortionAmount;

				// Distort the noise UV based off the RG channels (using xy here) of the distortion texture.
				// Also offset it by time, scaled by the scroll speed.
				float2 noiseUV = float2((i.noiseUV.x + _Time.y * _SurfaceNoiseScroll.x) + distortSample.x, 
				(i.noiseUV.y + _Time.y * _SurfaceNoiseScroll.y) + distortSample.y);
				float surfaceNoiseSample = tex2D(_SurfaceNoise, noiseUV).r;
				*/
				// Use smoothstep to ensure we get some anti-aliasing in the transition from foam to surface.
				// Uncomment the line below to see how it looks without AA.


				/*
				// Get pixel perfect distort value for water.
				float2 under_water_distort_time = _Time.yy * _UnderWaterDistortSpeed.xy;
				float2 under_water_distort = (tex2D(_SurfaceDistortion, i.distortUV + under_water_distort_time).xy * 2 - 1) * _UnderWaterDistortAmount.xy;
				float2 px = float2(384, 216);
				float2 distort_value = round(px * under_water_distort) / px;
				*/
				// Screen uv coords for under water pixel.
				/*
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
				float depthDifference_2 = orthoEyeDepth - orthoPlainDepth;				
				*/
				// Relfection uv.

				// Sample reflection camera color and depth value.
				float4 waterReflection = tex2D(_WaterReflectionTexture, screen_uv_reflection);
				waterReflection.a *= _WaterReflectionAmount;

				// If we are under water.
				if (depthDifferenceDistort01 != 0)
				{
					float4 under_color = float4(tex2D(_GrabTexture, screen_uv_distort).rgb, 1);
					waterColor = alphaBlend(waterColor, under_color);
				}

				waterReflection = alphaBlend(_WaterReflectionColor, waterReflection);
				waterColor = alphaBlend(waterReflection, waterColor);

				if (surfaceNoiseCutoff != 1)
				{
					float noiseValue = GetNoiseValue(worldPosWarped);
					float surfaceNoise = noiseValue > surfaceNoiseCutoff ? 1 : 0;

					float4 surfaceNoiseColor = _FoamColor;
					surfaceNoiseColor.a = surfaceNoise;

					return alphaBlend(surfaceNoiseColor, waterColor);
				}
				return waterColor;
				//float surfaceNoise = smoothstep(surfaceNoiseCutoff - SMOOTHSTEP_AA, surfaceNoiseCutoff + SMOOTHSTEP_AA, noiseValue);
            }
            ENDCG
        }
    }
}