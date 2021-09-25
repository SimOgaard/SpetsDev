#include "/Assets/Graphics/CGincFiles/BillboardSetup.cginc"

float _Size;

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

[maxvertexcount(3)]
void geo(triangle vertexOutput IN[3], inout TriangleStream<g2f> outStream)
{
	float3 flatNormal = normalize(cross(IN[1].vertex - IN[0].vertex, IN[2].vertex - IN[0].vertex));
	float3 center = (IN[0].vertex + IN[1].vertex + IN[1].vertex) / 3;

	float4 vectors[3];
	Get3VectorsUp(center + float3(0, _Size * 1.25, 0), _Size, vectors);

	outStream.Append(VertexOutput(vectors[0], float2(1, 0), center));
	outStream.Append(VertexOutput(vectors[1], float2(1, 1), center));
	outStream.Append(VertexOutput(vectors[2], float2(0, 0), center));
}
