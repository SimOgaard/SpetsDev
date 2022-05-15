using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Defines global water.
/// </summary>
[CreateAssetMenu()]
[System.Serializable]
public class WaterSettings : ScriptableObject
{
    public MaterialSettings waterMaterial;
    public float level;
    public float bobingFrequency;
    public float bobingAmplitude;

    /// <summary>
    /// Updates watermaterial and assigns it to global materials
    /// </summary>
    public void Update()
    {
        waterMaterial.Update();

        Global.waterMaterial = waterMaterial.material;
    }
}
