struct appdata
{
	float4 vertex : POSITION;				
	float4 uv : TEXCOORD0;
	float3 normal : NORMAL;
	float3 worldPosition : TEXCOORD1;
	float4 screenPosition : TEXCOORD3;
};

// For Geometry
struct v2f
{
	float4 pos : SV_POSITION;
	float3 worldNormal : NORMAL;
	float2 uv : TEXCOORD0;
	float3 worldPosition : TEXCOORD1;
	float4 screenPosition : TEXCOORD2;
	// Macro found in Autolight.cginc. Declares a vector4
	// into the TEXCOORD3 semantic with varying precision 
	// depending on platform target.
	SHADOW_COORDS(3)
};

// For Shadows
struct v2f_shadow
{
	V2F_SHADOW_CASTER;
};

// For Normal Replacement Shader
struct v2f_normal
{
	float4 pos : SV_POSITION;
	float3 viewNormal : NORMAL;
};