#include "/Assets/Graphics/CGincFiles/BillboardSetup.cginc"

float _TilePixelSize;

struct g2f
{
	float4 pos : SV_POSITION;
	float light : TEXCOORD0;
};

g2f VertexOutput(float3 pos, float directionalLightValue)
{
	g2f o;
	o.pos = UnityObjectToClipPos(pos);
	o.light = directionalLightValue;
	return o;
}

[maxvertexcount(3)]
void geo(triangle vertexOutput IN[3], inout TriangleStream<g2f> outStream)
{
	float3 flatNormal = normalize(cross(IN[1].vertex - IN[0].vertex, IN[2].vertex - IN[0].vertex));
	float3 center = (IN[0].vertex + IN[1].vertex + IN[1].vertex) / 3;

	fixed lightValue = LightCalculation(center, flatNormal);

	fixed pixelSize = _TilePixelSize / (5.4 * 2);
	float4 vectors[3];
	Get3Vectors(center, pixelSize, out vectors)

	g2f idealVertexOutput = VertexOutput(center, lightValue);

	// Only change SV_POSITION and append to outStream
	idealVertexOutput.pos = UnityObjectToClipPos(vectors[0]);
	outStream.Append(idealVertexOutput);
	idealVertexOutput.pos = UnityObjectToClipPos(vectors[1]);
	outStream.Append(idealVertexOutput);
	idealVertexOutput.pos = UnityObjectToClipPos(vectors[2]);
	outStream.Append(idealVertexOutput);
}
