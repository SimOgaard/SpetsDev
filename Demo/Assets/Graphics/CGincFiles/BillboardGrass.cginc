#include "/Assets/Graphics/CGincFiles/BillboardSetup.cginc"
#include "/Assets/Graphics/CGincFiles/WindSetup.cginc"

float _TilePixelSize;
float _YDisplacement;
float _XZDisplacementRandom;

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

float rand(float3 co)
{
	return sin(dot(co.xyz, float3(12.9898, 78.233, 53.539)));
}

[maxvertexcount(4)]
void geo(triangle vertexOutput IN[3], inout TriangleStream<g2f> outStream)
{
	float3 flatNormal = normalize(cross(IN[1].vertex - IN[0].vertex, IN[2].vertex - IN[0].vertex));
	float4 center = (IN[0].vertex + IN[1].vertex + IN[2].vertex) / 3.0;
	float3 world_center = mul(unity_ObjectToWorld, center).xyz;

	float lightValue = LightCalculation(world_center, flatNormal);
	
	float x_displacement = rand(world_center.xyz) * _XZDisplacementRandom;
	float y_displacement = _YDisplacement;
	float z_displacement = rand(world_center.xzy) * _XZDisplacementRandom;
	world_center += float3(x_displacement, y_displacement, z_displacement);

	center = mul(unity_WorldToObject, float4(world_center,1));

	float pixelSize = _TilePixelSize / (pixelsPerUnit * 2);
	float4 vectors[4];
	Get4VectorsUp(center, pixelSize, vectors);

	float2 wind = GetWind(center);

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
