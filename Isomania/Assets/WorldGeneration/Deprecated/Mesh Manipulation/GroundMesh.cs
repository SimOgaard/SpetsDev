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
        public GroundTriangleType changeFrom;
        [SerializeField] private GroundTriangleType _changeTo;
        public GroundTriangleType changeTo
        {
            get { return _changeTo; }
            set {
                _changeTo = value;
                changeToIndex = GroundTriangleTypeIndex((int)value);
            }
        }
        public int changeToIndex;

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

    //private int groundSubtypesLength;

    private int triangleAmount;
    private int triangleCornerAmount;

    private int[] triangles; // holds every triangle for ground mesh
    private GroundTriangleType[] trianglesTypes; // holds every triangle state
    private int[] trianglesTypesIndex;

    private Vector2 minMaxY;
    private Vector3[] trianglesMid;

    private Dictionary<int, int>[] groundSubtypes; // holds one dictionary for each groundtype. holds triangles
    private bool[] groundSubtypesChanged; // what submesh that needs to be updated

    /*
    private int[,] groundSubtypesArray; // holds every triangles type
    private int[] groundSubtypesLengths; // what are the lengths of the submeshes?
    private int[] groundSubtypesIndex; // holds current indices
    */

    private Mesh groundMesh;
    private Vector2 unitSize;
    private Vector2Int resolution;

    private Transform child;

    private JobHandle jobHandle;
    private MeshData meshData;
    private bool isDeallocated = false;
    private void OnDestroy()
    {
        if (!isDeallocated)
        {
            if (!jobHandle.IsCompleted)
            {
                jobHandle.Complete();
            }

            meshData.Dispose();
        }
    }

    public IEnumerator CreateGround(WaitForFixedUpdate wait, Vector2 unitSize, Vector2Int resolution, NativeArray<Noise.NoiseLayer> noiseLayersNativeArray, Material staticMaterial, NoiseLayerSettings.Foliage[] foliage)
    {
        this.unitSize = unitSize;
        this.resolution = resolution;

        triangleAmount = resolution.x * resolution.y * 2;
        triangleCornerAmount = triangleAmount * 3;
        //groundSubtypesLength = foliage.Length;

        groundSubtypes = new Dictionary<int, int>[GroundTriangleTypeLength];
        groundSubtypesChanged = new bool[GroundTriangleTypeLength];
        for (int i = 0; i < GroundTriangleTypeLength; i++)
        {
            groundSubtypes[i] = new Dictionary<int, int>(triangleCornerAmount);
            groundSubtypesChanged[i] = true;
        }
        trianglesTypes = new GroundTriangleType[triangleAmount];
        trianglesTypesIndex = new int[triangleAmount];

        int quadCount = resolution.x * resolution.y;
        int vertexCount = quadCount * 4;
        int triangleCount = quadCount * 6;

        meshData = new MeshData();
        meshData.vertices = new NativeArray<Vector3>(vertexCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        meshData.triangles = new NativeArray<int>(triangleCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        meshData.normals = new NativeArray<Vector3>(vertexCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        meshData.uv = new NativeArray<Vector2>(vertexCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        meshData.midPoint = new NativeArray<Vector3>(triangleAmount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        meshData.minY__maxY = new NativeArray<float>(2, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

        CreateMesh.CreatePlaneJob job = new CreateMesh.CreatePlaneJob
        {
            meshData = meshData,
            planeSize = resolution,
            quadSize = unitSize,
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
            noiseLayers = noiseLayersNativeArray
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

        minMaxY.x = meshData.minY__maxY[0];
        minMaxY.y = meshData.minY__maxY[1];

        trianglesMid = meshData.midPoint.ToArray();
        triangles = meshData.triangles.ToArray();

        // init parrent (this) that holds all const/static meshes
        MeshRenderer thisMeshRenderer = gameObject.AddComponent<MeshRenderer>();
        MeshFilter thisMeshFilter = gameObject.AddComponent<MeshFilter>();
        MeshCollider thisMeshCollider = gameObject.AddComponent<MeshCollider>();

        // create mesh for ground
        Mesh colliderGroundMesh = new Mesh();
        colliderGroundMesh.vertices = meshData.vertices.ToArray();
        colliderGroundMesh.triangles = meshData.triangles.ToArray();
        colliderGroundMesh.normals = meshData.normals.ToArray();
        colliderGroundMesh.uv = meshData.uv.ToArray();

        thisMeshRenderer.material = staticMaterial;
        thisMeshFilter.sharedMesh = colliderGroundMesh;
        thisMeshCollider.sharedMesh = colliderGroundMesh;

        // create child that holds changing meshes
        child = new GameObject("chaning meshes").transform;
        MeshRenderer childMeshRenderer = child.gameObject.AddComponent<MeshRenderer>();
        MeshFilter childMeshFilter = child.gameObject.AddComponent<MeshFilter>();

        child.transform.parent = transform;
        child.transform.localPosition = Vector3.up * 1.5f;

        // setup multi mesh for child
        groundMesh = new Mesh();
        groundMesh.MarkDynamic();
        groundMesh.vertices = meshData.vertices.ToArray();
        groundMesh.normals = meshData.normals.ToArray();
        groundMesh.uv = meshData.uv.ToArray();
        groundMesh.subMeshCount = GroundTriangleTypeLength;

        // native array of size triangleAmount
        NativeArray<GroundTriangleType> groundTypeNativeArray = new NativeArray<GroundTriangleType>(triangleAmount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        NativeArray<int> groundTypeIndexNativeArray = new NativeArray<int>(triangleAmount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        // create FoliageJob.FoliageNativeArray[]
        NativeArray<CreateMesh.FoliageJob.FoliageNativeArray> foliageNativeArray = new NativeArray<CreateMesh.FoliageJob.FoliageNativeArray>(foliage.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        for (int i = 0; i < foliage.Length; i++)
        {
            foliageNativeArray[i] = CreateMesh.FoliageJob.createFoliageNativeArray(foliage[i]);
        }
        // pass them into a FoliageJob and execute
        CreateMesh.FoliageJob foliageJob = new CreateMesh.FoliageJob
        {
            triangleAmount = triangleAmount,
            originalMeshData = meshData,
            foliage = foliageNativeArray,
            groundTypeNativeArray = groundTypeNativeArray,
            groundTypeIndexNativeArray = groundTypeIndexNativeArray,
            chunkOffset = child.position,
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
        //      groundSubtypes[nativearray[i][i] = triangles[i]
        for (int i = 0; i < triangleAmount; i++)
        {
            trianglesTypes[i] = groundTypeNativeArray[i];
            trianglesTypesIndex[i] = groundTypeIndexNativeArray[i];

            if (trianglesTypes[i] ==(GroundTriangleType) 0)
            {
                continue;
            }

            int cornerTriangle = i * 3;
            groundSubtypes[groundTypeIndexNativeArray[i]][cornerTriangle] = triangles[cornerTriangle];
            groundSubtypes[groundTypeIndexNativeArray[i]][cornerTriangle + 1] = triangles[cornerTriangle + 1];
            groundSubtypes[groundTypeIndexNativeArray[i]][cornerTriangle + 2] = triangles[cornerTriangle + 2];
        }
        // dispose of native arrays
        groundTypeNativeArray.Dispose();
        groundTypeIndexNativeArray.Dispose();
        foliageNativeArray.Dispose();

        // get materials for each foliage
        Material[] foliageMaterials = new Material[GroundTriangleTypeLength];
        for (int i = 0; i < foliage.Length; i++)
        {
            Debug.Log(foliage[i].type);
            foliageMaterials[MeshManipulationState.GroundTriangleTypeIndex((int)foliage[i].type)] = foliage[i].material.material;
        }
        childMeshRenderer.materials = foliageMaterials;
        childMeshFilter.sharedMesh = groundMesh;

        isDeallocated = true;

        meshData.Dispose();
    }

    // set triangles for each affected submesh:
    // goes quickly
    private void LateUpdate()
    {
        if (isDeallocated)
        {
            for (int i = 0; i < GroundTriangleTypeLength; i++)
            {
                if (groundSubtypesChanged[i])
                {
                    groundSubtypesChanged[i] = false;

                    int[] trianglesDictCopy = new int[groundSubtypes[i].Count];
                    groundSubtypes[i].Values.CopyTo(trianglesDictCopy, 0);
                    groundMesh.SetTriangles(trianglesDictCopy, i, false);
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

    public void SwitchTrainglesInCollider(Collider collider, MeshManipulationState[] meshManipulations)
    {
        (Vector2Int boundsMinToMeshIndex, Vector2Int boundsMaxToMeshIndex, Vector2 minMaxY) = ColliderBounds(collider.bounds, unitSize, resolution);

        // if collider is inbetween min max
        if (minMaxY.x < minMaxY.y || minMaxY.y > minMaxY.x)
        {
            int maxX = boundsMaxToMeshIndex.x * 2;
            int startX = boundsMinToMeshIndex.x * 2;

            if (startX >= maxX)
            {
                return;
            }

            int maxZ = boundsMaxToMeshIndex.y * resolution.y * 2;
            int addZ = resolution.y * 2;

            for (int z = boundsMinToMeshIndex.y * resolution.y * 2; z < maxZ; z += addZ)
            {
                for (int x = startX; x < maxX; x++)
                {
                    int triangleIndex = z + x;
                    if (IsInside1(collider, trianglesMid[triangleIndex] + child.position, 0.5f))
                    {
                        for (int i = 0; i < meshManipulations.Length; i++)
                        {
                            if (SwitchTraingle(meshManipulations[i], triangleIndex * 3, triangleIndex))
                            {
                                break;
                            }
                        }
                    }
                }
            }
        }
    }

    public void SwitchTrainglesInCollider(Collider collider, MeshManipulationState meshManipulation)
    {
        (Vector2Int boundsMinToMeshIndex, Vector2Int boundsMaxToMeshIndex, Vector2 minMaxY) = ColliderBounds(collider.bounds, unitSize, resolution);

        // if collider is inbetween min max
        if (minMaxY.x < minMaxY.y || minMaxY.y > minMaxY.x)
        {
            int maxX = boundsMaxToMeshIndex.x * 2;
            int startX = boundsMinToMeshIndex.x * 2;

            if (startX >= maxX)
            {
                return;
            }

            int maxZ = boundsMaxToMeshIndex.y * resolution.y * 2;
            int addZ = resolution.y * 2;

            for (int z = boundsMinToMeshIndex.y * resolution.y * 2; z < maxZ; z += addZ)
            {
                for (int x = startX; x < maxX; x++)
                {
                    int triangleIndex = z + x;
                    if (IsInside1(collider, trianglesMid[triangleIndex] + child.position, 0.5f))
                    {
                        SwitchTraingle(meshManipulation, triangleIndex * 3, triangleIndex);
                    }
                }
            }
        }
    }

    private (Vector2Int boundsMinToMeshIndex, Vector2Int boundsMaxToMeshIndex, Vector2 minMaxY) ColliderBounds(Bounds bounds, Vector2 unitSize, Vector2Int resolution)
    {
        Vector2 meshSize = Vector2.Scale(unitSize, resolution);
        Vector3 meshSizeHalf = new Vector3(meshSize.x * 0.5f, 0f, meshSize.y * 0.5f);

        Vector3 meshMidPoint = child.position;
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

    private bool SwitchTraingle(MeshManipulationState meshManipulation, int triangleCornerIndex, int triangleIndex)
    {
        // removes selected triangle from its submesh
        void RemoveTriangle()
        {
            // show that this mesh was updated
            groundSubtypesChanged[trianglesTypesIndex[triangleIndex]] = true;

            // remove triangle from last dict/submesh
            groundSubtypes[trianglesTypesIndex[triangleIndex]].Remove(triangleCornerIndex);
            groundSubtypes[trianglesTypesIndex[triangleIndex]].Remove(triangleCornerIndex + 1);
            groundSubtypes[trianglesTypesIndex[triangleIndex]].Remove(triangleCornerIndex + 2);
        }

        // adds a triangle to specified submesh at triangle point
        void AddTriangle()
        {
            // show that this mesh was updated
            groundSubtypesChanged[meshManipulation.changeToIndex] = true;

            // place new triangle in new submesh
            groundSubtypes[meshManipulation.changeToIndex][triangleCornerIndex] = triangles[triangleCornerIndex];
            groundSubtypes[meshManipulation.changeToIndex][triangleCornerIndex + 1] = triangles[triangleCornerIndex + 1];
            groundSubtypes[meshManipulation.changeToIndex][triangleCornerIndex + 2] = triangles[triangleCornerIndex + 2];
        }

        // if allready placed triangle is of any type in changeFrom
        if (MeshManipulationState.IsIn(trianglesTypes[triangleIndex], meshManipulation.changeFrom) && trianglesTypes[triangleIndex] != meshManipulation.changeTo)
        {
            if (meshManipulation.changeTo == (GroundTriangleType) 0)
            {
                // we should only remove triangle
                //Debug.Log("shouldt happen");
                RemoveTriangle();
            }
            else if (trianglesTypes[triangleIndex] == (GroundTriangleType) 0)
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
            trianglesTypes[triangleIndex] = meshManipulation.changeTo;
            trianglesTypesIndex[triangleIndex] = meshManipulation.changeToIndex;

            return true;
        }

        //Debug.Log($"{trianglesTypes[triangleIndex]} | {meshManipulation.changeFrom}");
        return false;
    }
}
