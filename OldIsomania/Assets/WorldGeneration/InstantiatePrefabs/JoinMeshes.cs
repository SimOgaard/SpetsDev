using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JoinMeshes : MonoBehaviour
{
    private GameObject _gameObject = null;
    //private Material material;
    private bool isCollider = false;
    private bool mergeAllChildren = true;
    /*
    public void SetMaterial(Material material)
    {
        this.material = material;
    }
    */

    public void SetCollider()
    {
        isCollider = true;
    }

    public void SetMergeByTags(bool mergeByTags)
    {
        mergeAllChildren = !mergeByTags;
    }

    public void SetGameObjectToHoldJoinedMeshes(GameObject gameObject)
    {
        _gameObject = gameObject;
    }

    public static UnityEngine.Component GetAddComponent(GameObject gameObject, System.Type componentToGet)
    {
        UnityEngine.Component component = gameObject.GetComponent(componentToGet);
        if (component == null)
        {
            component = gameObject.AddComponent(componentToGet);
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
            if (mergeAllChildren || Tag.IsTaggedWith(child.tag, Tag.merge))
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

        MeshRenderer meshRenderer = GetAddComponent(_gameObject, typeof(MeshRenderer)) as MeshRenderer;
        MeshFilter meshFilter = GetAddComponent(_gameObject, typeof(MeshFilter)) as MeshFilter;
        MeshCollider meshCollider = isCollider ? GetAddComponent(_gameObject, typeof(MeshCollider)) as MeshCollider : null;

        Vector3 oldScale = transform.localScale;
        Quaternion oldRot = transform.rotation;
        Vector3 oldPos = transform.position;
        transform.localScale = new Vector3(1f, 1f, 1f);
        transform.rotation = Quaternion.identity;
        transform.position = Vector3.zero;

        ArrayList materials = new ArrayList();
        ArrayList combineInstanceArrays = new ArrayList();
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();

        foreach (MeshFilter _meshFilter in meshFilters)
        {
            if (mergeAllChildren || Tag.IsTaggedWith(_meshFilter.tag, Tag.merge))
            {
                MeshRenderer _meshRenderer = _meshFilter.GetComponent<MeshRenderer>();

                if (!_meshRenderer || !_meshFilter.sharedMesh || _meshRenderer.sharedMaterials.Length != _meshFilter.sharedMesh.subMeshCount)
                {
                    continue;
                }

                for (int s = 0; s < _meshFilter.sharedMesh.subMeshCount; s++)
                {
                    int materialArrayIndex = Contains(materials, _meshRenderer.sharedMaterials[s].name);
                    if (materialArrayIndex == -1)
                    {
                        materials.Add(_meshRenderer.sharedMaterials[s]);
                        materialArrayIndex = materials.Count - 1;
                    }
                    combineInstanceArrays.Add(new ArrayList());

                    CombineInstance combineInstance = new CombineInstance();
                    combineInstance.transform = _meshRenderer.transform.localToWorldMatrix;
                    combineInstance.subMeshIndex = s;
                    combineInstance.mesh = _meshFilter.sharedMesh;
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
        meshFilter.sharedMesh = new Mesh()
        {
            indexFormat = UnityEngine.Rendering.IndexFormat.UInt16
        };
        meshFilter.sharedMesh.CombineMeshes(combineInstances, false, false);
        if (isCollider)
        {
            meshCollider.sharedMesh = meshFilter.sharedMesh;
        }

        // Assign materials
        Material[] materialsArray = materials.ToArray(typeof(Material)) as Material[];
        meshRenderer.materials = materialsArray;

        transform.localScale = oldScale;
        transform.rotation = oldRot;
        transform.position = oldPos;

        RecursiveDestroy(transform);
        Destroy(this);
    }
}
