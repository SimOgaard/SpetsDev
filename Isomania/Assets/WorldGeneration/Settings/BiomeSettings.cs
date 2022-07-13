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
    /// <summary>
    /// Seperate noise layers representing the probability of this biome spawning there
    /// </summary>
    [NonReorderable]
    public NoiseActivationSettings[] spawnConditions;

    /// <summary>
    /// Comunal noise layers representing the probability of this biome spawning there
    /// All noiselayers gets added before evaluation
    /// </summary>
    [NonReorderable]
    public NoiseAdditiveActivationSettings[] spawnConditionsAddative;

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

    [ContextMenu("Rename", false, 500)]
    public override void Rename()
    {
        base.Rename();
    }

    [ContextMenu("Update", false, -1000)]
    public override void Update()

    {
        for (int i = 0; i < spawnConditions.Length; i++)
        {
            spawnConditions[i].Update();
        }
        for (int i = 0; i < spawnConditionsAddative.Length; i++)
        {
            spawnConditionsAddative[i].Update();
        }

        for (int i = 0; i < elevation.Length; i++)
        {
            elevation[i].Update();
        }

        for (int i = 0; i < prefabInstances.Length; i++)
        {
            prefabInstances[i].Update();
        }

        materials.Update();
    }
}
