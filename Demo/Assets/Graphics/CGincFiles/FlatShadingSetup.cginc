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

	float4 shadowCoords = mul (unity_WorldToShadow[0], float4 (center, 1));
 
	// Sample the shadowmap
	float shadow = UNITY_SAMPLE_SHADOW (_DirectionalShadowmap, shadowCoords);
	shadow = saturate(shadow + _ShadowSoftness);

	return directionalLightValue * shadow;
}