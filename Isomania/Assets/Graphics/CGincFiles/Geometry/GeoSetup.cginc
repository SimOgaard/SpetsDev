struct appdata
{
	float4 vertex : POSITION;				
	float4 uv : TEXCOORD0;
	float3 normal : NORMAL;
	float3 worldPosition : TEXCOORD1;
	float4 screenPosition : TEXCOORD3;
};