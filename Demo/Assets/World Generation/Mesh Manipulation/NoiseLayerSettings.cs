using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class NoiseLayerSettings : ScriptableObject
{
    [Header("Terrain")]
    public NoiseLayer[] terrain_noise_layers;

    [Header("Grass")]
    public Material material_grass;
    public Curve curve_grass;

    [Header("Random Foliage")]
    public Foliage[] random_foliage;

    [Header("Tree")]
    public Material material_leaf;
    public Curve curve_leaf;
    public Material material_wood;
    public Curve curve_wood;

    [Header("Other")]
    public Material material_stone;

    [Header("Spawn")]
    [Range(0, 0.1f)] public float object_density;
    public GameObject[] spawn_prefabs;

    [System.Serializable]
    public class NoiseLayer
    {
        public bool enabled = true;

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
            public Vector2 offsett;

            [Min(0f)] public float min_value;
            [Min(0f)] public float smoothing_min;
            [Min(0f)] public float max_value;
            [Min(0f)] public float smoothing_max;
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
            [Range(1, 10)] public int octaves = 1;
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
            [Range(1, 10)] public int octaves = 5;
            public float lacunarity = 2.0f;
            public float gain = 0.5f;
        }
    }

    [System.Serializable]
    public class Curve
    {
        public AnimationCurve light_curve;
        public int resolution;
        public float col_diff;
    }

    [System.Serializable]
    public class Foliage
    {
        public string name;
        public bool enabled = true;
        public Material material;
        public Curve curve;
        public NoiseLayer noise_layer;
        public Vector2 keep_range_noise;
        public float keep_range_random_noise;
        public float keep_range_random;
    }
}