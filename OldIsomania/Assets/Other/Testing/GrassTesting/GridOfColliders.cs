using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridOfColliders : MonoBehaviour
{
    [SerializeField] private Vector3 size;
    [SerializeField] private Vector2Int grid;

    [SerializeField] private Transform dictionaryTestingTransform;
    [SerializeField] private Transform arrayTestingTransform;

    private void Start()
    {
        for (int x = -grid.x; x <= grid.x; x++)
        {
            for (int y = -grid.y; y <= grid.y; y++)
            {

                GameObject newGameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Collider col = newGameObject.GetComponent<Collider>();
                col.isTrigger = true;
                newGameObject.GetComponent<MeshRenderer>().enabled = false;

                newGameObject.transform.parent = transform;
                newGameObject.transform.localPosition = Vector3.Scale(size, new Vector3(x, 0f, y));
                newGameObject.transform.localScale = size;

                foreach (Transform child in dictionaryTestingTransform)
                {
                    child.GetChild(0).GetComponent<GrassTestingWithDictionary>().testColliders.Add(col);
                }
                foreach (Transform child in arrayTestingTransform)
                {

                    child.GetChild(0).GetComponent<GrassTestingWithArray>().testColliders.Add(col);
                }
            }
        }
    }
}
