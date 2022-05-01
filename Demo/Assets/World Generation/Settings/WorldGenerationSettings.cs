using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Defines how this world should generate. With global seeding.
/// Multiple selective biomes and where they should spawn.
/// Defines chunks and start spawn area.
/// </summary>
[CreateAssetMenu()]
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
    /// The biomes that should spawn
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
    }
}
