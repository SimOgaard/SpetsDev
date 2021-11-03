using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemies : MonoBehaviour
{
    public static readonly string[] enemy_names = { "lGolems", "mGolems", "sGolems" };

    /// <summary>
    /// List of all enemies.
    /// </summary>
    public static List<EnemyAI> all_enemy_ais = new List<EnemyAI>();

    /*
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
    */

    private void AddAllEnemiesInHierarchy()
    {
        for (int i = 0; i < enemy_names.Length; i++)
        {
            new GameObject(enemy_names[i]).transform.parent = transform;
        }
    }

    private void Awake()
    {
        AddAllEnemiesInHierarchy();
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

    public void MoveParrent()
    {
        string name;
        foreach (Transform enemy_parrent in transform)
        {
            name = enemy_parrent.name;

            foreach (Transform child in enemy_parrent)
            {
                Transform new_transform = WorldGenerationManager.ReturnNearestChunk(child.position);

                if (new_transform != transform.parent)
                {
                    child.parent = new_transform.Find("Enemies").Find(name);
                }
            }
        }
    }


    public static void Sound(Transform sound_origin, float sound, float max_sound = Mathf.Infinity, float hearing_threshold_change = 1f)
    {
        foreach (EnemyAI enemy_ai in all_enemy_ais)
        {
            if (enemy_ai.gameObject.activeInHierarchy)
            {
                float distance_value = (enemy_ai.transform.position - sound_origin.position).sqrMagnitude + 0.0001f;
                float sound_level = (sound * enemy_ai.hearing_amplification) / distance_value;
                float filtered_sound_level = Mathf.Min(max_sound, sound_level);
                enemy_ai.AttendToSound(sound_origin, filtered_sound_level, hearing_threshold_change);
            }
        }
    }
}
