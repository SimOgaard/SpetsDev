#include "UnityCG.cginc"
#include "AutoLight.cginc"
#include "Lighting.cginc"

sampler2D _LightTexture0;
float4x4 unity_WorldToLight;

float _ShadowSoftness;
float _Ambient;
float _Darkest;

float _LightColorValue;

struct v2f
{
	float2 uv : TEXCOORD0;
	SHADOW_COORDS(1)
	float light_val : COLOR0;
	float4 pos : SV_POSITION;
	float3 worldPos : TEXCOORD2;
};

v2f vert(appdata_base v)
{
	v2f o;
	o.pos = UnityObjectToClipPos(v.vertex);
	o.worldPos = mul (unity_ObjectToWorld, v.vertex);
	o.uv = v.texcoord;
	float3 worldNormal = UnityObjectToWorldNormal(v.normal);
	o.light_val = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
	TRANSFER_SHADOW(o)
	return o;
}

float CalculateLight(v2f i)
{
	float shadow = SHADOW_ATTENUATION(i);
	shadow = saturate(shadow + _ShadowSoftness);

	float2 uvCookie = mul(unity_WorldToLight, float4(i.worldPos, 1)).xy;
	float attenuation = tex2D(_LightTexture0, uvCookie).w;

	return max(saturate(i.light_val * shadow * attenuation + _Ambient), _Darkest);
}