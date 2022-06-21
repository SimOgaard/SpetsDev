#include "UnityCG.cginc"
#include "AutoLight.cginc"
#include "Lighting.cginc"

sampler2D _LightTexture0;
float4x4 unity_WorldToLight;
UNITY_DECLARE_SHADOWMAP(_DirectionalShadowmap);

float _LightColorValue; // what
float4 _AmbientColor;

float4 LightCalculation(float4 center, float3 normal)
{
	// get world center and normal of triangle
	float4 worldCenter = float4(mul(unity_ObjectToWorld, center).xyz, 1);
	float3 worldNormal = UnityObjectToWorldNormal(normal);

	// no light cookies so only get directional light value
	float directionalLightValue = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));

	// get shadow coord
	float4 shadowCoord = mul(unity_WorldToShadow[0], worldCenter);
	// Sample the shadowmap
	float shadow = UNITY_SAMPLE_SHADOW(_DirectionalShadowmap, shadowCoord);

	//float4 light = lerp(_AmbientColor, )

	return directionalLightValue;
}

float4 LightCalculation(float3 center, float3 normal)
{
	return LightCalculation(float4(center, 1), normal);
}
