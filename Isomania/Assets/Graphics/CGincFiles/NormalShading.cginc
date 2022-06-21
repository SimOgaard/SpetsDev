#include "UnityCG.cginc"
#include "AutoLight.cginc"
#include "Lighting.cginc"

sampler2D _LightTexture0;
float4x4 unity_WorldToLight;

float _LightColorValue; // what
float4 _AmbientColor;

struct v2f
{
	float2 uv : TEXCOORD0;
	SHADOW_COORDS(1)
	float directionalLightValue : COLOR0;
	float4 pos : SV_POSITION;
	float3 worldPos : TEXCOORD2;
};

v2f vert(appdata_base v)
{
	v2f o;
	o.pos = UnityObjectToClipPos(v.vertex);
	o.worldPos = mul(unity_ObjectToWorld, v.vertex);
	o.uv = v.texcoord;
	float3 worldNormal = UnityObjectToWorldNormal(v.normal);
	o.directionalLightValue = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
	TRANSFER_SHADOW(o)
	return o;
}

float4 CalculateLight(v2f i)
{
	float shadow = SHADOW_ATTENUATION(i);

	float2 uvCookie = mul(unity_WorldToLight, float4(i.worldPos, 1)).xy;
	float attenuation = tex2D(_LightTexture0, uvCookie).w;

	return i.directionalLightValue * shadow * attenuation;
}