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
public class BiomeSettings : ScriptableObject
{
    /// <summary>
    /// Seperate noise layers representing the probability of this biome spawning there
    /// </summary>
    public NoiseActivationSettings[] spawnPosition;

    /// <summary>
    /// Comunal noise layers representing the probability of this biome spawning there
    /// All noiselayers gets added before evaluation
    /// </summary>
    public NoiseAdditiveActivationSettings[] spawnPositionAddative;

    /// <summary>
    /// Noise layers representing the height for any given vertice
    /// </summary>
    public NoiseSettings[] verticeOffset;

    /// <summary>
    /// What prefabs and where they should spawn
    /// </summary>
    public InstantiateSettings[] prefabInstances;

    /// <summary>
    /// All materials for this biome
    /// Any null material on instance is refered to WorldGenerationSettings default BiomeMaterialSettings 
    /// </summary>
    public BiomeMaterialSettings materialSettings;

    /// <summary>
    /// 
    /// </summary>
    [ContextMenu("Update", false, -1000)]
    public void Update()
    {
        for (int i = 0; i < spawnPosition.Length; i++)
        {
            spawnPosition[i].Update();
        }
        for (int i = 0; i < spawnPositionAddative.Length; i++)
        {
            spawnPositionAddative[i].Update();
        }

        for (int i = 0; i < verticeOffset.Length; i++)
        {
            verticeOffset[i].Update();
        }

        for (int i = 0; i < prefabInstances.Length; i++)
        {
            prefabInstances[i].Update();
        }

        materialSettings.Update();
    }
}
