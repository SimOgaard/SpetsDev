float yScale;

float3 GetLookVector()
{
	float3 look = mul((float3x3)unity_CameraToWorld, float3(0,0,-1));
	return normalize(look);
}

float3 GetLookVectorUp()
{
	float3 look = mul(mul((float3x3)unity_CameraToWorld, float3(0,0,-1)), unity_ObjectToWorld);
	look.y = 0;
	return normalize(look);
}

float3 GetRightVector(float3 look)
{
	float3 world_up = float3(0, 1, 0);
	float3 right = normalize(cross(world_up, look));
	return right;
}

void Get3VectorsUp(float3 center, float pixelSize, out float4 vectors[3])
{
	//center = mul(unity_ObjectToWorld, center);

	float3 look = GetLookVectorUp();
	float3 right = GetRightVector(look) * pixelSize;
	float3 up = float3(0, pixelSize * yScale, 0);
	
	vectors[0] = float4(center + right - up, 1.0f);
	vectors[1] = float4(center + up, 1.0f);
	vectors[2] = float4(center - right - up, 1.0f);
}

void Get4VectorsUp(float3 center, float pixelSize, out float4 vectors[4])
{
	//center = mul(unity_ObjectToWorld, center);

	float3 look = GetLookVector();
	float3 right = mul((float3x3)unity_WorldToObject,GetRightVector(look) * pixelSize);
	float3 up = mul((float3x3)unity_WorldToObject,float3(0, pixelSize * yScale, 0));

	vectors[0] = float4(center + right - up, 1.0f);
	vectors[1] = float4(center + right + up, 1.0f);
	vectors[2] = float4(center - right - up, 1.0f);
	vectors[3] = float4(center - right + up, 1.0f);
}

void Get3Vectors(float3 center, float pixelSize, out float4 vectors[3])
{
	//center = mul(unity_ObjectToWorld, center);

	float3 look = GetLookVector();
	float3 right = GetRightVector(look) * pixelSize;
	float3 up = float3(0, pixelSize, 0);
	
	vectors[0] = float4(center + right - up, 1.0f);
	vectors[1] = float4(center + up, 1.0f);
	vectors[2] = float4(center - right - up, 1.0f);
}

void Get4Vectors(float3 center, float pixelSize, out float4 vectors[4])
{
	//center = mul(unity_ObjectToWorld, center);

	float3 look = GetLookVector();
	float3 right = mul((float3x3)unity_WorldToObject,GetRightVector(look) * pixelSize);
	float3 up = mul((float3x3)unity_WorldToObject,float3(0, pixelSize, 0));

	vectors[0] = float4(center + right - up, 1.0f);
	vectors[1] = float4(center + right + up, 1.0f);
	vectors[2] = float4(center - right - up, 1.0f);
	vectors[3] = float4(center - right + up, 1.0f);
}