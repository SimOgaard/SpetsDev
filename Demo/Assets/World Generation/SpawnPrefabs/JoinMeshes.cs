using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JoinMeshes : MonoBehaviour
{
    private GameObject _gameObject = null;
    private Material material;
    private bool is_collider = false;
    private bool merge_all_children = true;

    public void SetMaterial(Material material)
    {
        this.material = material;
    }

    public void SetCollider()
    {
        is_collider = true;
    }

    public void SetMergeByTags(bool merge_by_tags)
    {
        merge_all_children = !merge_by_tags;
    }

    public void SetGameObjectToHoldJoinedMeshes(GameObject gameObject)
    {
        _gameObject = gameObject;
    }

    public static UnityEngine.Component GetAddComponent(GameObject gameObject, System.Type component_to_get)
    {
        UnityEngine.Component component = gameObject.GetComponent(component_to_get);
        if (component == null)
        {
            component = gameObject.AddComponent(component_to_get);
        }

        return component;
    }

    private IEnumerator Start()
    {
        yield return new WaitForFixedUpdate();

        if(_gameObject == null)
        {
            _gameObject = gameObject;
        }

        MeshRenderer mesh_renderer = GetAddComponent(_gameObject, typeof(MeshRenderer)) as MeshRenderer;
        MeshFilter mesh_filter = GetAddComponent(_gameObject, typeof(MeshFilter)) as MeshFilter;
        MeshCollider mesh_collider = is_collider ? GetAddComponent(_gameObject, typeof(MeshCollider)) as MeshCollider : null;

        Vector3 old_scale = transform.localScale;
        Quaternion old_rot = transform.rotation;
        Vector3 old_pos = transform.position;
        transform.localScale = new Vector3(1f, 1f, 1f);
        transform.rotation = Quaternion.identity;
        transform.position = Vector3.zero;

        MeshFilter[] mesh_filters_array = GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[mesh_filters_array.Length];

        for (int i = 0; i < mesh_filters_array.Length; i++)
        {
            if (merge_all_children || Tag.IsTaggedWith(mesh_filters_array[i].tag, Tag.merge))
            {
                combine[i].mesh = mesh_filters_array[i].sharedMesh;
                combine[i].transform = mesh_filters_array[i].transform.localToWorldMatrix;
            }
        }
        Mesh mesh = mesh_filter.mesh = new Mesh()
        {
            indexFormat = UnityEngine.Rendering.IndexFormat.UInt16
        };
        mesh.CombineMeshes(combine);
        if (is_collider)
        {
            mesh_collider.sharedMesh = mesh;
        }
        mesh_renderer.sharedMaterial = material;

        foreach (Transform child in transform)
        {
            if (merge_all_children || Tag.IsTaggedWith(child.tag, Tag.merge))
            {
                Destroy(child.gameObject);
            }
        }

        transform.localScale = old_scale;
        transform.rotation = old_rot;
        transform.position = old_pos;

        Destroy(this);
    }
}
