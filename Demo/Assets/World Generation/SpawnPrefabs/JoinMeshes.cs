using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JoinMeshes : MonoBehaviour
{
    private GameObject _gameObject = null;
    //private Material material;
    private bool is_collider = false;
    private bool merge_all_children = true;
    /*
    public void SetMaterial(Material material)
    {
        this.material = material;
    }
    */

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

    private int Contains(ArrayList searchList, string searchName)
    {
        for (int i = 0; i < searchList.Count; i++)
        {
            if (((Material)searchList[i]).name == searchName)
            {
                return i;
            }
        }
        return -1;
    }

    private void RecursiveDestroy(Transform transform)
    {
        foreach (Transform child in transform)
        {
            if (merge_all_children || Tag.IsTaggedWith(child.tag, Tag.merge))
            {
                Destroy(child.gameObject);
            }
            else
            {
                RecursiveDestroy(child);
            }
        }
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

        ArrayList materials = new ArrayList();
        ArrayList combineInstanceArrays = new ArrayList();
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();

        foreach (MeshFilter meshFilter in meshFilters)
        {
            if (merge_all_children || Tag.IsTaggedWith(meshFilter.tag, Tag.merge))
            {
                MeshRenderer meshRenderer = meshFilter.GetComponent<MeshRenderer>();

                if (!meshRenderer || !meshFilter.sharedMesh || meshRenderer.sharedMaterials.Length != meshFilter.sharedMesh.subMeshCount)
                {
                    continue;
                }

                for (int s = 0; s < meshFilter.sharedMesh.subMeshCount; s++)
                {
                    int materialArrayIndex = Contains(materials, meshRenderer.sharedMaterials[s].name);
                    if (materialArrayIndex == -1)
                    {
                        materials.Add(meshRenderer.sharedMaterials[s]);
                        materialArrayIndex = materials.Count - 1;
                    }
                    combineInstanceArrays.Add(new ArrayList());

                    CombineInstance combineInstance = new CombineInstance();
                    combineInstance.transform = meshRenderer.transform.localToWorldMatrix;
                    combineInstance.subMeshIndex = s;
                    combineInstance.mesh = meshFilter.sharedMesh;
                    (combineInstanceArrays[materialArrayIndex] as ArrayList).Add(combineInstance);
                }
            }
        }

        // Combine by material index into per-material meshes
        // also, Create CombineInstance array for next step
        Mesh[] meshes = new Mesh[materials.Count];
        CombineInstance[] combineInstances = new CombineInstance[materials.Count];

        for (int m = 0; m < materials.Count; m++)
        {
            CombineInstance[] combineInstanceArray = (combineInstanceArrays[m] as ArrayList).ToArray(typeof(CombineInstance)) as CombineInstance[];
            meshes[m] = new Mesh();
            meshes[m].CombineMeshes(combineInstanceArray, true, true);

            combineInstances[m] = new CombineInstance();
            combineInstances[m].mesh = meshes[m];
            combineInstances[m].subMeshIndex = 0;
        }

        // Combine into one
        mesh_filter.sharedMesh = new Mesh()
        {
            indexFormat = UnityEngine.Rendering.IndexFormat.UInt16
        };
        mesh_filter.sharedMesh.CombineMeshes(combineInstances, false, false);
        if (is_collider)
        {
            mesh_collider.sharedMesh = mesh_filter.sharedMesh;
        }

        // Assign materials
        Material[] materialsArray = materials.ToArray(typeof(Material)) as Material[];
        mesh_renderer.materials = materialsArray;

        transform.localScale = old_scale;
        transform.rotation = old_rot;
        transform.position = old_pos;

        RecursiveDestroy(transform);
        Destroy(this);
    }
}
