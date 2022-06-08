using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Defines how this world should generate. With global seeding.
/// Multiple selective biomes and where they should spawn.
/// Defines chunks and start spawn area.
/// </summary>
[CreateAssetMenu(menuName = "WorldGeneration/WorldGeneration", order = 0)]
[System.Serializable]
public class WorldGenerationSettings : ScriptableObject
{
    /// <summary>
    /// Rules for the player spawn 
    /// </summary>
    public SpawnSettings spawnSettings;

    /// <summary>
    /// The global seed, all seeds get offset by this value
    /// </summary>
    public int globalSeed = 1337;

    /// <summary>
    /// Defines all chunks
    /// </summary>
    public ChunkSettings chunkSettings;

    /// <summary>
    /// Defines day night
    /// </summary>
    public DayNightCycleSettings dayNightCycleSettings;

    /// <summary>
    /// Defines the wind
    /// </summary>
    public WindSettings windSettings;

    /// <summary>
    /// Defines the global water
    /// </summary>
    public WaterSettings waterSettings;

    /// <summary>
    /// All BiomeMaterials for all BiomeMaterialSettings that are null references back to this
    /// </summary>
    public BiomeMaterialSettings defaultBiomeMaterials;

    /// <summary>
    /// The biomes that should spawn, the priority of each biome is represented by its index
    /// </summary>
    public BiomeSettings[] biomes;

    /// <summary>
    /// Runs all update functions for all children settings under this asset
    /// OBS! since some settings are shared asset.Update() could be ran multiple times
    /// </summary>
    public void Update()
    {
        chunkSettings.Update();

        dayNightCycleSettings.Update();

        windSettings.Update();
        
        waterSettings.Update();

        defaultBiomeMaterials.Update();

        for (int i = 0; i < biomes.Length; i++)
        {
            biomes[i].Update();
        }

        Ground.Update(this);
    }

    /// <summary>
    /// clears all data that has no gc collect like computebuffers
    /// </summary>
    public void Destroy()
    {
        dayNightCycleSettings.Destroy();
    }

    /// <summary>
    /// Get all FNL (fast noise lite) states that says which triangle should be in which biome
    /// </summary>
    public List<NoiseSettings.fnl_state> GetBiomeFNLStates()
    {
        List<NoiseSettings.fnl_state> fnlStates = new List<NoiseSettings.fnl_state>();

        for (int i = 0; i < biomes.Length; i++)
        {
            for (int q = 0; q < biomes[i].spawnPosition.Length; q++)
            {
                fnlStates.Add(biomes[i].spawnPosition[q].ToFNLState(i, false));
            }
        }

        return fnlStates;
    }

    /// <summary>
    /// Get all FNL (fast noise lite) warp states that says which triangle should be in which biome
    /// </summary>
    public List<NoiseSettings.fnl_state> GetBiomeFNLWarpStates()
    {
        List<NoiseSettings.fnl_state> fnlStates = new List<NoiseSettings.fnl_state>();

        for (int i = 0; i < biomes.Length; i++)
        {
            for (int q = 0; q < biomes[i].spawnPosition.Length; q++)
            {
                fnlStates.Add(biomes[i].spawnPosition[q].ToFNLState(i, true));
            }
        }

        return fnlStates;
    }

    /// <summary>
    /// Get all FNL (fast noise lite) states that offsets the vertices
    /// </summary>
    public List<NoiseSettings.fnl_state> GetVerticeOffsetFNLStates()
    {
        List<NoiseSettings.fnl_state> fnlStates = new List<NoiseSettings.fnl_state>();

        for (int i = 0; i < biomes.Length; i++)
        {
            for (int q = 0; q < biomes[i].verticeOffset.Length; q++)
            {
                fnlStates.Add(biomes[i].verticeOffset[q].ToFNLState(i, false));
            }
        }

        return fnlStates;
    }

    /// <summary>
    /// Get all FNL (fast noise lite) warp states that offsets the vertices
    /// </summary>
    public List<NoiseSettings.fnl_state> GetVerticeOffsetFNLWarpStates()
    {
        List<NoiseSettings.fnl_state> fnlStates = new List<NoiseSettings.fnl_state>();

        for (int i = 0; i < biomes.Length; i++)
        {
            for (int q = 0; q < biomes[i].verticeOffset.Length; q++)
            {
                fnlStates.Add(biomes[i].verticeOffset[q].ToFNLState(i, true));
            }
        }

        return fnlStates;
    }

    /// <summary>
    /// Get all static materials
    /// </summary>
    public List<Material> GetStaticMaterials()
    {
        List<Material> materials = new List<Material>();

        for (int i = 0; i < biomes.Length; i++)
        {
            for (int q = 0; q < biomes[i].materialSettings.biomeMaterialsCondition.Length; q++)
            {
                materials.Add(biomes[i].materialSettings.biomeMaterialsCondition[q].biomeMaterial.material);
            }
        }

        // remove duplicates

        return materials;
    }

    /// <summary>
    /// Get main static material from each biome
    /// </summary>
    public List<Material> GetMainStaticMaterials()
    {
        List<Material> materials = new List<Material>();

        for (int i = 0; i < biomes.Length; i++)
        {
            materials.Add(biomes[i].materialSettings.biomeMaterial.materialSettings.material);
        }

        // remove duplicates

        return materials;
    }
}
