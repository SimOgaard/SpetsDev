using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Defines how a singular game object should spawn.
/// </summary>
[CreateAssetMenu(menuName = "WorldGeneration/Instanciate/Instanciate", order = 2)]
[System.Serializable]
public class InstantiateSettings : ScriptableObject
{
    /// <summary>
    /// What prefab should spawn
    /// </summary>
    public GameObject prefab;

    /// <summary>
    /// How densly this prefab should spawn, accounts for chunkSize
    /// </summary>
    public float objectDensity = 0.01f;

    public void Update()
    {

    }
}
