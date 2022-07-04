float4 _AmbientColor;

sampler2D _Colors;
float4 _Colors_ST;

sampler2D _ColorShading;
float4 _ColorShading_ST;

float2 CalculateToonUV(float3 normal, float shadow, float cloudValue)
{
	// Calculate illumination from directional light.
	// _WorldSpaceLightPos0 is a vector pointing the OPPOSITE
	// direction of the main directional light.
	float NdotL = dot(_WorldSpaceLightPos0, normal);

	// Partition the intensity into light and dark.
	float lightIntensity = NdotL * (shadow >= 1.0);
	// Multiply by the main directional light's intensity and color.
	float4 light = lightIntensity * _LightColor0 * cloudValue;
	// Add ambient color
	light += _AmbientColor;

	// Get color shading uv based on light value
	return float2(light.a, 0.0);
}

///
/// Transfer world coordinate to cloud coordinate by using _WorldSpaceLightPos0.
/// OBS! Does not work on cases where _WorldSpaceLightPos0.y > 0
///
float3 CloudUV(float3 worldPosition)
{
	// At what y world coordinate do we sample cloud from?
	float cloudHeight = 500.0;

	// How long do we have to multiply directional light with to get this worldPosition to cloudHeight?
	float distance = (cloudHeight - worldPosition.y) / _WorldSpaceLightPos0.y;

	// Cloud uv that always has the same y value if _WorldSpaceLightPos0.y > 0
	float3 cloudUV = distance * _WorldSpaceLightPos0 + worldPosition;

	// Now offset that cloud uv based on time and global cloud offset
	cloudUV += _Time[0] * float3(200.0, 25.0, 75.0);

	// And snap it to grid
	//cloudUV = round(cloudUV * pixelsPerUnit3) / pixelsPerUnit3;

	// Return cloud uv
	return cloudUV;
}

fnl_state PrivateCloudWarpNoiseState()
{
	fnl_state warp = fnlCreateState(1337); //_Noise_Seed;
	warp.domain_warp_type = 0; //_Warp_DomainWarpType;
	warp.rotation_type_3d = 0; //_Warp_RotationType3D;
	warp.domain_warp_amp = 30; //_Warp_DomainWarpAmplitude;
	warp.frequency = 0.000025; //_Warp_Frequency;

	warp.fractal_type = 0; //_Warp_FractalType;
	warp.octaves = 5; //_Warp_FractalOctaves;
	warp.lacunarity = 2.0; //_Warp_FractalLacunarity;
	warp.gain = 2.0; //_Warp_FractalGain;
	
	return warp;
}

fnl_state PrivateCloudNoiseState()
{
	fnl_state noise = fnlCreateState(1337); //_Noise_Seed;
	noise.frequency = 0.01; //_Noise_Frequency;
	noise.noise_type = 1; //_Noise_NoiseType;
	noise.rotation_type_3d = 2; //_Noise_RotationType3D;

	noise.fractal_type = 1; //_Noise_FractalType;
	noise.octaves = 5; //_Noise_FractalOctaves;
	noise.lacunarity = 2.0; //_Noise_FractalLacunarity;
	noise.gain = 0.5; //_Noise_FractalGain;
	noise.weighted_strength = 0.0; //_Noise_FractalWeightedStrength;
	noise.ping_pong_strength = 2.0; //_Noise_FractalPingPongStrength;

	noise.cellular_distance_func = 1; //_Noise_CellularDistanceFunction;
	noise.cellular_return_type = 1; //_Noise_CellularReturnType;
	noise.cellular_jitter_mod = 1.0; //_Noise_CellularJitter;

	return noise;
}

sampler2D _CloudShading;
float4 _CloudShading_ST;

///
/// Calculates cloud noise value given cloudUV
///
float CloudNoiseValueFromUV(float3 cloudUV)
{
	// Warp state
	fnl_state warp = PrivateCloudWarpNoiseState();
	// Warp the cloud uvs
	fnlDomainWarp3D(warp, cloudUV.x, cloudUV.y, cloudUV.z);

	// Noise state
	fnl_state noise = PrivateCloudNoiseState();
	// use warped cloud uvs to calculate cloud noise value
	float noiseValue = fnlGetNoise3D(noise, cloudUV.x, cloudUV.y, cloudUV.z);

	// remap to 01 and return
	noiseValue = remap01(noiseValue);

	// Sample cloud alpha texture and return
	return noiseValue;
}

///
/// Calculates cloud value given cloud noise
///
float CloudValueFromNoise(float noiseValue)
{
	return tex2D(_CloudShading, float2(noiseValue, 0.0));
}
///
/// Calculates cloud value given cloud noise
///
float CloudValueFromNoiseFlat(float noiseValue)
{
	return tex2Dlod(_CloudShading, float4(noiseValue, 0.0, 0.0, 0.0));	
}

///
/// Calculates cloud value given cloudUV
///
float CloudValueFromUV(float3 cloudUV)
{
	return CloudValueFromNoise(CloudNoiseValueFromUV(cloudUV));
}
///
/// Calculates cloud value given cloudUV
///
float CloudValueFromUVFlat(float3 cloudUV)
{
	return CloudValueFromNoiseFlat(CloudNoiseValueFromUV(cloudUV));
}

///
/// Calculates cloud value given worldPosition
///
float CloudValueFromWorld(float3 worldPosition)
{
	return CloudValueFromUV(CloudUV(worldPosition));
}
///
/// Calculates cloud value given worldPosition
///
float CloudValueFromWorldFlat(float3 worldPosition)
{
	return CloudValueFromUVFlat(CloudUV(worldPosition));
}