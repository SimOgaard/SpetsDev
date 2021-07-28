using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Iterates through all GameObjects in all_enemy_game_objects and damages gameobjects within given distance of fire.
/// </summary>
public class DamageByFire : MonoBehaviour
{
    public List<GameObject> all_enemy_game_objects;
    public List<Vector3> all_fire_spots;

    [SerializeField] private float fire_distance = 4f;
    private float pow_fire_distance;

    /// <summary>
    /// Updates fire distance and invokes DamageClose() repeating with interval between each invoke to minimize compute and damage applied.
    /// </summary>
    private void Start()
    {
        UpdateFireDistance(fire_distance);
        InvokeRepeating("DamageClose", 0.25f, 0.25f);
    }

    /// <summary>
    /// Updates the squared fire distance.
    /// </summary>
    public void UpdateFireDistance(float fire_distance)
    {
        pow_fire_distance = fire_distance * fire_distance;
    }

    /// <summary>
    /// Iterates through all GameObjects in all_enemy_game_objects and damages gameobjects within given distance of fire.
    /// </summary>
    public void DamageClose()
    {
        foreach (GameObject game_object in all_enemy_game_objects)
        {
            foreach(Vector3 fire_pos in all_fire_spots)
            {
                Vector3 game_object_pos = game_object.transform.position;
                float distance = (game_object_pos - fire_pos).sqrMagnitude;
                if (distance < pow_fire_distance)
                {
                    Debug.Log("stood in fire " + game_object.name);
                    break;
                }
            }
        }
    }

}
