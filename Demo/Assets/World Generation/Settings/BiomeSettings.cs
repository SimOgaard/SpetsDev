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
[CreateAssetMenu()]
[System.Serializable]
public class BiomeSettings : ScriptableObject
{
    /// <summary>
    /// Noise layers representing the probability of this biome spawning there
    /// The highest biome noisevalue for any given mesh triangle defines that triangle's biome
    /// </summary>
    public NoiseSettings[] spawnPosition;

    /// <summary>
    /// Noise layers representing the height for any given triangle
    /// </summary>
    public NoiseSettings[] groundNoise;

    /// <summary>
    /// What prefabs and where they should spawn
    /// </summary>
    public InstantiateSettings[] prefabInstances;

    /// <summary>
    /// All materials for this biome
    /// Any null material on instance is refered to WorldGenerationSettings default BiomeMaterialSettings 
    /// </summary>
    public BiomeMaterialSettings biomeMaterials;

    /// <summary>
    /// 
    /// </summary>
    public void Update()
    {
        for (int i = 0; i < spawnPosition.Length; i++)
        {
            spawnPosition[i].Update();
        }

        for (int i = 0; i < groundNoise.Length; i++)
        {
            groundNoise[i].Update();
        }

        for (int i = 0; i < prefabInstances.Length; i++)
        {
            prefabInstances[i].Update();
        }

        biomeMaterials.Update();
    }
}
