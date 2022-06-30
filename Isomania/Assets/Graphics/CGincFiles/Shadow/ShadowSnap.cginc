#include "/Assets/Graphics/CGincFiles/Geometry/GeoSetup.cginc"

struct v2f
{
	V2F_SHADOW_CASTER;
};

float4x4 inverse(float4x4 m) {
    float n11 = m[0][0], n12 = m[1][0], n13 = m[2][0], n14 = m[3][0];
    float n21 = m[0][1], n22 = m[1][1], n23 = m[2][1], n24 = m[3][1];
    float n31 = m[0][2], n32 = m[1][2], n33 = m[2][2], n34 = m[3][2];
    float n41 = m[0][3], n42 = m[1][3], n43 = m[2][3], n44 = m[3][3];

    float t11 = n23 * n34 * n42 - n24 * n33 * n42 + n24 * n32 * n43 - n22 * n34 * n43 - n23 * n32 * n44 + n22 * n33 * n44;
    float t12 = n14 * n33 * n42 - n13 * n34 * n42 - n14 * n32 * n43 + n12 * n34 * n43 + n13 * n32 * n44 - n12 * n33 * n44;
    float t13 = n13 * n24 * n42 - n14 * n23 * n42 + n14 * n22 * n43 - n12 * n24 * n43 - n13 * n22 * n44 + n12 * n23 * n44;
    float t14 = n14 * n23 * n32 - n13 * n24 * n32 - n14 * n22 * n33 + n12 * n24 * n33 + n13 * n22 * n34 - n12 * n23 * n34;

    float det = n11 * t11 + n21 * t12 + n31 * t13 + n41 * t14;
    float idet = 1.0f / det;

    float4x4 ret;

    ret[0][0] = t11 * idet;
    ret[0][1] = (n24 * n33 * n41 - n23 * n34 * n41 - n24 * n31 * n43 + n21 * n34 * n43 + n23 * n31 * n44 - n21 * n33 * n44) * idet;
    ret[0][2] = (n22 * n34 * n41 - n24 * n32 * n41 + n24 * n31 * n42 - n21 * n34 * n42 - n22 * n31 * n44 + n21 * n32 * n44) * idet;
    ret[0][3] = (n23 * n32 * n41 - n22 * n33 * n41 - n23 * n31 * n42 + n21 * n33 * n42 + n22 * n31 * n43 - n21 * n32 * n43) * idet;

    ret[1][0] = t12 * idet;
    ret[1][1] = (n13 * n34 * n41 - n14 * n33 * n41 + n14 * n31 * n43 - n11 * n34 * n43 - n13 * n31 * n44 + n11 * n33 * n44) * idet;
    ret[1][2] = (n14 * n32 * n41 - n12 * n34 * n41 - n14 * n31 * n42 + n11 * n34 * n42 + n12 * n31 * n44 - n11 * n32 * n44) * idet;
    ret[1][3] = (n12 * n33 * n41 - n13 * n32 * n41 + n13 * n31 * n42 - n11 * n33 * n42 - n12 * n31 * n43 + n11 * n32 * n43) * idet;

    ret[2][0] = t13 * idet;
    ret[2][1] = (n14 * n23 * n41 - n13 * n24 * n41 - n14 * n21 * n43 + n11 * n24 * n43 + n13 * n21 * n44 - n11 * n23 * n44) * idet;
    ret[2][2] = (n12 * n24 * n41 - n14 * n22 * n41 + n14 * n21 * n42 - n11 * n24 * n42 - n12 * n21 * n44 + n11 * n22 * n44) * idet;
    ret[2][3] = (n13 * n22 * n41 - n12 * n23 * n41 - n13 * n21 * n42 + n11 * n23 * n42 + n12 * n21 * n43 - n11 * n22 * n43) * idet;

    ret[3][0] = t14 * idet;
    ret[3][1] = (n13 * n24 * n31 - n14 * n23 * n31 + n14 * n21 * n33 - n11 * n24 * n33 - n13 * n21 * n34 + n11 * n23 * n34) * idet;
    ret[3][2] = (n14 * n22 * n31 - n12 * n24 * n31 - n14 * n21 * n32 + n11 * n24 * n32 + n12 * n21 * n34 - n11 * n22 * n34) * idet;
    ret[3][3] = (n12 * n23 * n31 - n13 * n22 * n31 + n13 * n21 * n32 - n11 * n23 * n32 - n12 * n21 * n33 + n11 * n22 * n33) * idet;

    return ret;
}

v2f vert (appdata v)
{
	if (unity_OrthoParams.w == 0.0)
	{
		v2f o;
		o.pos = UnityObjectToClipPos(v.vertex);
		//o.worldNormal = UnityObjectToWorldNormal(v.normal);
		//o.worldPosition = mul(unity_ObjectToWorld, v.vertex);
		//o.screenPosition = ComputeScreenPos(o.pos);
		//o.viewDir = WorldSpaceViewDir(v.vertex);
		//o.uv = TRANSFORM_TEX(v.uv, _MainTex);
		// Defined in Autolight.cginc. Assigns the above shadow coordinate
		// by transforming the vertex from world space to shadow-map space.
		TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
		return o;
	}

	// create v2f
	v2f o;

	// get object center and vertex pos in world space
	float4 objectOriginWorld = float4(unity_ObjectToWorld._m03_m13_m23, 1.0);
	float4 vertexWorld = mul(unity_ObjectToWorld, v.vertex);

	// go from world to view
	float4 objectOriginView = mul(worldToCameraMatrix, objectOriginWorld);
	float4 vertexView = mul(worldToCameraMatrix, vertexWorld);

	// then view to projection
	float4 objectOriginProjection = mul(projectionMatrix, objectOriginView);
	float4 vertexProjection = mul(projectionMatrix, vertexView);

	// then snap object origin
	float4 objectOriginProjectionSnapped = ClipSnap(objectOriginProjection);
	// and use snapped object origin to get snapped vertexView 
	float4 vertexProjectionSnappedDiff = float4(objectOriginProjectionSnapped.xyz - objectOriginProjection.xyz, 1.0);
	float4 vertexProjectionSnapped = float4(vertexProjection.xyz + vertexProjectionSnappedDiff, 1.0);

	// we only care about vertex so all transformations will be applied to it
	// clip to view
	float4 vertexViewSnapped = mul(inverse(projectionMatrix), vertexProjectionSnapped);
	// then to world
	float4 vertexWorldSnapped = mul(inverse(worldToCameraMatrix), vertexViewSnapped);
	// and lastly back to object
	float4 vertexObjectSnapped = mul(unity_WorldToObject, vertexWorldSnapped);

	// and output to v.vertex, o.pos and o.worldPosition respectivly
	v.vertex = vertexObjectSnapped;
	//o.worldPosition = vertexWorldSnapped;
	//o.pos = vertexProjectionSnapped;

	// when you know how to snap rotation, worldnormal need to be accounted for! (https://forum.unity.com/threads/cancel-object-inspector-rotation-from-shader-but-keep-movement.758972/)

	/*
	// get object center in world space
	float3 objectOriginWorld = unity_ObjectToWorld._m03_m13_m23;
	// transform world center to clip space
	float4 objectOriginClip = mul(UNITY_MATRIX_VP, float4(objectOriginWorld, 1.0));
	// now snap that to grid
	float4 clipSnap = ClipSnap(objectOriginClip);
	// get the position difference so that we can offset it later
	float4 clipSnapDiff = float4(clipSnap.xyz - objectOriginClip.xyz, 1.0);
	// transfer clip to world
	float4 worldSnapDiff = mul(unity_CameraInvProjection, float4(clipSnapDiff.xyz, 1.0));
	// transfer world to object	
	float4 objectSnapDiff = mul(unity_WorldToObject, float4(worldSnapDiff.xyz, 1.0));

	// offset
	o.pos = UnityObjectToClipPos(v.vertex);
	o.pos.xyz += clipSnapDiff.xyz;
	*/

	//o.worldNormal = UnityObjectToWorldNormal(v.normal);
	//o.screenPosition = ComputeScreenPos(o.pos);
	//o.uv = TRANSFORM_TEX(v.uv, _MainTex);

	// Defined in Autolight.cginc. Assigns the above shadow coordinate
	// by transforming the vertex from world space to shadow-map space.
	TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)

	//o.worldPosition = viewSnapped.xyz;

	return o;
}