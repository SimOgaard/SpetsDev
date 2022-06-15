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

			float2 renderResolution;
			float2 renderResolutionExtended;

			float pixelsPerUnit;
			float pixelsPerUnit3;
			float unitsPerPixelWorld;

			float remap01(float v)
			{
				return saturate((v + 1) * 0.5);
			}

			float3 GetWarpValue(float3 worldPos)
			{
				fnl_state warp = fnlCreateState(1337); //_Noise_Seed;
				warp.domain_warp_type = 0; //_Warp_DomainWarpType;
				warp.rotation_type_3d = 2; //_Warp_RotationType3D;
				warp.domain_warp_amp = 10; //_Warp_DomainWarpAmplitude;
				warp.frequency = 0.3; //_Warp_Frequency;

				warp.fractal_type = 4; //_Warp_FractalType;
				warp.octaves = 3; //_Warp_FractalOctaves;
				warp.lacunarity = 0.5; //_Warp_FractalLacunarity;
				warp.gain = 2.0; //_Warp_FractalGain;

				fnlDomainWarp3D(warp, worldPos.x, worldPos.y, worldPos.z);
				return worldPos;
			}

			float GetNoiseValue(float3 worldPos)
			{
				fnl_state noise = fnlCreateState(1337); //_Noise_Seed;
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
				// For pixel perfect reasons to remove jitter as much as possible
				// we need to snap world position AND time to grid seperately!
				// OBS! worldPos gets snapped in transform since it is a flat plain

				// grab pixel world position and offset it so that any global position changes doenst break the shader
				float3 worldPos = (i.worldPos - _WorldOffset) * float3(0.35, 0.75, 0.75); // noise stretch

				// grab current noise offset by time
				float3 time = _Time[1] * float3(1.25, 1.75, 1.25); // noise scroll
				// snap time to grid
				time = round(time * pixelsPerUnit3) / pixelsPerUnit3;

				// create a current worldPos that has time and worldPos
				float3 currentWorldPos = worldPos + time;
				// now we can use this value without worrying about jitter :)

				// beggining with warping the value
				float3 currentWorldPosWarped = GetWarpValue(currentWorldPos);

				// get the warped value
				float3 warpValue = currentWorldPosWarped - currentWorldPos;
				
				// now translate that warped value to screen
				float2 warpUV = warpValue.xz / renderResolutionExtended;
				// and round it to pixel perfect
				warpUV = round(warpUV * renderResolutionExtended) / renderResolutionExtended;

				// get all screen uvs neccesary
				float2 screenUV = i.screenPosition.xy;
				float2 reflectionUV = float2(screenUV.x, 1.0-screenUV.y);

				// and apply the warpeduv
				float2 screenUVWarped = saturate(screenUV + warpUV);
				float2 reflectionUVWarped = saturate(reflectionUV + warpUV);

				//return float4(screenUVWarped, 0, 1);

				// Retrieve depth value to shader plane of current pixel. And recast to unity units.
				float orthoPlainDepth = lerp(_ProjectionParams.z, _ProjectionParams.y, i.screenPosition.z);

				// Retrieve the current linear depth value of the surface behind the pixel we are currently rendering.
				float rawDepth = tex2D(_CameraDepthTexture, screenUV).r;
				// Flip orthographic projection.
				float orthoLinearDepth = _ProjectionParams.x > 0.0 ? rawDepth : 1.0 - rawDepth;
				// Recast surface 01-linear depth value to unity units.
				float orthoEyeDepth = lerp(_ProjectionParams.y, _ProjectionParams.z, orthoLinearDepth);
				// Water depth for current pixel in unity units.
				float depthDifference = (orthoEyeDepth - orthoPlainDepth);
				// Depth scaled to our need
				float depthDifference01 = saturate(depthDifference / _DepthMaximumDistance);

				float deltaYUV = screenUVWarped - screenUV;
				float deltaYWorld = deltaYUV * unity_OrthoParams.y;
				float addedPlainDepth = deltaYWorld / tan(30.0);
				float orthoPlainDepthWarped = orthoPlainDepth + addedPlainDepth;

				// Retrieve the current linear depth value of the surface behind the pixel we are currently rendering.
				float rawDepthWarped = tex2D(_CameraDepthTexture, screenUVWarped).r;
				// Flip orthographic projection.
				float orthoLinearDepthWarped = _ProjectionParams.x > 0.0 ? rawDepthWarped : 1.0 - rawDepthWarped;
				// Recast surface 01-linear depth value to unity units.
				float orthoEyeDepthWarped = lerp(_ProjectionParams.y, _ProjectionParams.z, orthoLinearDepthWarped);
				// Water depth for current pixel in unity units.
				float depthDifferenceWarped = (orthoEyeDepthWarped - orthoPlainDepthWarped);
				// Depth scaled to our need
				float depthDifferenceWarped01 = saturate(depthDifferenceWarped / _DepthMaximumDistance);
				
				// now we have all data neccesary to know which uv's depts etc we should use!
				float2 underWaterUV;
				float depthDifferenceFixed;

				if (depthDifferenceWarped01 == 0)
				{
					underWaterUV = screenUV;
					depthDifferenceFixed = depthDifference01;
				}
				else
				{
					underWaterUV = screenUVWarped;
					depthDifferenceFixed = depthDifferenceWarped01;
				}

				// Calculate the color of the water based on the depth using our two gradient colors.
				float curve_value = tex2D(_ColorShading, depthDifferenceFixed - _WaterColOffset).r;
				float alpha = tex2D(_AlphaShading, depthDifferenceFixed).r;
				float4 waterColor = float4(tex2D(_WaterColors, curve_value).rgb, alpha);
				
				// Retrieve the view-space normal of the surface behind the
				// pixel we are currently rendering.
				float3 existingNormal = tex2Dproj(_CameraNormalsTexture, UNITY_PROJ_COORD(i.screenPosition)).rgb;

				// Modulate the amount of foam we display based on the difference
				// between the normals of our water surface and the object behind it.
				// Larger differences allow for extra foam to attempt to keep the overall
				// amount consistent.
				float3 cosSin30 = float3(0.0, 0.866, 0.5); // 0 cos 30 sin 30
				float3 normalDot = saturate(dot(existingNormal, cosSin30));

				float foamDistance = lerp(_FoamMaxDistance, _FoamMinDistance, normalDot);
				float foamDepthDifference01 = saturate(depthDifference / foamDistance);

				float surfaceNoiseCutoff = foamDepthDifference01 * _SurfaceNoiseCutoff;

				// get color under warped pixel if it is under water else non distorted
				float4 underWaterColor = float4(tex2D(_GrabTexture, underWaterUV).rgb, 1);
				waterColor = alphaBlend(waterColor, underWaterColor);

				// Relfection uv.
				// Sample reflection camera color.
				float4 waterReflection = tex2D(_WaterReflectionTexture, reflectionUVWarped);
				waterReflection.a *= _WaterReflectionAmount;

				waterReflection = alphaBlend(_WaterReflectionColor, waterReflection);
				waterColor = alphaBlend(waterReflection, waterColor);

				if (surfaceNoiseCutoff != 1)
				{
					float noiseValue = GetNoiseValue(currentWorldPos);
					float surfaceNoise = noiseValue > surfaceNoiseCutoff ? 1 : 0;

					float4 surfaceNoiseColor = _FoamColor;
					surfaceNoiseColor.a *= surfaceNoise;

					return alphaBlend(surfaceNoiseColor, waterColor);
				}
				return waterColor;
				//float surfaceNoise = smoothstep(surfaceNoiseCutoff - SMOOTHSTEP_AA, surfaceNoiseCutoff + SMOOTHSTEP_AA, noiseValue);
            }
            ENDCG
        }
    }
}