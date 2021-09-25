#include "UnityCG.cginc"
#include "AutoLight.cginc"
#include "Lighting.cginc"

sampler2D _LightTexture0;
float4x4 unity_WorldToLight;

UNITY_DECLARE_SHADOWMAP(_DirectionalShadowmap);
float _ShadowSoftness;

fixed LightCalculation(float3 center, float3 normal)
{
	float2 lightUVCookie = mul(unity_WorldToLight, float4(center, 1)).xy;
	float lightMap = tex2Dlod(_LightTexture0, float4(lightUVCookie,0,0)).w;
	float3 worldNormal = UnityObjectToWorldNormal(normal);
	float directionalLightValue = saturate(max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz)) * lightMap) * _LightColor0.r;

	// Support cascaded shadows
	float4 shadowCoords0 = mul (unity_WorldToShadow[0], float4 (center, 1));
	float4 shadowCoords1 = mul (unity_WorldToShadow[1], float4 (center, 1));
	float4 shadowCoords2 = mul (unity_WorldToShadow[2], float4 (center, 1));
	float4 shadowCoords3 = mul (unity_WorldToShadow[3], float4 (center, 1));
 
	// Find which cascaded shadow coords to use based on our distance to the camera
	float dist = distance (center, _WorldSpaceCameraPos.xyz);
	float4 zNear = dist >= _LightSplitsNear;
	float4 zFar = dist < _LightSplitsFar;
	float4 weights = zNear * zFar;
	float4 shadowCoords = shadowCoords0 * weights.x + shadowCoords1 * weights.y + shadowCoords2 * weights.z + shadowCoords3 * weights.w;
 
	// Sample the shadowmap
	float shadow = UNITY_SAMPLE_SHADOW (_DirectionalShadowmap, shadowCoords);
	shadow = saturate(shadow + _ShadowSoftness);

	return directionalLightValue * shadow;
}