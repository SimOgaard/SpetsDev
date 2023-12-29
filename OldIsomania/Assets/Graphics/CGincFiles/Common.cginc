// Compile multiple versions of this shader depending on lighting settings.
#pragma target 3.0
#pragma multi_compile_fwdbase

// Common unity features
#include "UnityCG.cginc"

// Files below include macros and functions to assist
// with lighting and shadows.
#include "Lighting.cginc"
#include "AutoLight.cginc"

// Generic functions like remap and alpha blend
#include "/Assets/Graphics/CGincFiles/GenericShaderFunctions.cginc"

// Pixel snap etc
#include "/Assets/Graphics/CGincFiles/PixelPerfectShaderFunctions.cginc"

// Fast noise lite
#include "/Assets/Graphics/CGincFiles/Noise/FastNoiseLite.cginc"

sampler2D _MainTex;
float4 _MainTex_ST;