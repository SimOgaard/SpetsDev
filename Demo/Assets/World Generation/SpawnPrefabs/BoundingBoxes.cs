using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundingBoxes : MonoBehaviour
{
    [SerializeField] private List<Collider> boundingBoxes = new List<Collider>();
    [SerializeField] private bool destroyChildren = false;

    public bool ShouldDestroyChildren()
    {
        return destroyChildren;
    }

    public List<Collider> GetBoundingBoxes()
    {
        return boundingBoxes;
    }
}
