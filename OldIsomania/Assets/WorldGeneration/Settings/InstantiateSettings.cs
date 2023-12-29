using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Defines how a singular game object should spawn.
/// </summary>
[CreateAssetMenu(menuName = "WorldGeneration/Instanciate/Instanciate", order = 2)]
[System.Serializable]
public class InstantiateSettings : Settings
{
    /// <summary>
    /// What prefab should spawn
    /// </summary>
    public GameObject prefab;

    /// <summary>
    /// How densly this prefab should spawn, accounts for meshSize
    /// </summary>
    public float objectDensity = 0.01f;

    [ContextMenu("Rename", false, 500)]
    public override void Rename()
    {
        base.Rename();
    }

    [ContextMenu("Update", false, -1000)]
    public override void Update()
    {

    }
}
