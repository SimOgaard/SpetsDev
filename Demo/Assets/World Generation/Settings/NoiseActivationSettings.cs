using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
[System.Serializable]
public class NoiseActivationSettings : ScriptableObject
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

    public void Update()
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
