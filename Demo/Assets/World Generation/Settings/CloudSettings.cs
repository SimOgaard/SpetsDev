using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Defines the directional light source
/// </summary>
[CreateAssetMenu()]
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

        computeBufferFNL = new ComputeBuffer(1, NoiseSettings.fnl_state.size);
        computeBufferFNLWarp = new ComputeBuffer(1, NoiseSettings.fnl_state.size);

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
        computeBufferFNL.Dispose();
        computeBufferFNLWarp.Dispose();
    }
}
