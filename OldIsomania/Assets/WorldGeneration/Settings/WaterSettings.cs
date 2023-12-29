using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Defines global water.
/// </summary>
[CreateAssetMenu(menuName = "WorldGeneration/Climate/Water", order = 40)]
[System.Serializable]
public class WaterSettings : Settings
{
    [Header("Water Level")]
    public float waterLevel;
    public float waterBobingFrequency;
    public float waterBobingAmplitude;

    [Header("Material")]
    public MaterialSettings waterMaterial;

    [ContextMenu("Rename", false, 500)]
    public override void Rename()
    {
        base.Rename();
    }

    /// <summary>
    /// Updates watermaterial and assigns it to global materials
    /// </summary>
    [ContextMenu("Update", false, -1000)]
    public override void Update()
    {
        waterMaterial.Update();

        Global.waterMaterial = waterMaterial.shaderMaterial;
    }
}
