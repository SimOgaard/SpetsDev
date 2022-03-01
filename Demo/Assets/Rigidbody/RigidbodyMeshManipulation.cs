using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigidbodyMeshManipulation : ColliderMeshManipulation
{
    protected Rigidbody _rigidbody;

    protected override void Start()
    {
        base.Start();
        _rigidbody = gameObject.GetComponent<Rigidbody>();
    }

    protected override void FixedUpdate()
    {
        if (!_rigidbody.IsSleeping())
        {
            SwitchTriangles();
        }
    }
}
