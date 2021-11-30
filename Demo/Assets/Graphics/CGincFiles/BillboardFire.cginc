#include "/Assets/Graphics/CGincFiles/BillboardSetup.cginc"

float _Size;
float _YDisplacement;
float _XZDisplacementRandom;

struct g2f
{
	float4 pos : SV_POSITION;
	float3 worldPos : TEXCOORD0;
	float2 uv : TEXCOORD1;
};

g2f VertexOutput(float3 pos, float2 uv, float3 center)
{
	g2f o;
	o.pos = UnityObjectToClipPos(pos);
	o.uv = uv;
	o.worldPos = center;
	return o;
}

float rand(float3 co)
{
	return sin(dot(co.xyz, float3(12.9898, 78.233, 53.539)));
}

[maxvertexcount(3)]
void geo(triangle vertexOutput IN[3], inout TriangleStream<g2f> outStream)
{
	float3 flatNormal = normalize(cross(IN[1].vertex - IN[0].vertex, IN[2].vertex - IN[0].vertex));
	float4 center = (IN[0].vertex + IN[1].vertex + IN[2].vertex) / 3.0;
	float3 world_center = mul(unity_ObjectToWorld, center).xyz;

	float x_displacement = rand(world_center.xyz) * _XZDisplacementRandom;
	float y_displacement = _YDisplacement;
	float z_displacement = rand(world_center.xzy) * _XZDisplacementRandom;
	world_center += float3(x_displacement, y_displacement, z_displacement);

	center = mul(unity_WorldToObject, float4(world_center,1));

	float4 vectors[3];
	Get3VectorsUp(center, _Size, vectors);

	outStream.Append(VertexOutput(vectors[0], float2(1, 0), center));
	outStream.Append(VertexOutput(vectors[1], float2(1, 1), center));
	outStream.Append(VertexOutput(vectors[2], float2(0, 0), center));
}
