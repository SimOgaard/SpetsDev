using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public struct MeshData
{
    public NativeArray<Vector3> vertices;
    public NativeArray<int> triangles;
    public NativeArray<Vector3> normals;
    public NativeArray<Vector2> uv;

    public NativeArray<Vector3> mid_point;
    public NativeArray<float> min_y__max_y;
}

public struct MeshDataList
{
    public NativeList<Vector3> vertices;
    public NativeList<int> triangles;
    public NativeList<Vector3> normals;
    public NativeList<Vector2> uv;

    public NativeArray<Vector3> mid_point;
    public NativeArray<float> min_y__max_y;
}

public static class MeshDataExtensions
{
    public static void Dispose(this MeshData meshData)
    {
        meshData.vertices.Cleanup();
        meshData.triangles.Cleanup();
        meshData.normals.Cleanup();
        meshData.uv.Cleanup();

        meshData.mid_point.Cleanup();
        meshData.min_y__max_y.Cleanup();
    }
    public static void Dispose(this MeshDataList meshData)
    {
        meshData.vertices.Dispose();
        meshData.triangles.Dispose();
        meshData.normals.Dispose();
        meshData.uv.Dispose();

        meshData.mid_point.Cleanup();
        meshData.min_y__max_y.Cleanup();
    }
}

public static class NativeArrayExtensions
{
    public static void Cleanup<T>(this NativeArray<T> array)
        where T : struct
    {
        if (array.IsCreated)
            array.Dispose();
    }
}

public class CreateMesh : MonoBehaviour
{
    public static MeshData CreateMeshData(Vector2 unit_size, Vector2Int resolution, Vector3 offset)
    {
        resolution.x = Mathf.Max(resolution.x + 1, 2);
        resolution.y = Mathf.Max(resolution.y + 1, 2);

        unit_size.x = Mathf.Max(unit_size.x, 0.01f);
        unit_size.y = Mathf.Max(unit_size.y, 0.01f);

        int vertices_length = resolution.x * resolution.y;
        int triangles_length = (resolution.x - 1) * (resolution.y - 1) * 2 * 3;

        MeshData meshData = new MeshData();

        meshData.vertices = new NativeArray<Vector3>(vertices_length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        meshData.triangles = new NativeArray<int>(triangles_length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        meshData.normals = new NativeArray<Vector3>(vertices_length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        meshData.uv = new NativeArray<Vector2>(vertices_length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

        return meshData;
    }

    public struct CreatePlaneJob : IJob
    {
        [ReadOnly] public Vector2Int planeSize;
        [ReadOnly] public Vector2 quadSize;
        [ReadOnly] public int quadCount;
        [ReadOnly] public int vertexCount;
        [ReadOnly] public int triangleCount;

        [WriteOnly] public MeshData meshData;

        public void Execute()
        {
            var halfWidth = (planeSize.x * quadSize.x) * .5f;
            var halfLength = (planeSize.y * quadSize.y) * .5f;

            for (int i = 0; i < quadCount; i++)
            {
                var x = i % planeSize.x;
                var z = i / planeSize.x;

                var left = (x * quadSize.x) - halfWidth;
                var right = (left + quadSize.x);

                var bottom = (z * quadSize.y) - halfLength;
                var top = (bottom + quadSize.y);

                var v = i * 4;

                meshData.vertices[v + 0] = new Vector3(left, 0, bottom);
                meshData.vertices[v + 1] = new Vector3(left, 0, top);
                meshData.vertices[v + 2] = new Vector3(right, 0, top);
                meshData.vertices[v + 3] = new Vector3(right, 0, bottom);

                var t = i * 6;

                meshData.triangles[t + 0] = v + 0;
                meshData.triangles[t + 1] = v + 1;
                meshData.triangles[t + 2] = v + 2;
                meshData.triangles[t + 3] = v + 2;
                meshData.triangles[t + 4] = v + 3;
                meshData.triangles[t + 5] = v + 0;

                meshData.normals[v + 0] = Vector3.up;
                meshData.normals[v + 1] = Vector3.up;
                meshData.normals[v + 2] = Vector3.up;
                meshData.normals[v + 3] = Vector3.up;

                meshData.uv[v + 0] = new Vector2(0, 0);
                meshData.uv[v + 1] = new Vector2(0, 1);
                meshData.uv[v + 2] = new Vector2(1, 1);
                meshData.uv[v + 3] = new Vector2(1, 0);
            }
        }
    }

    public struct ParallelNoiseJob : IJobParallelFor
    {
        public NativeArray<Vector3> vertices;
        [ReadOnly] public NativeArray<Vector2> octaveOffsets;
        [ReadOnly] public Vector3 offset;
        [ReadOnly] public Vector2 scroll;
        [ReadOnly] public float time;
        [ReadOnly] public float scale;
        [ReadOnly] public int octaves;
        [ReadOnly] public float persistance;
        [ReadOnly] public float lacunarity;
        [ReadOnly] public float maxPossibleHeight;
        [ReadOnly] public float meshScale;

        public void Execute(int index)
        {
            var vertex = vertices[index];

            var amplitude = 1f;
            var frequency = 1f;
            var noiseHeight = 0f;

            for (int i = 0; i < octaves; i++)
            {
                var sampleX = (vertex.x + octaveOffsets[i].x) / scale * frequency;
                var sampleY = (vertex.z + octaveOffsets[i].y) / scale * frequency;

                var perlinValue = Mathf.PerlinNoise(sampleX + scroll.x * time, sampleY + scroll.y * time) * 2 - 1;
                noiseHeight += perlinValue * amplitude;

                amplitude *= persistance;
                frequency *= lacunarity;
            }

            var normalizedHeight = (noiseHeight + 1) / maxPossibleHeight;
            var y = Mathf.Clamp(normalizedHeight, 0f, int.MaxValue - vertex.y);

            vertices[index] = new Vector3(vertex.x, y * meshScale, vertex.z);
        }
    }

    public struct NoiseJob : IJob
    {
        public NativeArray<Vector3> vertices;
        [ReadOnly] public NativeArray<Vector2> octaveOffsets;
        [ReadOnly] public Vector2 offset;
        [ReadOnly] public Vector2 scroll;
        [ReadOnly] public float time;
        [ReadOnly] public float scale;
        [ReadOnly] public int octaves;
        [ReadOnly] public float persistance;
        [ReadOnly] public float lacunarity;
        [ReadOnly] public float maxPossibleHeight;
        [ReadOnly] public float meshScale;

        public void Execute()
        {
            var vertexCount = vertices.Length;
            for (var index = 0; index < vertexCount; index++)
            {
                var vertex = vertices[index];

                var amplitude = 1f;
                var frequency = 1f;
                var noiseHeight = 0f;

                for (int i = 0; i < octaves; i++)
                {
                    var sampleX = (vertex.x + octaveOffsets[i].x) / scale * frequency;
                    var sampleY = (vertex.z + octaveOffsets[i].y) / scale * frequency;

                    var perlinValue = Mathf.PerlinNoise(sampleX + scroll.x * time, sampleY + scroll.y * time) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                var normalizedHeight = (noiseHeight + 1) / maxPossibleHeight;
                var y = Mathf.Clamp(normalizedHeight, 0f, int.MaxValue - vertex.y);

                vertices[index] = new Vector3(vertex.x, y * meshScale, vertex.z);
            }
        }
    }

    public struct NormalizeJob : IJob
    {
        [ReadOnly] public int quadCount;
        public MeshData meshData;

        public void Execute()
        {
            for (int i = 0; i < quadCount; i++)
            {
                var v = i * 4;
                var a = meshData.vertices[v + 0];
                var b = meshData.vertices[v + 1];
                var c = meshData.vertices[v + 2];
                var p = Vector3.Cross((b - a), (c - a));
                var l = p.magnitude;
                var r = p / l;

                meshData.normals[v + 0] = r;
                meshData.normals[v + 1] = r;
                meshData.normals[v + 2] = r;

                a = meshData.vertices[v + 2];
                b = meshData.vertices[v + 3];
                c = meshData.vertices[v + 0];
                p = Vector3.Cross((b - a), (c - a));
                l = p.magnitude;
                r = p / l;

                meshData.normals[v + 2] = r;
                meshData.normals[v + 3] = r;
                meshData.normals[v + 0] = r;
            }
        }
    }

    /// <summary>
    ///     Recalculate the normals of a mesh based on an angle threshold. This takes
    ///     into account distinct vertices that have the same position.
    /// </summary>
    /// <param name="mesh"></param>
    /// <param name="angle">
    ///     The smoothing angle. Note that triangles that already share
    ///     the same vertex will be smooth regardless of the angle! 
    /// </param>
    public struct SmoothNormalizeJob : IJob
    {
        [ReadOnly] public int triangleCount;
        [ReadOnly] public int vertexCount;        

        [ReadOnly] public float angle;
        public MeshData meshData;

        private struct VertexKey
        {
            private readonly long _x;
            private readonly long _y;
            private readonly long _z;

            // Change this if you require a different precision.
            private const int Tolerance = 100000;

            // Magic FNV values. Do not change these.
            private const long FNV32Init = 0x811c9dc5;
            private const long FNV32Prime = 0x01000193;

            public VertexKey(Vector3 position)
            {
                _x = (long)(Mathf.Round(position.x * Tolerance));
                _y = (long)(Mathf.Round(position.y * Tolerance));
                _z = (long)(Mathf.Round(position.z * Tolerance));
            }

            public override bool Equals(object obj)
            {
                var key = (VertexKey)obj;
                return _x == key._x && _y == key._y && _z == key._z;
            }

            public override int GetHashCode()
            {
                long rv = FNV32Init;
                rv ^= _x;
                rv *= FNV32Prime;
                rv ^= _y;
                rv *= FNV32Prime;
                rv ^= _z;
                rv *= FNV32Prime;

                return rv.GetHashCode();
            }
        }

        private struct VertexEntry
        {
            public int TriangleIndex;
            public int VertexIndex;

            public VertexEntry(int triIndex, int vertIndex)
            {
                TriangleIndex = triIndex;
                VertexIndex = vertIndex;
            }
        }

        public void Execute()
        {
            float cosineThreshold = Mathf.Cos(angle * Mathf.Deg2Rad);
            Vector3[] triNormals = new Vector3[triangleCount / 3];
            Dictionary<VertexKey, List<VertexEntry>> dictionary = new Dictionary<VertexKey, List<VertexEntry>>(vertexCount);

            for (var i = 0; i < triangleCount; i += 3)
            {
                int i1 = meshData.triangles[i];
                int i2 = meshData.triangles[i + 1];
                int i3 = meshData.triangles[i + 2];

                // Calculate the normal of the triangle
                Vector3 p1 = meshData.vertices[i2] - meshData.vertices[i1];
                Vector3 p2 = meshData.vertices[i3] - meshData.vertices[i1];
                Vector3 normal = Vector3.Cross(p1, p2).normalized;
                int triIndex = i / 3;
                triNormals[triIndex] = normal;

                List<VertexEntry> entry;
                VertexKey key;

                if (!dictionary.TryGetValue(key = new VertexKey(meshData.vertices[i1]), out entry))
                {
                    entry = new List<VertexEntry>(4);
                    dictionary.Add(key, entry);
                }
                entry.Add(new VertexEntry(triIndex, i1));

                if (!dictionary.TryGetValue(key = new VertexKey(meshData.vertices[i2]), out entry))
                {
                    entry = new List<VertexEntry>();
                    dictionary.Add(key, entry);
                }
                entry.Add(new VertexEntry(triIndex, i2));

                if (!dictionary.TryGetValue(key = new VertexKey(meshData.vertices[i3]), out entry))
                {
                    entry = new List<VertexEntry>();
                    dictionary.Add(key, entry);
                }
                entry.Add(new VertexEntry(triIndex, i3));
            }

            // Each entry in the dictionary represents a unique vertex position.

            foreach (var vertList in dictionary.Values)
            {
                for (var i = 0; i < vertList.Count; ++i)
                {

                    Vector3 sum = new Vector3();
                    var lhsEntry = vertList[i];

                    for (var j = 0; j < vertList.Count; ++j)
                    {
                        var rhsEntry = vertList[j];

                        if (lhsEntry.VertexIndex == rhsEntry.VertexIndex)
                        {
                            sum += triNormals[rhsEntry.TriangleIndex];
                        }
                        else
                        {
                            // The dot product is the cosine of the angle between the two triangles.
                            // A larger cosine means a smaller angle.
                            float dot = Vector3.Dot(
                                triNormals[lhsEntry.TriangleIndex],
                                triNormals[rhsEntry.TriangleIndex]);
                            if (dot >= cosineThreshold)
                            {
                                sum += triNormals[rhsEntry.TriangleIndex];
                            }
                        }
                    }

                    meshData.normals[lhsEntry.VertexIndex] = sum.normalized;
                }
            }
        }
    }

    public struct MidPointJob : IJob
    {
        [ReadOnly] public int triangleCount;
        public MeshData meshData;

        public void Execute()
        {
            for (int i = 0; i < triangleCount; i+=3)
            {
                int tri_index = i / 3;
                meshData.mid_point[tri_index] = (meshData.vertices[meshData.triangles[i]] + meshData.vertices[meshData.triangles[i + 1]] + meshData.vertices[meshData.triangles[i + 2]]) / 3.0f;

                if (meshData.mid_point[tri_index].y < meshData.min_y__max_y[0])
                {
                    meshData.min_y__max_y[0] = meshData.mid_point[tri_index].y;
                }
                if (meshData.mid_point[tri_index].y > meshData.min_y__max_y[1])
                {
                    meshData.min_y__max_y[1] = meshData.mid_point[tri_index].y;
                }
            }
        }
    }

    // the triangles are the same list for each mesh no?
    // just reuse it in chunk.loadchunk

    /// <summary>
    /// 
    /// </summary>
    public struct CreateMeshByNoiseJob : IJob
    {
        public NativeArray<Vector3> vertices;

        [ReadOnly] public NativeArray<Noise.NoiseLayer> noise_layers;
        [ReadOnly] public Vector3 offset;

        public void Execute()
        {
            int noise_length = noise_layers.Length;
            for (int q = 0; q < noise_length; q++)
            {
                if (noise_layers[q].enabled)
                {
                    for (int i = 0; i < vertices.Length; i++)
                    {
                        vertices[i] = new Vector3(vertices[i].x, vertices[i].y + noise_layers[q].GetNoiseValue(vertices[i].x + offset.x, vertices[i].z + offset.z), vertices[i].z);
                    }
                }
            }
        }
    }

    public struct FoliageJob : IJob
    {
        public static FoliageNativeArray create_foliage_native_array(NoiseLayerSettings.Foliage random_foliage)
        {
            FoliageNativeArray native = new FoliageNativeArray();
            native.enabled = random_foliage.enabled;
            native.noise_layer = new Noise.NoiseLayer(random_foliage.noise_layer);
            native.keep_range_noise = random_foliage.keep_range_noise;
            native.keep_range_random_noise = random_foliage.keep_range_random_noise;
            native.keep_range_random = random_foliage.keep_range_random;
            return native;
        }

        public struct FoliageNativeArray
        {
            [ReadOnly] public bool enabled;
            [ReadOnly] public Noise.NoiseLayer noise_layer;
            [ReadOnly] public Vector2 keep_range_noise;
            [ReadOnly] public float keep_range_random_noise;
            [ReadOnly] public float keep_range_random;
        }

        [ReadOnly] public int triangle_amount;
        [ReadOnly] public MeshData original_mesh_data;
        [ReadOnly] public NativeArray<FoliageNativeArray> foliage;
        [ReadOnly] public Vector3 chunk_offset;

        [WriteOnly] public NativeArray<int> ground_type_native_array;

        public void Execute()
        {
            System.Random rand = new System.Random();

            // iterate each triangle
            for (int i = 0; i < triangle_amount; i++)
            {
                ground_type_native_array[i] = -1;

                float random_value = (float)rand.NextDouble();

                // for each foliage
                for (int f = 0; f < foliage.Length; f++)
                {
                    if (foliage[f].enabled)
                    {
                        if (random_value <= foliage[f].keep_range_random)
                        {
                            ground_type_native_array[i] = f;
                            break;
                        }
                        else
                        {
                            float noise_value = foliage[f].noise_layer.GetNoiseValue(original_mesh_data.mid_point[i].x + chunk_offset.x, original_mesh_data.mid_point[i].z + chunk_offset.z);

                            bool keep_threshold_low = noise_value > foliage[f].keep_range_noise.x;
                            bool keep_threshold_high = noise_value < foliage[f].keep_range_noise.y;
                            bool keep_random = random_value <= (noise_value - foliage[f].keep_range_noise.x) * foliage[f].keep_range_random_noise;

                            if (keep_threshold_low && keep_threshold_high && keep_random)
                            {
                                ground_type_native_array[i] = f;
                                break;
                            }
                        }
                        // triangle middle
                        // if foliage is valid for this triangle
                        // set triangle to foliage type
                        // break loop
                    }
                }
            }
        }
    }

    public struct DropMeshVerticesJob : IJob
    {
        public MeshDataList mesh_data_list; // copy over the already created mesh to this as a list instead
        [ReadOnly] public MeshData original_mesh_data; // copy over the already created mesh to this as a list instead

        [ReadOnly] public Noise.NoiseLayer noise_layer; // = new Noise.NoiseLayer(settings_noise_layer);
        //Random.InitState(settings_noise_layer.general_noise.seed);

        [ReadOnly] public Vector2 keep_range_noise;
        [ReadOnly] public float keep_range_random_noise;
        [ReadOnly] public float keep_range_random;
        [ReadOnly] public Vector3 offset;
        [ReadOnly] public Vector3 transform_offset;

        private void AddTriangle(int vert_index_1, int vert_index_2, int vert_index_3)
        {
            mesh_data_list.triangles.Add(mesh_data_list.vertices.Length);
            mesh_data_list.triangles.Add(mesh_data_list.vertices.Length + 1);
            mesh_data_list.triangles.Add(mesh_data_list.vertices.Length + 2);

            mesh_data_list.vertices.Add(original_mesh_data.vertices[vert_index_1] + offset);
            mesh_data_list.vertices.Add(original_mesh_data.vertices[vert_index_2] + offset);
            mesh_data_list.vertices.Add(original_mesh_data.vertices[vert_index_3] + offset);

            mesh_data_list.normals.Add(original_mesh_data.normals[vert_index_1]);
            mesh_data_list.normals.Add(original_mesh_data.normals[vert_index_2]);
            mesh_data_list.normals.Add(original_mesh_data.normals[vert_index_3]);

            mesh_data_list.uv.Add(original_mesh_data.uv[vert_index_1]);
            mesh_data_list.uv.Add(original_mesh_data.uv[vert_index_2]);
            mesh_data_list.uv.Add(original_mesh_data.uv[vert_index_3]);
        }

        public void Execute()
        {
            System.Random rand = new System.Random();

            for (int triangle_index = 0; triangle_index < original_mesh_data.triangles.Length; triangle_index += 3)
            {
                int vert_index_1 = original_mesh_data.triangles[triangle_index];
                int vert_index_2 = original_mesh_data.triangles[triangle_index + 1];
                int vert_index_3 = original_mesh_data.triangles[triangle_index + 2];

                float random_value = (float) rand.NextDouble();
                if (random_value <= keep_range_random)
                {
                    AddTriangle(vert_index_1, vert_index_2, vert_index_3);
                }
                else
                {
                    float x = ((original_mesh_data.vertices[vert_index_1].x + original_mesh_data.vertices[vert_index_2].x + original_mesh_data.vertices[vert_index_3].x) / 3f) + transform_offset.x;
                    float z = ((original_mesh_data.vertices[vert_index_1].z + original_mesh_data.vertices[vert_index_2].z + original_mesh_data.vertices[vert_index_3].z) / 3f) + transform_offset.z;

                    float noise_value = noise_layer.GetNoiseValue(x, z);
                    if (noise_value > keep_range_noise.x && noise_value < keep_range_noise.y && noise_value * random_value <= keep_range_random_noise)
                    {
                        AddTriangle(vert_index_1, vert_index_2, vert_index_3);
                    }
                }
            }
        }
    }

    public static int[] CreateTrianglesForMesh(Vector2Int resolution)
    {
        int triangles_length = (resolution.x - 1) * (resolution.y - 1) * 2 * 3;
        int[] triangles = new int[triangles_length];

        int vertices_index = 0;
        int triangles_index = 0;
        for (int z = 0; z < resolution.y; z++)
        {
            for (int x = 0; x < resolution.x; x++)
            {
                vertices_index = z * resolution.x + x;
                if (x != resolution.x - 1 && z != 0)
                {
                    triangles[triangles_index] = vertices_index;
                    triangles[triangles_index + 1] = vertices_index - resolution.x + 1;
                    triangles[triangles_index + 2] = vertices_index - resolution.x;

                    triangles[triangles_index + 3] = vertices_index;
                    triangles[triangles_index + 4] = vertices_index + 1;
                    triangles[triangles_index + 5] = vertices_index - resolution.x + 1;

                    triangles_index += 6;
                }
            }
        }
        return triangles;
    }

    /*
    public static void CreateMeshByNoiseOnThread()
    {
        Vector3[] vertices = new Vector3[vertices_length];
        Vector3[] normals = new Vector3[triangles_length / 3];


    }
    */

    public static IEnumerator CreateMeshByNoise(WaitForFixedUpdate wait, Noise.NoiseLayer[] noise_layers, Vector2 unit_size, Vector2Int resolution, Vector3 offset)
    {
        Mesh mesh = new Mesh();

        resolution.x = Mathf.Max(resolution.x + 1, 2);
        resolution.y = Mathf.Max(resolution.y + 1, 2);

        unit_size.x = Mathf.Max(unit_size.x, 0.01f);
        unit_size.y = Mathf.Max(unit_size.y, 0.01f);

        int vertices_length = resolution.x * resolution.y;
        int triangles_length = (resolution.x - 1) * (resolution.y - 1) * 2 * 3;

        Vector3[] vertices = new Vector3[vertices_length];
        int[] triangles = new int[triangles_length];

        Vector3 reused_point = Vector3.zero;
        int vertices_index = 0;
        int triangles_index = 0;

        int noise_length = noise_layers.Length;
        for (int z = 0; z < resolution.y; z++)
        {
            reused_point.z = unit_size.y * z - (resolution.y - 1) * unit_size.y * 0.5f;

            for (int x = 0; x < resolution.x; x++)
            {
                vertices_index = z * resolution.x + x;
                reused_point.x = unit_size.x * x - (resolution.x - 1) * unit_size.x * 0.5f;
                reused_point.y = 0f;

                float x_value = reused_point.x + offset.x;
                float z_value = reused_point.z + offset.z;
                for (int q = 0; q < noise_length; q++)
                {
                    if (noise_layers[q].enabled)
                    {
                        // CAN GIVE ERRORS WITH WRONG VALUES, TRY EXCEPT OR MAX/MIN/RANGE VALUE
                        reused_point.y += noise_layers[q].GetNoiseValue(x_value, z_value);
                    }
                }

                vertices[vertices_index] = reused_point;

                if (x != resolution.x - 1 && z != 0)
                {
                    triangles[triangles_index] = vertices_index;
                    triangles[triangles_index + 1] = vertices_index - resolution.x + 1;
                    triangles[triangles_index + 2] = vertices_index - resolution.x;

                    triangles[triangles_index + 3] = vertices_index;
                    triangles[triangles_index + 4] = vertices_index + 1;
                    triangles[triangles_index + 5] = vertices_index - resolution.x + 1;

                    triangles_index += 6;
                }
            }
            yield return wait;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();
        yield return wait;
        mesh.RecalculateTangents();
        yield return wait;
        mesh.RecalculateBounds();
        yield return mesh;
    }

    public static Mesh CreateMeshByNoise(ref Mesh mesh, Noise.NoiseLayer[] noise_layers, Vector2 unit_size, Vector2Int resolution, Vector3 offset)
    {
        mesh = new Mesh();

        resolution.x = Mathf.Max(resolution.x + 1, 2);
        resolution.y = Mathf.Max(resolution.y + 1, 2);

        unit_size.x = Mathf.Max(unit_size.x, 0.01f);
        unit_size.y = Mathf.Max(unit_size.y, 0.01f);

        int vertices_length = resolution.x * resolution.y;
        int triangles_length = (resolution.x - 1) * (resolution.y - 1) * 2 * 3;

        Vector3[] vertices = new Vector3[vertices_length + 1];
        int[] triangles = new int[triangles_length];

        vertices[vertices_length] = new Vector3(0f, -5f, 0f); // value that wont overblow bounding box of mesh so culling still works, yet low enough to be behind opaque material

        Vector3 reused_point = Vector3.zero;
        int vertices_index = 0;
        int triangles_index = 0;

        int noise_length = noise_layers.Length;
        for (int z = 0; z < resolution.y; z++)
        {
            reused_point.z = unit_size.y * z - (resolution.y - 1) * unit_size.y * 0.5f;

            for (int x = 0; x < resolution.x; x++)
            {
                vertices_index = z * resolution.x + x;
                reused_point.x = unit_size.x * x - (resolution.x - 1) * unit_size.x * 0.5f;
                reused_point.y = 0f;

                float x_value = reused_point.x + offset.x;
                float z_value = reused_point.z + offset.z;
                for (int q = 0; q < noise_length; q++)
                {
                    if (noise_layers[q].enabled)
                    {
                        // CAN GIVE ERRORS WITH WRONG VALUES, TRY EXCEPT OR MAX/MIN/RANGE VALUE
                        reused_point.y += noise_layers[q].GetNoiseValue(x_value, z_value);
                    }
                }

                vertices[vertices_index] = reused_point;

                if (x != resolution.x - 1 && z != 0)
                {
                    triangles[triangles_index] = vertices_index;
                    triangles[triangles_index + 1] = vertices_index - resolution.x + 1;
                    triangles[triangles_index + 2] = vertices_index - resolution.x;

                    triangles[triangles_index + 3] = vertices_index;
                    triangles[triangles_index + 4] = vertices_index + 1;
                    triangles[triangles_index + 5] = vertices_index - resolution.x + 1;

                    triangles_index += 6;
                }
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();

        return mesh;
    }

    public static IEnumerator DropMeshVertices(WaitForFixedUpdate wait, Mesh reference_mesh, NoiseLayerSettings.NoiseLayer settings_noise_layer, Vector2 keep_range_noise, float keep_range_random_noise, float keep_range_random, Vector3 offset, Vector3 transform_offset)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        Noise.NoiseLayer noise_layer = new Noise.NoiseLayer(settings_noise_layer);
        Random.InitState(settings_noise_layer.general_noise.seed);

        Vector3[] vertices_copy = reference_mesh.vertices;
        int[] triangles_copy = reference_mesh.triangles;

        void AddVertice(Vector3 vertice_1, Vector3 vertice_2, Vector3 vertice_3)
        {
            int index = vertices.Count;

            triangles.Add(index);
            triangles.Add(index + 1);
            triangles.Add(index + 2);

            vertices.Add(vertice_1 + offset);
            vertices.Add(vertice_2 + offset);
            vertices.Add(vertice_3 + offset);
        }

        for (int triangle_index = 0; triangle_index < triangles_copy.Length; triangle_index += 3)
        {
            Vector3 vertice_1 = vertices_copy[triangles_copy[triangle_index]];
            Vector3 vertice_2 = vertices_copy[triangles_copy[triangle_index + 1]];
            Vector3 vertice_3 = vertices_copy[triangles_copy[triangle_index + 2]];

            float x = (vertice_1.x + vertice_2.x + vertice_3.x) / 3 + transform_offset.x;
            float z = (vertice_1.z + vertice_2.z + vertice_3.z) / 3 + transform_offset.z;

            float random_value = Random.value;
            if (random_value <= keep_range_random)
            {
                AddVertice(vertice_1, vertice_2, vertice_3);
            }
            else
            {
                float noise_value = noise_layer.GetNoiseValue(x, z);
                if (noise_value > keep_range_noise.x && noise_value < keep_range_noise.y && noise_value * random_value <= keep_range_random_noise)
                {
                    AddVertice(vertice_1, vertice_2, vertice_3);
                }
            }

            if (triangle_index % 999 == 0)
            {
                yield return wait;
            }
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();

        mesh.Optimize();
        yield return wait;
        mesh.RecalculateNormals();
        yield return wait;
        mesh.RecalculateTangents();
        yield return wait;
        mesh.RecalculateBounds();
        yield return mesh;
    }

    public static Mesh CubeMesh()
    {
        Vector3[] vertices = {
            new Vector3 (-0.5f, -0.5f, -0.5f),
            new Vector3 (0.5f, -0.5f, -0.5f),
            new Vector3 (0.5f, 0.5f, -0.5f),
            new Vector3 (-0.5f, 0.5f, -0.5f),
            new Vector3 (-0.5f, 0.5f, 0.5f),
            new Vector3 (0.5f, 0.5f, 0.5f),
            new Vector3 (0.5f, -0.5f, 0.5f),
            new Vector3 (-0.5f, -0.5f, 0.5f),
        };

        int[] triangles = {
            0, 2, 1, //face front
			0, 3, 2,
            2, 3, 4, //face top
			2, 4, 5,
            1, 2, 5, //face right
			1, 5, 6,
            0, 7, 4, //face left
			0, 4, 3,
            5, 4, 7, //face back
			5, 7, 6,
            0, 6, 7, //face bottom
			0, 1, 6
        };

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        return mesh;
    }

    public static Mesh QuadMesh()
    {
        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[4]
        {
            new Vector3(-0.5f, 0, -0.5f),
            new Vector3(0.5f, 0, -0.5f),
            new Vector3(-0.5f, 0, 0.5f),
            new Vector3(0.5f, 0, 0.5f)
        };
        mesh.vertices = vertices;

        int[] tris = new int[6]
        {
            0, 2, 1,
            2, 3, 1
        };
        mesh.triangles = tris;

        return mesh;
    }

    public static Mesh IcosahedronMesh()
    {
        /// <summary>
        /// Global struct of all vertices for icosahedron.
        /// </summary>
        Vector3[] GetVectors()
        {
            float s = 0.3f;
            float t = (1.0f + Mathf.Sqrt(5.0f)) / 2.0f;

            return new Vector3[]
            {
            new Vector3(-1,  t,  0) * s,
            new Vector3( 1,  t,  0) * s,
            new Vector3(-1, -t,  0) * s,
            new Vector3( 1, -t,  0) * s,
            new Vector3( 0, -1,  t) * s,
            new Vector3( 0,  1,  t) * s,
            new Vector3( 0, -1, -t) * s,
            new Vector3( 0,  1, -t) * s,
            new Vector3( t,  0, -1) * s,
            new Vector3( t,  0,  1) * s,
            new Vector3(-t,  0, -1) * s,
            new Vector3(-t,  0,  1) * s
            };
        }

        /// <summary>
        /// Global struct of all triangles for icosahedron.
        /// </summary>
        int[] GetTriangles()
        {
            return new int[]
            {
                0, 11,  5,
                0,  5,  1,
                0,  1,  7,
                0,  7, 10,
                0, 10, 11,
                1,  5,  9,
                5, 11,  4,
            11, 10,  2,
            10,  7,  6,
                7,  1,  8,
                3,  9,  4,
                3,  4,  2,
                3,  2,  6,
                3,  6,  8,
                3,  8,  9,
                4,  9,  5,
                2,  4, 11,
                6,  2, 10,
                8,  6,  7,
                9,  8,  1
            };
        }

        /// <summary>
        /// Global function returning low poly sphere mesh (icosahedron).
        /// </summary>
        Mesh mesh = new Mesh();
        mesh.vertices = GetVectors();
        mesh.triangles = GetTriangles();
        return mesh;
    }

    public static GameObject CreatePrimitive(Mesh mesh, Material material, string name = "Primitive")
    {

        GameObject game_object = GameObject.CreatePrimitive(PrimitiveType.Cube);
        MeshRenderer mesh_renderer = game_object.GetComponent<MeshRenderer>();
        mesh_renderer.material = material;
        return game_object;

        /*
        GameObject game_object = new GameObject(name);
        MeshFilter mesh_filter = game_object.AddComponent<MeshFilter>();
        mesh_filter.mesh = mesh;
        MeshRenderer mesh_renderer = game_object.AddComponent<MeshRenderer>();
        mesh_renderer.material = material;
        return game_object;
        */
    }

    private enum MeshType { Cube, Quad, Icosahedron }
    [SerializeField] private MeshType mesh_type = MeshType.Icosahedron;
    private void Awake()
    {
        MeshFilter mesh_filter = gameObject.GetComponent<MeshFilter>();

        switch (mesh_type)
        {
            case MeshType.Cube:
                mesh_filter.mesh = CubeMesh();
                break;
            case MeshType.Quad:
                mesh_filter.mesh = QuadMesh();
                break;
            case MeshType.Icosahedron:
                mesh_filter.mesh = IcosahedronMesh();
                break;
            default:
                break;
        }
        Destroy(this);
    }
}