using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A layer inside a ring
/// </summary>
[CreateAssetMenu(menuName = "WorldGeneration/Spawn/Layer", order = 120)]
[System.Serializable]
public class LayerSettings : Settings
{
    /// <summary>
    /// Each layer contains a given set of biomes.
    /// This array gets randomized.
    /// </summary>
    [NonReorderable]
    public BiomeSettings[] biomes;

    /// <summary>
    /// A singular layers inscribed angle is the inscribed angle of its region.
    /// </summary>
    /// <param name="inscribedAngleRegion">
    /// The inscribed angle of this layers region.
    /// </param>
    public void NormalizeInscribedAngle(float inscribedAngleRegion)
    {
        // get the sum of inscribed angles for all biomes in this layer
        float inscribedAngleSum = 0;
        for (int i = 0; i < biomes.Length; i++)
        {
            inscribedAngleSum += biomes[i].spawn.spawnValues.inscribedAngle;
        }

        // for each biome
        for (int i = 0; i < biomes.Length; i++)
        {
            // normalize that biomes inscribed angle
            biomes[i].NormalizeInscribedAngle(inscribedAngleSum, inscribedAngleRegion);
        }
    }

    [ContextMenu("Rename", false, 500)]
    public override void Rename()
    {
        base.Rename();
    }

    [ContextMenu("Update", false, -1000)]
    public override void Update()
    {
        for (int i = 0; i < biomes.Length; i++)
        {
            biomes[i].Update();
        }
    }
}

