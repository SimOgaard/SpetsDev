sampler2D _WindDistortionMap;
float4 _WindDistortionMap_ST;
float2 _WindFrequency;
float _WindStrength;
float _TileAmount;

float2 GetWind(float3 center)
{
	float2 uv = center.xz * _WindDistortionMap_ST.xy + _WindDistortionMap_ST.zw + _WindFrequency * _Time.y;
	float2 windSample = tex2Dlod(_WindDistortionMap, float4(uv, 0, 0)).xy; // get wind value ranging (0.33 - 0.66)
	float2 windSample01 = saturate(windSample * 3 - 1); // remap to 0-1
	float2 windSamplenegpos = windSample01 * 2 - 1; // remap to -1 - 1
	float2 windSampleStrength = windSamplenegpos * _WindStrength; // multiply by windstrength
	float2 remap_01 = saturate(windSampleStrength * 0.5 + 0.5); // saturate between 01 to keep low/all/high values dependent on windstrength
	float2 windSampleGrid = floor(remap_01 * (_TileAmount - 0.0001)) * (1 / _TileAmount);
	return windSampleGrid;
}