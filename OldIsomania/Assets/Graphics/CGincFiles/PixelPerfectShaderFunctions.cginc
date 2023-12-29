float yScale;

float pixelsPerUnit;
float unitsPerPixelWorld;

float2 renderResolutionExtended;
float2 renderResolution;

// snaps given clip pos to grid
float3 ClipSnap(float3 clipPos)
{
    // note that clipPos goes from -1 to 1 so we half renderResolutionExtended
    float2 renderResolutionExtendedHalf = renderResolutionExtended * 0.5;
    // get the rounded clipXY (to snap to camera grid) 
    float2 rounded = round(renderResolutionExtendedHalf * clipPos.xy) / renderResolutionExtendedHalf;

    // now snap to camera z, by calculating the z world length
    float zClipLength = (_ProjectionParams.z - _ProjectionParams.y);
    float z = zClipLength * clipPos.z;
    // because camera is already snapped in its local z space in world, we can snap clippos to z world grid
    float roundedZ = round(z * pixelsPerUnit) / pixelsPerUnit;
    // then transform back to clip
    float roundedZClip = roundedZ / zClipLength;

    // create float4 clippos and return it
    return float3(
        rounded.xy,
        roundedZClip
    );
}
float4 ClipSnap(float4 clipPos)
{
    return float4(
        ClipSnap(clipPos.xyz),
        clipPos.w
    );
}

// snaps given normalized device coordinates (ndc) to grid
float3 NDCSnap(float3 ndcCoords)
{
    // NDC goes from -1 to 1 so we need to snap to half the resolution
    float2 renderResolutionExtendedHalf = renderResolutionExtended * 0.5;

    return float3(
        round(ndcCoords.x * renderResolutionExtendedHalf.x) / renderResolutionExtendedHalf.x,
        round(ndcCoords.y * renderResolutionExtendedHalf.y) / renderResolutionExtendedHalf.y,
        ndcCoords.z
    );
}
float4 NDCSnap(float4 ndcCoords)
{
    return float4(
        NDCSnap(ndcCoords.xyz),
        ndcCoords.w
    );
}

// snaps world coordinate to grid
float3 WorldSnap(float3 worldPos)
{
    return float3
    (
        round(worldPos.x / (unitsPerPixelWorld)) * (unitsPerPixelWorld),
        round(worldPos.y / (unitsPerPixelWorld * yScale)) * (unitsPerPixelWorld * yScale),
        round(worldPos.z / (unitsPerPixelWorld)) * (unitsPerPixelWorld)
    );
}
float4 WorldSnap(float4 worldPos)
{
    return float4
    (
        WorldSnap(worldPos.xyz),
        worldPos.w
    );
}