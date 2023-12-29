using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A ring of biomes
/// 
/// NOTE:
/// If you want a outer ring of biomes next to each others, just set the first element of layerSettings to no biome.
/// If you want a random biome, with no ring, make a region biome-less.
/// Biomeless means a negative biome area with no rounding.
/// </summary>
[CreateAssetMenu(menuName = "WorldGeneration/Spawn/Ring", order = 100)]
[System.Serializable]
public class RingSettings : Settings
{
    [HideInInspector]
    public string SlotName = "lol";
    
    /// <summary>
    /// Each ring are composed of a given set of regions.
    /// This array gets randomized.
    /// </summary>
    [NonReorderable]
    public RegionSettings[] regions;

    /// <summary>
    /// A regions inscribed angle always sums to specified degrees from ring
    /// </summary>
    public void NormalizeInscribedAngle()
    {
        // get the sum of all inscribed angles of the first layers biomes of all regions in this ring
        float inscribedAngleSum = 0;
        for (int i = 0; i < regions.Length; i++)
        {
            for (int q = 0; q < regions[i].layers[0].biomes.Length; q++)
            {
                inscribedAngleSum += regions[i].layers[0].biomes[q].spawn.spawnValues.inscribedAngle;
            }
        }

        // for all regions in ring
        for (int i = 0; i < regions.Length; i++)
        {
            // normalize the regions angle 
            regions[i].NormalizeInscribedAngle(inscribedAngleSum);
        }
    }

    [ContextMenu("Update", false, -1000)]
    public override void Update()
    {
        // first update the values
        for (int i = 0; i < regions.Length; i++)
        {
            regions[i].Update();
        }
    }
}
