using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Noise/NoiseAdditiveActivation", order = 3)]
[System.Serializable]
public class NoiseAdditiveActivationSettings : ScriptableObject
{
    /// <summary>
    /// The activation function for the combined noise layers value
    /// </summary>
    public NoiseActivationSettings.Activation activation;

    /// <summary>
    /// The noisesettings
    /// </summary>
    public NoiseSettings[] noiseSettings;

    public void Update()
    {

    }

    public List<FastNoiseLite> ToFNLStateCS(int index, bool warp)
    {
        List<FastNoiseLite> noiseSettingsFNLStates = new List<FastNoiseLite>();
        for (int i = 0; i < noiseSettings.Length; i++)
        {
            noiseSettingsFNLStates.Add(noiseSettings[i].ToFNLStateCS(index, warp));
        }

        return noiseSettingsFNLStates;
    }

    public List<NoiseSettings.fnl_state> ToFNLState(int index, bool warp)
    {
        List<NoiseSettings.fnl_state> noiseSettingsFNLStates = new List<NoiseSettings.fnl_state>();
        for (int i = 0; i < noiseSettings.Length; i++)
        {
            noiseSettingsFNLStates.Add(new NoiseSettings.fnl_state(noiseSettings[i], index, warp, activation));
        }

        return noiseSettingsFNLStates;
    }
}
