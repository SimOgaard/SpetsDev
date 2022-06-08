using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Defines the directional light source
/// </summary>
[CreateAssetMenu(menuName = "WorldGeneration/Climate/Cloud", order = 20)]
[System.Serializable]
public class CloudSettings : ScriptableObject
{
    [Header("Light")]
    public Color color;

    public float intensity = 1f;
    public float indirectMultiplier = 1f;

    public float horizonAngleThreshold = 15f;
    public float horizonAngleFade = 15f;

    [Header("Noise")]
    public NoiseSettings cloudNoise;

    [Header("Shader")]
    public float cookieSize = 175f;
    public int shadowRenderTexutreResolution = 512;

    public MaterialSettings material;

    private ComputeBuffer computeBufferFNL;
    private ComputeBuffer computeBufferFNLWarp;

    /// <summary>
    /// updates material properties
    /// </summary>
    public void Update()
    {
        material.Update();

        material.material.SetFloat("_HorizonAngleThreshold", horizonAngleThreshold);
        material.material.SetFloat("_HorizonAngleFade", horizonAngleFade);


        material.material.SetInt("seed", cloudNoise.general.seed);
        material.material.SetInt("noise_type", (int)cloudNoise.general.noiseType);
        material.material.SetFloat("weighted_strength", cloudNoise.fractal.weightedStrength);
        material.material.SetFloat("ping_pong_strength", cloudNoise.fractal.pingPongStrength);
        material.material.SetInt("cellular_distance_func", (int)cloudNoise.cellular.cellularDistanceFunction);
        material.material.SetInt("cellular_return_type", (int)cloudNoise.cellular.cellularReturnType);
        material.material.SetFloat("cellular_jitter_mod", cloudNoise.cellular.jitter);
        material.material.SetInt("domain_warp_type", (int)cloudNoise.domainWarp.domainWarpType);
        material.material.SetFloat("domain_warp_amp", cloudNoise.domainWarp.amplitude);

        material.material.SetFloat("amplitude", cloudNoise.value.amplitude);
        material.material.SetFloat("min_value", cloudNoise.value.minValue);
        material.material.SetFloat("smoothing_min", cloudNoise.value.smoothingMin);
        material.material.SetFloat("max_value", cloudNoise.value.maxValue);
        material.material.SetFloat("smoothing_max", cloudNoise.value.smoothingMax);

        material.material.SetInt("invert", cloudNoise.value.invert ? 1 : 0);

        material.material.SetFloat("frequency", cloudNoise.general.frequency);
        material.material.SetInt("rotation_type_3d", (int)cloudNoise.general.rotationType3D);
        material.material.SetInt("fractal_type", (int)cloudNoise.fractal.fractalType);
        material.material.SetInt("octaves", cloudNoise.fractal.octaves);
        material.material.SetFloat("lacunarity", cloudNoise.fractal.lacunarity);
        material.material.SetFloat("gain", cloudNoise.fractal.gain);

        material.material.SetFloat("frequency_warp", cloudNoise.domainWarp.frequency);
        material.material.SetInt("rotation_type_3d_warp", (int)cloudNoise.domainWarp.rotationType3D);
        material.material.SetInt("fractal_type_warp", (int)cloudNoise.domainWarpFractal.warpFractalType);
        material.material.SetInt("octaves_warp", cloudNoise.domainWarpFractal.octaves);
        material.material.SetFloat("lacunarity_warp", cloudNoise.domainWarpFractal.lacunarity);
        material.material.SetFloat("gain_warp", cloudNoise.domainWarpFractal.gain);

        computeBufferFNL = new ComputeBuffer(1, NoiseSettings.fnl_state.size, ComputeBufferType.Constant, ComputeBufferMode.Immutable);
        computeBufferFNLWarp = new ComputeBuffer(1, NoiseSettings.fnl_state.size, ComputeBufferType.Constant, ComputeBufferMode.Immutable);

        computeBufferFNL.SetData(new NoiseSettings.fnl_state[1] { cloudNoise.ToFNLState(0, false) });
        computeBufferFNLWarp.SetData(new NoiseSettings.fnl_state[1] { cloudNoise.ToFNLState(0, true) });

        material.material.SetBuffer("fnl_warp_state", computeBufferFNL);
        material.material.SetBuffer("fnl_noise_state", computeBufferFNLWarp);
    }

    /// <summary>
    /// clears all data that has no gc collect like computebuffers
    /// </summary>
    public void Destroy()
    {
        if (computeBufferFNL != null)
            computeBufferFNL.Dispose();
        if (computeBufferFNLWarp != null)
            computeBufferFNLWarp.Dispose();
    }
}
