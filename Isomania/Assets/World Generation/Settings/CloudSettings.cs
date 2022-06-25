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
    [Header("Cloud values")]
    public MaterialSettings.Curve cloudValueCurve;

    [Header("Horizon")]
    public float horizonAngleThreshold = 15f;
    public float horizonAngleFade = 15f;

    [Header("Noise")]
    public NoiseSettings cloudNoise;

    private ComputeBuffer computeBufferFNL;
    private ComputeBuffer computeBufferFNLWarp;

    /// <summary>
    /// updates material properties
    /// </summary>
    [ContextMenu("Update", false, -1000)]
    public void Update()
    {
        Destroy();

        cloudValueCurve.AddGlobalCurveTexture();

        computeBufferFNL = new ComputeBuffer(1, NoiseSettings.fnl_state.size, ComputeBufferType.Constant, ComputeBufferMode.Immutable);
        computeBufferFNLWarp = new ComputeBuffer(1, NoiseSettings.fnl_state.size, ComputeBufferType.Constant, ComputeBufferMode.Immutable);

        computeBufferFNL.SetData(new NoiseSettings.fnl_state[1] { cloudNoise.ToFNLState(0, false) });
        computeBufferFNLWarp.SetData(new NoiseSettings.fnl_state[1] { cloudNoise.ToFNLState(0, true) });
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
