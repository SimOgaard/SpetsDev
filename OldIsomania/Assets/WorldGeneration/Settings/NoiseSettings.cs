using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NoiseExtension
{
    public static List<NoiseSettings.fnl_state> ToFnlStates(this NoiseSettings[] fnl_states, bool warp)
    {
        List<NoiseSettings.fnl_state> states = new List<NoiseSettings.fnl_state>();
        for (int i = 0; i < fnl_states.Length; i++)
        {
            states.Add(fnl_states[i].ToFNLState(warp));
        }
        return states;
    }

    /*
    public static NoiseSettings.fnl_state ToFnlState(this NoiseSettings fnl_state, bool warp)
    {
        return fnl_state.ToFNLState(warp);
    }
    */
}

/// <summary>
/// Given XY, XYZ, return float
/// </summary>
[System.Serializable]
public class Noise
{
    public Value value;
    public General general;
    public Fractal fractal;
    public Cellular cellular;

    // instanciated noise states
    [HideInInspector]
    public GPU_Noise gpu_noise;
    [HideInInspector]
    public FastNoiseLite cpu_noise;

    /// <summary>
    /// Affects the noisevalue output
    /// </summary>
    [System.Serializable]
    public class Value
    {
        public bool invert = false;
        public float amplitude = 1f;

        [Header("Smoothing")]
        [Range(-1f, 1f)] public float minValue = -1f;
        public float smoothingMin = 0f;
        [Range(-1f, 1f)] public float maxValue = 1f;
        public float smoothingMax = 0f;
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
    /// Struct similar to the one on the gpu
    /// </summary>
    public struct GPU_Noise
    {


        public GPU_Noise(Noise noise)
        {

        }
    }

    public FastNoiseLite CPU_Noise(Noise noise)
    {
        // create it
        FastNoiseLite fastNoiseLite = new FastNoiseLite();

        // set all values of noise state
        //fastNoiseLite.set

        // and return it
        return fastNoiseLite;
    }

    /// <summary>
    /// Updates this noise state to have correct values in cpu and gpu version
    /// </summary>
    public void Update()
    {
        gpu_noise = new GPU_Noise(this);
        cpu_noise = CPU_Noise(this);
    }
}

/// <summary>
///  Given XY, XYZ, return warped XY, XYZ
/// </summary>
[System.Serializable]
public class Warp
{
    public Vector2 offsett;

    public DomainWarp domainWarp;
    public DomainWarpFractal domainWarpFractal;

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
}

/// <summary>
/// Defines a singular layer of noise.
/// Mainly from FastNoiseLite but also added functionality.
/// </summary>
[CreateAssetMenu(menuName = "Noise/Noise", order = 1)]
[System.Serializable]
public class NoiseSettings : Settings
{
    public Noise noise;

    public Warp warp;

    /// <summary>
    /// Uses both noise and warp states to sample 2D noise
    /// </summary>
    public float SampleNoise2D(Vector2 pos)
    {
        return SampleNoise2D(pos.x, pos.y);
    }
    public float SampleNoise2D(float x, float y)
    {
        // warp x and y
        warp.cpu_warp.warp2d();
        // sample noise using warped
        noise.cpu_noise.sample2d();
    }

    /// <summary>
    /// Uses both noise and warp states to sample 3D noise
    /// </summary>
    public float SampleNoise3D()
    {

    }

    [ContextMenu("Rename", false, 500)]
    public override void Rename()
    {
        base.Rename();
    }

    [ContextMenu("Update", false, -1000)]
    public override void Update()
    {

    }

    #region fnl cs
    public FastNoiseLite ToFNLStateCS(int index, bool warp)
    {
        FastNoiseLite fastNoiseLite = new FastNoiseLite();

        if (!warp)
        {
            fastNoiseLite.SetFrequency(general.frequency);
            fastNoiseLite.SetRotationType3D(general.rotationType3D);
            fastNoiseLite.SetFractalType(fractal.fractalType);
            fastNoiseLite.SetFractalOctaves(fractal.octaves);
            fastNoiseLite.SetFractalLacunarity(fractal.lacunarity);
            fastNoiseLite.SetFractalGain(fractal.gain);
        }
        else
        {
            fastNoiseLite.SetFrequency(domainWarp.frequency);
            fastNoiseLite.SetRotationType3D(domainWarp.rotationType3D);
            fastNoiseLite.SetWarpFractalType(domainWarpFractal.warpFractalType);
            fastNoiseLite.SetFractalOctaves(domainWarpFractal.octaves);
            fastNoiseLite.SetFractalLacunarity(domainWarpFractal.lacunarity);
            fastNoiseLite.SetFractalGain(domainWarpFractal.gain);
        }
        fastNoiseLite.SetSeed(general.seed);
        fastNoiseLite.SetNoiseType(general.noiseType);
        fastNoiseLite.SetFractalWeightedStrength(fractal.weightedStrength);
        fastNoiseLite.SetFractalPingPongStrength(fractal.pingPongStrength);
        fastNoiseLite.SetCellularDistanceFunction(cellular.cellularDistanceFunction);
        fastNoiseLite.SetCellularReturnType(cellular.cellularReturnType);
        fastNoiseLite.SetCellularJitter(cellular.jitter);
        fastNoiseLite.SetDomainWarpType(domainWarp.domainWarpType);
        fastNoiseLite.SetDomainWarpAmp(domainWarp.amplitude);

        fastNoiseLite.amplitude = value.amplitude;
        fastNoiseLite.min_value = value.minValue;
        fastNoiseLite.smoothing_min = value.smoothingMin;
        fastNoiseLite.max_value = value.maxValue;
        fastNoiseLite.smoothing_max = value.smoothingMax;

        fastNoiseLite.index = index;

        fastNoiseLite.invert = value.invert;

        return fastNoiseLite;
    }
    #endregion

    #region cginc
    public static List<fnl_state> ToFNLStates(NoiseSettings[] fnl_states, bool warp)
    {
        List<fnl_state> states = new List<fnl_state>();
        for (int i = 0; i < fnl_states.Length; i++)
        {
            states.Add(fnl_states[i].ToFNLState(warp));
        }
        return states;
    }

    public fnl_state ToFNLState(bool warp)
    {
        return new fnl_state(this, warp, new NoiseActivationSettings.Activation());
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

        public fnl_state(NoiseSettings noiseSettings, bool warp, NoiseActivationSettings.Activation activation)
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
        }

        public static int size
        {
            get { return sizeof(int) * 9 + sizeof(float) * 12; }
        }
    }
    #endregion
}
