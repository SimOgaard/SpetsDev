using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Defines how this world should generate. With global seeding.
/// Multiple selective biomes and where they should spawn.
/// Defines chunks and start spawn area.
/// </summary>
[CreateAssetMenu()]
public class WorldGenerationSettings : ScriptableObject
{
    /// <summary>
    /// Rules for how the spawn should be. Difficulity radius, spawn items, guide, etc.
    /// </summary>
    public class Spawn
    {

    }

    /// <summary>
    /// The biomes that should spawn
    /// </summary>
    public BiomeSettings[] biomes;
}
