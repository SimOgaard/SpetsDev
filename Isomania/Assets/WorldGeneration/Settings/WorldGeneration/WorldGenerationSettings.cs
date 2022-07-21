using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/// <summary>
/// Defines how this world should generate. With global seeding.
/// Multiple selective biomes and where they should spawn.
/// Defines chunks and start spawn area.
/// </summary>
[CreateAssetMenu(menuName = "WorldGeneration/WorldGeneration", order = 0)]
[System.Serializable]
public class WorldGenerationSettings : Settings
{
    /// <summary>
    /// The global seed, all seeds get offset by this value
    /// </summary>
    public int globalSeed = 1337;

    /// <summary>
    /// Rules for the player spawn 
    /// </summary>
    public SpawnSettings playerSpawn;

    /// <summary>
    /// Defines all chunks
    /// </summary>
    public ChunkSettings chunk;

    /// <summary>
    /// Defines day night
    /// </summary>
    public DayNightSettings dayNight;

    /// <summary>
    /// Defines the wind, 123 SEE IF YOU CAN HAVE ONE WINDSETTING FOR EACH BIOME, YOU NEED TO BE ABLE TO GET CURRENT PIXEL OR VERTEX BIOME (TRANSLATE WORLD TO BIOME TEXTURE)
    /// </summary>
    public WindSettings wind;

    /// <summary>
    /// Defines the global water, 123 YOU SHOULD HAVE A WATER FOR EACH BIOME
    /// </summary>
    public WaterSettings water;

    /*
    /// <summary>
    /// All BiomeMaterials for all BiomeMaterialSettings that are null references back to this
    /// </summary>
    public BiomeMaterialSettings defaultBiomeMaterials;
    */

    /// <summary>
    /// The world is firstly constructed from a given set of ring biomes that should spawn from edge of map moving twords middle.
    /// This array keeps its order.
    /// </summary>
    [NonReorderable]
    public RingSettings[] rings;

    /// <summary>
    /// All regions of a rings inscribed angle always sums to 360 degrees
    /// </summary>
    public void NormalizeInscribedAngle()
    {
        // for all rings
        for (int i = 0; i < rings.Length; i++)
        {
            // normalize the rings angle to 360 degrees
            rings[i].NormalizeInscribedAngle();
        }
    }

    [ContextMenu("Rename", false, 500)]
    public override void Rename()
    {
        base.Rename();
    }

    [ContextMenu("Regenerate", false, -1001)]
    private void Regenerate()
    {
        Global.worldGenerationManager.Regenerate();
    }

    [ContextMenu("Update", false, -1000)]
    public override void Update()
    {
        if (chunk != null)
            chunk.Update();

        if (dayNight != null)
            dayNight.Update();

        if (wind != null)
            wind.Update();

        if (water != null)
            water.Update();

        //defaultBiomeMaterials.Update();

        for (int i = 0; i < rings.Length; i++)
        {
            rings[i].Update();
        }

        NormalizeInscribedAngle();

        if (false)
            Ground.Update(this);
    }

    /// <summary>
    /// clears all data that has no gc collect like computebuffers
    /// </summary>
    public override void Destroy()
    {
        dayNight.Destroy();
    }

    /// <summary>
    /// Adds all update renders to pixelperfect update render deligate
    /// </summary>
    public void AddUpdateRenders()
    {
        //PixelPerfect.updateRenders += dayNightCycleSettings.UpdateRender;
    }

    /// <summary>
    /// Get all FNL (fast noise lite) states that says which triangle should be in which biome
    /// </summary>
    public List<NoiseSettings.fnl_state> GetBiomeFNLStates()
    {
        List<NoiseSettings.fnl_state> fnlStates = new List<NoiseSettings.fnl_state>();
        /*
        for (int i = 0; i < biomes.Length; i++)
        {
            for (int q = 0; q < biomes[i].spawnConditions.Length; q++)
            {
                fnlStates.Add(biomes[i].spawnConditions[q].ToFNLState(i, false));
            }
        }
        */
        return fnlStates;
    }

    /// <summary>
    /// Get all FNL (fast noise lite) warp states that says which triangle should be in which biome
    /// </summary>
    public List<NoiseSettings.fnl_state> GetBiomeFNLWarpStates()
    {
        List<NoiseSettings.fnl_state> fnlStates = new List<NoiseSettings.fnl_state>();
        /*
        for (int i = 0; i < biomes.Length; i++)
        {
            for (int q = 0; q < biomes[i].spawnConditions.Length; q++)
            {
                fnlStates.Add(biomes[i].spawnConditions[q].ToFNLState(i, true));
            }
        }
        */
        return fnlStates;
    }

    /// <summary>
    /// Get all FNL (fast noise lite) states that offsets the vertices
    /// </summary>
    public List<NoiseSettings.fnl_state> GetVerticeOffsetFNLStates()
    {
        List<NoiseSettings.fnl_state> fnlStates = new List<NoiseSettings.fnl_state>();
        /*
        for (int i = 0; i < biomes.Length; i++)
        {
            for (int q = 0; q < biomes[i].elevation.Length; q++)
            {
                fnlStates.Add(biomes[i].elevation[q].ToFNLState(i, false));
            }
        }
        */
        return fnlStates;
    }

    /// <summary>
    /// Get all FNL (fast noise lite) warp states that offsets the vertices
    /// </summary>
    public List<NoiseSettings.fnl_state> GetVerticeOffsetFNLWarpStates()
    {
        List<NoiseSettings.fnl_state> fnlStates = new List<NoiseSettings.fnl_state>();
        /*
        for (int i = 0; i < biomes.Length; i++)
        {
            for (int q = 0; q < biomes[i].elevation.Length; q++)
            {
                fnlStates.Add(biomes[i].elevation[q].ToFNLState(i, true));
            }
        }
        */
        return fnlStates;
    }

    /// <summary>
    /// Get all static materials
    /// </summary>
    public List<Material> GetStaticMaterials()
    {
        List<Material> materials = new List<Material>();
        /*
        for (int i = 0; i < biomes.Length; i++)
        {
            for (int q = 0; q < biomes[i].materials.biomeMaterialsCondition.Length; q++)
            {
                materials.Add(biomes[i].materials.biomeMaterialsCondition[q].material.shaderMaterial);
            }
        }
        */
        // remove duplicates

        return materials;
    }

    /// <summary>
    /// Get main static material from each biome
    /// </summary>
    public List<Material> GetMainStaticMaterials()
    {
        List<Material> materials = new List<Material>();
        /*
        for (int i = 0; i < biomes.Length; i++)
        {
            materials.Add(biomes[i].materials.biomeMaterial.material.shaderMaterial);
        }
        */
        // remove duplicates

        return materials;
    }
}
