#include "/Assets/Graphics/CGincFiles/Geo/GeoSetup.cginc"

struct v2f
{
	float4 pos : SV_POSITION;
	float3 worldNormal : NORMAL;
	float2 uv : TEXCOORD0;
	float3 worldPosition : TEXCOORD1;
	float4 screenPosition : TEXCOORD3;
	//float3 viewDir : TEXCOORD1;	
	// Macro found in Autolight.cginc. Declares a vector4
	// into the TEXCOORD2 semantic with varying precision 
	// depending on platform target.
	SHADOW_COORDS(2)
};

v2f vert (appdata v)
{
	v2f o;
	o.pos = UnityObjectToClipPos(v.vertex);
	o.worldNormal = UnityObjectToWorldNormal(v.normal);
	o.worldPosition = mul(unity_ObjectToWorld, v.vertex);
	o.screenPosition = ComputeScreenPos(o.pos);
	//o.viewDir = WorldSpaceViewDir(v.vertex);
	o.uv = TRANSFORM_TEX(v.uv, _MainTex);
	// Defined in Autolight.cginc. Assigns the above shadow coordinate
	// by transforming the vertex from world space to shadow-map space.
	TRANSFER_SHADOW(o)
	return o;
}
