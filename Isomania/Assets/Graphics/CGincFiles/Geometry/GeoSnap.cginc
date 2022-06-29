#include "/Assets/Graphics/CGincFiles/Geometry/GeoSetup.cginc"

struct v2f
{
	float4 pos : SV_POSITION;
	float3 worldNormal : NORMAL;
	float2 uv : TEXCOORD0;
	float3 worldPosition : TEXCOORD1;
	float4 screenPosition : TEXCOORD3;
	//float3 viewDir : TEXCOORD1;	
	// Macro found in Autolight.cginc. Declares a vector4
	// into the TEXCOORD2 semantic with varying precision 
	// depending on platform target.
	SHADOW_COORDS(2)
};

v2f vert (appdata v)
{
	/*
	// get object world center
	float3 objectOrigin = unity_ObjectToWorld._m03_m13_m23;
	// translate world to ndc
	float3 objectOriginNDC = mul( Camera_ForwardMatrix, float4(objectOrigin, 1.0));
	// snap object origin to grid
	float3 objectInViewSpaceGridOrigin = SnapToGrid(objectInViewSpaceOrigin);
	// get grid delta
	float3 gridDelta = objectInViewSpaceGridOrigin - objectInViewSpaceOrigin;
	// translate that grid delta back to world space
	float4 gridDeltaWorldSpace = mul(Camera_ForwardMatrixInverse, float4(gridDelta, 1.0));
	// then to object
	float4 gridDeltaObjectSpace = mul(gridDeltaWorldSpace, unity_WorldToObject);
	*/
	// translate from object space to view space / ndc space
	// that ndc space is from -1,-1,-1 to 1,1,1 so conver it to camera pixel grid by multiplying by renderresolution
	// and divide by that to round to that grid, OBS! need to account for far and near plane
	// get difference
	// now convert it back to object space and add difference to vertice



	v2f o;
	/*
	// get object center
	float3 objectOrigin = unity_ObjectToWorld._m03_m13_m23;
	// rotate object center to face camera
	float3 objectInViewSpaceOrigin = mul(Camera_ForwardMatrix, float4(objectOrigin, 1.0));
	// snap object origin to grid
	float3 objectInViewSpaceGridOrigin = SnapToGrid(objectInViewSpaceOrigin);
	// get grid delta
	float3 gridDelta = objectInViewSpaceGridOrigin - objectInViewSpaceOrigin;
	// translate that grid delta back to world space
	float4 gridDeltaWorldSpace = mul(Camera_ForwardMatrixInverse, float4(gridDelta, 1.0));
	// then to object
	float4 gridDeltaObjectSpace = mul(gridDeltaWorldSpace, unity_WorldToObject);
	// offset all positions by that delta
	v.vertex.xyz += gridDeltaObjectSpace.xyz;
	*/
	// object space is scale 1, scaling applies later, so we have to account for that scaling on gridDelta

	// get object center in world space
	float3 objectOriginWorld = unity_ObjectToWorld._m03_m13_m23;
	// transform world center to clip space
	float4 objectOriginClip = mul(UNITY_MATRIX_VP, float4(objectOriginWorld, 1.0));
	// now snap that to grid
	float4 clipSnap = ClipSnap(objectOriginClip);
	// get the position difference so that we can offset it later
	float4 clipSnapDiff = float4(objectOriginClip.xyz - clipSnap.xyz, 1.0);
	// transfer clip to world
	float4 worldSnapDiff = mul(unity_CameraInvProjection, clipSnapDiff);
	// transfer world to object	
	float4 objectSnapDiff = mul(unity_WorldToObject, worldSnapDiff);
	// offset
	//v.vertex -= objectSnapDiff;

	o.pos = UnityObjectToClipPos(v.vertex);
	o.pos.xyz -= clipSnapDiff.xyz;
	o.worldNormal = UnityObjectToWorldNormal(v.normal);
	o.worldPosition = mul(unity_ObjectToWorld, v.vertex);
	o.screenPosition = ComputeScreenPos(o.pos);
	//o.viewDir = WorldSpaceViewDir(v.vertex);
	o.uv = TRANSFORM_TEX(v.uv, _MainTex);
	// Defined in Autolight.cginc. Assigns the above shadow coordinate
	// by transforming the vertex from world space to shadow-map space.
	TRANSFER_SHADOW(o)

	//o.worldPosition = objectOriginClip.xyz;

	return o;
}
