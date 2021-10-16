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
	float4 center = (IN[0].vertex + IN[1].vertex + IN[2].vertex) / 3.0;

	fixed lightValue = LightCalculation(center, flatNormal);

	g2f idealVertexOutput = VertexOutput(center, lightValue);

	// Only change SV_POSITION and append to outStream
	idealVertexOutput.pos = UnityObjectToClipPos(IN[0].vertex);
	outStream.Append(idealVertexOutput);
	idealVertexOutput.pos = UnityObjectToClipPos(IN[1].vertex);
	outStream.Append(idealVertexOutput);
	idealVertexOutput.pos = UnityObjectToClipPos(IN[2].vertex);
	outStream.Append(idealVertexOutput);
}
