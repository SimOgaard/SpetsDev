using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;

public class GroundMesh : MonoBehaviour
{
    private int ground_subtypes_length;

    private int triangle_amount;
    private int triangle_corner_amount;

    private int[] triangles; // holds every triangle for ground mesh
    private int[] triangles_types; // holds every triangle state

    private float min_y;
    private float max_y;
    private Vector3[] triangles_mid;

    private Dictionary<int, int>[] ground_subtypes; // holds one dictionary for each groundtype. holds triangles
    private bool[] ground_subtypes_changed; // what submesh that needs to be updated

    private Mesh ground_mesh;
    private Vector2 unit_size;
    private Vector2Int resolution;

    public void Init()
    {
        // TESTING
        test_colliders.Add(GameObject.Find("TESTING REMOVING GRASS").GetComponent<Collider>());
    }

    private JobHandle jobHandle;
    private MeshData meshData;
    private bool is_deallocated = false;
    private void OnDestroy()
    {
        if (!is_deallocated)
        {
            if (!jobHandle.IsCompleted)
            {
                jobHandle.Complete();
            }

            meshData.Dispose();
        }
    }

    public IEnumerator CreateGround(WaitForFixedUpdate wait, Vector2 unit_size, Vector2Int resolution, NativeArray<Noise.NoiseLayer> noise_layers_native_array, Material static_material, NoiseLayerSettings.Foliage[] foliage)
    {
        this.unit_size = unit_size;
        this.resolution = resolution;

        triangle_amount = resolution.x * resolution.y * 2;
        triangle_corner_amount = triangle_amount * 3;
        ground_subtypes_length = foliage.Length;

        ground_subtypes = new Dictionary<int, int>[ground_subtypes_length];
        ground_subtypes_changed = new bool[ground_subtypes_length];
        for (int i = 0; i < ground_subtypes_length; i++)
        {
            ground_subtypes[i] = new Dictionary<int, int>(triangle_corner_amount);
            ground_subtypes_changed[i] = true;
        }
        triangles_types = new int[triangle_amount];

        int quadCount = resolution.x * resolution.y;
        int vertexCount = quadCount * 4;
        int triangleCount = quadCount * 6;

        meshData = new MeshData();
        meshData.vertices = new NativeArray<Vector3>(vertexCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        meshData.triangles = new NativeArray<int>(triangleCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        meshData.normals = new NativeArray<Vector3>(vertexCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        meshData.uv = new NativeArray<Vector2>(vertexCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        meshData.mid_point = new NativeArray<Vector3>(triangle_amount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        meshData.min_y__max_y = new NativeArray<float>(2, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

        CreateMesh.CreatePlaneJob job = new CreateMesh.CreatePlaneJob
        {
            meshData = meshData,
            planeSize = resolution,
            quadSize = unit_size,
            quadCount = quadCount,
            vertexCount = vertexCount,
            triangleCount = triangleCount
        };

        jobHandle = job.Schedule();
        while (!jobHandle.IsCompleted)
        {
            yield return wait;
        }
        jobHandle.Complete();


        CreateMesh.CreateMeshByNoiseJob noiseJob = new CreateMesh.CreateMeshByNoiseJob
        {
            vertices = meshData.vertices,
            offset = transform.position,
            noise_layers = noise_layers_native_array
        };

        jobHandle = noiseJob.Schedule();
        while (!jobHandle.IsCompleted)
        {
            yield return wait;
        }
        jobHandle.Complete();

        
        CreateMesh.SmoothNormalizeJob normalizeJob = new CreateMesh.SmoothNormalizeJob
        {
            triangleCount = triangleCount,
            vertexCount = vertexCount,
            angle = 10f,
            meshData = meshData
        };
        /*
        CreateMesh.NormalizeJob normalizeJob = new CreateMesh.NormalizeJob
        {
            quadCount = quadCount,
            meshData = meshData
        };
        */
        jobHandle = normalizeJob.Schedule(jobHandle);
        while (!jobHandle.IsCompleted)
        {
            yield return wait;
        }
        jobHandle.Complete();


        CreateMesh.MidPointJob midPointJob = new CreateMesh.MidPointJob
        {
            triangleCount = triangleCount,
            meshData = meshData
        };

        jobHandle = midPointJob.Schedule(jobHandle);
        while (!jobHandle.IsCompleted)
        {
            yield return wait;
        }
        jobHandle.Complete();

        min_y = meshData.min_y__max_y[0] + transform.position.y;
        max_y = meshData.min_y__max_y[1] + transform.position.y;

        triangles_mid = meshData.mid_point.ToArray();
        triangles = meshData.triangles.ToArray();

        // init parrent (this) that holds all const/static meshes
        MeshRenderer this_mesh_renderer = gameObject.AddComponent<MeshRenderer>();
        MeshFilter this_mesh_filter = gameObject.AddComponent<MeshFilter>();
        MeshCollider this_mesh_collider = gameObject.AddComponent<MeshCollider>();

        // create mesh for ground
        Mesh collider_ground_mesh = new Mesh();
        collider_ground_mesh.vertices = meshData.vertices.ToArray();
        collider_ground_mesh.triangles = meshData.triangles.ToArray();
        collider_ground_mesh.normals = meshData.normals.ToArray();
        collider_ground_mesh.uv = meshData.uv.ToArray();

        this_mesh_renderer.material = static_material;
        this_mesh_filter.sharedMesh = collider_ground_mesh;
        this_mesh_collider.sharedMesh = collider_ground_mesh;

        // create child that holds changing meshes
        GameObject child = new GameObject("chaning meshes");
        MeshRenderer child_mesh_renderer = child.AddComponent<MeshRenderer>();
        MeshFilter child_mesh_filter = child.AddComponent<MeshFilter>();

        child.transform.parent = transform;
        child.transform.localPosition = Vector3.up * 0.1f;

        // setup multi mesh for child
        ground_mesh = new Mesh();
        ground_mesh.vertices = meshData.vertices.ToArray();
        ground_mesh.normals = meshData.normals.ToArray();
        ground_mesh.uv = meshData.uv.ToArray();

        ground_mesh.subMeshCount = ground_subtypes_length;


        // native array of size triangle_amount
        NativeArray<int> ground_type_native_array = new NativeArray<int>(triangle_amount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        // create FoliageJob.FoliageNativeArray[]
        NativeArray<CreateMesh.FoliageJob.FoliageNativeArray> foliage_native_array = new NativeArray<CreateMesh.FoliageJob.FoliageNativeArray>(foliage.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        for (int i = 0; i < foliage.Length; i++)
        {
            foliage_native_array[i] = CreateMesh.FoliageJob.create_foliage_native_array(foliage[i]);
        }
        // pass them into a FoliageJob and execute
        CreateMesh.FoliageJob foliageJob = new CreateMesh.FoliageJob
        {
            triangle_amount = triangle_amount,
            original_mesh_data = meshData,
            foliage = foliage_native_array,
            ground_type_native_array = ground_type_native_array,
        };
        jobHandle = foliageJob.Schedule(jobHandle);
        while (!jobHandle.IsCompleted)
        {
            yield return wait;
        }
        jobHandle.Complete();
        // populate nativearray with value of GroundType
        //      dependent on if any random foliage is true
        //      otherwise do grass
        // grab native array back and itterate over it setting
        //      ground_subtypes[nativearray[i][i] = triangles[i]
        for (int i = 0; i < triangle_amount; i++)
        {
            triangles_types[i] = ground_type_native_array[i];

            if (triangles_types[i] == -1)
            {
                continue;
            }

            int corner_triangle = i * 3;
            ground_subtypes[ground_type_native_array[i]][corner_triangle] = triangles[corner_triangle];
            corner_triangle++;
            ground_subtypes[ground_type_native_array[i]][corner_triangle] = triangles[corner_triangle];
            corner_triangle++;
            ground_subtypes[ground_type_native_array[i]][corner_triangle] = triangles[corner_triangle];
        }
        // dispose of native array
        ground_type_native_array.Dispose();
        foliage_native_array.Dispose();

        // get materials for each foliage
        Material[] foliage_materials = new Material[foliage.Length];
        for (int i = 0; i < foliage.Length; i++)
        {
            foliage_materials[i] = foliage[i].material.material;
        }
        child_mesh_renderer.materials = foliage_materials;
        child_mesh_filter.sharedMesh = ground_mesh;

        is_deallocated = true;

        meshData.Dispose();
    }

    public List<Collider> test_colliders = new List<Collider>();
    // switches traingles that are in this collider:
    // takes some time
    private void FixedUpdate()
    {
        if (is_deallocated)
        {
            foreach (Collider test_collider in test_colliders)
            {
                SwitchTrainglesInCollider(test_collider, -1);
            }
        }
    }

    // set triangles for each affected submesh:
    // goes quickly
    private void LateUpdate()
    {
        if (is_deallocated)
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

        // if collider is inbetween min max
        if (min_max_y.x < max_y || min_max_y.y > min_y)
        {
            int max_x = bounds_max_to_mesh_index.x * 2;
            int start_x = bounds_min_to_mesh_index.x * 2;

            if (start_x >= max_x)
            {
                return;
            }

            int max_z = bounds_max_to_mesh_index.y * resolution.y * 2;
            int add_z = resolution.y * 2;

            for (int z = bounds_min_to_mesh_index.y * resolution.y * 2; z < max_z; z += add_z)
            {
                for (int x = start_x; x < max_x; x++)
                {
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
        if (ground_type == -1)
        {
            // we should only remove triangle
            RemoveTriangle();
        }
        else if (triangles_types[triangle_index] == -1)
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
