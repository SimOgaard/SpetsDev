#include "/Assets/Graphics/CGincFiles/ToonShading/ToonShadingSetup.cginc"

sampler2D _LightTexture0;
float4x4 unity_WorldToLight;
UNITY_DECLARE_SHADOWMAP(_DirectionalShadowmap);

float2 CalculateToonUVFlat(float4 objectCenter, float3 objectNormal)
{
	// Get normal
	float3 normal = normalize(UnityObjectToWorldNormal(objectNormal));

	// Get world center and normal of triangle.
	float4 center = float4(mul(unity_ObjectToWorld, objectCenter).xyz, 1);

	// Get shadow coord for current triangle.
	float4 shadowCoord = mul(unity_WorldToShadow[0], center);
	// Samples the shadow map, returning a value in the 0...1 range,
	// where 0 is in the shadow, and 1 is not.
	float shadow = UNITY_SAMPLE_SHADOW(_DirectionalShadowmap, shadowCoord);

	// Do the same light calculation that is for all toon shaders
	return CalculateToonUV(normal, shadow);
}

float2 CalculateToonUVFlat(float3 objectCenter, float3 objectNormal)
{
	return CalculateToonUVFlat(float4(objectCenter, 1), objectNormal);
}

float4 ToonShade(float2 toonUV)
{
	// And extract shade value from texture
	float shade = tex2D(_ColorShading, toonUV); 

	// Now use that shade value to get the right color
	return tex2D(_Colors, float2(shade, 0.0)); 
}