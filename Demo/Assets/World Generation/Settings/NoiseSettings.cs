using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Defines a singular layer of noise.
/// Mainly from FastNoiseLite but also added functionality.
/// </summary>
[CreateAssetMenu()]
[System.Serializable]
public class NoiseSettings : ScriptableObject
{
    /// <summary>
    /// function that returns a boolean using given settings keep ranges and generated noise
    /// </summary>
    public Vector2 keepRangeNoise;
    public float keepRangeRandomNoise;
    public float keepRangeRandom;

    public Vector2 offsett;

    public Value value;
    public General general;
    public Fractal fractal;
    public Cellular cellular;
    public DomainWarp domainWarp;
    public DomainWarpFractal domainWarpFractal;

    /// <summary>
    /// Affects the noisevalue output
    /// </summary>
    [System.Serializable]
    public class Value
    {
        public float amplitude = 0.1f;

        [Range(-1f, 1f)] public float minValue;
        public float smoothingMin;
        [Range(-1f, 1f)] public float maxValue;
        public float smoothingMax;
    }

    /// <summary>
    /// General noisesettings
    /// </summary>
    [System.Serializable]
    public class General
    {
        public FastNoiseLite.NoiseType noiseType = FastNoiseLite.NoiseType.OpenSimplex2;
        public FastNoiseLite.RotationType3D rotationType3D = FastNoiseLite.RotationType3D.None;

        public int seed = 1337;
        public float frequency = 0.02f;
    }

    /// <summary>
    /// Addition of multiple noiselayers ontop of eachothers offsetted
    /// </summary>
    [System.Serializable]
    public class Fractal
    {
        public FastNoiseLite.FractalType fractalType = FastNoiseLite.FractalType.None;
        [Range(1, 10)] public int octaves = 1;
        public float lacunarity = 2.0f;
        public float gain = 0.5f;
        public float weightedStrength = 0.0f;
        public float pingPongStrength = 2.0f;
    }

    /// <summary>
    /// Cellular noise
    /// </summary>
    [System.Serializable]
    public class Cellular
    {
        public FastNoiseLite.CellularDistanceFunction cellularDistanceFunction = FastNoiseLite.CellularDistanceFunction.Euclidean;
        public FastNoiseLite.CellularReturnType cellularReturnType = FastNoiseLite.CellularReturnType.CellValue;
        public float jitter = 1.0f;
    }

    /// <summary>
    /// Warps noise
    /// </summary>
    [System.Serializable]
    public class DomainWarp
    {
        public FastNoiseLite.DomainWarpType domainWarpType = FastNoiseLite.DomainWarpType.None;
        public FastNoiseLite.RotationType3D rotationType3D = FastNoiseLite.RotationType3D.None;
        public float amplitude = 30.0f;
        public float frequency = 0.005f;
    }

    /// <summary>
    /// Addition of multiple noise warps ontop of eachothers offsetted
    /// </summary>
    [System.Serializable]
    public class DomainWarpFractal
    {
        public FastNoiseLite.WarpFractalType warpFractalType = FastNoiseLite.WarpFractalType.None;
        [Range(1, 10)] public int octaves = 5;
        public float lacunarity = 2.0f;
        public float gain = 0.5f;
    }

    public void Update()
    {

    }

    #region noise functions
    public float SmoothMin(float a)
    {
        float k = Mathf.Max(0, value.smoothingMax);
        float h = Mathf.Max(0f, Mathf.Min(1f, (value.maxValue - a + k) / (2f * k)));
        return a * h + value.maxValue * (1f - h) - k * h * (1f - h);
    }

    public float SmoothMax(float a)
    {
        float k = Mathf.Max(0, -value.smoothingMin);
        float h = Mathf.Max(0f, Mathf.Min(1f, (value.minValue - a + k) / (2f * k)));
        return a * h + value.minValue * (1f - h) - k * h * (1f - h);
    }
    #endregion noise functions

    #region cginc
    /// <summary>
    /// OBS! DomainWarpType.None might break a lot of things, consider adding default break cases in FastNoiseLite.cginc/cs
    /// </summary>
    public struct fnl_state
    {
        public int seed;
        public float frequency;
        public int noise_type;
        public int rotation_type_3d;
        public int fractal_type;
        public int octaves;
        public float lacunarity;
        public float gain;
        public float weighted_strength;
        public float ping_pong_strength;
        public int cellular_distance_func;
        public int cellular_return_type;
        public float cellular_jitter_mod;
        public int domain_warp_type;
        public float domain_warp_amp;
        public float amplitude;
        public float min_value;
        public float smoothing_min;
        public float max_value;
        public float smoothing_max;
        public int index;

        public fnl_state(NoiseSettings noiseSettings, int index, bool warp)
        {
            if (!warp)
            {
                this.frequency = noiseSettings.general.frequency;
                this.rotation_type_3d = (int)noiseSettings.general.rotationType3D;
                this.fractal_type = (int)noiseSettings.fractal.fractalType;
                this.octaves = noiseSettings.fractal.octaves;
                this.lacunarity = noiseSettings.fractal.lacunarity;
                this.gain = noiseSettings.fractal.gain;
            }
            else
            {
                this.frequency = noiseSettings.domainWarp.frequency;
                this.rotation_type_3d = (int)noiseSettings.domainWarp.rotationType3D;
                this.fractal_type = (int)noiseSettings.domainWarpFractal.warpFractalType;
                this.octaves = noiseSettings.domainWarpFractal.octaves;
                this.lacunarity = noiseSettings.domainWarpFractal.lacunarity;
                this.gain = noiseSettings.domainWarpFractal.gain;
            }
            this.seed = noiseSettings.general.seed;
            this.noise_type = (int)noiseSettings.general.noiseType;
            this.weighted_strength = noiseSettings.fractal.weightedStrength;
            this.ping_pong_strength = noiseSettings.fractal.pingPongStrength;
            this.cellular_distance_func = (int)noiseSettings.cellular.cellularDistanceFunction;
            this.cellular_return_type = (int)noiseSettings.cellular.cellularReturnType;
            this.cellular_jitter_mod = noiseSettings.cellular.jitter;
            this.domain_warp_type = (int)noiseSettings.domainWarp.domainWarpType;
            this.domain_warp_amp = noiseSettings.domainWarp.amplitude;

            this.amplitude = noiseSettings.value.amplitude;
            this.min_value = noiseSettings.value.minValue;
            this.smoothing_min = noiseSettings.value.smoothingMin;
            this.max_value = noiseSettings.value.maxValue;
            this.smoothing_max = noiseSettings.value.smoothingMax;
            this.index = index;
        }

        public static int size
        {
            get { return sizeof(int) * 9 + sizeof(float) * 12; }
        }
    }
    #endregion
}
