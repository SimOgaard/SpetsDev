#include "/Assets/Graphics/CGincFiles/ToonShading/ToonShadingSetup.cginc"

float2 ToonUV(v2f i)
{
	// Get normal
	float3 normal = normalize(i.worldNormal);
	// Samples the shadow map, returning a value in the 0...1 range,
	// where 0 is in the shadow, and 1 is not.
	float shadow = SHADOW_ATTENUATION(i);

	// Calculate toon uv for shading
	return CalculateToonUV(normal, shadow, CloudValueFromWorld(i.worldPosition));
}

float4 ToonShade(float2 toonUV)
{
	// And extract shade value from texture
	float shade = tex2D(_ColorShading, toonUV); 

	// Now use that shade value to get the right color
	return tex2D(_Colors, float2(shade, 0.0)); 
}

float4 ToonShade(v2f i)
{
	// Calculate toon uv for shading
	float2 toonUV = ToonUV(i);

	// And toon shade
	return ToonShade(toonUV);
}