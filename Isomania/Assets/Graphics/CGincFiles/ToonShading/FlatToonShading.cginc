#include "/Assets/Graphics/CGincFiles/ToonShading/ToonShadingSetup.cginc"

sampler2D _LightTexture0;
float4x4 unity_WorldToLight;
UNITY_DECLARE_SHADOWMAP(_DirectionalShadowmap);

float4 LightCalculation(float4 objectCenter, float3 objectNormal)
{
	// Get normal
	float3 normal = normalize(UnityObjectToWorldNormal(normal));

	// Get world center and normal of triangle.
	float4 center = float4(mul(unity_ObjectToWorld, center).xyz, 1);

	// Get shadow coord for current triangle.
	float4 shadowCoord = mul(unity_WorldToShadow[0], center);
	// Samples the shadow map, returning a value in the 0...1 range,
	// where 0 is in the shadow, and 1 is not.
	float shadow = UNITY_SAMPLE_SHADOW(_DirectionalShadowmap, shadowCoord);

	// Do the same light calculation that is for all toon shaders
	return CalculateLightPrivate(normal, shadow);

}

float4 LightCalculation(float3 objectCenter, float3 objectNormal)
{
	return LightCalculation(float4(objectCenter, 1), objectNormal);
}
