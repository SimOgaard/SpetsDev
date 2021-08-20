using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPrefabs : MonoBehaviour
{
    public static List<Collider> bounding_boxes = new List<Collider>();

    private List<GameObject> all_instanciated_game_objects = new List<GameObject>();

    public void Spawn(GameObject[] prefabs)
    {
        if (!Application.isPlaying)
        {
            return;
        }

        /*
        foreach(GameObject game_object in all_instanciated_game_objects)
        {
            DestroyImmediate(gameObject);
        }
        */

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

        for (int i = 0; i < 200; i++)
        {
            float x = Random.Range(-1f, 1f) * 100f;
            float z = Random.Range(-1f, 1f) * 100f;

            Instantiate(prefabs[Mathf.RoundToInt(Random.value * (prefabs.Length - 1))], new Vector3(x, 100f, z), Quaternion.identity);
            //all_instanciated_game_objects.Add(instanciated);
        }

        //List<GameObject> game_objects = new List<GameObject>();
        //Random.InitState(1337);

        // YOU NEED TO KNOW: does unity raycasthit account for newly placed collider

        // select random x and z values
        // choose random placable_objects to instanciate

        // after everything is placed, go thorugh all of them and set layer to 12
    }
}
