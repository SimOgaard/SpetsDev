using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundingBoxes : MonoBehaviour
{
    [SerializeField] private List<Collider> bounding_boxes = new List<Collider>();
    [SerializeField] private bool destroy_children = false;

    public bool ShouldDestroyChildren()
    {
        return destroy_children;
    }

    public List<Collider> GetBoundingBoxes()
    {
        StartCoroutine(DestroyAfterReturn());
        return bounding_boxes;
    }

    private IEnumerator DestroyAfterReturn()
    {
        yield return new WaitForFixedUpdate();
        Destroy(this);
    }
}
