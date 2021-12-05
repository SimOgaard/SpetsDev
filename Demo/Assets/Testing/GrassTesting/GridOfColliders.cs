using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridOfColliders : MonoBehaviour
{
    [SerializeField] private Vector3 size;
    [SerializeField] private Vector2Int grid;

    [SerializeField] private Transform dictionary_testing_transform;
    [SerializeField] private Transform array_testing_transform;

    private void Start()
    {
        for (int x = -grid.x; x <= grid.x; x++)
        {
            for (int y = -grid.y; y <= grid.y; y++)
            {

                GameObject new_game_object = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Collider col = new_game_object.GetComponent<Collider>();
                col.isTrigger = true;
                new_game_object.GetComponent<MeshRenderer>().enabled = false;

                new_game_object.transform.parent = transform;
                new_game_object.transform.localPosition = Vector3.Scale(size, new Vector3(x, 0f, y));
                new_game_object.transform.localScale = size;

                foreach (Transform child in dictionary_testing_transform)
                {
                    child.GetComponent<GrassTestingWithDictionary>().test_colliders.Add(col);
                }
                foreach (Transform child in array_testing_transform)
                {
                    child.GetComponent<GrassTestingWithArray>().test_colliders.Add(col);
                }
            }
        }
    }
}
