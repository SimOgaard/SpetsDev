using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using UnityEngine.Rendering;

public class Chunk : MonoBehaviour
{    
    public class Ground
    {
        public void Update(WorldGenerationSettings worldGenerationSettings)
        {
            chunkSize = worldGenerationSettings.chunkSettings.chunkSize;
            quadAmount = worldGenerationSettings.chunkSettings.quadAmount;

            chunkLoadDist = worldGenerationSettings.chunkSettings.chunkLoadDist;
            chunkDisableDistance = worldGenerationSettings.chunkSettings.chunkDisableDistance;
            chunkEnableDistance = worldGenerationSettings.chunkSettings.chunkEnableDistance;

            // read in the correct compute shader that has multiple kernels to reduce the amount of buffers that need to be set
            computeShader = Resources.Load<ComputeShader>("ComputeShaders/ChunkGround");
            computeShaderOccupied = false;

            // get mesh sizes
            verticeWidth = quadAmount.x + 1;
            verticeHeight = quadAmount.y + 1;
            verticeAmount = verticeWidth * verticeHeight;

            quadAmountInt = quadAmount.x * quadAmount.y;
            triangleAmount = quadAmountInt * 2;
            triangleIndexAmount = triangleAmount * 3;

            // set new vertices and triangles from newly calculated sizes
            trianglesMesh = new int[triangleIndexAmount];
            normals = new Vector3[triangleAmount];
            verticesMesh = new Vector3[verticeAmount];
            verticesMesh2D = new Vector2[verticeAmount];
            verticesMeshY = new float[verticeAmount];
            triangles = new Triangle[triangleAmount];

            // populate vertices starting from bottom left with an offset of chunkSize * 0.5f
            float constOffsetX = -chunkSize.x * 0.5f;
            float constOffsetZ = -chunkSize.y * 0.5f;

            float iterativeOffsetX = chunkSize.x / quadAmount.x;
            float iterativeOffsetZ = chunkSize.y / quadAmount.y;

            for (int z = 0; z < verticeHeight; z++)
            {
                for (int x = 0; x < verticeWidth; x++)
                {
                    int vertexIndex = (z * verticeWidth) + x;
                    verticesMesh[vertexIndex] = new Vector3(constOffsetX + iterativeOffsetX * x, 0f, constOffsetZ + iterativeOffsetZ * z);
                    verticesMesh2D[vertexIndex] = new Vector2(verticesMesh[vertexIndex].x, verticesMesh[vertexIndex].z);
                }
            }

            // itterate every quad and populate the triangles with correct vertice indecees for that quad
            for (int z = 0; z < quadAmount.y; z++)
            {
                for (int x = 0; x < quadAmount.x; x++)
                {
                    // get all the correct indicees
                    int quadIndex = (z * quadAmount.x) + x;
                    int quadTriangleIndex = quadIndex * 2;
                    int quadTriangleVertIndex = quadTriangleIndex * 3;

                    // get the indicees for verticees row and col
                    int verticeIndexBot = z * verticeWidth; // 2 * 4
                    int verticeIndexTop = (z + 1) * verticeWidth; // 3 * 4
                    int verticeIndexLeft = x; // 2
                    int verticeIndexRight = x + 1; // 2 + 1

                    // using indicees for row and col get verticees indicees for all four corners
                    int verticeIndexBL = verticeIndexBot + verticeIndexLeft; // 2 * 4 + 2 = 10
                    int verticeIndexTL = verticeIndexTop + verticeIndexLeft; // 3 * 4 + 2 = 14
                    int verticeIndexBR = verticeIndexBot + verticeIndexRight;// 2 * 4 + 3 = 11
                    int verticeIndexTR = verticeIndexTop + verticeIndexRight;// 3 * 4 + 3 = 15

                    // populate bottom left triangle
                    trianglesMesh[quadTriangleVertIndex + 0] = verticeIndexBL;
                    trianglesMesh[quadTriangleVertIndex + 1] = verticeIndexTL;
                    trianglesMesh[quadTriangleVertIndex + 2] = verticeIndexBR;

                    triangles[quadTriangleIndex].t0 = verticeIndexBL;
                    triangles[quadTriangleIndex].t1 = verticeIndexTL;
                    triangles[quadTriangleIndex].t2 = verticeIndexBR;

                    triangles[quadTriangleIndex].midPoint = (verticesMesh[verticeIndexBL] + verticesMesh[verticeIndexTL] + verticesMesh[verticeIndexBR]) / 3f;

                    normals[quadTriangleIndex] = Vector3.up;

                    // popultae topright triangle
                    trianglesMesh[quadTriangleVertIndex + 3] = verticeIndexTL;
                    trianglesMesh[quadTriangleVertIndex + 4] = verticeIndexTR;
                    trianglesMesh[quadTriangleVertIndex + 5] = verticeIndexBR;

                    quadTriangleIndex++;
                    triangles[quadTriangleIndex].t0 = verticeIndexTL;
                    triangles[quadTriangleIndex].t1 = verticeIndexTR;
                    triangles[quadTriangleIndex].t2 = verticeIndexBR;

                    triangles[quadTriangleIndex].midPoint = (verticesMesh[verticeIndexTL] + verticesMesh[verticeIndexTR] + verticesMesh[verticeIndexBR]) / 3f;

                    normals[quadTriangleIndex] = Vector3.up;
                }
            }

            // We now have precalculated triangles and verticees that we can copy over
            // Therefore only need to change the y value of verticees and triangles.midPoint as well as triangle type

            // set constant vertex xz position
            BufferVertice = new ComputeBuffer(verticeAmount, sizeof(float) * 2);
            BufferVertice.SetData(verticesMesh2D);
            computeShader.SetBuffer(0, "vertXZPos", BufferVertice);

            // set read write buffer for vertex y values, needs to be available on all three kernels
            RWBufferVerticeY = new ComputeBuffer(verticeAmount, sizeof(float)); // is constant the size of the buffer or is the values constant?
            for (int i = 0; i < 3; i++)
            {
                computeShader.SetBuffer(i, "vertYPos", RWBufferVerticeY);
            }

            // set readback buffer size
            ReadBackBuffer = new NativeArray<float>(verticeAmount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

            List<NoiseSettings.fnl_state> fnl_States = new List<NoiseSettings.fnl_state>();
            fnl_States.Add(new NoiseSettings.fnl_state(worldGenerationSettings.biomes[0].groundNoise[0], 0, false));

            // we require a const structured buffer of fnl_state for each biomes noiseSettings
            FNLStateBuffer = new ComputeBuffer(1, NoiseSettings.fnl_state.size);
            FNLStateBuffer.SetData<NoiseSettings.fnl_state>(fnl_States);
            computeShader.SetBuffer(0, "FNLStateBuffer", FNLStateBuffer);
        }

        public void Destroy()
        {
            if (RWBufferVerticeY != null)
            {
                RWBufferVerticeY.Dispose();
                RWBufferVerticeY = null;
            }
            if (BufferVertice != null)
            {
                BufferVertice.Dispose();
                BufferVertice = null;
            }
            if (FNLStateBuffer != null)
            {
                FNLStateBuffer.Dispose();
                FNLStateBuffer = null;
            }
            if (ReadBackBuffer.IsCreated)
            {
                ReadBackBuffer.Dispose();
            }
        }

        public Vector2 chunkSize;
        public Vector2Int quadAmount;

        public float chunkLoadDist;
        public float chunkDisableDistance;
        public float chunkEnableDistance;

        public int[] trianglesMesh;

        public Vector3[] normals;
        public Vector3[] verticesMesh;
        public Vector2[] verticesMesh2D;
        public float[] verticesMeshY;

        public Triangle[] triangles;

        private int _verticeWidth;
        public int verticeWidth
        {
            get { return _verticeWidth; }
            set { _verticeWidth = value; computeShader.SetInt("verticeWidth", value); }
        }
        private int _verticeHeight;
        public int verticeHeight
        {
            get { return _verticeHeight; }
            set { _verticeHeight = value; computeShader.SetInt("verticeHeight", value); }
        }
        private int _verticeAmount;
        public int verticeAmount
        {
            get { return _verticeAmount; }
            set { _verticeAmount = value; computeShader.SetInt("verticeAmount", value); }
        }

        private int _quadAmountInt;
        public int quadAmountInt
        {
            get { return _quadAmountInt; }
            set { _quadAmountInt = value; computeShader.SetInt("quadAmount", value); }
        }
        private int _triangleAmount;
        public int triangleAmount
        {
            get { return _triangleAmount; }
            set { _triangleAmount = value; computeShader.SetInt("triangleAmount", value); }
        }
        private int _triangleIndexAmount;
        public int triangleIndexAmount
        {
            get { return _triangleIndexAmount; }
            set { _triangleIndexAmount = value; computeShader.SetInt("triangleIndexAmount", value); }
        }

        public bool computeShaderOccupied; // is the compute shader occupied?
        public ComputeShader computeShader; // compute shader that is used to generate mesh
        public ComputeBuffer BufferVertice; // is readonly (constant) and holds all vertice xz values for this mesh, is used to get 2d noise
        public ComputeBuffer RWBufferVerticeY; // is writeonly and holds all vertices y values for this mesh, is later read from the gpu to create the mesh
        public ComputeBuffer FNLStateBuffer; // contains the fast noise lite states
        //public ComputeBuffer computeBufferTriangles; // holds all triangles for this mesh
        public NativeArray<float> ReadBackBuffer;
        public bool CanReadBackBuffer;
    }
    public static Ground groundMeshConst = new Ground();
    
    /// <summary>
    /// One static triangle in chunk ground mesh
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
        while (groundMeshConst.computeShaderOccupied)
        {
            yield return wait;
        }
        // here we have the whole shader to ourselves >:)
        groundMeshConst.computeShaderOccupied = true;
        
        // start by setting values specific for this/the next dispatch
        groundMeshConst.computeShader.SetFloats("meshXZPos", transform.position.x, transform.position.z);

        // and then dispatch the mesh noise kernel
        groundMeshConst.computeShader.Dispatch(0, groundMeshConst.verticeWidth, 1, 1);

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
        groundMeshConst.RWBufferVerticeY.GetData(groundMeshConst.verticesMeshY);

        // set final y values to mesh vertices
        for (int i = 0; i < groundMeshConst.verticeAmount; i++)
        {
            groundMeshConst.verticesMesh[i].y = groundMeshConst.verticesMeshY[i];
        }

        // create a mesh using precalculated triangle and vertex xz as well as newly calculated y positions
        Mesh groundMesh = new Mesh()
        {
            vertices = groundMeshConst.verticesMesh,
            triangles = groundMeshConst.trianglesMesh,
            bounds = new Bounds(Vector3.zero, new Vector3(groundMeshConst.chunkSize.x, 10f, groundMeshConst.chunkSize.y))
        };
        // get the largest and smallest y values from the rows for this mesh, do this before specifying bounds size

        // signal that we no longer need the compute shader
        groundMeshConst.computeShaderOccupied = false;

        // wait to offset the next lag spike
        yield return wait;
        yield return wait;

        //.CopyTo(, 0);
        groundMesh.RecalculateNormals(); // do not do this, calculate it on gpu, then you can also account for triangles outside the corners to get the normal, this is to remove the seam between chunks

        // create ground game object
        GameObject groundGameObject = WorldGenerationManager.InitNewChild(transform, "Ground");
        // create a mesh renderer, mesh filter and collider to the ground game object
        MeshRenderer staticMeshRenderer = groundGameObject.AddComponent<MeshRenderer>();
        MeshFilter staticMeshFilter = groundGameObject.AddComponent<MeshFilter>();
        MeshCollider staticMeshCollider = groundGameObject.AddComponent<MeshCollider>();

        staticMeshRenderer.material = worldGenerationSetting.defaultBiomeMaterials.biomeMaterial.biomeMaterial.material;
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
        return (transform.position - PixelPerfectCameraRotation.CameraRayHitPlane()).magnitude;
    }

    private void Update()
    {
        if (DistToPlayer() > groundMeshConst.chunkDisableDistance / PixelPerfectCameraRotation.zoom && isLoaded)
        {
            enemies.MoveParrent();
            gameObject.SetActive(false);
        }
    }
    #endregion

    public void OnDrawGizmos()
    {
        var r = GetComponent<MeshRenderer>();
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
