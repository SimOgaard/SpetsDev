using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Defines all materials for a given biome.
/// </summary>
[CreateAssetMenu()]
[System.Serializable]
public class BiomeMaterialSettings : ScriptableObject
{
    [System.Serializable]
    public class BiomeMaterial
    {
        /// <summary>
        /// The static ground material for this biome
        /// </summary>
        public MaterialSettings biomeMaterial;

        /// <summary>
        /// All possible foliages for the biomeMaterial, the nonstatic triangle over biomeMaterial triangle
        /// If .Length == 0 draw no triangle
        /// </summary>
        public FoliageSettings[] biomeFoliages;

        /// <summary>
        /// Creates curvetextures from all curves of biomeMaterial and all foliages
        /// Is done once at runtime
        /// </summary>
        public void Update()
        {
            biomeMaterial.Update();

            for (int i = 0; i < biomeFoliages.Length; i++)
            {
                biomeFoliages[i].Update();
            }
        }
    }

    /// <summary>
    /// The ground material for this biome
    /// References foliages since it is material meant for ground
    /// Can be used on enemies to signal what biome they stem from
    /// </summary>
    public BiomeMaterial biomeMaterial;

    /// <summary>
    /// All prefabs spawned with these next materialSettings in this biome gets a reference to this material
    /// </summary>
    public MaterialSettings stoneMaterial;
    // fire, water, stone, etc enemies could reference materials from their spawning biome depending on what triangle they raycasted

    /// <summary>
    /// Creates curvetextures from all curves of all materials and all of those materials foliages
    /// Is done once at runtime
    /// </summary>
    public void Update()
    {
        biomeMaterial.Update();
    }
}
