using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Noise : MonoBehaviour
{
    public struct NoiseLayer
    {
        private FastNoiseLite warp;
        private FastNoiseLite noise;

        public bool enabled;

        private float amplitude;
        private Vector2 offsett;

        private float minValue;
        private float smoothingMin;
        private float maxValue;
        private float smoothingMax;

        private float SmoothMin(float a)
        {
            float h = Mathf.Max(0f, Mathf.Min(1f, (maxValue - a + smoothingMax) / (2f * smoothingMax)));
            return a * h + maxValue * (1f - h) - smoothingMax * h * (1f - h);
        }

        private float SmoothMax(float a)
        {
            float h = Mathf.Max(0f, Mathf.Min(1f, (minValue - a + smoothingMin) / (2f * smoothingMin)));
            return a * h + minValue * (1f - h) - smoothingMin * h * (1f - h);
        }

        public NoiseLayer(NoiseLayerSettings.NoiseLayer noiseLayer)
        {
            // sets noise
            noise = new FastNoiseLite();
            noise.SetSeed(noiseLayer.generalNoise.seed);
            noise.SetFrequency(noiseLayer.generalNoise.frequency);
            noise.SetNoiseType(noiseLayer.generalNoise.noiseType);

            noise.SetFractalType(noiseLayer.fractal.fractalType);
            noise.SetFractalOctaves(noiseLayer.fractal.octaves);
            noise.SetFractalLacunarity(noiseLayer.fractal.lacunarity);
            noise.SetFractalGain(noiseLayer.fractal.gain);
            noise.SetFractalWeightedStrength(noiseLayer.fractal.weightedStrength);
            noise.SetFractalPingPongStrength(noiseLayer.fractal.pingPongStrength);

            noise.SetCellularDistanceFunction(noiseLayer.cellular.cellularDistanceFunction);
            noise.SetCellularReturnType(noiseLayer.cellular.cellularReturnType);
            noise.SetCellularJitter(noiseLayer.cellular.jitter);

            // domain warp
            warp = new FastNoiseLite();
            warp.SetSeed(noiseLayer.generalNoise.seed);
            warp.SetDomainWarpType(noiseLayer.domainWarp.domainWarpType);
            warp.SetDomainWarpAmp(noiseLayer.domainWarp.amplitude);
            warp.SetFrequency(noiseLayer.domainWarp.frequency);

            warp.SetFractalType(noiseLayer.domainWarpFractal.fractalType);
            warp.SetFractalOctaves(noiseLayer.domainWarpFractal.octaves);
            warp.SetFractalLacunarity(noiseLayer.domainWarpFractal.lacunarity);
            warp.SetFractalGain(noiseLayer.domainWarpFractal.gain);

            enabled = noiseLayer.enabled;

            amplitude = noiseLayer.general.amplitude;
            offsett = noiseLayer.general.offsett;

            minValue = noiseLayer.general.minValue;
            smoothingMax = Mathf.Max(0f, noiseLayer.general.smoothingMax);
            maxValue = noiseLayer.general.maxValue;
            smoothingMin = Mathf.Min(0f, -noiseLayer.general.smoothingMin);
        }

        public float GetNoiseValueColor(float x, float z)
        {
            x += offsett.x;
            z += offsett.y;
            warp.DomainWarp(ref x, ref z);
            float noiseValue = noise.GetNoise(x, z);
            noiseValue = (noiseValue + 1f) * 0.5f;
            noiseValue = SmoothMin(noiseValue);
            noiseValue = SmoothMax(noiseValue);
            return noiseValue;
        }

        public float GetNoiseValue(float x, float z)
        {
            return GetNoiseValueColor(x, z) * amplitude;
        }

        public Texture2D GetNoiseTexture(Vector2Int textureSize)
        {
            Texture2D noiseTexture = new Texture2D(textureSize.x, textureSize.y);
            for (int x = 0; x < textureSize.x; x++)
            {
                for (int y = 0; y < textureSize.y; y++)
                {
                    float noiseValue = GetNoiseValueColor(x, y);

                    Color color = new Color(noiseValue, noiseValue, noiseValue);
                    noiseTexture.SetPixel(x, y, color);
                }
            }
            noiseTexture.Apply();
            return noiseTexture;
        }
    }

    public static NoiseLayer[] CreateNoiseLayers(NoiseLayerSettings noiseLayerSettings)
    {
        int noiseLength = noiseLayerSettings.terrainNoiseLayers.Length;
        NoiseLayer[] noiseLayers = new NoiseLayer[noiseLength];
        for (int i = 0; i < noiseLength; i++)
        {
            noiseLayers[i] = new NoiseLayer(noiseLayerSettings.terrainNoiseLayers[i]);
        }
        return noiseLayers;
    }
}