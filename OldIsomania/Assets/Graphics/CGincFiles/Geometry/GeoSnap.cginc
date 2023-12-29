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

	// get vertex object snapped difference
	float4 m_vertexClipSnappedDiff = MainCameraClipSnappedDifference();
	float4 vertexWorldSnappedDiff = SnappedDifferenceWorld(m_vertexClipSnappedDiff);
	float4 vertexObjectSnappedDiff = SnappedDifferenceObject(vertexWorldSnappedDiff);

	// snap current vertex
	v.vertex += vertexObjectSnappedDiff;

	// get vertex world using now clipped vertex
	o.worldPosition = mul(unity_ObjectToWorld, v.vertex);
	// get vertex clip
	o.pos = UnityObjectToClipPos(v.vertex);

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

	// get vertex object snapped difference
	float4 m_vertexClipSnappedDiff = MainCameraClipSnappedDifference();
	float4 vertexWorldSnappedDiff = SnappedDifferenceWorld(m_vertexClipSnappedDiff);
	float4 vertexObjectSnappedDiff = SnappedDifferenceObject(vertexWorldSnappedDiff);

	// snap current vertex
	v.vertex += vertexObjectSnappedDiff;

	// get vertex clip
	o.pos = UnityObjectToClipPos(v.vertex);

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
	
	// get vertex object snapped difference
	float4 m_vertexClipSnappedDiff = MainCameraClipSnappedDifference();
	float4 vertexWorldSnappedDiff = SnappedDifferenceWorld(m_vertexClipSnappedDiff);
	float4 vertexObjectSnappedDiff = SnappedDifferenceObject(vertexWorldSnappedDiff);

	// snap current vertex
	v.vertex += vertexObjectSnappedDiff;

	// get vertex clip
	o.pos = UnityObjectToClipPos(v.vertex);

	o.viewNormal = COMPUTE_VIEW_NORMAL;

	// Defined in Autolight.cginc.
	TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)

	return o;
}