float yScale;

float pixelsPerUnit;
float pixelsPerUnit3;
float unitsPerPixelWorld;
float unitsPerPixelWorld3;

float2 renderResolutionExtended;
float2 renderResolution;

// Snaps given clip pos to grid
float4 ClipSnap(float4 clipPos)
{
    //clipPos goes from -1 to 1 so we need to half renderResolutionExtended
    float2 renderResolutionExtendedHalf = renderResolutionExtended * 0.5;

    // however for some reason z value goes from 0 to 1?!?!? so dont do that for z
    // we need to however still snap z to unitsPerPixelWorld (z is between 0 and 1 right now)
    float clipZToWorld = _ProjectionParams.z - _ProjectionParams.y; // far - near plane
    float z = clipPos.z * clipZToWorld;

    return float4(
        round(clipPos.x * renderResolutionExtendedHalf.x) / renderResolutionExtendedHalf.x,
        round(clipPos.y * renderResolutionExtendedHalf.y) / renderResolutionExtendedHalf.y,
        (round(z * pixelsPerUnit3) / (pixelsPerUnit3)) / clipZToWorld,
        clipPos.w
    );
}

// Snaps given normalized device coordinates (ndc) to grid
float4 NDCSnap(float4 ndcCoords)
{
    // NDC goes from -1 to 1 so we need to snap to half the resolution
    float2 renderResolutionExtendedHalf = renderResolutionExtended * 0.5;

    // we need to snap z to unitsPerPixelWorld
    float ndcZToWorld = (_ProjectionParams.z - _ProjectionParams.y) * 0.5; // far - near plane
    float z = ndcCoords.z * ndcZToWorld;

    return float4(
        round(ndcCoords.x * renderResolutionExtendedHalf.x) / renderResolutionExtendedHalf.x,
        round(ndcCoords.y * renderResolutionExtendedHalf.y) / renderResolutionExtendedHalf.y,
        round(z * unitsPerPixelWorld) / unitsPerPixelWorld,
        ndcCoords.w
    );
}

// Rounds given Vector3 position to pixel grid.
float3 SnapToGrid(float3 position)
{
    return float3
    (
        round(position.x / (unitsPerPixelWorld * 3.0)) * (unitsPerPixelWorld * 3.0),
        round(position.y / (unitsPerPixelWorld * 3.0 * yScale)) * (unitsPerPixelWorld * 3.0 * yScale),
        round(position.z / (unitsPerPixelWorld * 3.0)) * (unitsPerPixelWorld * 3.0)
    );
}

// Rounds given Vector3 position to pixel grid.
float4 SnapToGrid(float4 position)
{
    return float4
    (
        round(position.x / (unitsPerPixelWorld * 3.0)) * (unitsPerPixelWorld * 3.0),
        round(position.y / (unitsPerPixelWorld * 3.0 * yScale)) * (unitsPerPixelWorld * 3.0 * yScale),
        round(position.z / (unitsPerPixelWorld * 3.0)) * (unitsPerPixelWorld * 3.0),
        1.0
    );
}