using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controlls the global wind
/// </summary>
[CreateAssetMenu()]
[System.Serializable]
public class WindSettings : ScriptableObject
{
    public Texture2D WindDistortionMap;
    public Vector4 WindDistortionMap_ST;

    public Vector2 WindFrequency;
    public float WindStrength;

    public Vector3 cloudSpeed;
    public NoiseSettings noiseScrollNoise;

    public void Update()
    {
        Shader.SetGlobalTexture("_WindDistortionMap", WindDistortionMap);
        Shader.SetGlobalVector("_WindDistortionMap_ST", WindDistortionMap_ST);

        Shader.SetGlobalVector("_WindFrequency", WindFrequency);
        Shader.SetGlobalFloat("_WindStrength", WindStrength);

        Global.windController.UpdateSettings(this);
    }
}
