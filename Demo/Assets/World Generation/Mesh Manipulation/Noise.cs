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

        private float min_value;
        private float smoothing_min;
        private float max_value;
        private float smoothing_max;

        private float SmoothMin(float a)
        {
            float h = Mathf.Max(0f, Mathf.Min(1f, (max_value - a + smoothing_max) / (2f * smoothing_max)));
            return a * h + max_value * (1f - h) - smoothing_max * h * (1f - h);
        }

        private float SmoothMax(float a)
        {
            float h = Mathf.Max(0f, Mathf.Min(1f, (min_value - a + smoothing_min) / (2f * smoothing_min)));
            return a * h + min_value * (1f - h) - smoothing_min * h * (1f - h);
        }

        public NoiseLayer(NoiseLayerSettings.NoiseLayer noise_layer)
        {
            // sets noise
            noise = new FastNoiseLite();
            noise.SetSeed(noise_layer.general_noise.seed);
            noise.SetFrequency(noise_layer.general_noise.frequency);
            noise.SetNoiseType(noise_layer.general_noise.noise_type);

            noise.SetFractalType(noise_layer.fractal.fractal_type);
            noise.SetFractalOctaves(noise_layer.fractal.octaves);
            noise.SetFractalLacunarity(noise_layer.fractal.lacunarity);
            noise.SetFractalGain(noise_layer.fractal.gain);
            noise.SetFractalWeightedStrength(noise_layer.fractal.weighted_strength);
            noise.SetFractalPingPongStrength(noise_layer.fractal.ping_pong_strength);

            noise.SetCellularDistanceFunction(noise_layer.cellular.cellular_distance_function);
            noise.SetCellularReturnType(noise_layer.cellular.cellular_return_type);
            noise.SetCellularJitter(noise_layer.cellular.jitter);

            // domain warp
            warp = new FastNoiseLite();
            warp.SetSeed(noise_layer.general_noise.seed);
            warp.SetDomainWarpType(noise_layer.domain_warp.domain_warp_type);
            warp.SetDomainWarpAmp(noise_layer.domain_warp.amplitude);
            warp.SetFrequency(noise_layer.domain_warp.frequency);

            warp.SetFractalType(noise_layer.domain_warp_fractal.fractal_type);
            warp.SetFractalOctaves(noise_layer.domain_warp_fractal.octaves);
            warp.SetFractalLacunarity(noise_layer.domain_warp_fractal.lacunarity);
            warp.SetFractalGain(noise_layer.domain_warp_fractal.gain);

            enabled = noise_layer.enabled;

            amplitude = noise_layer.general.amplitude;
            offsett = noise_layer.general.offsett;

            min_value = noise_layer.general.min_value;
            smoothing_max = Mathf.Max(0f, noise_layer.general.smoothing_max);
            max_value = noise_layer.general.max_value;
            smoothing_min = Mathf.Min(0f, -noise_layer.general.smoothing_min);
        }

        public float GetNoiseValueColor(float x, float z)
        {
            x += offsett.x;
            z += offsett.y;
            warp.DomainWarp(ref x, ref z);
            float noise_value = noise.GetNoise(x, z);
            noise_value = (noise_value + 1f) * 0.5f;
            noise_value = SmoothMin(noise_value);
            noise_value = SmoothMax(noise_value);
            return noise_value;
        }

        public float GetNoiseValue(float x, float z)
        {
            return GetNoiseValueColor(x, z) * amplitude;
        }

        public Texture2D GetNoiseTexture(Vector2Int texture_size)
        {
            Texture2D noise_texture = new Texture2D(texture_size.x, texture_size.y);
            for (int x = 0; x < texture_size.x; x++)
            {
                for (int y = 0; y < texture_size.y; y++)
                {
                    float noise_value = GetNoiseValueColor(x, y);

                    Color color = new Color(noise_value, noise_value, noise_value);
                    noise_texture.SetPixel(x, y, color);
                }
            }
            noise_texture.Apply();
            return noise_texture;
        }
    }

    public static NoiseLayer[] CreateNoiseLayers(NoiseLayerSettings noise_layer_settings)
    {
        int noise_length = noise_layer_settings.terrain_noise_layers.Length;
        NoiseLayer[] noise_layers = new NoiseLayer[noise_length];
        for (int i = 0; i < noise_length; i++)
        {
            noise_layers[i] = new NoiseLayer(noise_layer_settings.terrain_noise_layers[i]);
        }
        return noise_layers;
    }
}