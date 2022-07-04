#include "/Assets/Graphics/CGincFiles/Geometry/Geo.cginc"
#include "/Assets/Graphics/CGincFiles/Geometry/SnapSetup.cginc"

v2f vertSnap (appdata v)
{
	if (unity_OrthoParams.w == 0.0)
	{
		return vert(v);
	}

	// create v2f
	v2f o;

	float4 vertexClipSnapped = GetVertexClipSnapped(v.vertex);
	float4 vertexWorldSnapped = GetVertexWorldSnapped(vertexClipSnapped);
	float4 vertexObjectSnapped = GetVertexObjectSnapped(vertexWorldSnapped);

	// and output to v.vertex, o.pos and o.worldPosition respectivly
	v.vertex = vertexObjectSnapped;
	o.worldPosition = vertexWorldSnapped;
	o.pos = vertexClipSnapped;

	// when you know how to snap rotation, worldnormal need to be accounted for! (https://forum.unity.com/threads/cancel-object-inspector-rotation-from-shader-but-keep-movement.758972/)

	o.worldNormal = UnityObjectToWorldNormal(v.normal);
	o.screenPosition = ComputeScreenPos(o.pos);
	o.uv = TRANSFORM_TEX(v.uv, _MainTex);

	// Defined in Autolight.cginc. Assigns the above shadow coordinate
	// by transforming the vertex from world space to shadow-map space.
	TRANSFER_SHADOW(o)

	//o.worldPosition = objectOriginWorld.xyz;

	return o;
}

v2f_shadow vertShadowSnap (appdata v)
{
	if (unity_OrthoParams.w == 0.0)
	{
		return vertShadow(v);
	}

	// create v2f_shadow
	v2f_shadow o;

	float4 vertexClipSnapped = GetVertexClipSnapped(v.vertex);
	float4 vertexWorldSnapped = GetVertexWorldSnapped(vertexClipSnapped);
	float4 vertexObjectSnapped = GetVertexObjectSnapped(vertexWorldSnapped);

	// and output to v.vertex
	v.vertex = vertexObjectSnapped;
	o.pos = vertexClipSnapped;

	// Defined in Autolight.cginc.
	TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)

	return o;
}

v2f_normal vertNormalSnap (appdata v)
{
	if (unity_OrthoParams.w == 0.0)
	{
		return vertNormal(v);
	}

	// create v2f_normal
	v2f_normal o;

	float4 vertexClipSnapped = GetVertexClipSnapped(v.vertex);
	float4 vertexWorldSnapped = GetVertexWorldSnapped(vertexClipSnapped);
	float4 vertexObjectSnapped = GetVertexObjectSnapped(vertexWorldSnapped);

	// and output to v.vertex
	v.vertex = vertexObjectSnapped;
	o.pos = vertexClipSnapped;

	o.viewNormal = COMPUTE_VIEW_NORMAL;

	// Defined in Autolight.cginc.
	TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)

	return o;
}
