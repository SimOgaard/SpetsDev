#include "/Assets/Graphics/CGincFiles/BillboardSetup.cginc"
#include "/Assets/Graphics/CGincFiles/WindSetup.cginc"

float _TilePixelSize;
float _ExtrudeDistance;
float3 _UniformDisplacementRandom;

struct g2f
{
	float4 pos : SV_POSITION;
	float light : TEXCOORD0;
	float2 wind : TEXCOORD1;
	float2 uv : TEXCOORD2;
};

g2f VertexOutput(float3 pos, float directionalLightValue, float2 wind)
{
	g2f o;
	o.pos = UnityObjectToClipPos(pos);
	o.light = directionalLightValue;
	o.wind = wind;
	return o;
}

fixed rand(float3 co)
{
	return sin(dot(co.xyz, float3(12.9898, 78.233, 53.539)));
}

[maxvertexcount(4)]
void geo(triangle vertexOutput IN[3], inout TriangleStream<g2f> outStream)
{
	float4 flatNormal = float4(normalize(cross(IN[1].vertex - IN[0].vertex, IN[2].vertex - IN[0].vertex)),0);
	float4 center = (IN[0].vertex + IN[1].vertex + IN[2].vertex) / 3.0;

	fixed lightValue = LightCalculation(center, flatNormal);
	
	fixed4 uniform_displacement = fixed4(rand(center), rand(center.yzx), rand(center.xzy), 0) * fixed4(_UniformDisplacementRandom, 0);
	center += uniform_displacement + flatNormal * _ExtrudeDistance;

	fixed pixelSize = _TilePixelSize / (5.4 * 2);
	float4 vectors[4];
	Get4Vectors(center, pixelSize, vectors);

	fixed2 wind = GetWind(center);

	g2f idealVertexOutput = VertexOutput(center, lightValue, wind);

	// Only change SV_POSITION and append to outStream
	idealVertexOutput.pos = UnityObjectToClipPos(vectors[0]);
	idealVertexOutput.uv = float2(1, 0);
	outStream.Append(idealVertexOutput);

	idealVertexOutput.pos = UnityObjectToClipPos(vectors[1]);
	idealVertexOutput.uv = float2(1, 1);
	outStream.Append(idealVertexOutput);

	idealVertexOutput.pos = UnityObjectToClipPos(vectors[2]);
	idealVertexOutput.uv = float2(0, 0);
	outStream.Append(idealVertexOutput);

	idealVertexOutput.pos = UnityObjectToClipPos(vectors[3]);
	idealVertexOutput.uv = float2(0, 1);
	outStream.Append(idealVertexOutput);
}

/*
[maxvertexcount(4)]
void geo(triangle vertexOutput IN[3], inout TriangleStream<g2f> outStream)
{
	float3 flatNormal = normalize(cross(IN[1].vertex - IN[0].vertex, IN[2].vertex - IN[0].vertex));
	float4 center = (IN[0].vertex + IN[1].vertex + IN[2].vertex) / 3.0;
	float3 world_center = mul(unity_ObjectToWorld, center).xyz;

	fixed lightValue = LightCalculation(world_center, flatNormal);
	
	fixed3 uniform_displacement = float3(rand(center), rand(center.yzx), rand(center.xzy)) * _UniformDisplacementRandom;
	world_center += uniform_displacement + flatNormal * _ExtrudeDistance;

	center = mul(unity_WorldToObject, world_center);

	fixed pixelSize = _TilePixelSize / (5.4 * 2);
	float4 vectors[4];
	Get4Vectors(center, pixelSize, vectors);

	fixed2 wind = GetWind(world_center);

	g2f idealVertexOutput = VertexOutput(center, lightValue, wind);

	// Only change SV_POSITION and append to outStream
	idealVertexOutput.pos = UnityObjectToClipPos(vectors[0]);
	idealVertexOutput.uv = float2(1, 0);
	outStream.Append(idealVertexOutput);

	idealVertexOutput.pos = UnityObjectToClipPos(vectors[1]);
	idealVertexOutput.uv = float2(1, 1);
	outStream.Append(idealVertexOutput);

	idealVertexOutput.pos = UnityObjectToClipPos(vectors[2]);
	idealVertexOutput.uv = float2(0, 0);
	outStream.Append(idealVertexOutput);

	idealVertexOutput.pos = UnityObjectToClipPos(vectors[3]);
	idealVertexOutput.uv = float2(0, 1);
	outStream.Append(idealVertexOutput);
}
*/