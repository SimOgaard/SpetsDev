using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderMeshManipulation : MonoBehaviour
{
    protected Bounds old_bounds;
    protected Collider _collider;

    /// <summary>
    /// Float that represent smallest position delta between any two triangles on our mesh
    /// Gets set from GroundMesh ChunkDetails UnitSize
    /// </summary>
    protected static float _triangle_size_margin;
    protected static float triangle_size_margin_squared;
    public static float triangle_size_margin
    {
        get
        {
            return _triangle_size_margin;
        }
        set
        {
            // add to the margin to reduce SwitchTriangles calls
            value *= 1.1f;
            triangle_size_margin_squared = value * value;
            _triangle_size_margin = value;
        }
    }

    public GroundMesh.MeshManipulationState mesh_manipulation;

    protected virtual void Start()
    {
        //for (int i = 0; i < mesh_manipulations.Length; i++)
        //{
        //    mesh_manipulations[i].change_to_index = GroundMesh.MeshManipulationState.GroundTriangleTypeIndex((int)mesh_manipulations[i].change_to);
        //}
        mesh_manipulation.change_to = mesh_manipulation.change_to;

        Debug.Log((int)mesh_manipulation.change_from);
        _collider = gameObject.GetComponent<Collider>();
        old_bounds = _collider.bounds;
    }

    protected virtual void FixedUpdate()
    {
        // if bounds has not grown or changed position given triangle margin that would make it possible to hit another triangle it has not been on before
        if (
            (old_bounds.center - _collider.bounds.center).sqrMagnitude > triangle_size_margin_squared ||
            Mathf.Abs(old_bounds.extents.x - _collider.bounds.extents.x) > _triangle_size_margin ||
            Mathf.Abs(old_bounds.extents.y - _collider.bounds.extents.y) > _triangle_size_margin ||
            Mathf.Abs(old_bounds.extents.z - _collider.bounds.extents.z) > _triangle_size_margin
            )
        {
            old_bounds = _collider.bounds;
            SwitchTriangles();
        }
    }

    protected virtual void SwitchTriangles()
    {
        Debug.Log("SwitchTriangles");
        Chunk[] chunks = WorldGenerationManager.ReturnAllCunksInBounds(old_bounds);
        for (int i = 0; i < chunks.Length; i++)
        {
            if (chunks[i] != null && chunks[i].is_loaded)
            {
                chunks[i].ground_mesh.SwitchTrainglesInCollider(_collider, mesh_manipulation);
            }
        }
    }
}
