#include "/Assets/Graphics/CGincFiles/Geometry/GeoSetup.cginc"

struct v2f
{
    float4 pos : SV_POSITION;
	float3 viewNormal : NORMAL;
};

v2f vert (appdata v)
{
    v2f o;
    o.pos = UnityObjectToClipPos(v.vertex);
    o.viewNormal = COMPUTE_VIEW_NORMAL;
    return o;
}