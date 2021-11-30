#include "UnityCG.cginc"
#include "AutoLight.cginc"
#include "Lighting.cginc"

sampler2D _LightTexture0;
float4x4 unity_WorldToLight;

UNITY_DECLARE_SHADOWMAP(_DirectionalShadowmap);
float _ShadowSoftness;
float _DarkestValue;
float _DayNightTime;

float LightCalculation(float4 center, float3 normal)
{
	float4 world_center = float4(mul (unity_ObjectToWorld, center).xyz, 1);

	float2 lightUVCookie = mul(unity_WorldToLight, world_center).xy;
	float lightMap = tex2Dlod(_LightTexture0, float4(lightUVCookie,0,0)).w;
	float3 worldNormal = UnityObjectToWorldNormal(normal);
	float directionalLightValue = saturate(max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz)) * lightMap) * _LightColor0.r;

	float4 shadowCoords0 = mul (unity_WorldToShadow[0], world_center);
    float4 shadowCoords1 = mul (unity_WorldToShadow[1], world_center);
    float4 shadowCoords2 = mul (unity_WorldToShadow[2], world_center);
    float4 shadowCoords3 = mul (unity_WorldToShadow[3], world_center);
 
    // Find which cascaded shadow coords to use based on our distance to the camera
    float dist = distance (world_center.xyz, _WorldSpaceCameraPos.xyz);
    float4 zNear = dist >= _LightSplitsNear;
    float4 zFar = dist < _LightSplitsFar;
    float4 weights = zNear * zFar;
    float4 shadowCoords = shadowCoords0 * weights.x + shadowCoords1 * weights.y + shadowCoords2 * weights.z + shadowCoords3 * weights.w;
 
	// Sample the shadowmap
	float shadow = UNITY_SAMPLE_SHADOW (_DirectionalShadowmap, shadowCoords);
	shadow = saturate(shadow + _ShadowSoftness * _DayNightTime);

	return max(directionalLightValue * shadow, _DarkestValue * _DayNightTime);
}

float LightCalculation(float3 center, float3 normal)
{
	float4 world_center = float4(center, 1);

	float2 lightUVCookie = mul(unity_WorldToLight, world_center).xy;
	float lightMap = tex2Dlod(_LightTexture0, float4(lightUVCookie,0,0)).w;
	float3 worldNormal = UnityObjectToWorldNormal(normal);
	float directionalLightValue = saturate(max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz)) * lightMap) * _LightColor0.r;

	float4 shadowCoords0 = mul (unity_WorldToShadow[0], world_center);
    float4 shadowCoords1 = mul (unity_WorldToShadow[1], world_center);
    float4 shadowCoords2 = mul (unity_WorldToShadow[2], world_center);
    float4 shadowCoords3 = mul (unity_WorldToShadow[3], world_center);
 
    // Find which cascaded shadow coords to use based on our distance to the camera
    float dist = distance (world_center.xyz, _WorldSpaceCameraPos.xyz);
    float4 zNear = dist >= _LightSplitsNear;
    float4 zFar = dist < _LightSplitsFar;
    float4 weights = zNear * zFar;
    float4 shadowCoords = shadowCoords0 * weights.x + shadowCoords1 * weights.y + shadowCoords2 * weights.z + shadowCoords3 * weights.w;
 
	// Sample the shadowmap
	float shadow = UNITY_SAMPLE_SHADOW (_DirectionalShadowmap, shadowCoords);
	shadow = saturate(shadow + _ShadowSoftness * _DayNightTime);

	return max(directionalLightValue * shadow, _DarkestValue * _DayNightTime);
}