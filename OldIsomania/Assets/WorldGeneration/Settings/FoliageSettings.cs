using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Defines all possible dynamic triangles over ground mesh.
/// </summary>
[CreateAssetMenu(menuName = "WorldGeneration/Biome/Foliage", order = 30)]
[System.Serializable]
public class FoliageSettings : Settings
{
    /// <summary>
    /// Is the material to render this triangle foliage
    /// </summary>
    public MaterialSettings foliageMaterial;
    /// <summary>
    /// Where this foliage triangle should spawn
    /// </summary>
    public NoiseActivationSettings foliageSpawn;

    [ContextMenu("Rename", false, 500)]
    public override void Rename()
    {
        base.Rename();
    }

    [ContextMenu("Update", false, -1000)]
    public override void Update()
    {
        // add curve textures to foliageMaterial
        foliageMaterial.Update();

        // update foliage noise settings
        foliageSpawn.Update();
    }
}
