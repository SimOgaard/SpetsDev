using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemies : MonoBehaviour
{
    /// <summary>
    /// List of all enemies.
    /// </summary>
    public static List<EnemyAI> all_enemy_ais = new List<EnemyAI>();

    private void AddAllEnemiesInHierarchy(Transform parrent)
    {
        foreach (Transform child in parrent)
        {
            if (child.TryGetComponent(out EnemyAI enemy_ai))
            {
                AddEnemyToList(enemy_ai);
            }
            else
            {
                AddAllEnemiesInHierarchy(child);
            }
        }
    }

    private void Start()
    {
        AddAllEnemiesInHierarchy(transform);
    }

    /// <summary>
    /// Adds EnemyAi component pointer to all_enemy_ais list.
    /// </summary>
    public static void AddEnemyToList(EnemyAI enemy_ai)
    {
        all_enemy_ais.Add(enemy_ai);
    }

    /// <summary>
    /// Removes ALL null objects in all_enemy_ais list.
    /// </summary>
    public static void RemoveEnemyFromList()
    {
        all_enemy_ais.RemoveAll(enemy_ai => enemy_ai == null);
    }
}
