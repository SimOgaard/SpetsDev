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

        [Min(0f)] public float minValue;
        [Min(0f)] public float smoothingMin;
        [Min(0f)] public float maxValue;
        [Min(0f)] public float smoothingMax;
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
}
