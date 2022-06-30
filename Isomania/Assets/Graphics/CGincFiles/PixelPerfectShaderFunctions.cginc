float yScale;

float pixelsPerUnit;
float unitsPerPixelWorld;

float2 renderResolutionExtended;
float2 renderResolution;

// snaps given clip pos to grid
float3 ClipSnap(float3 clipPos)
{
    // note that clipPos goes from -1 to 1 so we transfer it to go from 01
    float2 clipPos01 = (clipPos.xy + 1.0) * 0.5;
    // get the rounded clipXY (to snap to camera grid) 
    float2 rounded = round(renderResolutionExtended * clipPos01) / renderResolutionExtended;

    // offset by half a pixel
    float2 offset = 0.5 / renderResolutionExtended;

    // get the new clippos and remap to -1 to 1
    float2 newClipPos = (rounded + offset) * 2.0 - 1.0;

    // create float4 clippos and return it
    return float3(
        newClipPos.xy,
        clipPos.z
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
        round(worldPos.x / (unitsPerPixelWorld * 3.0)) * (unitsPerPixelWorld * 3.0),
        round(worldPos.y / (unitsPerPixelWorld * 3.0 * yScale)) * (unitsPerPixelWorld * 3.0 * yScale),
        round(worldPos.z / (unitsPerPixelWorld * 3.0)) * (unitsPerPixelWorld * 3.0)
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