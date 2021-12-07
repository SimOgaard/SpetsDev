using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridOfGrassTestingWithDictionary : MonoBehaviour
{
    [SerializeField] private Vector3 size;
    [SerializeField] private Vector2Int grid;

    private void Start()
    {
        for (int x = -grid.x; x <= grid.x; x++)
        {
            for (int y = -grid.y; y <= grid.y; y++)
            {
                GameObject new_game_object = new GameObject();

                new_game_object.transform.parent = transform;
                new_game_object.transform.localPosition = Vector3.Scale(size, new Vector3(x, 0f, y));
            }
        }
    }
}
