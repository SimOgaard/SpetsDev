#include "/Assets/Graphics/CGincFiles/ToonShading/ToonShadingSetup.cginc"

struct appdata
{
	float4 vertex : POSITION;				
	//float4 uv : TEXCOORD0;
	float3 normal : NORMAL;
};

struct v2f
{
	float4 pos : SV_POSITION;
	float3 worldNormal : NORMAL;
	//float2 uv : TEXCOORD0;
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
	//o.viewDir = WorldSpaceViewDir(v.vertex);
	//o.uv = TRANSFORM_TEX(v.uv, _MainTex);
	// Defined in Autolight.cginc. Assigns the above shadow coordinate
	// by transforming the vertex from world space to shadow-map space.
	TRANSFER_SHADOW(o)
	return o;
}

float4 CalculateLight(v2f i)
{
	// Get normal
	float3 normal = normalize(i.worldNormal);
	// Samples the shadow map, returning a value in the 0...1 range,
	// where 0 is in the shadow, and 1 is not.
	float shadow = SHADOW_ATTENUATION(i);

	// Do the same light calculation that is for all toon shaders
	return CalculateLightPrivate(normal, shadow);
}