using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPrefabs : MonoBehaviour
{
    [SerializeField] private List<Collider> bounding_boxes;

    private void AddToBoundingBoxes(List<Collider> colliders)
    {
        bounding_boxes.AddRange(colliders);
    }

    private List<Collider> GetBoundingBoxes()
    {
        return bounding_boxes;
    }


    public IEnumerator Spawn(WaitForFixedUpdate wait, GameObject[] prefabs, float object_density, Vector2 area, float chunk_load_speed)
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            yield break;
        }
#endif

        int prefab_length = prefabs.Length;
        if (prefab_length == 0)
        {
            yield break;
        }

        List<Task> tasks = new List<Task>();

        float object_amount = object_density * area.x * area.y;
        float wait_amount = Mathf.Max(object_amount * object_density);
        //Debug.Log("tried spawning: " + object_amount + " objects on: " + transform.name);
        bounding_boxes = new List<Collider>();
        for (int i = 0; i < object_amount; i++)
        {
            float x = Random.Range(-0.5f, 0.5f) * area.x;
            float z = Random.Range(-0.5f, 0.5f) * area.y;
            int prefab_index = Mathf.RoundToInt(Random.value * (prefabs.Length - 1));

            if (prefabs[prefab_index] == null)
            {
                continue;
            }
            GameObject new_game_object = Instantiate(prefabs[prefab_index], transform.position, Quaternion.identity, transform.parent.parent);
            PlaceInWorld place = new_game_object.GetComponent<PlaceInWorld>();

            tasks.Add(new Task(place.InitAsParrent(wait, AddToBoundingBoxes, GetBoundingBoxes, x, z, transform)));

            if (i % (wait_amount) == 0)
            {
                yield return wait;
            }
        }

        bool continue_with_list = true;
        while (continue_with_list)
        {
            continue_with_list = false;
            yield return wait;
            foreach (Task task in tasks)
            {
                if (task.Running)
                {
                    continue_with_list = true;
                    break;
                }
            }
        }

        foreach (Collider collider in bounding_boxes)
        {
            Destroy(collider);
        }
        bounding_boxes = null;
    }
}
