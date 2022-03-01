using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;

public class GroundMesh : MonoBehaviour
{
    [System.Serializable]
    public struct MeshManipulationState
    {
        public GroundTriangleType change_from;
        [SerializeField] private GroundTriangleType _change_to;
        public GroundTriangleType change_to
        {
            get { return _change_to; }
            set {
                _change_to = value;
                change_to_index = GroundTriangleTypeIndex((int)value);
            }
        }
        public int change_to_index;

        public static bool IsIn(GroundTriangleType a, GroundTriangleType b)
        {
            return a == b || (a & b) != 0;
        }

        // Returns position of the only set bit in 'n'
        public static int GroundTriangleTypeIndex(int n)
        {
            if (n == 0)
            {
                return -1;
            }

            int i = 1, pos = 1;

            // Iterate through bits of n till we find a set bit
            // i&n will be non-zero only when 'i' and 'n' have a set bit
            // at same position
            while ((i & n) == 0)
            {
                // Unset current bit and set the next bit in 'i'
                i = i << 1;

                // increment position
                ++pos;
            }
            return pos - 1;
        }
    }

    // (int) GroundTriangleType - 1 = 
    public const int GroundTriangleTypeLength = 5;
    [System.Flags]
    public enum GroundTriangleType
    {
        //Nothing = 0,
        //Everything = 0x7FFFFFFF,
        //EverythingExcludingNothing = 0x3FFFFFFF, // 2^30, 
                                                 //        EverythingIncludingNothing = //0x7FFFFFFF,
        Grass = 1,  // 2
        GrassTrampled = 2,  // 2
        Flower = 4,  // 4
        Wheat = 8,  // 8
        WheatTrampled = 16,  // 32
        //Everything = 0b0001_1111
    }

    //private int ground_subtypes_length;

    private int triangle_amount;
    private int triangle_corner_amount;

    private int[] triangles; // holds every triangle for ground mesh
    private GroundTriangleType[] triangles_types; // holds every triangle state
    private int[] triangles_types_index;

    private Vector2 min_max_y;
    private Vector3[] triangles_mid;

    private Dictionary<int, int>[] ground_subtypes; // holds one dictionary for each groundtype. holds triangles
    private bool[] ground_subtypes_changed; // what submesh that needs to be updated

    /*
    private int[,] ground_subtypes_array; // holds every triangles type
    private int[] ground_subtypes_lengths; // what are the lengths of the submeshes?
    private int[] ground_subtypes_index; // holds current indices
    */

    private Mesh ground_mesh;
    private Vector2 unit_size;
    private Vector2Int resolution;

    private Transform child;

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
        //ground_subtypes_length = foliage.Length;

        ground_subtypes = new Dictionary<int, int>[GroundTriangleTypeLength];
        ground_subtypes_changed = new bool[GroundTriangleTypeLength];
        for (int i = 0; i < GroundTriangleTypeLength; i++)
        {
            ground_subtypes[i] = new Dictionary<int, int>(triangle_corner_amount);
            ground_subtypes_changed[i] = true;
        }
        triangles_types = new GroundTriangleType[triangle_amount];
        triangles_types_index = new int[triangle_amount];

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

        min_max_y.x = meshData.min_y__max_y[0];
        min_max_y.y = meshData.min_y__max_y[1];

        triangles_mid = meshData.mid_point.ToArray();
        triangles = meshData.triangles.ToArray();

        // init parrent (this) that holds all const/static meshes
        MeshRenderer this_mesh_renderer = gameObject.AddComponent<MeshRenderer>();
        MeshFilter this_mesh_filter = gameObject.AddComponent<MeshFilter>();
        MeshCollider this_mesh_collider = gameObject.AddComponent<MeshCollider>();

        // create mesh for ground
        Mesh collider_ground_mesh = new Mesh();
        collider_ground_mesh.MarkDynamic();
        collider_ground_mesh.vertices = meshData.vertices.ToArray();
        collider_ground_mesh.triangles = meshData.triangles.ToArray();
        collider_ground_mesh.normals = meshData.normals.ToArray();
        collider_ground_mesh.uv = meshData.uv.ToArray();

        this_mesh_renderer.material = static_material;
        this_mesh_filter.sharedMesh = collider_ground_mesh;
        this_mesh_collider.sharedMesh = collider_ground_mesh;

        // create child that holds changing meshes
        child = new GameObject("chaning meshes").transform;
        MeshRenderer child_mesh_renderer = child.gameObject.AddComponent<MeshRenderer>();
        MeshFilter child_mesh_filter = child.gameObject.AddComponent<MeshFilter>();

        child.transform.parent = transform;
        child.transform.localPosition = Vector3.up * 1.5f;

        // setup multi mesh for child
        ground_mesh = new Mesh();
        ground_mesh.MarkDynamic();
        ground_mesh.vertices = meshData.vertices.ToArray();
        ground_mesh.normals = meshData.normals.ToArray();
        ground_mesh.uv = meshData.uv.ToArray();
        ground_mesh.subMeshCount = GroundTriangleTypeLength;

        // native array of size triangle_amount
        NativeArray<GroundTriangleType> ground_type_native_array = new NativeArray<GroundTriangleType>(triangle_amount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        NativeArray<int> ground_type_index_native_array = new NativeArray<int>(triangle_amount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
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
            ground_type_index_native_array = ground_type_index_native_array,
            chunk_offset = child.position,
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
            triangles_types_index[i] = ground_type_index_native_array[i];

            if (triangles_types[i] ==(GroundTriangleType) 0)
            {
                continue;
            }

            int corner_triangle = i * 3;
            ground_subtypes[ground_type_index_native_array[i]][corner_triangle] = triangles[corner_triangle];
            corner_triangle++;
            ground_subtypes[ground_type_index_native_array[i]][corner_triangle] = triangles[corner_triangle];
            corner_triangle++;
            ground_subtypes[ground_type_index_native_array[i]][corner_triangle] = triangles[corner_triangle];
        }
        // dispose of native arrays
        ground_type_native_array.Dispose();
        ground_type_index_native_array.Dispose();
        foliage_native_array.Dispose();

        // get materials for each foliage
        Material[] foliage_materials = new Material[GroundTriangleTypeLength];
        for (int i = 0; i < foliage.Length; i++)
        {
            Debug.Log(foliage[i].type);
            foliage_materials[MeshManipulationState.GroundTriangleTypeIndex((int)foliage[i].type)] = foliage[i].material.material;
        }
        child_mesh_renderer.materials = foliage_materials;
        child_mesh_filter.sharedMesh = ground_mesh;

        is_deallocated = true;

        meshData.Dispose();
    }

    // set triangles for each affected submesh:
    // goes quickly
    private void LateUpdate()
    {
        if (is_deallocated)
        {
            for (int i = 0; i < GroundTriangleTypeLength; i++)
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

    public static bool IsInside1(Collider collider, Vector3 point, float margin = 1e-3f)
    {
        return (collider.ClosestPoint(point) - point).sqrMagnitude < margin;
    }

    public static bool IsInside2(Collider collider, Vector3 point)
    {
        return collider.ClosestPoint(point) == point;
    }

    public void SwitchTrainglesInCollider(Collider collider, MeshManipulationState[] mesh_manipulations)
    {
        (Vector2Int bounds_min_to_mesh_index, Vector2Int bounds_max_to_mesh_index, Vector2 min_max_y) = ColliderBounds(collider.bounds, unit_size, resolution);

        // if collider is inbetween min max
        if (min_max_y.x < min_max_y.y || min_max_y.y > min_max_y.x)
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
                    if (IsInside1(collider, triangles_mid[triangle_index] + child.position, 0.5f))
                    {
                        for (int i = 0; i < mesh_manipulations.Length; i++)
                        {
                            if (SwitchTraingle(mesh_manipulations[i], triangle_index * 3, triangle_index))
                            {
                                break;
                            }
                        }
                    }
                }
            }
        }
    }

    public void SwitchTrainglesInCollider(Collider collider, MeshManipulationState mesh_manipulation)
    {
        (Vector2Int bounds_min_to_mesh_index, Vector2Int bounds_max_to_mesh_index, Vector2 min_max_y) = ColliderBounds(collider.bounds, unit_size, resolution);

        // if collider is inbetween min max
        if (min_max_y.x < min_max_y.y || min_max_y.y > min_max_y.x)
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
                    if (IsInside1(collider, triangles_mid[triangle_index] + child.position, 0.5f))
                    {
                        SwitchTraingle(mesh_manipulation, triangle_index * 3, triangle_index);
                    }
                }
            }
        }
    }

    private (Vector2Int bounds_min_to_mesh_index, Vector2Int bounds_max_to_mesh_index, Vector2 min_max_y) ColliderBounds(Bounds bounds, Vector2 unit_size, Vector2Int resolution)
    {
        Vector2 mesh_size = Vector2.Scale(unit_size, resolution);
        Vector3 mesh_size_half = new Vector3(mesh_size.x * 0.5f, 0f, mesh_size.y * 0.5f);

        Vector3 mesh_mid_point = child.position;
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

    private bool SwitchTraingle(MeshManipulationState mesh_manipulation, int triangle_corner_index, int triangle_index)
    {
        // removes selected triangle from its submesh
        void RemoveTriangle()
        {
            // show that this mesh was updated
            ground_subtypes_changed[triangles_types_index[triangle_index]] = true;

            // remove triangle from last dict/submesh
            ground_subtypes[triangles_types_index[triangle_index]].Remove(triangle_corner_index);
            ground_subtypes[triangles_types_index[triangle_index]].Remove(triangle_corner_index + 1);
            ground_subtypes[triangles_types_index[triangle_index]].Remove(triangle_corner_index + 2);
        }

        // adds a triangle to specified submesh at triangle point
        void AddTriangle()
        {
            // show that this mesh was updated
            ground_subtypes_changed[mesh_manipulation.change_to_index] = true;

            // place new triangle in new submesh
            ground_subtypes[mesh_manipulation.change_to_index][triangle_corner_index] = triangles[triangle_corner_index];
            ground_subtypes[mesh_manipulation.change_to_index][triangle_corner_index + 1] = triangles[triangle_corner_index + 1];
            ground_subtypes[mesh_manipulation.change_to_index][triangle_corner_index + 2] = triangles[triangle_corner_index + 2];
        }

        // if allready placed triangle is of any type in change_from
        if (MeshManipulationState.IsIn(triangles_types[triangle_index], mesh_manipulation.change_from) && triangles_types[triangle_index] != mesh_manipulation.change_to)
        {
            if (mesh_manipulation.change_to == (GroundTriangleType) 0)
            {
                // we should only remove triangle
                //Debug.Log("shouldt happen");
                RemoveTriangle();
            }
            else if (triangles_types[triangle_index] == (GroundTriangleType) 0)
            {
                // we should only add triangle
                //Debug.Log("shouldt happen");
                AddTriangle();
            }
            else
            {
                //Debug.Log("shouldt happen");
                RemoveTriangle();
                AddTriangle();
            }

            // change triangle type to new
            triangles_types[triangle_index] = mesh_manipulation.change_to;
            triangles_types_index[triangle_index] = mesh_manipulation.change_to_index;

            return true;
        }

        //Debug.Log($"{triangles_types[triangle_index]} | {mesh_manipulation.change_from}");
        return false;
    }
}
