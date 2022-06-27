#include "/Assets/Graphics/CGincFiles/Geometry/GeoSetup.cginc"
#include "/Assets/Graphics/CGincFiles/Geometry/GeoFlatSetup.cginc"

[maxvertexcount(3)]
void geo(triangle v2f IN[3], inout TriangleStream<v2f> outStream)
{
	float3 worldNormal = normalize(cross(IN[1].worldPosition - IN[0].worldPosition, IN[2].worldPosition - IN[0].worldPosition));
	float3 worldCenter = (IN[0].worldPosition + IN[1].worldPosition + IN[2].worldPosition) / 3.0;

	float2 toonUV = CalculateToonUVFlat(worldCenter, worldNormal);

	flattifyVert(IN[0], worldCenter, worldNormal, toonUV);
	outStream.Append(IN[0]);
	flattifyVert(IN[1], worldCenter, worldNormal, toonUV);
	outStream.Append(IN[1]);
	flattifyVert(IN[2], worldCenter, worldNormal, toonUV);
	outStream.Append(IN[2]);
}
