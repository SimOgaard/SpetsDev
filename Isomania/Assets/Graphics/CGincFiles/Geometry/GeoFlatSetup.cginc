struct appdata
{
	float4 vertex : POSITION;				
	float4 uv : TEXCOORD0;
	float3 normal : NORMAL;
	float3 worldPosition : TEXCOORD1;
	float4 screenPosition : TEXCOORD3;
};

struct v2f
{
	float4 pos : SV_POSITION;
	float3 worldNormal : NORMAL;
	float2 uv : TEXCOORD0;
	float3 worldPosition : TEXCOORD1;
	float3 worldCenter : TEXCOORD4;
	float4 screenPosition : TEXCOORD3;
	float2 toonUV : TEXCOORD2;
};

v2f vert (appdata v)
{
	v2f o;
	o.pos = UnityObjectToClipPos(v.vertex);
	o.worldNormal = UnityObjectToWorldNormal(v.normal);
	o.uv = TRANSFORM_TEX(v.uv, _MainTex);
	o.worldPosition = mul(unity_ObjectToWorld, v.vertex);
	o.worldCenter = 0; // we only have 1 vert in this constructor so we have to fill this value later on
	o.screenPosition = ComputeScreenPos(o.pos);
	o.toonUV = 0; // we only have 1 vert in this constructor so we have to fill this value later on
	return o;
}

void flattifyVert(inout v2f i, float3 worldCenter, float3 flatNormal, float2 toonUV)
{
	i.worldCenter = worldCenter;
	i.worldNormal = flatNormal;
	i.toonUV = toonUV;
}
