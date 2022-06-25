// Blends two colors using the same algorithm that our shader is using
// to blend with the screen. This is usually called "normal blending",
// and is similar to how software like Photoshop blends two layers.
float4 alphaBlend(float4 top, float4 bottom)
{
	float3 color = (top.rgb * top.a) + (bottom.rgb * (1 - top.a));
	float alpha = top.a + bottom.a * (1 - top.a);

	return float4(color, alpha);
}

float remap01(float v)
{
	return saturate((v + 1) * 0.5);
}

float pixelsPerUnit;
float pixelsPerUnit3;
float unitsPerPixelWorld;