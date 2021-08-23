using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPrefabs : MonoBehaviour
{
    public static List<Collider> bounding_boxes = new List<Collider>();

    public void Spawn(GameObject[] prefabs)
    {
        if (!Application.isPlaying)
        {
            return;
        }

        int prefab_length = prefabs.Length;
        if (prefab_length == 0)
        {
            return;
        }
        else
        {
            for (int i = 0; i < prefab_length; i++)
            {
                if (prefabs[i] == null)
                {
                    return;
                }
            }
        }

        bounding_boxes = new List<Collider>();

        for (int i = 0; i < 100; i++)
        {
            float x = Random.Range(-1f, 1f) * 100f;
            float z = Random.Range(-1f, 1f) * 100f;

            GameObject new_game_object = Instantiate(prefabs[Mathf.RoundToInt(Random.value * (prefabs.Length - 1))]);
            PlaceInWorld place = new_game_object.GetComponent<PlaceInWorld>();
            place.InitAsParrent(x, z);
        }

        foreach (Collider collider in bounding_boxes)
        {
            Destroy(collider);
        }
        bounding_boxes = null;
    }
}
