using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageByFire : MonoBehaviour
{
    public List<GameObject> all_enemy_game_objects;
    public List<Vector3> all_fire_spots;

    public float fire_distance = 10f;
    private float pow_fire_distance;

    private void Start()
    {
        UpdateFireDistance(fire_distance);
        InvokeRepeating("DamageClose", 0.25f, 0.25f);
    }

    private void UpdateFireDistance(float fire_distance)
    {
        pow_fire_distance = fire_distance * fire_distance;
    }

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
