using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Defines a singular biome.
/// Where it should spawn.
/// How The ground should be created.
/// What should spawn (items, enemies, buildings).
/// What the ground should look like.
/// How the water should look.
/// </summary>
[CreateAssetMenu(menuName = "WorldGeneration/Biome/Biome", order = 10)]
[System.Serializable]
public class BiomeSettings : Settings
{
    [Header("Where")]
    /// <summary>
    /// Where the biome should spawn
    /// </summary>
    [NonReorderable]
    public BiomeSpawnSettings spawn;

    public Color biomeMapColor;

    [Header("How")]
    /// <summary>
    /// Noise layers representing the height for any given vertice
    /// </summary>
    [NonReorderable]
    public NoiseSettings[] elevation;

    /// <summary>
    /// What prefabs and where they should spawn
    /// </summary>
    [NonReorderable]
    public InstantiateSettings[] prefabInstances;

    /// <summary>
    /// All materials for this biome
    /// Any null material on instance is refered to WorldGenerationSettings default BiomeMaterialSettings 
    /// </summary>
    public BiomeMaterialSettings materials;

    /// <summary>
    /// A struct that holds all values for this biome that will be sent to the compute shader
    /// </summary>
    public struct BiomeValues
    {
        /// <summary>
        /// Where it spawns
        /// </summary>
        public BiomeSpawnSettings.SpawnValues spawnValues;

        // how it is/looks
        public Color color;

        // indices that are warp/noise states for elevation
        public int elevationFrom;
        public int elevationTo;

        public static int size
        {
            get
            {
                return
                    BiomeSpawnSettings.SpawnValues.size +
                    sizeof(float) * 4 + // color
                    sizeof(int) * 2;    // integers
            }
        }
    }
    [HideInInspector] public BiomeValues biomeValues = new BiomeValues();

    /// <summary>
    /// The sum of all biomes inscribed angle always sums to specified degrees from layer.
    /// </summary>
    /// <param name="inscribedAngleSum">
    /// The sum of all biomes inscribed angles in this biomes layer.
    /// </param>
    /// <param name="inscribedAngleLayer">
    /// The inscribed angle of this layer.
    /// </param>
    public void NormalizeInscribedAngle(float inscribedAngleSum, float inscribedAngleRegion)
    {
        spawn.NormalizeInscribedAngle(inscribedAngleSum, inscribedAngleRegion);
    }

    [ContextMenu("Rename", false, 500)]
    public override void Rename()
    {
        base.Rename();
    }

    [ContextMenu("Update", false, -1000)]
    public override void Update()
    {
        // first update spawn values
        spawn.Update();

        // then the biome values
        biomeValues.spawnValues = spawn.spawnValues;
        biomeValues.color = biomeMapColor;

        for (int i = 0; i < elevation.Length; i++)
        {
            elevation[i].Update();
        }

        for (int i = 0; i < prefabInstances.Length; i++)
        {
            prefabInstances[i].Update();
        }

        if (materials != null)
            materials.Update();
    }
}
