using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JoinMeshes : MonoBehaviour
{
    private Material material;

    public void SetMaterial(Material material)
    {
        this.material = material;
    }

    private void Awake()
    {
        gameObject.AddComponent<MeshRenderer>();
        gameObject.AddComponent<MeshFilter>();
        gameObject.AddComponent<MeshCollider>();
    }

    private void Start()
    {
        MeshFilter[] mesh_filters_array = GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[mesh_filters_array.Length];

        int i = 0;
        while (i < mesh_filters_array.Length)
        {
            combine[i].mesh = mesh_filters_array[i].sharedMesh;
            combine[i].transform = mesh_filters_array[i].transform.localToWorldMatrix;

            i++;
        }
        Mesh mesh = transform.GetComponent<MeshFilter>().mesh = new Mesh()
        {
            indexFormat = UnityEngine.Rendering.IndexFormat.UInt16
        };
        mesh.CombineMeshes(combine);
        transform.GetComponent<MeshCollider>().sharedMesh = mesh;
        GetComponent<MeshRenderer>().sharedMaterial = material;

        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }
}
