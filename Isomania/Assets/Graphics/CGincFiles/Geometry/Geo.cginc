#include "/Assets/Graphics/CGincFiles/Geometry/GeoSetup.cginc"

v2f vert(appdata v)
{
	v2f o;
	o.pos = UnityObjectToClipPos(v.vertex);
	o.worldNormal = UnityObjectToWorldNormal(v.normal);
	o.worldPosition = mul(unity_ObjectToWorld, v.vertex);
	o.screenPosition = ComputeScreenPos(o.pos);
	o.uv = TRANSFORM_TEX(v.uv, _MainTex);
	// Defined in Autolight.cginc. Assigns the above shadow coordinate
	// by transforming the vertex from world space to shadow-map space.
	TRANSFER_SHADOW(o)
	return o;
}
 
v2f_shadow vertShadow(appdata v)
{
    v2f_shadow o;
    TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
    return o;
}

v2f_normal vertNormal(appdata v)
{
	v2f_normal o;
	o.pos = UnityObjectToClipPos(v.vertex);
	o.viewNormal = COMPUTE_VIEW_NORMAL;
	return o;
}
