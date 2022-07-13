using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Defines rules for the player spawn area/position. Difficulity radius, spawn items, guide, etc.
/// </summary>
[CreateAssetMenu(menuName = "WorldGeneration/Spawn", order = 2)]
[System.Serializable]
public class SpawnSettings : Settings
{
    public string idk;

    [ContextMenu("Rename", false, 500)]
    public override void Rename()
    {
        base.Rename();
    }
}
