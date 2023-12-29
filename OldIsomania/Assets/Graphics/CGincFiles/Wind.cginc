float3 GetWind(float3 worldPos)
{
	fnl_state warp = fnlCreateState(1337); //_Noise_Seed;
	warp.domain_warp_type = 0; //_Warp_DomainWarpType;
	warp.rotation_type_3d = 2; //_Warp_RotationType3D;
	warp.domain_warp_amp = 2; //_Warp_DomainWarpAmplitude;
	warp.frequency = 0.015; //_Warp_Frequency;

	warp.fractal_type = 4; //_Warp_FractalType;
	warp.octaves = 3; //_Warp_FractalOctaves;
	warp.lacunarity = 0.5; //_Warp_FractalLacunarity;
	warp.gain = 2.0; //_Warp_FractalGain;

	worldPos += _Time[0] * float3(500, 250, 500); // noise scroll

	float3 worldPosWarped = worldPos;
	fnlDomainWarp3D(warp, worldPosWarped.x, worldPosWarped.y, worldPosWarped.z);
	return worldPos - worldPosWarped;
}

float2 WindToUVWindCoords(float3 wind)
{
	return saturate((wind.xz + 1.0) * 0.5);
}

float2 UVWindCoordsToTile(float2 windUV)
{
	return floor(windUV * (_TileAmount - 0.0001));
}
/*
float2 GetWind()
{
	float2 remap_01 = saturate(windSampleStrength * 0.5 + 0.5); // saturate between 01 to keep low/all/high values dependent on windstrength
	float2 windSampleGrid = floor(remap_01 * (_TileAmount - 0.0001)) * (1 / _TileAmount);
	return windSampleGrid;
}
*/