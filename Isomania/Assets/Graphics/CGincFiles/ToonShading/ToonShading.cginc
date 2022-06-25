#include "/Assets/Graphics/CGincFiles/ToonShading/ToonShadingSetup.cginc"

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

float2 ToonUV(v2f i)
{
	// Get normal
	float3 normal = normalize(i.worldNormal);
	// Samples the shadow map, returning a value in the 0...1 range,
	// where 0 is in the shadow, and 1 is not.
	float shadow = SHADOW_ATTENUATION(i);

	// Calculate toon uv for shading
	return CalculateToonUV(normal, shadow, CloudValueFromWorld(i.worldPosition));
}

float4 ToonShade(float2 toonUV)
{
	// And extract shade value from texture
	float shade = tex2D(_ColorShading, toonUV); 

	// Now use that shade value to get the right color
	return tex2D(_Colors, float2(shade, 0.0)); 
}

float4 ToonShade(v2f i)
{
	// Calculate toon uv for shading
	float2 toonUV = ToonUV(i);

	// And toon shade
	return ToonShade(toonUV);
}