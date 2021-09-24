#define y_scale 1 / cos(3.14159265 / 6)

fixed3 GetLookVector()
{
	float3 look = mul(mul((float3x3)unity_CameraToWorld, float3(0,0,-1)), unity_ObjectToWorld);
	look.y = 0;
	return normalize(look);
}

fixed3 GetRightVector(fixed3 look)
{
	fixed3 world_up = float3(0, 1, 0);
	fixed3 right = normalize(cross(world_up, look));
	return right;
}

void Get3Vectors(float3 center, fixed pixelSize, out float4 vectors[3])
{
	fixed3 look = GetLookVector();
	fixed3 right = GetRightVector(look) * pixelSize;
	fixed3 up = fixed3(0, pixelSize * y_scale, 0);
	
	vectors[0] = float4(center + right - up, 1.0f);
	vectors[1] = float4(center + up, 1.0f);
	vectors[2] = float4(center - right - up, 1.0f);
}

void Get4Vectors(float3 center, fixed pixelSize, out float4 vectors[4])
{
	fixed3 look = GetLookVector();
	fixed3 right = GetRightVector(look) * pixelSize;
	fixed3 up = fixed3(0, pixelSize * y_scale, 0);
	
	vectors[0] = float4(center + right - up, 1.0f);
	vectors[1] = float4(center + right + up, 1.0f);
	vectors[2] = float4(center - right - up, 1.0f);
	vectors[3] = float4(center - right + up, 1.0f);
}