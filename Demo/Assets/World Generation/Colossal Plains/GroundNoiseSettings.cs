﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class GroundNoiseSettings : ScriptableObject
{
    public General general;
    public GeneralNoise general_noise;
    public Fractal fractal;
    public Cellular cellular;
    public DomainWarping domain_warp;
    public DomainWarpingFractal domain_warp_fractal;

    [System.Serializable]
    public class General
    {
        public float amplitude = 0.1f;
        public Vector3 offsett;

        [Range(0, 1)]
        public float min_value;
        [Range(0, 1)]
        public float smoothing_min;
        [Range(0, 1)]
        public float max_value;
        [Range(0, 1)]
        public float smoothing_max;
    }

    [System.Serializable]
    public class GeneralNoise
    {
        public FastNoiseLite.NoiseType noise_type = FastNoiseLite.NoiseType.OpenSimplex2;
        public int seed = 1337;
        public float frequency = 0.02f;
    }

    [System.Serializable]
    public class Fractal
    {
        public FastNoiseLite.FractalType fractal_type = FastNoiseLite.FractalType.None;
        public int octaves = 1;
        public float lacunarity = 2.0f;
        public float gain = 0.5f;
        public float weighted_strength = 0.0f;
        public float ping_pong_strength = 2.0f;
    }

    [System.Serializable]
    public class Cellular
    {
        public FastNoiseLite.CellularDistanceFunction cellular_distance_function = FastNoiseLite.CellularDistanceFunction.Euclidean;
        public FastNoiseLite.CellularReturnType cellular_return_type = FastNoiseLite.CellularReturnType.CellValue;
        public float jitter = 1.0f;
    }

    [System.Serializable]
    public class DomainWarping
    {
        public FastNoiseLite.DomainWarpType domain_warp_type = FastNoiseLite.DomainWarpType.OpenSimplex2;
        public float amplitude = 30.0f;
        public float frequency = 0.005f;
    }

    [System.Serializable]
    public class DomainWarpingFractal
    {
        public FastNoiseLite.FractalType fractal_type = FastNoiseLite.FractalType.None;
        public int octaves = 5;
        public float lacunarity = 2.0f;
        public float gain = 0.5f;
    }
}