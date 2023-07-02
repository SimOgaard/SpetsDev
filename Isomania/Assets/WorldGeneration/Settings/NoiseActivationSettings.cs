using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Defines a singular layer of noise, that returns a binary yes or no.
/// Mainly from FastNoiseLite but also added functionality.
/// </summary>
[CreateAssetMenu(menuName = "Noise/NoiseActivation", order = 2)]
[System.Serializable]
public class NoiseActivationSettings : Settings
{
    /// <summary>
    /// Activation values for this noise
    /// </summary>
    [System.Serializable]
    public class Activation
    {
        public float activationMin = 0f;
        public float activationMax = 0f;
    }

    /// <summary>
    /// The activation function for this singular noise
    /// </summary>
    public Activation activation;

    /// <summary>
    /// The noisesettings
    /// </summary>
    public NoiseSettings noiseSettings;

    [ContextMenu("Rename", false, 500)]
    public override void Rename()
    {
        base.Rename();
    }

    [ContextMenu("Update", false, -1000)]
    public override void Update()
    {

    }

    public FastNoiseLite ToFNLStateCS(int index, bool warp)
    {
        return noiseSettings.ToFNLStateCS(index, warp);
    }

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
        public int invert;
        public float threshold_min;
        public float threshold_max;

        public fnl_state(NoiseSettings noiseSettings, int index, bool warp, NoiseActivationSettings.Activation activation)
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

            this.invert = noiseSettings.value.invert ? 1 : 0;

            // activation specific
            this.threshold_min = activation.activationMin;
            this.threshold_max = activation.activationMax;
        }

        public static int size
        {
            get { return sizeof(int) * 10 + sizeof(float) * 14; }
        }
    }
}
