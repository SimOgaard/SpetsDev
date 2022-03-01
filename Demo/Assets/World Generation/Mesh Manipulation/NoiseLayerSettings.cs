using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class NoiseLayerSettings : ScriptableObject
{
    [Header("Terrain")]
    public NoiseLayer[] terrainNoiseLayers;

    [Header("Materials")]
    public MaterialWithCurve materialLeaf;
    public MaterialWithCurve materialWood;

    public MaterialWithCurve materialStone;

    public MaterialWithCurve materialGolem;
    public MaterialWithCurve materialGolemShoulder;

    public MaterialWithCurve materialStatic;

    [Header("Foliage")]
    public Foliage[] randomFoliage;

    [Header("Water")]
    public Water water;

    [Header("Clouds")]
    public MaterialWithCurve materialSun;
    public MaterialWithCurve materialMoon;

    [Header("Spawn")]
    [Range(0, 0.1f)] public float objectDensity;
    public GameObject[] spawnPrefabs;

    [System.Serializable]
    public class NoiseLayer
    {
        public bool enabled = true;

        public General general;
        public GeneralNoise generalNoise;
        public Fractal fractal;
        public Cellular cellular;
        public DomainWarping domainWarp;
        public DomainWarpingFractal domainWarpFractal;

        [System.Serializable]
        public class General
        {
            public float amplitude = 0.1f;
            public Vector2 offsett;

            [Min(0f)] public float minValue;
            [Min(0f)] public float smoothingMin;
            [Min(0f)] public float maxValue;
            [Min(0f)] public float smoothingMax;
        }

        [System.Serializable]
        public class GeneralNoise
        {
            public FastNoiseLite.NoiseType noiseType = FastNoiseLite.NoiseType.OpenSimplex2;
            public int seed = 1337;
            public float frequency = 0.02f;
        }

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

        [System.Serializable]
        public class Cellular
        {
            public FastNoiseLite.CellularDistanceFunction cellularDistanceFunction = FastNoiseLite.CellularDistanceFunction.Euclidean;
            public FastNoiseLite.CellularReturnType cellularReturnType = FastNoiseLite.CellularReturnType.CellValue;
            public float jitter = 1.0f;
        }

        [System.Serializable]
        public class DomainWarping
        {
            public FastNoiseLite.DomainWarpType domainWarpType = FastNoiseLite.DomainWarpType.OpenSimplex2;
            public float amplitude = 30.0f;
            public float frequency = 0.005f;
        }

        [System.Serializable]
        public class DomainWarpingFractal
        {
            public FastNoiseLite.FractalType fractalType = FastNoiseLite.FractalType.None;
            [Range(1, 10)] public int octaves = 5;
            public float lacunarity = 2.0f;
            public float gain = 0.5f;
        }
    }

    [System.Serializable]
    public class Water
    {
        public MaterialWithCurve material;
        public float level;
        public float bobingFrequency;
        public float bobingAmplitude;
    }

    [System.Serializable]
    public class Curve
    {
        public string textureName = "_ColorShading";
        public AnimationCurve lightCurve;
        public int resolution = 256;
        public float colDiff;
    }

    [System.Serializable]
    public class Foliage
    {
        public GroundMesh.GroundTriangleType type;
        public bool enabled = true;
        public MaterialWithCurve material;
        public NoiseLayer noiseLayer;
        public Vector2 keepRangeNoise;
        public float keepRangeRandomNoise;
        public float keepRangeRandom;
    }

    [System.Serializable]
    public class MaterialWithCurve
    {
        public Material material;
        public Curve[] curves;
    }
}