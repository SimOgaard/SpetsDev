// returns snapped object center to main camera grid, returns snap difference
float4 MainCameraClipSnappedDifference()
{
	// remove T (translation) from TRS (translation, rotation, scalar) matrix by setting objectOriginWorld.w to zero
	// get object center in world
	float4 objectOriginWorld = float4(unity_ObjectToWorld._m03_m13_m23, 0.0);	
	// then go from world to clip using MAIN_CAMERA_UNITY_MATRIX_VP, and since objectOriginWorld.w we ignore translation :)
	float4 m_objectOriginClip = mul(MAIN_CAMERA_UNITY_MATRIX_VP, objectOriginWorld);

	// then snap objectOriginClip
	float4 m_objectOriginProjectionSnapped = ClipSnap(m_objectOriginClip);
	// and use snapped and non-snapped object origin to get m_vertexClipSnappedDiff
	float4 m_vertexClipSnappedDiff = float4(m_objectOriginProjectionSnapped.xyz - m_objectOriginClip.xyz, 0.0);
	
	return m_vertexClipSnappedDiff;
}

// Transforms main camera vertex clip snapped difference to world
float4 SnappedDifferenceWorld(float4 m_vertexClipSnappedDiff)
{
	// go from clip to world
	float4 vertexWorldSnappedDiff = mul(MAIN_CAMERA_UNITY_MATRIX_I_VP, m_vertexClipSnappedDiff);
	return vertexWorldSnappedDiff;
}

// Transforms vertex world snapped difference to object
float4 SnappedDifferenceObject(float4 vertexWorldSnappedDiff)
{
	// go from world to object
	float4 vertexObjectSnappedDiff = mul(unity_WorldToObject, vertexWorldSnappedDiff);
	return vertexObjectSnappedDiff;
}