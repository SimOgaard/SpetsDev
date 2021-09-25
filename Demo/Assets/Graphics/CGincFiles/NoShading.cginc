#include "UnityCG.cginc"

struct appdata
{
    float2 uv : TEXCOORD0;
	float4 vertex : POSITION;
	float3 worldPos : TEXCOORD2;
};

struct v2f
{
	float2 uv : TEXCOORD0;
	float4 pos : SV_POSITION;
	float3 worldPos : TEXCOORD2;
};

v2f vert(appdata v)
{
	v2f o;
	o.pos = UnityObjectToClipPos(v.vertex);
	o.worldPos = mul (unity_ObjectToWorld, v.vertex);
	o.uv = v.uv;
	return o;
}