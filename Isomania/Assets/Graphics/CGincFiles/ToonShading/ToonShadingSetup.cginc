// Compile multiple versions of this shader depending on lighting settings.
#pragma multi_compile_fwdbase

#include "UnityCG.cginc"
// Files below include macros and functions to assist
// with lighting and shadows.
#include "Lighting.cginc"
#include "AutoLight.cginc"

float4 _AmbientColor;

sampler2D _Colors;
float4 _Colors_ST;

sampler2D _ColorShading;
float4 _ColorShading_ST;

float4 CalculateLightPrivate(float3 normal, float shadow)
{
	// Calculate illumination from directional light.
	// _WorldSpaceLightPos0 is a vector pointing the OPPOSITE
	// direction of the main directional light.
	float NdotL = dot(_WorldSpaceLightPos0, normal);

	// Partition the intensity into light and dark.
	float lightIntensity = NdotL * (shadow >= 1);
	// Multiply by the main directional light's intensity and color.
	float4 light = lightIntensity * _LightColor0;
	// Add ambient color
	light += _AmbientColor;

	// Get color shading uv based on light value
	float2 shadingUV = float2(light.a, 0.0);
	// And extract shade value from texture
	float shade = tex2D(_ColorShading, shadingUV); 

	// Now use that shade value to get the right color
	return tex2D(_Colors, float2(shade, 0.0)); 
}
