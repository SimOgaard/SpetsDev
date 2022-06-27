#include "/Assets/Graphics/CGincFiles/Geometry/GeoSetup.cginc"
#include "/Assets/Graphics/CGincFiles/Geometry/GeoFlatSetup.cginc"
#include "/Assets/Graphics/CGincFiles/Geometry/Billboard/BillboardSetup.cginc"

[maxvertexcount(4)]
void geo(triangle v2f IN[3], inout TriangleStream<v2f> outStream)
{
	float3 worldNormal = normalize(cross(IN[1].worldPosition - IN[0].worldPosition, IN[2].worldPosition - IN[0].worldPosition));
	float3 worldCenter = (IN[0].worldPosition + IN[1].worldPosition + IN[2].worldPosition) / 3.0;

	worldCenter += float3(
		rand(worldCenter.xyz) * _BillboardDisplacement.x,
		_BillboardDisplacement.y,
		rand(worldCenter.xzy) * _BillboardDisplacement.z
	);

	float2 toonUV = CalculateToonUVFlat(worldCenter, worldNormal);

	flattifyVert(IN[0], worldCenter, worldNormal, toonUV);
	// IN[0] is now our ideal vertex

	// we need to billboard this mofo
	float pixelSize = _TilePixelSize / (pixelsPerUnit * 2);
	float4 vectors[4];
	Get4VectorsUp(worldCenter, pixelSize, vectors);

	// Only change SV_POSITION and append to outStream
	IN[0].pos = UnityObjectToClipPos(vectors[0]);
	IN[0].uv = float2(1, 0);
	outStream.Append(IN[0]);

	IN[0].pos = UnityObjectToClipPos(vectors[1]);
	IN[0].uv = float2(1, 1);
	outStream.Append(IN[0]);

	IN[0].pos = UnityObjectToClipPos(vectors[2]);
	IN[0].uv = float2(0, 0);
	outStream.Append(IN[0]);

	IN[0].pos = UnityObjectToClipPos(vectors[3]);
	IN[0].uv = float2(0, 1);
	outStream.Append(IN[0]);
}
