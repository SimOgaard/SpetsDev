using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;

public class GrassTestingWithDictionary : MonoBehaviour
{
    public enum GroundType { none = -1, grass, flower, fire };
    private int ground_subtypes_length;

    private int triangle_amount;
    private int triangle_corner_amount;

    private int[] triangles; // holds every triangle for ground mesh
    private int[] triangles_types; // holds every triangle state
    private Vector3[] triangles_mid;

    private Vector2 min_max_y_mesh;

    private Dictionary<int, int>[] ground_subtypes; // holds one dictionary for each groundtype. holds triangles
    private bool[] ground_subtypes_changed; // what submesh that needs to be updated

    private Mesh ground_mesh;

    private void Awake()
    {
        triangle_amount = resolution.x * resolution.y * 2;
        triangle_corner_amount = triangle_amount * 3;
        ground_subtypes_length = GroundType.GetNames(typeof(GroundType)).Length - 1;

        ground_subtypes = new Dictionary<int, int>[ground_subtypes_length];
        ground_subtypes_changed = new bool[ground_subtypes_length];
        for (int i = 0; i < ground_subtypes_length; i++)
        {
            ground_subtypes[i] = new Dictionary<int, int>(triangle_corner_amount);
            ground_subtypes_changed[i] = false;
        }
        triangles_types = new int[triangle_amount];
        for (int i = 0; i < triangle_amount; i++)
        {
            triangles_types[i] = (int)GroundType.grass;
        }

        CreateMesh();
    }

    [SerializeField] private Vector2 unit_size;
    [SerializeField] private Vector2Int resolution;
    [SerializeField] private NoiseLayerSettings noise_layer_settings;
    [SerializeField] private Material[] materials;
    private void CreateMesh()
    {
        NativeArray<Noise.NoiseLayer> noise_layers_native_array = new NativeArray<Noise.NoiseLayer>(Noise.CreateNoiseLayers(noise_layer_settings), Allocator.Persistent);

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
            quadSize = unit_size,
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
            noise_layers = noise_layers_native_array
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

        Mesh collider_ground_mesh = new Mesh();
        collider_ground_mesh.vertices = meshData.vertices.ToArray();
        collider_ground_mesh.triangles = meshData.triangles.ToArray();
        collider_ground_mesh.normals = meshData.normals.ToArray();
        collider_ground_mesh.uv = meshData.uv.ToArray();
        MeshCollider mesh_collider = gameObject.AddComponent<MeshCollider>();
        mesh_collider.sharedMesh = collider_ground_mesh;

        ground_mesh = new Mesh();
        ground_mesh.vertices = meshData.vertices.ToArray();
        triangles = meshData.triangles.ToArray();
        ground_mesh.normals = meshData.normals.ToArray();
        ground_mesh.uv = meshData.uv.ToArray();

        ground_mesh.subMeshCount = ground_subtypes_length;
        ground_mesh.SetTriangles(triangles, 0);
        ground_mesh.SetTriangles(new int[0], 1);
        ground_mesh.SetTriangles(new int[0], 2);

        meshData.Dispose();
        noise_layers_native_array.Dispose();

        // setup mesh
        MeshFilter mesh_filter = gameObject.AddComponent<MeshFilter>();
        MeshRenderer mesh_renderer = gameObject.AddComponent<MeshRenderer>();
        mesh_filter.sharedMesh = ground_mesh;
        mesh_renderer.sharedMaterials = materials;

        // set triangle type and midpoint for each triangle
        triangles_mid = new Vector3[triangles.Length / 3];

        min_max_y_mesh = new Vector2(Mathf.Infinity, Mathf.NegativeInfinity);

        Vector3[] vertices = ground_mesh.vertices;
        for (int i = 0; i < triangle_amount; i++)
        {
            int triangle_point_index = i * 3;
            triangles_mid[i] = (vertices[triangles[triangle_point_index]] + vertices[triangles[triangle_point_index + 1]] + vertices[triangles[triangle_point_index + 2]]) / 3.0f;

            if (triangles_mid[i].y < min_max_y_mesh.x)
            {
                min_max_y_mesh.x = triangles_mid[i].y;
            }
            if (triangles_mid[i].y > min_max_y_mesh.y)
            {
                min_max_y_mesh.x = triangles_mid[i].y;
            }
        }
        min_max_y_mesh += Vector2.up * transform.position.y;

        for (int i = 0; i < triangle_corner_amount; i++)
        {
            ground_subtypes[(int)GroundType.grass][i] = triangles[i];
        }
    }

    public List<Collider> test_colliders;
    // switches traingles that are in this collider:
    // takes some time
    private void FixedUpdate()
    {
        foreach (Collider test_collider in test_colliders)
        {
            SwitchTrainglesInCollider(test_collider, (int)GroundType.flower);
        }
    }

    // set triangles for each affected submesh:
    // goes quickly
    private void LateUpdate()
    {
        for (int i = 0; i < ground_subtypes_length; i++)
        {
            if (ground_subtypes_changed[i])
            {
                ground_subtypes_changed[i] = false;

                int[] triangles_dict_copy = new int[ground_subtypes[i].Count];
                ground_subtypes[i].Values.CopyTo(triangles_dict_copy, 0);
                ground_mesh.SetTriangles(triangles_dict_copy, i, false);
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

    public void SwitchTrainglesInCollider(Collider collider, int ground_type)
    {
        (Vector2Int bounds_min_to_mesh_index, Vector2Int bounds_max_to_mesh_index, Vector2 min_max_y) = ColliderBounds(collider.bounds, unit_size, resolution);

        // if collider is inbetween
        if (min_max_y_mesh.x > min_max_y.x && min_max_y_mesh.y < min_max_y.x || min_max_y_mesh.x > min_max_y.y && min_max_y_mesh.y < min_max_y.y)
        {
            int max_z = bounds_max_to_mesh_index.y * resolution.y * 2;
            int add_z = resolution.y * 2;

            int max_x = bounds_max_to_mesh_index.x * 2;
            for (int z = bounds_min_to_mesh_index.y * resolution.y * 2; z < max_z; z += add_z)
            {
                for (int x = bounds_min_to_mesh_index.x * 2; x < max_x; x++)
                {
                    /*
                    int triangle_index = z + x;
                    int single_triangle_index = triangle_index / 3;
                    SwitchTraingle(ground_type, triangle_index, single_triangle_index);

                    single_triangle_index++;
                    triangle_index += 3;
                    SwitchTraingle(ground_type, triangle_index, single_triangle_index);
                    */
                    int triangle_index = z + x;
                    if (triangles_types[triangle_index] != ground_type && IsInside2(collider, triangles_mid[triangle_index] + transform.position))
                    {
                        SwitchTraingle(ground_type, triangle_index * 3, triangle_index);
                    }
                }
            }
        }
    }

    private (Vector2Int bounds_min_to_mesh_index, Vector2Int bounds_max_to_mesh_index, Vector2 min_max_y) ColliderBounds(Bounds bounds, Vector2 unit_size, Vector2Int resolution)
    {
        Vector2 mesh_size = Vector2.Scale(unit_size, resolution);
        Vector3 mesh_size_half = new Vector3(mesh_size.x * 0.5f, 0f, mesh_size.y * 0.5f);

        Vector3 mesh_mid_point = transform.position;
        Vector3 mesh_origo_point = mesh_mid_point - mesh_size_half;

        bounds.center -= mesh_origo_point;

        Vector3 bounds_min = bounds.min;
        Vector3 bounds_max = bounds.max;

        Vector2Int bounds_min_to_mesh_index = new Vector2Int(
            Mathf.Clamp(Mathf.RoundToInt(bounds_min.x / unit_size.x), 0, resolution.x),
            Mathf.Clamp(Mathf.RoundToInt(bounds_min.z / unit_size.y), 0, resolution.y)
        );

        Vector2Int bounds_max_to_mesh_index = new Vector2Int(
            Mathf.Clamp(Mathf.RoundToInt(bounds_max.x / unit_size.x), 0, resolution.x),
            Mathf.Clamp(Mathf.RoundToInt(bounds_max.z / unit_size.y), 0, resolution.y)
        );

        return (bounds_min_to_mesh_index, bounds_max_to_mesh_index, new Vector2(bounds_min.y, bounds_max.y));
    }

    private void SwitchTraingle(int ground_type, int triangle_corner_index, int triangle_index)
    {
        void RemoveTriangle()
        {
            // show that this mesh was updated
            ground_subtypes_changed[triangles_types[triangle_index]] = true;

            // remove triangle from last dict/submesh
            ground_subtypes[triangles_types[triangle_index]].Remove(triangle_corner_index);
            ground_subtypes[triangles_types[triangle_index]].Remove(triangle_corner_index + 1);
            ground_subtypes[triangles_types[triangle_index]].Remove(triangle_corner_index + 2);
        }

        void AddTriangle()
        {
            // change triangle type to new
            triangles_types[triangle_index] = ground_type;

            // show that this mesh was updated
            ground_subtypes_changed[ground_type] = true;

            // place new triangle in new submesh
            ground_subtypes[ground_type][triangle_corner_index] = triangles[triangle_corner_index];
            ground_subtypes[ground_type][triangle_corner_index + 1] = triangles[triangle_corner_index + 1];
            ground_subtypes[ground_type][triangle_corner_index + 2] = triangles[triangle_corner_index + 2];
        }

        // if triangle is not allready in the wanted submesh
        if (ground_type == (int)GroundType.none)
        {
            // we should only remove triangle
            RemoveTriangle();
        }
        else if (triangles_types[triangle_index] == (int)GroundType.none)
        {
            // we should only add triangle
            AddTriangle();
        }
        else
        {
            RemoveTriangle();
            AddTriangle();
        }
    }
}
