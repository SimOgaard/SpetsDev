using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Contains everything needed for a chunk to create a ground mesh
/// </summary>
public static class Ground
{
    /// <summary>
    /// Contains functions for creating a plain mesh of specified size
    /// </summary>
    public static class Mesh
    {
        /// <summary>
        /// 
        /// </summary>
        public static (Vector3[] vertices, Vector3[] normals) Vertices(int verticeCountWidth, int verticeCountHeight, float verticeSizeWidth, float verticeSizeHeight)
        {
            return (new Vector3[0], new Vector3[0]);
        }


    }

    /// <summary>
    /// Updates both gpu and cpu data
    /// </summary>
    public static void Update(WorldGenerationSettings worldGenerationSettings)
    {
        CPUData.Update(worldGenerationSettings);
        GPUData.Update(worldGenerationSettings);
    }

    /// <summary>
    /// Mesh data specific for the gpu (larger mesh to calculate smooth normals)
    /// </summary>
    public static class GPUData
    {

        // any arrays that are meant to be readback once under initilization and never used again are declared here

        /// <summary>
        /// Is the compute shader occupied?
        /// </summary>
        public static bool computeShaderOccupied;

        /// <summary>
        /// Readonly buffer that holds all vertice xz values for ground mesh, is used alongside chunk transform offset to get noise, constant only on gpu side
        /// </summary>
        public static ComputeBuffer BufferVerticesExtendedXZ;

        /// <summary>
        /// ReadWrite buffer with all vertices y values for this mesh, is later read from the gpu to create the mesh
        /// </summary>
        public static ComputeBuffer BufferVerticesExtendedY;

        /// <summary>
        /// Readonly buffer that contains all triangle midpoints
        /// </summary>
        public static ComputeBuffer BufferMidPoint;

        /// <summary>
        /// ReadWrite buffer that contains all triangles biomes
        /// </summary>
        public static ComputeBuffer BufferTriangleBiome;

        /// <summary>
        /// ReadWrite buffer that contains all vertices biomes
        /// </summary>
        public static ComputeBuffer BufferVerticeBiome;

        /// <summary>
        /// Contains the fast noise lite states (with added data from our side like amp/index etc) for selecting biome
        /// </summary>
        public static ComputeBuffer FNLStateBiomeBuffer;

        /// <summary>
        /// Contains the fast noise lite warp states (with added data from our side like amp/index etc) for selecting biome
        /// </summary>
        public static ComputeBuffer FNLWarpStateBiomeBuffer;

        /// <summary>
        /// Contains the fast noise lite states (with added data from our side like amp/index etc) for vertex offset
        /// </summary>
        public static ComputeBuffer FNLStateVerticeOffsetBuffer;

        /// <summary>
        /// Contains the fast noise lite warp states (with added data from our side like amp/index etc) for vertex offset
        /// </summary>
        public static ComputeBuffer FNLWarpStateVerticeOffsetBuffer;

        /// <summary>
        /// Contains the triangle normals for extended mesh used to calculate all vertices normals later on
        /// </summary>
        public static ComputeBuffer TriangleExtendedNormalsBuffer;

        /// <summary>
        /// Contains the vertice normals for final mesh
        /// </summary>
        public static ComputeBuffer VerticesNormalsBuffer;

        /// <summary>
        /// Contains the min vertice y values for each thread
        /// </summary>
        public static ComputeBuffer BufferVerticesMinYValues;

        /// <summary>
        /// Contains the max vertice y values for each thread
        /// </summary>
        public static ComputeBuffer BufferVerticesMaxYValues;

        /// <summary>
        /// Contains the max vertice y values for each triangle
        /// </summary>
        public static ComputeBuffer TriangleMidpointYBuffer;

        /// <summary>
        /// Compute shader that is used to generate mesh
        /// </summary>
        public static ComputeShader computeShader;

        // WILL BE USED LATER FOR ASYNCHRONOUS READBACK FROM GPU
        /*public NativeArray<float> ReadBackBuffer;
        public bool CanReadBackBuffer;*/
        // WILL BE USED LATER FOR ASYNCHRONOUS READBACK FROM GPU

        public static void Update(WorldGenerationSettings worldGenerationSettings)
        {
            // clear allocated data if there are any
            Destroy();

            // load in the right compute shader
            computeShader = Resources.Load<ComputeShader>("ComputeShaders/ChunkGround");

            // set static properties of compute shader
            computeShader.SetInt("verticeCountWidthExtended", CPUData.verticeCountWidthExtended);
            computeShader.SetInt("quadCountWidth", CPUData.quadCountWidth);

            // data that only needs to be offloaded to a single gpu kernel once and not shared between cpu and gpu or gpu kernels
            // kernel 0 (triangle biome selector)
            // get biome noise as gpu representative structs
            List<NoiseSettings.fnl_state> FNLStateBiomeList = worldGenerationSettings.GetBiomeFNLStates();
            List<NoiseSettings.fnl_state> FNLWarpStateBiomeList = worldGenerationSettings.GetBiomeFNLWarpStates();

            // create new compute buffers
            FNLStateBiomeBuffer = new ComputeBuffer(FNLStateBiomeList.Count, NoiseSettings.fnl_state.size);
            FNLWarpStateBiomeBuffer = new ComputeBuffer(FNLWarpStateBiomeList.Count, NoiseSettings.fnl_state.size);

            // allocate data for new compute buffers
            FNLStateBiomeBuffer.SetData(FNLStateBiomeList);
            FNLWarpStateBiomeBuffer.SetData(FNLWarpStateBiomeList);

            // set those buffers to the kernel 0 for our compute shader
            computeShader.SetBuffer(0, "FNLStateBiomeBuffer", FNLStateBiomeBuffer);
            computeShader.SetBuffer(0, "FNLWarpStateBiomeBuffer", FNLWarpStateBiomeBuffer);
            computeShader.SetBuffer(1, "FNLStateBiomeBuffer", FNLStateBiomeBuffer);
            computeShader.SetBuffer(1, "FNLWarpStateBiomeBuffer", FNLWarpStateBiomeBuffer);
            computeShader.SetInt("biomeBufferLength", FNLStateBiomeList.Count);

            // set midpoints for each triangle
            Vector2[] triangleMidPoint = new Vector2[CPUData.triangleCount];
            float baseLength = ChunkSettings.meshSize.x / CPUData.quadCountWidth;
            float heightLength = ChunkSettings.meshSize.y / CPUData.quadCountHeight;
            for (int h = 0; h < CPUData.quadCountHeight; h++)
            {
                float h1 = (1f * heightLength) / 3f + h * heightLength;
                float h2 = (2f * heightLength) / 3f + h * heightLength;

                for (int w = 0; w < CPUData.quadCountWidth; w++)
                {
                    float b1 = (1f * baseLength) / 3f + w * baseLength;
                    float b2 = (2f * baseLength) / 3f + w * baseLength;

                    int triangleIndex = w * 2 + h * CPUData.quadCountWidth * 2;

                    triangleMidPoint[triangleIndex] = new Vector2
                    (
                        b1,
                        h1
                    );
                    triangleMidPoint[triangleIndex + 1] = new Vector2
                    (
                        b2,
                        h2
                    );
                }
            }

            // create and populate buffer
            BufferMidPoint = new ComputeBuffer(CPUData.triangleCount, sizeof(float) * 2);
            BufferMidPoint.SetData(triangleMidPoint);
            // set buffer to compute shader
            computeShader.SetBuffer(0, "trianglesMidPoint", BufferMidPoint);

            // create and populate buffer
            BufferTriangleBiome = new ComputeBuffer(CPUData.triangleCount, sizeof(int), ComputeBufferType.Raw);
            // set buffer to compute shader
            computeShader.SetBuffer(0, "triangleBiome", BufferTriangleBiome);

            // populate vertices starting from bottom left with an offset of (meshSize * (verticeCountExtended)/(verticeCount)) * 0.5f
            Vector2[] verticesExtendedXZ = new Vector2[CPUData.verticeCountExtended];

            float constOffsetX = -ChunkSettings.meshSize.x * 0.5f;
            float constOffsetZ = -ChunkSettings.meshSize.y * 0.5f;

            float iterativeOffsetX = ChunkSettings.meshSize.x / CPUData.quadCountWidth;
            float iterativeOffsetZ = ChunkSettings.meshSize.y / CPUData.quadCountHeight;

            for (int z = 0; z < CPUData.verticeCountHeightExtended; z++)
            {
                for (int x = 0; x < CPUData.verticeCountWidthExtended; x++)
                {
                    int vertexIndex = (z * CPUData.verticeCountWidthExtended) + x;
                    verticesExtendedXZ[vertexIndex] = new Vector2(constOffsetX + iterativeOffsetX * (x - 1), constOffsetZ + iterativeOffsetZ * (z - 1));
                }
            }

            // create and populate buffer
            BufferVerticesExtendedXZ = new ComputeBuffer(CPUData.verticeCountExtended, sizeof(float) * 2);
            BufferVerticesExtendedXZ.SetData(verticesExtendedXZ);
            // set buffer to compute shader
            computeShader.SetBuffer(1, "verticesExtendedXZ", BufferVerticesExtendedXZ);
            computeShader.SetBuffer(2, "verticesExtendedXZ", BufferVerticesExtendedXZ);
            computeShader.SetBuffer(3, "verticesExtendedXZ", BufferVerticesExtendedXZ);

            // create and populate buffer
            BufferVerticeBiome = new ComputeBuffer(CPUData.verticeCountExtended, sizeof(int), ComputeBufferType.Raw);
            // set buffer to compute shader
            computeShader.SetBuffer(1, "verticeBiome", BufferVerticeBiome);
            computeShader.SetBuffer(2, "verticeBiome", BufferVerticeBiome);

            // create and populate buffer
            BufferVerticesExtendedY = new ComputeBuffer(CPUData.verticeCountExtended, sizeof(float), ComputeBufferType.Raw);
            // set buffer to compute shader
            computeShader.SetBuffer(2, "verticesExtendedY", BufferVerticesExtendedY);
            computeShader.SetBuffer(3, "verticesExtendedY", BufferVerticesExtendedY);
            computeShader.SetBuffer(5, "verticesExtendedY", BufferVerticesExtendedY);

            // min max y values of mesh
            BufferVerticesMinYValues = new ComputeBuffer(CPUData.verticeCountHeight, sizeof(float), ComputeBufferType.Raw);
            BufferVerticesMaxYValues = new ComputeBuffer(CPUData.verticeCountHeight, sizeof(float), ComputeBufferType.Raw);
            computeShader.SetBuffer(2, "minYValues", BufferVerticesMinYValues);
            computeShader.SetBuffer(2, "maxYValues", BufferVerticesMaxYValues);

            // get all vertice offset fnl_states
            List<NoiseSettings.fnl_state> FNLStateVerticeOffsetList = worldGenerationSettings.GetVerticeOffsetFNLStates();
            List<NoiseSettings.fnl_state> FNLWarpStateVerticeOffsetList = worldGenerationSettings.GetVerticeOffsetFNLWarpStates();

            // create new compute buffers
            FNLStateVerticeOffsetBuffer = new ComputeBuffer(FNLStateVerticeOffsetList.Count, NoiseSettings.fnl_state.size);
            FNLWarpStateVerticeOffsetBuffer = new ComputeBuffer(FNLWarpStateVerticeOffsetList.Count, NoiseSettings.fnl_state.size);

            // allocate data for new compute buffers
            FNLStateVerticeOffsetBuffer.SetData(FNLStateVerticeOffsetList);
            FNLWarpStateVerticeOffsetBuffer.SetData(FNLWarpStateVerticeOffsetList);

            // set those buffers to the kernels on our compute shader that require them
            computeShader.SetBuffer(2, "FNLStateVerticeOffsetBuffer", FNLStateVerticeOffsetBuffer);
            computeShader.SetBuffer(2, "FNLWarpStateVerticeOffsetBuffer", FNLWarpStateVerticeOffsetBuffer);
            computeShader.SetInt("verticeOffsetBufferLength", FNLStateVerticeOffsetList.Count);
            computeShader.SetInt("verticeCountHeightExtended", CPUData.verticeCountHeightExtended);

            // set static properties of compute shader
            computeShader.SetInt("quadCountWidthExtended", CPUData.quadCountWidthExtended);

            // create and populate buffer that only is on gpu side for triangle normals
            TriangleExtendedNormalsBuffer = new ComputeBuffer(CPUData.triangleCountExtended, sizeof(float) * 3);
            // set buffer to compute shader
            computeShader.SetBuffer(3, "triangleExtendedNormals", TriangleExtendedNormalsBuffer);
            computeShader.SetBuffer(4, "triangleExtendedNormals", TriangleExtendedNormalsBuffer);

            // set static properties of compute shader
            computeShader.SetInt("verticeCountWidth", CPUData.verticeCountWidth);
            // create and populate buffer that only is on gpu side for triangle normals
            VerticesNormalsBuffer = new ComputeBuffer(CPUData.verticeCount, sizeof(float) * 3);
            // set buffer to compute shader
            computeShader.SetBuffer(4, "verticesNormals", VerticesNormalsBuffer);

            // create and populate buffer that only is on gpu side for triangle normals
            TriangleMidpointYBuffer = new ComputeBuffer(CPUData.triangleCount, sizeof(float), ComputeBufferType.Raw);
            // set buffer to compute shader
            computeShader.SetBuffer(5, "triangleMidpointY", TriangleMidpointYBuffer);
        }

        /// <summary>
        /// Disposes of allocated memory on the gpu side
        /// </summary>
        public static void Destroy()
        {
            if (BufferVerticesExtendedXZ != null)
            {
                BufferVerticesExtendedXZ.Dispose();
                BufferVerticesExtendedXZ = null;
            }
            if (BufferVerticesExtendedY != null)
            {
                BufferVerticesExtendedY.Dispose();
                BufferVerticesExtendedY = null;
            }
            if (FNLStateBiomeBuffer != null)
            {
                FNLStateBiomeBuffer.Dispose();
                FNLStateBiomeBuffer = null;
            }
            if (FNLWarpStateBiomeBuffer != null)
            {
                FNLWarpStateBiomeBuffer.Dispose();
                FNLWarpStateBiomeBuffer = null;
            }
            if (BufferMidPoint != null)
            {
                BufferMidPoint.Dispose();
                BufferMidPoint = null;
            }
            if (BufferTriangleBiome != null)
            {
                BufferTriangleBiome.Dispose();
                BufferTriangleBiome = null;
            }
            if (BufferVerticeBiome != null)
            {
                BufferVerticeBiome.Dispose();
                BufferVerticeBiome = null;
            }
            if (FNLStateVerticeOffsetBuffer != null)
            {
                FNLStateVerticeOffsetBuffer.Dispose();
                FNLStateVerticeOffsetBuffer = null;
            }
            if (FNLWarpStateVerticeOffsetBuffer != null)
            {
                FNLWarpStateVerticeOffsetBuffer.Dispose();
                FNLWarpStateVerticeOffsetBuffer = null;
            }
            if (TriangleExtendedNormalsBuffer != null)
            {
                TriangleExtendedNormalsBuffer.Dispose();
                TriangleExtendedNormalsBuffer = null;
            }
            if (VerticesNormalsBuffer != null)
            {
                VerticesNormalsBuffer.Dispose();
                VerticesNormalsBuffer = null;
            }
            if (BufferVerticesMinYValues != null)
            {
                BufferVerticesMinYValues.Dispose();
                BufferVerticesMinYValues = null;
            }
            if (BufferVerticesMaxYValues != null)
            {
                BufferVerticesMaxYValues.Dispose();
                BufferVerticesMaxYValues = null;
            }
            if (TriangleMidpointYBuffer != null)
            {
                TriangleMidpointYBuffer.Dispose();
                TriangleMidpointYBuffer = null;
            }
            
            /*if (ReadBackBuffer.IsCreated)
            {
                ReadBackBuffer.Dispose();
            }*/
        }
    }

    /// <summary>
    /// Mesh data specific for the cpu (actual mesh to be rendered)
    /// </summary>
    public static class CPUData
    {
        public static int verticeCountWidth;
        public static int verticeCountHeight;
        public static int verticeCount;

        public static int verticeCountWidthExtended;
        public static int verticeCountHeightExtended;
        public static int verticeCountExtended;

        public static int quadCountWidth;
        public static int quadCountHeight;
        public static int quadCount;

        public static int quadCountWidthExtended;
        public static int quadCountHeightExtended;
        public static int quadCountExtended;

        public static int triangleCount;
        public static int triangleCountExtended;

        /// <summary>
        /// Is extended because we have no buffer on gpu that is not extended
        /// </summary>
        public static float[] verticeYArrayExtended;
        public static Vector3[] verticeArray;

        public static int[] triangleArray;

        public static Vector3[] verticeNormalArray;

        public static float[] minYArray;
        public static float[] maxYArray;

        public static void Update(WorldGenerationSettings worldGenerationSettings)
        {
            // get the right meshurments of both possible meshes (one that extends 2 in all directions)
            verticeCountWidth = worldGenerationSettings.chunk.quadAmount.x + 1;
            verticeCountHeight = worldGenerationSettings.chunk.quadAmount.y + 1;
            verticeCount = verticeCountWidth * verticeCountHeight;

            verticeCountWidthExtended = worldGenerationSettings.chunk.quadAmount.x + 1 + 2;
            verticeCountHeightExtended = worldGenerationSettings.chunk.quadAmount.y + 1 + 2;
            verticeCountExtended = verticeCountWidthExtended * verticeCountHeightExtended;

            quadCountWidth = worldGenerationSettings.chunk.quadAmount.x;
            quadCountHeight = worldGenerationSettings.chunk.quadAmount.y;
            quadCount = quadCountWidth * quadCountHeight;

            quadCountWidthExtended = worldGenerationSettings.chunk.quadAmount.x + 2;
            quadCountHeightExtended = worldGenerationSettings.chunk.quadAmount.y + 2;
            quadCountExtended = quadCountWidthExtended * quadCountHeightExtended;
            
            triangleCount = quadCount * 2;
            triangleCountExtended = quadCountExtended * 2;

            // populate readback "buffers" static arrays that get filled from gpu after dispatch
            verticeYArrayExtended = new float[verticeCountExtended];
            verticeArray = new Vector3[verticeCount];

            triangleArray = new int[triangleCount * 3];

            verticeNormalArray = new Vector3[verticeCount];

            minYArray = new float[verticeCountHeight];
            maxYArray = new float[verticeCountHeight];

            // fill the arrays that needs to be filled with constants once
            // vertice xz values
            float constOffsetX = -ChunkSettings.meshSize.x * 0.5f;
            float constOffsetZ = -ChunkSettings.meshSize.y * 0.5f;

            float iterativeOffsetX = ChunkSettings.meshSize.x / quadCountWidth;
            float iterativeOffsetZ = ChunkSettings.meshSize.y / quadCountHeight;

            for (int z = 0; z < verticeCountHeight; z++)
            {
                for (int x = 0; x < verticeCountWidth; x++)
                {
                    int vertexIndex = (z * verticeCountWidth) + x;
                    verticeArray[vertexIndex] = new Vector3(constOffsetX + iterativeOffsetX * x, 0f, constOffsetZ + iterativeOffsetZ * z);
                }
            }

            // triangles (vertice indices) itterate every quad and populate the triangles with correct vertice indecees for that quad
            for (int z = 0; z < quadCountHeight; z++)
            {
                for (int x = 0; x < quadCountWidth; x++)
                {
                    // get all the correct indicees
                    int quadIndex = (z * quadCountWidth) + x;
                    int quadTriangleIndex = quadIndex * 2;
                    int quadTriangleVertIndex = quadTriangleIndex * 3;

                    // get the indicees for verticees row and col
                    int verticeIndexBot = z * verticeCountWidth;
                    int verticeIndexTop = (z + 1) * verticeCountWidth;
                    int verticeIndexLeft = x;
                    int verticeIndexRight = x + 1;

                    // using indicees for row and col get verticees indicees for all four corners
                    int verticeIndexBL = verticeIndexBot + verticeIndexLeft;
                    int verticeIndexTL = verticeIndexTop + verticeIndexLeft;
                    int verticeIndexBR = verticeIndexBot + verticeIndexRight;
                    int verticeIndexTR = verticeIndexTop + verticeIndexRight;

                    // populate bottom left triangle
                    triangleArray[quadTriangleVertIndex + 0] = verticeIndexBL;
                    triangleArray[quadTriangleVertIndex + 1] = verticeIndexTL;
                    triangleArray[quadTriangleVertIndex + 2] = verticeIndexBR;

                    // popultae topright triangle
                    triangleArray[quadTriangleVertIndex + 3] = verticeIndexTL;
                    triangleArray[quadTriangleVertIndex + 4] = verticeIndexTR;
                    triangleArray[quadTriangleVertIndex + 5] = verticeIndexBR;
                }
            }
        }
    }

    /*
    public int[] trianglesMesh;

    public Vector3[] normals;
    public Vector3[] verticesMesh;
    public Vector2[] verticesMesh2D;
    public float[] verticesMeshY;

    public Triangle[] triangles;

    public void Update(WorldGenerationSettings worldGenerationSettings)
    {
        meshSize = worldGenerationSettings.chunkSettings.meshSize;
        quadAmount = worldGenerationSettings.chunkSettings.quadAmount;

        chunkLoadDistance = worldGenerationSettings.chunkSettings.chunkLoadDistance;
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
        normals = new Vector3[verticeAmount];
        verticesMesh = new Vector3[verticeAmount];
        verticesMesh2D = new Vector2[verticeAmount];
        verticesMeshY = new float[verticeAmount];
        triangles = new Triangle[triangleAmount];

        // populate vertices starting from bottom left with an offset of meshSize * 0.5f
        float constOffsetX = -meshSize.x * 0.5f;
        float constOffsetZ = -meshSize.y * 0.5f;

        float iterativeOffsetX = meshSize.x / quadAmount.x;
        float iterativeOffsetZ = meshSize.y / quadAmount.y;

        for (int z = 0; z < verticeHeight; z++)
        {
            for (int x = 0; x < verticeWidth; x++)
            {
                int vertexIndex = (z * verticeWidth) + x;
                verticesMesh[vertexIndex] = new Vector3(constOffsetX + iterativeOffsetX * x, 0f, constOffsetZ + iterativeOffsetZ * z);
                verticesMesh2D[vertexIndex] = new Vector2(verticesMesh[vertexIndex].x, verticesMesh[vertexIndex].z);
                normals[vertexIndex] = Vector3.up;
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

                // popultae topright triangle
                trianglesMesh[quadTriangleVertIndex + 3] = verticeIndexTL;
                trianglesMesh[quadTriangleVertIndex + 4] = verticeIndexTR;
                trianglesMesh[quadTriangleVertIndex + 5] = verticeIndexBR;

                quadTriangleIndex++;
                triangles[quadTriangleIndex].t0 = verticeIndexTL;
                triangles[quadTriangleIndex].t1 = verticeIndexTR;
                triangles[quadTriangleIndex].t2 = verticeIndexBR;

                triangles[quadTriangleIndex].midPoint = (verticesMesh[verticeIndexTL] + verticesMesh[verticeIndexTR] + verticesMesh[verticeIndexBR]) / 3f;
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

    }
    */
}