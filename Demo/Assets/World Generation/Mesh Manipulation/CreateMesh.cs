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

    public NativeArray<Vector3> midPoint;
    public NativeArray<float> minY__maxY;
}

public struct MeshDataList
{
    public NativeList<Vector3> vertices;
    public NativeList<int> triangles;
    public NativeList<Vector3> normals;
    public NativeList<Vector2> uv;

    public NativeArray<Vector3> midPoint;
    public NativeArray<float> minY__maxY;
}

public static class MeshDataExtensions
{
    public static void Dispose(this MeshData meshData)
    {
        meshData.vertices.Cleanup();
        meshData.triangles.Cleanup();
        meshData.normals.Cleanup();
        meshData.uv.Cleanup();

        meshData.midPoint.Cleanup();
        meshData.minY__maxY.Cleanup();
    }
    public static void Dispose(this MeshDataList meshData)
    {
        meshData.vertices.Dispose();
        meshData.triangles.Dispose();
        meshData.normals.Dispose();
        meshData.uv.Dispose();

        meshData.midPoint.Cleanup();
        meshData.minY__maxY.Cleanup();
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
    public static MeshData CreateMeshData(Vector2 unitSize, Vector2Int resolution, Vector3 offset)
    {
        resolution.x = Mathf.Max(resolution.x + 1, 2);
        resolution.y = Mathf.Max(resolution.y + 1, 2);

        unitSize.x = Mathf.Max(unitSize.x, 0.01f);
        unitSize.y = Mathf.Max(unitSize.y, 0.01f);

        int verticesLength = resolution.x * resolution.y;
        int trianglesLength = (resolution.x - 1) * (resolution.y - 1) * 2 * 3;

        MeshData meshData = new MeshData();

        meshData.vertices = new NativeArray<Vector3>(verticesLength, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        meshData.triangles = new NativeArray<int>(trianglesLength, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        meshData.normals = new NativeArray<Vector3>(verticesLength, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        meshData.uv = new NativeArray<Vector2>(verticesLength, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

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
                int triIndex = i / 3;
                meshData.midPoint[triIndex] = (meshData.vertices[meshData.triangles[i]] + meshData.vertices[meshData.triangles[i + 1]] + meshData.vertices[meshData.triangles[i + 2]]) / 3.0f;

                if (meshData.midPoint[triIndex].y < meshData.minY__maxY[0])
                {
                    meshData.minY__maxY[0] = meshData.midPoint[triIndex].y;
                }
                if (meshData.midPoint[triIndex].y > meshData.minY__maxY[1])
                {
                    meshData.minY__maxY[1] = meshData.midPoint[triIndex].y;
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

        [ReadOnly] public NativeArray<Noise.NoiseLayer> noiseLayers;
        [ReadOnly] public Vector3 offset;

        public void Execute()
        {
            int noiseLength = noiseLayers.Length;
            for (int q = 0; q < noiseLength; q++)
            {
                if (noiseLayers[q].enabled)
                {
                    for (int i = 0; i < vertices.Length; i++)
                    {
                        vertices[i] = new Vector3(vertices[i].x, vertices[i].y + noiseLayers[q].GetNoiseValue(vertices[i].x + offset.x, vertices[i].z + offset.z), vertices[i].z);
                    }
                }
            }
        }
    }

    public struct FoliageJob : IJob
    {
        public static FoliageNativeArray createFoliageNativeArray(NoiseLayerSettings.Foliage randomFoliage)
        {
            FoliageNativeArray native = new FoliageNativeArray();
            native.type = randomFoliage.type;
            native.enabled = randomFoliage.enabled;
            native.noiseLayer = new Noise.NoiseLayer(randomFoliage.noiseLayer);
            native.keepRangeNoise = randomFoliage.keepRangeNoise;
            native.keepRangeRandomNoise = randomFoliage.keepRangeRandomNoise;
            native.keepRangeRandom = randomFoliage.keepRangeRandom;
            return native;
        }

        public struct FoliageNativeArray
        {
            [ReadOnly] public GroundMesh.GroundTriangleType type;
            [ReadOnly] public bool enabled;
            [ReadOnly] public Noise.NoiseLayer noiseLayer;
            [ReadOnly] public Vector2 keepRangeNoise;
            [ReadOnly] public float keepRangeRandomNoise;
            [ReadOnly] public float keepRangeRandom;
        }

        [ReadOnly] public int triangleAmount;
        [ReadOnly] public MeshData originalMeshData;
        [ReadOnly] public NativeArray<FoliageNativeArray> foliage;
        [ReadOnly] public Vector3 chunkOffset;

        [WriteOnly] public NativeArray<GroundMesh.GroundTriangleType> groundTypeNativeArray;
        [WriteOnly] public NativeArray<int> groundTypeIndexNativeArray;

        public void Execute()
        {
            System.Random rand = new System.Random();

            // iterate each triangle
            for (int i = 0; i < triangleAmount; i++)
            {
                groundTypeNativeArray[i] = (GroundMesh.GroundTriangleType) 0;
                groundTypeIndexNativeArray[i] = -1;

                float randomValue = (float)rand.NextDouble();

                // for each foliage
                for (int f = 0; f < foliage.Length; f++)
                {
                    if (foliage[f].enabled)
                    {
                        if (randomValue <= foliage[f].keepRangeRandom)
                        {
                            groundTypeNativeArray[i] = foliage[f].type;
                            groundTypeIndexNativeArray[i] = GroundMesh.MeshManipulationState.GroundTriangleTypeIndex((int)foliage[f].type);
                            break;
                        }
                        else
                        {
                            float noiseValue = foliage[f].noiseLayer.GetNoiseValue(originalMeshData.midPoint[i].x + chunkOffset.x, originalMeshData.midPoint[i].z + chunkOffset.z);

                            bool keepThresholdLow = noiseValue > foliage[f].keepRangeNoise.x;
                            bool keepThresholdHigh = noiseValue < foliage[f].keepRangeNoise.y;
                            bool keepRandom = randomValue <= (noiseValue - foliage[f].keepRangeNoise.x) * foliage[f].keepRangeRandomNoise;

                            if (keepThresholdLow && keepThresholdHigh && keepRandom)
                            {
                                groundTypeNativeArray[i] = foliage[f].type;
                                groundTypeIndexNativeArray[i] = GroundMesh.MeshManipulationState.GroundTriangleTypeIndex((int)foliage[f].type);
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
        public MeshDataList meshDataList; // copy over the already created mesh to this as a list instead
        [ReadOnly] public MeshData originalMeshData; // copy over the already created mesh to this as a list instead

        [ReadOnly] public Noise.NoiseLayer noiseLayer; // = new Noise.NoiseLayer(settingsNoiseLayer);
        //Random.InitState(settingsNoiseLayer.generalNoise.seed);

        [ReadOnly] public Vector2 keepRangeNoise;
        [ReadOnly] public float keepRangeRandomNoise;
        [ReadOnly] public float keepRangeRandom;
        [ReadOnly] public Vector3 offset;
        [ReadOnly] public Vector3 transformOffset;

        private void AddTriangle(int vertIndex_1, int vertIndex_2, int vertIndex_3)
        {
            meshDataList.triangles.Add(meshDataList.vertices.Length);
            meshDataList.triangles.Add(meshDataList.vertices.Length + 1);
            meshDataList.triangles.Add(meshDataList.vertices.Length + 2);

            meshDataList.vertices.Add(originalMeshData.vertices[vertIndex_1] + offset);
            meshDataList.vertices.Add(originalMeshData.vertices[vertIndex_2] + offset);
            meshDataList.vertices.Add(originalMeshData.vertices[vertIndex_3] + offset);

            meshDataList.normals.Add(originalMeshData.normals[vertIndex_1]);
            meshDataList.normals.Add(originalMeshData.normals[vertIndex_2]);
            meshDataList.normals.Add(originalMeshData.normals[vertIndex_3]);

            meshDataList.uv.Add(originalMeshData.uv[vertIndex_1]);
            meshDataList.uv.Add(originalMeshData.uv[vertIndex_2]);
            meshDataList.uv.Add(originalMeshData.uv[vertIndex_3]);
        }

        public void Execute()
        {
            System.Random rand = new System.Random();

            for (int triangleIndex = 0; triangleIndex < originalMeshData.triangles.Length; triangleIndex += 3)
            {
                int vertIndex_1 = originalMeshData.triangles[triangleIndex];
                int vertIndex_2 = originalMeshData.triangles[triangleIndex + 1];
                int vertIndex_3 = originalMeshData.triangles[triangleIndex + 2];

                float randomValue = (float) rand.NextDouble();
                if (randomValue <= keepRangeRandom)
                {
                    AddTriangle(vertIndex_1, vertIndex_2, vertIndex_3);
                }
                else
                {
                    float x = ((originalMeshData.vertices[vertIndex_1].x + originalMeshData.vertices[vertIndex_2].x + originalMeshData.vertices[vertIndex_3].x) / 3f) + transformOffset.x;
                    float z = ((originalMeshData.vertices[vertIndex_1].z + originalMeshData.vertices[vertIndex_2].z + originalMeshData.vertices[vertIndex_3].z) / 3f) + transformOffset.z;

                    float noiseValue = noiseLayer.GetNoiseValue(x, z);
                    if (noiseValue > keepRangeNoise.x && noiseValue < keepRangeNoise.y && noiseValue * randomValue <= keepRangeRandomNoise)
                    {
                        AddTriangle(vertIndex_1, vertIndex_2, vertIndex_3);
                    }
                }
            }
        }
    }

    public static int[] CreateTrianglesForMesh(Vector2Int resolution)
    {
        int trianglesLength = (resolution.x - 1) * (resolution.y - 1) * 2 * 3;
        int[] triangles = new int[trianglesLength];

        int verticesIndex = 0;
        int trianglesIndex = 0;
        for (int z = 0; z < resolution.y; z++)
        {
            for (int x = 0; x < resolution.x; x++)
            {
                verticesIndex = z * resolution.x + x;
                if (x != resolution.x - 1 && z != 0)
                {
                    triangles[trianglesIndex] = verticesIndex;
                    triangles[trianglesIndex + 1] = verticesIndex - resolution.x + 1;
                    triangles[trianglesIndex + 2] = verticesIndex - resolution.x;

                    triangles[trianglesIndex + 3] = verticesIndex;
                    triangles[trianglesIndex + 4] = verticesIndex + 1;
                    triangles[trianglesIndex + 5] = verticesIndex - resolution.x + 1;

                    trianglesIndex += 6;
                }
            }
        }
        return triangles;
    }

    /*
    public static void CreateMeshByNoiseOnThread()
    {
        Vector3[] vertices = new Vector3[verticesLength];
        Vector3[] normals = new Vector3[trianglesLength / 3];


    }
    */

    public static IEnumerator CreateMeshByNoise(WaitForFixedUpdate wait, Noise.NoiseLayer[] noiseLayers, Vector2 unitSize, Vector2Int resolution, Vector3 offset)
    {
        Mesh mesh = new Mesh();

        resolution.x = Mathf.Max(resolution.x + 1, 2);
        resolution.y = Mathf.Max(resolution.y + 1, 2);

        unitSize.x = Mathf.Max(unitSize.x, 0.01f);
        unitSize.y = Mathf.Max(unitSize.y, 0.01f);

        int verticesLength = resolution.x * resolution.y;
        int trianglesLength = (resolution.x - 1) * (resolution.y - 1) * 2 * 3;

        Vector3[] vertices = new Vector3[verticesLength];
        int[] triangles = new int[trianglesLength];

        Vector3 reusedPoint = Vector3.zero;
        int verticesIndex = 0;
        int trianglesIndex = 0;

        int noiseLength = noiseLayers.Length;
        for (int z = 0; z < resolution.y; z++)
        {
            reusedPoint.z = unitSize.y * z - (resolution.y - 1) * unitSize.y * 0.5f;

            for (int x = 0; x < resolution.x; x++)
            {
                verticesIndex = z * resolution.x + x;
                reusedPoint.x = unitSize.x * x - (resolution.x - 1) * unitSize.x * 0.5f;
                reusedPoint.y = 0f;

                float xValue = reusedPoint.x + offset.x;
                float zValue = reusedPoint.z + offset.z;
                for (int q = 0; q < noiseLength; q++)
                {
                    if (noiseLayers[q].enabled)
                    {
                        // CAN GIVE ERRORS WITH WRONG VALUES, TRY EXCEPT OR MAX/MIN/RANGE VALUE
                        reusedPoint.y += noiseLayers[q].GetNoiseValue(xValue, zValue);
                    }
                }

                vertices[verticesIndex] = reusedPoint;

                if (x != resolution.x - 1 && z != 0)
                {
                    triangles[trianglesIndex] = verticesIndex;
                    triangles[trianglesIndex + 1] = verticesIndex - resolution.x + 1;
                    triangles[trianglesIndex + 2] = verticesIndex - resolution.x;

                    triangles[trianglesIndex + 3] = verticesIndex;
                    triangles[trianglesIndex + 4] = verticesIndex + 1;
                    triangles[trianglesIndex + 5] = verticesIndex - resolution.x + 1;

                    trianglesIndex += 6;
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

    public static Mesh CreateMeshByNoise(ref Mesh mesh, Noise.NoiseLayer[] noiseLayers, Vector2 unitSize, Vector2Int resolution, Vector3 offset)
    {
        mesh = new Mesh();

        resolution.x = Mathf.Max(resolution.x + 1, 2);
        resolution.y = Mathf.Max(resolution.y + 1, 2);

        unitSize.x = Mathf.Max(unitSize.x, 0.01f);
        unitSize.y = Mathf.Max(unitSize.y, 0.01f);

        int verticesLength = resolution.x * resolution.y;
        int trianglesLength = (resolution.x - 1) * (resolution.y - 1) * 2 * 3;

        Vector3[] vertices = new Vector3[verticesLength + 1];
        int[] triangles = new int[trianglesLength];

        vertices[verticesLength] = new Vector3(0f, -5f, 0f); // value that wont overblow bounding box of mesh so culling still works, yet low enough to be behind opaque material

        Vector3 reusedPoint = Vector3.zero;
        int verticesIndex = 0;
        int trianglesIndex = 0;

        int noiseLength = noiseLayers.Length;
        for (int z = 0; z < resolution.y; z++)
        {
            reusedPoint.z = unitSize.y * z - (resolution.y - 1) * unitSize.y * 0.5f;

            for (int x = 0; x < resolution.x; x++)
            {
                verticesIndex = z * resolution.x + x;
                reusedPoint.x = unitSize.x * x - (resolution.x - 1) * unitSize.x * 0.5f;
                reusedPoint.y = 0f;

                float xValue = reusedPoint.x + offset.x;
                float zValue = reusedPoint.z + offset.z;
                for (int q = 0; q < noiseLength; q++)
                {
                    if (noiseLayers[q].enabled)
                    {
                        // CAN GIVE ERRORS WITH WRONG VALUES, TRY EXCEPT OR MAX/MIN/RANGE VALUE
                        reusedPoint.y += noiseLayers[q].GetNoiseValue(xValue, zValue);
                    }
                }

                vertices[verticesIndex] = reusedPoint;

                if (x != resolution.x - 1 && z != 0)
                {
                    triangles[trianglesIndex] = verticesIndex;
                    triangles[trianglesIndex + 1] = verticesIndex - resolution.x + 1;
                    triangles[trianglesIndex + 2] = verticesIndex - resolution.x;

                    triangles[trianglesIndex + 3] = verticesIndex;
                    triangles[trianglesIndex + 4] = verticesIndex + 1;
                    triangles[trianglesIndex + 5] = verticesIndex - resolution.x + 1;

                    trianglesIndex += 6;
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

    public static IEnumerator DropMeshVertices(WaitForFixedUpdate wait, Mesh referenceMesh, NoiseLayerSettings.NoiseLayer settingsNoiseLayer, Vector2 keepRangeNoise, float keepRangeRandomNoise, float keepRangeRandom, Vector3 offset, Vector3 transformOffset)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        Noise.NoiseLayer noiseLayer = new Noise.NoiseLayer(settingsNoiseLayer);
        Random.InitState(settingsNoiseLayer.generalNoise.seed);

        Vector3[] verticesCopy = referenceMesh.vertices;
        int[] trianglesCopy = referenceMesh.triangles;

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

        for (int triangleIndex = 0; triangleIndex < trianglesCopy.Length; triangleIndex += 3)
        {
            Vector3 vertice_1 = verticesCopy[trianglesCopy[triangleIndex]];
            Vector3 vertice_2 = verticesCopy[trianglesCopy[triangleIndex + 1]];
            Vector3 vertice_3 = verticesCopy[trianglesCopy[triangleIndex + 2]];

            float x = (vertice_1.x + vertice_2.x + vertice_3.x) / 3 + transformOffset.x;
            float z = (vertice_1.z + vertice_2.z + vertice_3.z) / 3 + transformOffset.z;

            float randomValue = Random.value;
            if (randomValue <= keepRangeRandom)
            {
                AddVertice(vertice_1, vertice_2, vertice_3);
            }
            else
            {
                float noiseValue = noiseLayer.GetNoiseValue(x, z);
                if (noiseValue > keepRangeNoise.x && noiseValue < keepRangeNoise.y && noiseValue * randomValue <= keepRangeRandomNoise)
                {
                    AddVertice(vertice_1, vertice_2, vertice_3);
                }
            }

            if (triangleIndex % 999 == 0)
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

        GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
        meshRenderer.material = material;
        return gameObject;

        /*
        GameObject gameObject = new GameObject(name);
        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;
        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.material = material;
        return gameObject;
        */
    }

    private enum MeshType { Cube, Quad, Icosahedron }
    [SerializeField] private MeshType meshType = MeshType.Icosahedron;
    private void Awake()
    {
        MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();

        switch (meshType)
        {
            case MeshType.Cube:
                meshFilter.mesh = CubeMesh();
                break;
            case MeshType.Quad:
                meshFilter.mesh = QuadMesh();
                break;
            case MeshType.Icosahedron:
                meshFilter.mesh = IcosahedronMesh();
                break;
            default:
                break;
        }
        Destroy(this);
    }
}