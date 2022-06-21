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
                GameObject newGameObject = new GameObject();

                newGameObject.transform.parent = transform;
                newGameObject.transform.localPosition = Vector3.Scale(size, new Vector3(x, 0f, y));
            }
        }
    }
}
