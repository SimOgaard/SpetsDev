using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;

public class GrassTestingWithArray : MonoBehaviour
{
    public enum GroundType { none = -1, grass, flower, fire };
    private int groundSubtypesLength;

    private int triangleAmount;
    private int triangleCornerAmount;

    private int[] triangles; // holds every triangle for ground mesh
    private int[] trianglesTypes; // holds every triangle state
    private Vector3[] trianglesMid; // mid point for each triangle
    private List<int>[] trianglesListTemporary;
    private bool[] trianglesTypesChanged; // what submesh that needs to be updated

    private Vector2 minMaxYMesh;

    private Mesh groundMesh;

    private void Awake()
    {
        triangleAmount = resolution.x * resolution.y * 2;
        triangleCornerAmount = triangleAmount * 3;
        groundSubtypesLength = GroundType.GetNames(typeof(GroundType)).Length - 1;

        trianglesTypesChanged = new bool[groundSubtypesLength];
        trianglesListTemporary = new List<int>[groundSubtypesLength];
        for (int i = 0; i < groundSubtypesLength; i++)
        {
            trianglesTypesChanged[i] = false;
            trianglesListTemporary[i] = new List<int>(triangleCornerAmount);
        }
        trianglesTypes = new int[triangleAmount];
        for (int i = 0; i < triangleAmount; i++)
        {
            trianglesTypes[i] = (int)GroundType.grass;
        }

        CreateMesh();
    }

    [SerializeField] private Vector2 unitSize;
    [SerializeField] private Vector2Int resolution;
    [SerializeField] private NoiseLayerSettings noiseLayerSettings;
    [SerializeField] private Material[] materials;
    private void CreateMesh()
    {
        NativeArray<Noise.NoiseLayer> noiseLayersNativeArray = new NativeArray<Noise.NoiseLayer>(Noise.CreateNoiseLayers(noiseLayerSettings), Allocator.Persistent);

        int quadCount = resolution.x * resolution.y;
        int vertexCount = quadCount * 4;
        int triangleCount = quadCount * 6;

        MeshData meshData = new MeshData();
        meshData.vertices = new NativeArray<Vector3>(vertexCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        meshData.triangles = new NativeArray<int>(triangleCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        meshData.normals = new NativeArray<Vector3>(vertexCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        meshData.uv = new NativeArray<Vector2>(vertexCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

        CreateMesh.CreatePlaneJob job = new CreateMesh.CreatePlaneJob
        {
            meshData = meshData,
            planeSize = resolution,
            quadSize = unitSize,
            quadCount = quadCount,
            vertexCount = vertexCount,
            triangleCount = triangleCount
        };

        JobHandle jobHandle = job.Schedule();
        jobHandle.Complete();

        CreateMesh.CreateMeshByNoiseJob noiseJob = new CreateMesh.CreateMeshByNoiseJob
        {
            vertices = meshData.vertices,
            offset = transform.position,
            noiseLayers = noiseLayersNativeArray
        };

        jobHandle = noiseJob.Schedule();
        jobHandle.Complete();

        CreateMesh.NormalizeJob normalizeJob = new CreateMesh.NormalizeJob
        {
            quadCount = quadCount,
            meshData = meshData
        };

        jobHandle = normalizeJob.Schedule(jobHandle);
        jobHandle.Complete();

        Mesh colliderGroundMesh = new Mesh();
        colliderGroundMesh.vertices = meshData.vertices.ToArray();
        colliderGroundMesh.triangles = meshData.triangles.ToArray();
        colliderGroundMesh.normals = meshData.normals.ToArray();
        colliderGroundMesh.uv = meshData.uv.ToArray();
        MeshCollider meshCollider = gameObject.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = colliderGroundMesh;

        groundMesh = new Mesh();
        groundMesh.vertices = meshData.vertices.ToArray();
        triangles = meshData.triangles.ToArray();
        groundMesh.normals = meshData.normals.ToArray();
        groundMesh.uv = meshData.uv.ToArray();

        groundMesh.subMeshCount = groundSubtypesLength;
        groundMesh.SetTriangles(triangles, 0);
        groundMesh.SetTriangles(new int[0], 1);
        groundMesh.SetTriangles(new int[0], 2);

        meshData.Dispose();
        noiseLayersNativeArray.Dispose();

        // setup mesh
        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshFilter.sharedMesh = groundMesh;
        meshRenderer.sharedMaterials = materials;

        // set triangle type and midpoint for each triangle
        trianglesMid = new Vector3[triangleAmount];

        minMaxYMesh = new Vector2(Mathf.Infinity, Mathf.NegativeInfinity);

        Vector3[] vertices = groundMesh.vertices;
        for (int i = 0; i < triangleAmount; i++)
        {
            int trianglePointIndex = i * 3;
            trianglesMid[i] = (vertices[triangles[trianglePointIndex]] + vertices[triangles[trianglePointIndex + 1]] + vertices[triangles[trianglePointIndex + 2]]) / 3.0f;

            if (trianglesMid[i].y < minMaxYMesh.x)
            {
                minMaxYMesh.x = trianglesMid[i].y;
            }
            if (trianglesMid[i].y > minMaxYMesh.y)
            {
                minMaxYMesh.x = trianglesMid[i].y;
            }
        }
        minMaxYMesh += Vector2.up * transform.position.y;
    }

    public List<Collider> testColliders;
    // switches traingles that are in this collider:
    // goes really quickly
    private void FixedUpdate()
    {
        foreach (Collider testCollider in testColliders)
        {
            SwitchTrainglesInCollider(testCollider, (int)GroundType.flower);
        }
    }

    // takes a long time
    private void LateUpdate()
    {
        // set triangles for each affected submesh:
        // minimize the use of this operation. O(m*n) n = triangle amount, m = different triangles states
        for (int i = 0; i < triangleAmount; i++)
        {
            if (trianglesTypes[i] == (int)GroundType.none)
            {
                continue;
            }

            // if this one has changed
            if (trianglesTypesChanged[trianglesTypes[i]])
            {
                // add this triangle to right temporary list
                int triangleIndex = i * 3;
                trianglesListTemporary[trianglesTypes[i]].Add(triangles[triangleIndex]);
                trianglesListTemporary[trianglesTypes[i]].Add(triangles[triangleIndex + 1]);
                trianglesListTemporary[trianglesTypes[i]].Add(triangles[triangleIndex + 2]);
            }
        }

        for (int i = 0; i < groundSubtypesLength; i++)
        {
            if (trianglesTypesChanged[i])
            {
                // convert list to array and set triangles
                groundMesh.SetTriangles(trianglesListTemporary[i].ToArray(), i);
                // remove all items in list
                trianglesListTemporary[i].Clear();
                // set to has changed
                trianglesTypesChanged[i] = false;
            }
        }
    }

    public static bool IsInside1(Collider collider, Vector3 point)
    {
        return (collider.ClosestPoint(point) - point).sqrMagnitude < Mathf.Epsilon * Mathf.Epsilon;
    }

    public static bool IsInside2(Collider collider, Vector3 point)
    {
        return collider.ClosestPoint(point) == point;
    }

    public void SwitchTrainglesInCollider(Collider collider, int groundType)
    {
        (Vector2Int boundsMinToMeshIndex, Vector2Int boundsMaxToMeshIndex, Vector2 minMaxY) = ColliderBounds(collider.bounds, unitSize, resolution);

        // if collider is inbetween
        if (minMaxYMesh.x > minMaxY.x && minMaxYMesh.y < minMaxY.x || minMaxYMesh.x > minMaxY.y && minMaxYMesh.y < minMaxY.y)
        {
            int maxZ = boundsMaxToMeshIndex.y * resolution.y * 2;
            int addZ = resolution.y * 2;

            int maxX = boundsMaxToMeshIndex.x * 2;
            for (int z = boundsMinToMeshIndex.y * resolution.y * 2; z < maxZ; z += addZ)
            {
                for (int x = boundsMinToMeshIndex.x * 2; x < maxX; x++)
                {
                    int triangleIndex = z + x;

                    if (trianglesTypes[triangleIndex] != groundType && IsInside2(collider, trianglesMid[triangleIndex] + transform.position))
                    {
                        // signal that that both triangles has been changed
                        trianglesTypesChanged[trianglesTypes[triangleIndex]] = true;
                        if (groundType != (int)GroundType.none)
                        {
                            trianglesTypesChanged[groundType] = true;
                        }

                        // switch the triangle
                        trianglesTypes[triangleIndex] = groundType;
                    }
                }
            }
        }
    }

    private (Vector2Int boundsMinToMeshIndex, Vector2Int boundsMaxToMeshIndex, Vector2 minMaxY) ColliderBounds(Bounds bounds, Vector2 unitSize, Vector2Int resolution)
    {
        Vector2 meshSize = Vector2.Scale(unitSize, resolution);
        Vector3 meshSizeHalf = new Vector3(meshSize.x * 0.5f, 0f, meshSize.y * 0.5f);

        Vector3 meshMidPoint = transform.position;
        Vector3 meshOrigoPoint = meshMidPoint - meshSizeHalf;

        bounds.center -= meshOrigoPoint;

        Vector3 boundsMin = bounds.min;
        Vector3 boundsMax = bounds.max;

        Vector2Int boundsMinToMeshIndex = new Vector2Int(
            Mathf.Clamp(Mathf.RoundToInt(boundsMin.x / unitSize.x), 0, resolution.x),
            Mathf.Clamp(Mathf.RoundToInt(boundsMin.z / unitSize.y), 0, resolution.y)
        );

        Vector2Int boundsMaxToMeshIndex = new Vector2Int(
            Mathf.Clamp(Mathf.RoundToInt(boundsMax.x / unitSize.x), 0, resolution.x),
            Mathf.Clamp(Mathf.RoundToInt(boundsMax.z / unitSize.y), 0, resolution.y)
        );

        return (boundsMinToMeshIndex, boundsMaxToMeshIndex, new Vector2(boundsMin.y, boundsMax.y));
    }
}
