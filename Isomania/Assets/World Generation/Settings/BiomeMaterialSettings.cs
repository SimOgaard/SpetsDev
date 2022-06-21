using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Defines all materials for a given biome.
/// </summary>
[CreateAssetMenu(menuName = "WorldGeneration/Biome/BiomeMaterial", order = 20)]
[System.Serializable]
public class BiomeMaterialSettings : ScriptableObject
{
    [System.Serializable]
    public class BiomeMaterial
    {
        /// <summary>
        /// The static ground material for this biome
        /// </summary>
        public MaterialSettings materialSettings;

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
            materialSettings.Update();

            for (int i = 0; i < biomeFoliages.Length; i++)
            {
                biomeFoliages[i].Update();
            }
        }
    }

    [System.Serializable]
    public class BiomeMaterialCondition
    {
        /// <summary>
        /// The static conditional ground material for this biome
        /// </summary>
        public MaterialSettings biomeMaterial;

        /// <summary>
        /// All possible foliages for the biomeMaterial, the nonstatic triangle over biomeMaterial triangle
        /// If .Length == 0 draw no triangle
        /// </summary>
        public FoliageSettings[] biomeFoliages;

        /// <summary>
        /// At what condition should this triangle spawn
        /// </summary>
        public Condition condition;

        [System.Serializable]
        public class Condition
        {
            public float minHeight = -10f;
            public float maxHeight = 10f;

            [Range(-1f, 1f)] public float minNormalDot = -1f;
            [Range(-1f, 1f)] public float maxNormalDot = 1f;
        }

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
    /// The main ground material for this biome
    /// References foliages since it is material meant for ground
    /// Can be used on enemies to signal what biome they stem from (fire, water, grass)
    /// </summary>
    public BiomeMaterial biomeMaterial;

    /// <summary>
    /// The other ground materials for this biome which gets added on condition
    /// </summary>
    public BiomeMaterialCondition[] biomeMaterialsCondition;

    /// <summary>
    /// All prefabs spawned with these next materialSettings in this biome gets a reference to this material
    /// Can be used on enemies to signal what biome they stem from
    /// </summary>
    public MaterialSettings stoneMaterialSettings;
    // fire, water, stone, etc enemies could reference materials from their spawning biome depending on what triangle they raycasted

    /// <summary>
    /// Creates curvetextures from all curves of all materials and all of those materials foliages
    /// Is done once at runtime
    /// </summary>
    public void Update()
    {
        biomeMaterial.Update();
        for (int i = 0; i < biomeMaterialsCondition.Length; i++)
        {
            biomeMaterialsCondition[i].Update();
        }
    }
}
