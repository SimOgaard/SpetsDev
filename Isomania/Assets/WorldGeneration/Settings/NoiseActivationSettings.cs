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

    public NoiseSettings.fnl_state ToFNLState(int index, bool warp)
    {
        return new NoiseSettings.fnl_state(noiseSettings, index, warp, activation);
    }
}
