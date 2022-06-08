using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using UnityEngine.Rendering;

public class Chunk : MonoBehaviour
{
    /// <summary>
    /// One static triangle in chunk ground mesh, OBS place it inside Ground
    /// </summary>
    public struct StaticTriangle
    {
        /// <summary>
        /// One of the static biome indices defined at worldGenerationSettings.Update();
        /// </summary>
        public int biome;
    }

    #region Init
    public IEnumerator LoadChunk(WorldGenerationSettings worldGenerationSetting)
    {
        // initilize variables
        isLoading = true;




        /*
        // init ground type enum values
        groundTriangleTypesArray = GroundTriangleType.GetValues(typeof(GroundTriangleType)) as GroundTriangleType[];
        groundTriangleTypeLength = groundTriangleTypesArray.Length;

        // create subtype array
        subtype = new Subtype[groundTriangleTypeLength];
        for (int i = 0; i < groundTriangleTypeLength; i++)
        {
            subtype[i] = new Subtype
            {
                count = 0, // gets set after compute shader calculations
                hasChanged = true,
                type = groundTriangleTypesArray[i],
                typeIndex = MeshManipulationState.GroundTriangleTypeIndex((int)groundTriangleTypesArray[i])
            };
        }
        */

        // now we need to start doing some work on the gpu, but to do that we need to wait for the computeshader to finnish
        WaitForEndOfFrame wait = new WaitForEndOfFrame();
        while (Ground.GPUData.computeShaderOccupied)
        {
            yield return wait;
        }
        // here we have the whole shader to ourselves >:)
        Ground.GPUData.computeShaderOccupied = true;

        // start by setting values specific for this/the next dispatch
        Ground.GPUData.computeShader.SetFloats("meshXZPosition", transform.position.x, transform.position.z);

        // and then dispatch the triangle biome selector kernel
        Ground.GPUData.computeShader.Dispatch(0, Ground.CPUData.quadCountHeight, 1, 1);
        /*
        // copy triangle biomes over to cpu
        int[] trianglesBiomes = new int[Ground.GPUData.triangleCount];
        Ground.GPUData.BufferTriangleBiome.GetData(trianglesBiomes);

        for (int i = 0; i < Ground.GPUData.triangleCount; i++)
        {
            if (trianglesBiomes[i] == 1)
                Debug.Log(trianglesBiomes[i]);
        }
        // just print it out because debugging
        */

        // and then dispatch the vertice biome selector kernel
        Ground.GPUData.computeShader.Dispatch(1, Ground.CPUData.verticeCountHeightExtended, 1, 1);
        /*
        // copy triangle biomes over to cpu
        int[] verticesBiomes = new int[Ground.GPUData.verticeCountExtended];
        Ground.GPUData.BufferVerticeBiome.GetData(verticesBiomes);

        for (int i = 0; i < Ground.GPUData.verticeCountExtended; i++)
        {
            if (verticesBiomes[i] == 0)
                Debug.Log(verticesBiomes[i]);
        }
        // just print it out because debugging
        */

        // now dispatch the vertice offset kernel that uses each vertices biome for displacement
        Ground.GPUData.computeShader.Dispatch(2, Ground.CPUData.verticeCountHeightExtended, 1, 1);

        // create mesh bounds using the max and min vertice y value of mesh
        Ground.GPUData.BufferVerticesMinYValues.GetData(Ground.CPUData.minYArray);
        Ground.GPUData.BufferVerticesMaxYValues.GetData(Ground.CPUData.maxYArray);

        float minY = float.MaxValue;
        float maxY = float.MinValue;

        for (int i = 0; i < Ground.CPUData.verticeCountHeight; i++)
        {
            if (Ground.CPUData.minYArray[i] < minY)
                minY = Ground.CPUData.minYArray[i];
            if (Ground.CPUData.maxYArray[i] > maxY)
                maxY = Ground.CPUData.maxYArray[i];
        }

        float boundsCenterOffset = (minY + maxY) * 0.5f;
        float boundsHeight = (maxY - minY);
        Bounds meshBounds = new Bounds(Vector3.up * boundsCenterOffset, new Vector3(Ground.chunkSize.x, boundsHeight, Ground.chunkSize.y));

        // also get the vertice y value
        Ground.GPUData.BufferVerticesExtendedY.GetData(Ground.CPUData.verticeYArrayExtended);

        // and fill verticeArray y values from extended
        for (int z = 0; z < Ground.CPUData.verticeCountHeight; z++)
        {
            int zExtended = z + 1;
            for (int x = 0; x < Ground.CPUData.verticeCountWidth; x++)
            {
                int xExtended = x + 1;

                int vertexIndex = (z * Ground.CPUData.verticeCountWidth) + x;
                int vertexIndexExtended = (zExtended * Ground.CPUData.verticeCountWidthExtended) + xExtended;
         
                Ground.CPUData.verticeArray[vertexIndex].y = Ground.CPUData.verticeYArrayExtended[vertexIndexExtended];
            }
        }

        /*
        // copy vertice y values over to cpu
        float[] verticesExtendedY = new float[Ground.GPUData.verticeCountExtended];
        Vector2[] verticesExtendedXZ = new Vector2[Ground.GPUData.verticeCountExtended];
        Ground.GPUData.BufferVerticesExtendedY.GetData(verticesExtendedY);
        Ground.GPUData.BufferVerticesExtendedXZ.GetData(verticesExtendedXZ);

        for (int i = 0; i < Ground.GPUData.verticeCountExtended; i++)
        {
            Debug.Log(new Vector3(verticesExtendedXZ[i].x, verticesExtendedY[i], verticesExtendedXZ[i].y));
        }
        // just print it out because debugging
        */

        // now we know xyz of each vertice in the mesh so dispatch the NormalizeMeshTriangles kernel
        Ground.GPUData.computeShader.Dispatch(3, Ground.CPUData.quadCountHeightExtended, 1, 1);
        /*
        // copy triangle normals to cpu
        Vector3[] trianglesNormals = new Vector3[Ground.CPUData.triangleCountExtended];
        Ground.GPUData.TriangleExtendedNormalsBuffer.GetData(trianglesNormals);

        for (int h = 0; h < Ground.CPUData.quadCountHeightExtended; h++)
        {
            for (int w = 0; w < Ground.CPUData.quadCountWidthExtended; w++)
            {
                int triangleIndex = w * 2 + h * Ground.CPUData.quadCountWidthExtended * 2;

                Debug.Log($"{trianglesNormals[triangleIndex].x}, {trianglesNormals[triangleIndex].y}, {trianglesNormals[triangleIndex].z}");
                Debug.Log($"{trianglesNormals[triangleIndex + 1].x}, {trianglesNormals[triangleIndex + 1].y}, {trianglesNormals[triangleIndex + 1].z}");
            }
        }
        Debug.LogError("We want to pause now sir plz");
        yield break;
        // just print it out because debugging
        */

        // now we need to calculate the mesh normal since nothing more of the mesh vertices will change
        Ground.GPUData.computeShader.Dispatch(4, Ground.CPUData.verticeCountHeight, 1, 1);

        // get mesh normals
        Ground.GPUData.VerticesNormalsBuffer.GetData(Ground.CPUData.verticeNormalArray);

        /*
        // copy vertice normals to cpu
        Vector3[] verticesNormals = new Vector3[Ground.GPUData.verticeCount];
        Ground.GPUData.TriangleExtendedNormalsBuffer.GetData(verticesNormals);

        for (int i = 0; i < Ground.GPUData.verticeCount; i++)
        {
            Debug.Log(verticesNormals[i]);
        }
        // just print it out because debugging
        */

        // now we calculate the triangles mid points y values
        Ground.GPUData.computeShader.Dispatch(5, Ground.CPUData.verticeCountHeight, 1, 1);

        // and fetch the largest and smallest values to 

        // now we know each triangles biome for when we will select which triangles are in which submeshes

        // and then dispatch the mesh noise kernel
        //Ground.GPUData.computeShader.Dispatch(0, groundMeshConst.verticeWidth, 1, 1);

        // copy over height to cpu with AsyncGPUReadback
        /*
        void OnCompleteReadback(AsyncGPUReadbackRequest request)
        {
            if (request.hasError || !request.done)
            {
                Debug.Log("Lamaop");
                groundMeshConst.CanReadBackBuffer = false;
                return;
            }

            groundMeshConst.CanReadBackBuffer = true;
            groundMeshConst.RWBufferVerticeY.GetData(groundMeshConst.verticesMeshY);
        }
        do
        {
            yield return wait;
            AsyncGPUReadback.Request(groundMeshConst.RWBufferVerticeY, OnCompleteReadback);
        }
        while (!groundMeshConst.CanReadBackBuffer);
        */
        /*
        groundMeshConst.RWBufferVerticeY.GetData(groundMeshConst.verticesMeshY);

        // set final y values to mesh vertices
        for (int i = 0; i < groundMeshConst.verticeAmount; i++)
        {
            groundMeshConst.verticesMesh[i].y = groundMeshConst.verticesMeshY[i];
        }
        */

        // create a mesh using data fetched from gpu
        Mesh groundMesh = new Mesh()
        {
            vertices = Ground.CPUData.verticeArray,
            triangles = Ground.CPUData.triangleArray,
            normals = Ground.CPUData.verticeNormalArray,
            bounds = meshBounds,
        };
        // get the largest and smallest y values from the rows for this mesh, do this before specifying bounds size
        // signal that we no longer need the compute shader
        Ground.GPUData.computeShaderOccupied = false;


        // wait to offset the next lag spike
        yield return wait;
        yield return wait;

        //.CopyTo(, 0);
        //groundMesh.RecalculateNormals(); // do not do this, calculate it on gpu, then you can also account for triangles outside the corners to get the normal, this is to remove the seam between chunks

        // create ground game object
        GameObject groundGameObject = WorldGenerationManager.InitNewChild(transform, "Ground");
        // create a mesh renderer, mesh filter and collider to the ground game object
        MeshRenderer staticMeshRenderer = groundGameObject.AddComponent<MeshRenderer>();
        MeshFilter staticMeshFilter = groundGameObject.AddComponent<MeshFilter>();
        MeshCollider staticMeshCollider = groundGameObject.AddComponent<MeshCollider>();

        staticMeshRenderer.material = worldGenerationSetting.defaultBiomeMaterials.biomeMaterial.materialSettings.material;
        staticMeshFilter.mesh = groundMesh;
        staticMeshCollider.sharedMesh = groundMesh;

        // create foliage game object
        GameObject foliageGameObject = WorldGenerationManager.InitNewChild(groundGameObject.transform, "Foliage");
        // copy ground mesh to foliage game object

        // initilizes all parrent objects of prefabs
        GameObject landMarksGameObject = WorldGenerationManager.InitNewChild(transform, InstantiateInstruction.PlacableGameObjectsParrent.landMarks);
        GameObject rocksGameObject = WorldGenerationManager.InitNewChild(transform, InstantiateInstruction.PlacableGameObjectsParrent.rocks);
        GameObject treesGameObject = WorldGenerationManager.InitNewChild(transform, InstantiateInstruction.PlacableGameObjectsParrent.trees);
        GameObject chestsGameObject = WorldGenerationManager.InitNewChild(transform, InstantiateInstruction.PlacableGameObjectsParrent.chests);
        GameObject enemiesGameObject = WorldGenerationManager.InitNewChild(transform, InstantiateInstruction.PlacableGameObjectsParrent.enemies);
        enemies = enemiesGameObject.AddComponent<Enemies>();

        // spawns prefabs
        InstantiatePrefabs spawnPrefabs = gameObject.AddComponent<InstantiatePrefabs>();
        //yield return StartCoroutine(spawnPrefabs.Spawn(new WaitForFixedUpdate(), noiseLayerSettings.spawnPrefabs, noiseLayerSettings.objectDensity, groundMeshConst.chunkSize));

        // set layer to gameworld
        Layer.SetRecursiveTo(gameObject, Layer.gameWorldStatic);

        // signal that this chunk is done loading
        isLoading = false;
        isLoaded = true;
        WorldGenerationManager.chunksInLoading.Remove(this);
    }
    #endregion

    #region IsLoaded
    public bool isLoading = false;
    public bool isLoaded = false;

    private Enemies enemies;

    public float DistToPlayer()
    {
        return (transform.position - MainCamera.CameraRayHitPlane()).magnitude;
    }

    private void Update()
    {
        if (DistToPlayer() > Ground.chunkDisableDistance && isLoaded)
        {
            enemies.MoveParrent();
            gameObject.SetActive(false);
        }
    }
    #endregion

    public void OnDrawGizmos()
    {
        var r = gameObject.GetComponentInChildren<MeshRenderer>();
        if (r == null)
            return;
        var bounds = r.bounds;
        Gizmos.matrix = Matrix4x4.identity;
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(bounds.center, bounds.extents * 2);
    }

    #region MeshManipulation
    // how many groundTriangleTypes do we have
    public static int groundTriangleTypeLength;
    [System.Flags]
    public enum GroundTriangleType
    {
        Nothing = 1,
        Grass = 2,
        GrassTrampled = 4,
        Flower = 8,
        Wheat = 16,
        WheatTrampled = 32,
    }
    private GroundTriangleType[] groundTriangleTypesArray;

    /// <summary>
    /// From what to what triangle type should we transform to
    /// </summary>
    [System.Serializable]
    public struct MeshManipulationState
    {
        public GroundTriangleType changeFrom;
        [SerializeField] private GroundTriangleType _changeTo;
        public GroundTriangleType changeTo
        {
            get { return _changeTo; }
            set
            {
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

            int i = 1;
            int pos = 1;

            // Iterate through bits of n till we find a set bit
            // i&n will be non-zero only when 'i' and 'n' have a set bit
            // at same position
            while ((i & n) == 0)
            {
                // Unset current bit and set the next bit in 'i'
                i <<= 1;

                // increment position
                ++pos;
            }
            return pos - 1;
        }
    }

    /// <summary>
    /// Contains information for all triangles of its represented type
    /// </summary>
    private struct Subtype
    {
        public GroundTriangleType type; // what triangle type is it?
        public int typeIndex;
        public int count; // how many of those triangles are there?
        public bool hasChanged; // has this type been updated this frame?
    }
    private Subtype[] subtype; // holds info for all submeshes, the array is allready ordered from typeIndex

    /// <summary>
    /// Contains information for a single triangle of type
    /// </summary>
    public struct Triangle
    {
        // the 3 vertex indicees for the represented triangle 
        public int t0;
        public int t1;
        public int t2;

        // the midpoint for this triangle
        public Vector3 midPoint;

        // what type is this triangle
        public GroundTriangleType type;
        public int typeIndex;
    }
    private Triangle[] triangles; // holds all triangles for current mesh

    // constant value that holds min and max y vertice for this mesh calculated from compute shader
    private float minY;
    private float maxY;

    private Mesh groundMesh;
    private Transform child;

    /*
    // set triangles for each affected submesh:
    private void LateUpdate()
    {
        if (isLoaded)
        {
            for (int i = 0; i < groundTriangleTypeLength; i++)
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

    public void CreateGround()
    {
        

        groundSubtypes = new Dictionary<int, int>[groundTriangleTypeLength];
        groundSubtypesChanged = new bool[groundTriangleTypeLength];
        for (int i = 0; i < groundTriangleTypeLength; i++)
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
        
        //CreateMesh.NormalizeJob normalizeJob = new CreateMesh.NormalizeJob
        //{
        //    quadCount = quadCount,
        //    meshData = meshData
        //};
        
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

            if (trianglesTypes[i] == (GroundTriangleType)0)
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
        // move collider instead of doing trianglesMid[triangleIndex] + child.position then move it back to where it was

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
        // move collider instead of doing trianglesMid[triangleIndex] + child.position then move it back to where it was

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
            if (meshManipulation.changeTo == (GroundTriangleType)0)
            {
                // we should only remove triangle
                //Debug.Log("shouldt happen");
                RemoveTriangle();
            }
            else if (trianglesTypes[triangleIndex] == (GroundTriangleType)0)
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
    */
    #endregion
}
