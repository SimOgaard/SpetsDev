using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantiatePrefabs : MonoBehaviour
{
    [SerializeField] private List<Collider> boundingBoxes;

    private void AddToBoundingBoxes(List<Collider> colliders)
    {
        boundingBoxes.AddRange(colliders);
    }

    private List<Collider> GetBoundingBoxes()
    {
        return boundingBoxes;
    }


    public IEnumerator Spawn(WaitForFixedUpdate wait, GameObject[] prefabs, float objectDensity, Vector2 area)
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            yield break;
        }
#endif

        int prefabLength = prefabs.Length;
        if (prefabLength == 0)
        {
            yield break;
        }

        float objectAmount = objectDensity * area.x * area.y;
        boundingBoxes = new List<Collider>();
        for (int i = 0; i < objectAmount; i++)
        {
            float x = Random.Range(-0.5f, 0.5f) * area.x;
            float z = Random.Range(-0.5f, 0.5f) * area.y;
            int prefabIndex = Mathf.RoundToInt(Random.value * (prefabs.Length - 1));

            if (prefabs[prefabIndex] == null)
            {
                continue;
            }
            GameObject newGameObject = Instantiate(prefabs[prefabIndex], transform.position, Quaternion.identity, transform.parent.parent);
            PlaceInWorld place = newGameObject.GetComponent<PlaceInWorld>();

            yield return place.InitAsParrent(wait, AddToBoundingBoxes, GetBoundingBoxes, x, z, transform);
        }

        foreach (Collider collider in boundingBoxes)
        {
            Destroy(collider);
        }
        boundingBoxes = null;
    }
}
