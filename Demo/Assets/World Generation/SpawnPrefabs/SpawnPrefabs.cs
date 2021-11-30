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


    public IEnumerator Spawn(WaitForFixedUpdate wait, GameObject[] prefabs, float object_density, Vector2 area)
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

            yield return place.InitAsParrent(wait, AddToBoundingBoxes, GetBoundingBoxes, x, z, transform);
        }

        foreach (Collider collider in bounding_boxes)
        {
            Destroy(collider);
        }
        bounding_boxes = null;
    }
}
