using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderMeshManipulation : MonoBehaviour
{
    protected Bounds oldBounds;
    protected Collider _collider;

    /// <summary>
    /// Float that represent smallest position delta between any two triangles on our mesh
    /// Gets set from GroundMesh ChunkDetails UnitSize
    /// </summary>
    protected static float _triangleSizeMargin;
    protected static float triangleSizeMarginSquared;
    public static float triangleSizeMargin
    {
        get
        {
            return _triangleSizeMargin;
        }
        set
        {
            // add to the margin to reduce SwitchTriangles calls
            value *= 1.1f;
            triangleSizeMarginSquared = value * value;
            _triangleSizeMargin = value;
        }
    }

    public GroundMesh.MeshManipulationState meshManipulation;

    protected virtual void Start()
    {
        //for (int i = 0; i < meshManipulations.Length; i++)
        //{
        //    meshManipulations[i].changeToIndex = GroundMesh.MeshManipulationState.GroundTriangleTypeIndex((int)meshManipulations[i].changeTo);
        //}
        meshManipulation.changeTo = meshManipulation.changeTo;

        Debug.Log((int)meshManipulation.changeFrom);
        _collider = gameObject.GetComponent<Collider>();
        oldBounds = _collider.bounds;
    }

    protected virtual void FixedUpdate()
    {
        // if bounds has not grown or changed position given triangle margin that would make it possible to hit another triangle it has not been on before
        if (
            (oldBounds.center - _collider.bounds.center).sqrMagnitude > triangleSizeMarginSquared ||
            Mathf.Abs(oldBounds.extents.x - _collider.bounds.extents.x) > _triangleSizeMargin ||
            Mathf.Abs(oldBounds.extents.y - _collider.bounds.extents.y) > _triangleSizeMargin ||
            Mathf.Abs(oldBounds.extents.z - _collider.bounds.extents.z) > _triangleSizeMargin
            )
        {
            oldBounds = _collider.bounds;
            SwitchTriangles();
        }
    }

    protected virtual void SwitchTriangles()
    {
        Debug.Log("SwitchTriangles");
        Chunk[] chunks = WorldGenerationManager.ReturnAllCunksInBounds(oldBounds);
        for (int i = 0; i < chunks.Length; i++)
        {
            if (chunks[i] != null && chunks[i].isLoaded)
            {
                chunks[i].groundMesh.SwitchTrainglesInCollider(_collider, meshManipulation);
            }
        }
    }
}
