using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A region inside a ring
/// </summary>
[CreateAssetMenu(menuName = "WorldGeneration/Spawn/Region", order = 110)]
[System.Serializable]
public class RegionSettings : Settings
{
    /// <summary>
    /// Each region are composed by a given set of layers.
    /// This array keeps its order.
    /// </summary>
    [NonReorderable]
    public LayerSettings[] layers;

    /// <summary>
    /// A singular regions inscribed angle depends on its first layers sum.
    /// </summary>
    /// <param name="inscribedAngleRingSum">
    /// The sum of all inscribed angles of all biomes in the first layers of all reagions in given ring.
    /// </param>
    /// <param name="inscribedAngleResultingSum">
    /// The resulting sum of each region.
    /// </param>
    public void NormalizeInscribedAngle(float inscribedAngleRingSum)
    {
        // calculate the sum of inscribed angles of this region
        float inscribedAngleRegionSum = 0;
        for (int i = 0; i < layers[0].biomes.Length; i++)
        {
            inscribedAngleRegionSum += layers[0].biomes[i].spawn.spawnValues.inscribedAngle;
        }

        // use this regions inscribed angle sum and all of the regions inscribed angle sums to calculate norm
        float inscribedAngleRegion = (inscribedAngleRegionSum / inscribedAngleRingSum);

        // for each layer
        for (int i = 0; i < layers.Length; i++)
        {
            // normalize that layers inscribed angle to all sum to inscribedAngleRegion
            layers[i].NormalizeInscribedAngle(inscribedAngleRegion);
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
        for (int i = 0; i < layers.Length; i++)
        {
            layers[i].Update();
        }
    }
}
