// Transforms vertex from object space to clip space and snaps it
float4 GetVertexClipSnapped(float4 vertex)
{
	// remove T (translation) from TRS (translation, rotation, scalar) matrix by setting objectOriginWorld.w to zero
	// get object center in world
	float4 objectOriginWorld = float4(unity_ObjectToWorld._m03_m13_m23, 0.0);	
	// then go from world to clip using UNITY_MATRIX_VP, and since objectOriginWorld.w we ignore translation :)
	float4 objectOriginClip = mul(UNITY_MATRIX_VP, objectOriginWorld);

	// then snap objectOriginClip
	float4 objectOriginProjectionSnapped = ClipSnap(objectOriginClip);
	// and use snapped and non-snapped object origin to get vertexClipSnappedDiff
	float4 vertexClipSnappedDiff = float4(objectOriginProjectionSnapped.xyz - objectOriginClip.xyz, 1.0);
	
	// get vertex clip position in one swoop using UnityObjectToClipPos (UNITY_MATRIX_VP is at its original transformation)
	float4 vertexClip = UnityObjectToClipPos(vertex);
	// this difference is added to vertexClip to get vertexClipSnapped
	float4 vertexClipSnapped = float4(vertexClip.xyz + vertexClipSnappedDiff, 1.0);
	return vertexClipSnapped;
}

// Transforms vertex from clip space to world
float4 GetVertexWorldSnapped(float4 vertexClipSnapped)
{
	// go from clip to world
	float4 vertexWorldSnapped = mul(UNITY_MATRIX_I_VP, vertexClipSnapped);
	return vertexWorldSnapped;
}

// Transforms vertex from world space to object
float4 GetVertexObjectSnapped(float4 vertexWorldSnapped)
{
	// and lastly back to object
	float4 vertexObjectSnapped = mul(unity_WorldToObject, vertexWorldSnapped);
	return vertexObjectSnapped;
}

/// Shadow

// 