using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// Generates the world
/// </summary>
public static class WorldGenerator
{
    #region functions
    /// <summary>
    /// Converts RenderTexture to Texture2D
    /// </summary>
    public static Texture2D ToTexture2D(this RenderTexture rTex)
    {
        Texture2D tex = new Texture2D(rTex.width, rTex.height, TextureFormat.RGB24, false);
        var old_rt = RenderTexture.active;
        RenderTexture.active = rTex;

        tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
        tex.Apply();

        RenderTexture.active = old_rt;
        return tex;
    }

    /// <summary>
    /// Creates a rendertexture of worldResolution size with correct 
    /// </summary>
    public static RenderTexture CreateRenderTexture(RenderTextureFormat format, int width, int height)
    {
        RenderTexture rt = new RenderTexture(
            width,
            height,
            0,
            format,
            1
        );
        rt.enableRandomWrite = true;
        rt.Create();
        return rt;
    }

    /// <summary>
    /// Disposes of rendertexture
    /// </summary>
    public static void DisposeOf(ref RenderTexture rt)
    {
        if (rt != null)
        {
            rt.Release();
            rt = null;
        }
    }

    /// <summary>
    /// renderTexture?.DisposeOf();
    /// </summary>
    public static void DisposeOf(this RenderTexture rt)
    {
        rt.Release();
        rt = null;
    }

    /// <summary>
    /// Disposes of rendertexture
    /// </summary>
    public static void DisposeOf(ref ComputeBuffer buffer)
    {
        if (buffer != null)
        {
            buffer.Release();
            buffer = null;
        }
    }

    /// <summary>
    /// computeBuffer?.DisposeOf();
    /// </summary>
    public static void DisposeOf(this ComputeBuffer buffer)
    {
        buffer.Release();
        buffer = null;
    }

    /// <summary>
    /// Passes all textures to specified kernel of computeShader
    /// </summary>
    public static void PassTextures(ref ComputeShader computeShader, int kernel, params KeyValuePair<string, RenderTexture>[] textures)
    {
        for (int i = 0; i < textures.Length; i++)
        {
            computeShader.SetTexture(kernel, textures[i].Key, textures[i].Value);
        }
    }

    /// <summary>
    /// Passes all textures to specified kernel of computeShader
    /// </summary>
    public static void PassTextures(this ComputeShader computeShader, int kernel, params KeyValuePair<string, RenderTexture>[] textures)
    {
        for (int i = 0; i < textures.Length; i++)
        {
            computeShader.SetTexture(kernel, textures[i].Key, textures[i].Value);
        }
    }

    /// <summary>
    /// Passes all buffers to specified kernel of computeShader
    /// </summary>
    public static void PassBuffers(ref ComputeShader computeShader, int kernel, params KeyValuePair<string, ComputeBuffer>[] buffers)
    {
        for (int i = 0; i < buffers.Length; i++)
        {
            computeShader.SetBuffer(kernel, buffers[i].Key, buffers[i].Value);
        }
    }

    /// <summary>
    /// Passes all buffers to specified kernel of computeShader
    /// </summary>
    public static void PassBuffers(this ComputeShader computeShader, int kernel, params KeyValuePair<string, ComputeBuffer>[] buffers)
    {
        for (int i = 0; i < buffers.Length; i++)
        {
            computeShader.SetBuffer(kernel, buffers[i].Key, buffers[i].Value);
        }
    }

    /// <summary>
    /// Passes buffer to specified kernels of computeShader
    /// </summary>
    public static void PassBuffer(ref ComputeShader computeShader, string bufferName, ComputeBuffer buffer, params int[] kernels)
    {
        for (int i = 0; i < kernels.Length; i++)
        {
            computeShader.SetBuffer(kernels[i], bufferName, buffer);
        }
    }

    /// <summary>
    /// Passes buffer to specified kernels of computeShader
    /// </summary>
    public static void PassBuffer(this ComputeShader computeShader, string bufferName, ComputeBuffer buffer, params int[] kernels)
    {
        for (int i = 0; i < kernels.Length; i++)
        {
            computeShader.SetBuffer(kernels[i], bufferName, buffer);
        }
    }

    /// <summary>
    /// Populates computebuffer with data
    /// </summary>
    public static void PopulateComputeBuffer(int size, out ComputeBuffer buffer, System.Array data)
    {
        // create the buffer with count that is never zero
        buffer = new ComputeBuffer(
            data.Length > 0 ? data.Length : 1,
            size
        );
        // if it is not length zero
        if (data.Length > 0)
        {
            // populate it
            buffer.SetData(data);
        }
    }

    /// <summary>
    /// Populates computebuffer with data
    /// </summary>
    public static void PopulateComputeBuffer<T>(int size, out ComputeBuffer buffer, List<T> data)
    {
        PopulateComputeBuffer(size, out buffer, data.ToArray());
    }
    #endregion

    #region values
    // Mesh
    public static int verticeCountWidth;
    public static int verticeCountHeight;
    public static int verticeCount;

    public static int verticeCountWidthExtended;
    public static int verticeCountHeightExtended;
    public static int verticeCountExtended;

    public static int quadCountWidth;
    public static int quadCountHeight;
    public static int quadCount;
    public static int triangleCount;

    public static int quadCountWidthExtended;
    public static int quadCountHeightExtended;
    public static int quadCountExtended;
    public static int triangleCountExtended;

    // World
    public static Vector2Int worldVerticeResolution;
    public static Vector2Int worldTriangleResolution;
    #endregion

    #region compute shader
    public static ComputeShader computeShader;
    #endregion

    #region buffers
    // constant buffers
    /// <summary>
    /// Data for what where and how all the biomes should spawn
    /// </summary>
    public static ComputeBuffer biomeBuffer;

    public static ComputeBuffer vertExtendedBuffer2D;
    public static Vector2[] vertExtendedArray2D;
    public static ComputeBuffer triMidpointBuffer2D;
    public static Vector2[] triMidpointArray2D;

    // readwrite buffers
    /// <summary>
    /// Holds integer values for all vertices biome indices in a chunk + 2x2 for later triNormal calculation
    /// </summary>
    public static ComputeBuffer vertBiomeBuffer;

    /// <summary>
    /// Holds integer values for all triangles biome indices in a chunk
    /// </summary>
    public static ComputeBuffer triBiomeBuffer;
    /// <summary>
    /// Readback array that gets populated by triBiome
    /// </summary>
    public static int[] triBiomeArray;

    /// <summary>
    /// Holds float y values for all vertices in a chunk + 2x2 for later triNormal calculation
    /// </summary>
    public static ComputeBuffer vertElevationBuffer;
    /// <summary>
    /// Readback array that gets populated by vertElevation
    /// </summary>
    public static float[] vertElevationArray;

    /// <summary>
    /// Holds float3 values for all triangle normals in a chunk + 4x4 for later vertNormal calculation
    /// </summary>
    public static ComputeBuffer triNormalBuffer;

    /// <summary>
    /// Holds float3 values for all vertice smooth normals in a chunk
    /// </summary>
    public static ComputeBuffer vertNormalBuffer;
    /// <summary>
    /// Readback array that gets populated by vertNormal
    /// </summary>
    public static Vector3[] vertNormalArray;

    /// <summary>
    /// Holds float y values for all triangle midpoint y in a chunk
    /// </summary>
    public static ComputeBuffer triHeightBuffer;
    /// <summary>
    /// Readback array that gets populated by triHeight 
    /// </summary>
    public static float[] triHeightArray;
    #endregion

    #region cpu_buffer
    /// <summary>
    /// Holds all triangles vertices indices for a chunk mesh
    /// </summary>
    public static int[] triArray;

    /// <summary>
    /// Holds all xyz positions of a chunks local vertices, y values gets populated by vertElevationArray, xz are constants
    /// </summary>
    public static Vector3[] verticeArray;

    /// <summary>
    /// Holds all xyz positions of the middle of a chunks local triangles, y values gets populated by triHeightArray, xz are constants
    /// </summary>
    public static Vector3[] triMidpointArray;
    #endregion

    /// <summary>
    /// Does the prework for creating the mesh/map of a chunk. IE constructs all buffers/arrays and sends all constant data to the gpu.
    /// </summary>
    public static void Update(WorldGenerationSettings worldGenerationSettings)
    {
        // clear all data on gpu
        Destroy();

        // get the right meshurments of a singular chunk mesh
        verticeCountWidth = worldGenerationSettings.chunk.quadAmount.x + 1;
        verticeCountHeight = worldGenerationSettings.chunk.quadAmount.y + 1;
        verticeCount = verticeCountWidth * verticeCountHeight;

        verticeCountWidthExtended = worldGenerationSettings.chunk.quadAmount.x + 1 + 2;
        verticeCountHeightExtended = worldGenerationSettings.chunk.quadAmount.y + 1 + 2;
        verticeCountExtended = verticeCountWidthExtended * verticeCountHeightExtended;

        quadCountWidth = worldGenerationSettings.chunk.quadAmount.x;
        quadCountHeight = worldGenerationSettings.chunk.quadAmount.y;
        quadCount = quadCountWidth * quadCountHeight;
        triangleCount = quadCount * 2;

        quadCountWidthExtended = worldGenerationSettings.chunk.quadAmount.x + 2;
        quadCountHeightExtended = worldGenerationSettings.chunk.quadAmount.y + 2;
        quadCountExtended = quadCountWidthExtended * quadCountHeightExtended;
        triangleCountExtended = quadCountExtended * 2;

        // first we have to do things on the cpu
        // create the readback "buffers" static arrays that get filled from compute shaders
        triBiomeArray = new int[triangleCount];
        vertElevationArray = new float[verticeCountExtended];
        vertNormalArray = new Vector3[verticeCount];
        triHeightArray = new float[triangleCount];

        // create the readback "buffers" static arrays that get partially filled from compute shaders, work needs to be done on the cpu after creating
        verticeArray = new Vector3[verticeCount]; // xz constants
        triArray = new int[triangleCount * 3]; // all constants
        triMidpointArray = new Vector3[triangleCount]; // xz constants

        // and constant buffers, static arrays that gets fully initilized by cpu and passed to compute shader once
        vertExtendedArray2D = new Vector2[verticeCountExtended];
        triMidpointArray2D = new Vector2[triangleCount];

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

        for (int z = -1; z <= verticeCountHeight; z++)
        {
            for (int x = -1; x <= verticeCountWidth; x++)
            {
                int vertexIndex = ((z + 1) * verticeCountWidth) + (x + 1);
                vertExtendedArray2D[vertexIndex] = new Vector2(constOffsetX + iterativeOffsetX * x, constOffsetZ + iterativeOffsetZ * z);
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
                triArray[quadTriangleVertIndex + 0] = verticeIndexBL;
                triArray[quadTriangleVertIndex + 1] = verticeIndexTL;
                triArray[quadTriangleVertIndex + 2] = verticeIndexBR;

                // popultae topright triangle
                triArray[quadTriangleVertIndex + 3] = verticeIndexTL;
                triArray[quadTriangleVertIndex + 4] = verticeIndexTR;
                triArray[quadTriangleVertIndex + 5] = verticeIndexBR;
            }
        }

        // triangle midpoints (xz)
        float baseLength = ChunkSettings.meshSize.x / quadCountWidth;
        float heightLength = ChunkSettings.meshSize.y / quadCountHeight;

        for (int h = 0; h < quadCountHeight; h++)
        {
            float h1 = (1f * heightLength) / 3f + h * heightLength;
            float h2 = (2f * heightLength) / 3f + h * heightLength;

            for (int w = 0; w < quadCountWidth; w++)
            {
                float b1 = (1f * baseLength) / 3f + w * baseLength;
                float b2 = (2f * baseLength) / 3f + w * baseLength;

                int triangleIndex = w * 2 + h * quadCountWidth * 2;

                triMidpointArray[triangleIndex] = new Vector3
                (
                    b1,
                    0f,
                    h1
                );
                triMidpointArray[triangleIndex + 1] = new Vector3
                (
                    b2,
                    0f,
                    h2
                );

                triMidpointArray2D[triangleIndex] = new Vector2
                (
                    b1,
                    h1
                );
                triMidpointArray2D[triangleIndex + 1] = new Vector2
                (
                    b2,
                    h2
                );
            }
        }

        // get final world resolution
        Vector2Int quadAmount = Vector2Int.Scale(worldGenerationSettings.chunk._worldSize, worldGenerationSettings.chunk.quadAmount);
        worldVerticeResolution = quadAmount + Vector2Int.one;
        worldTriangleResolution = Vector2Int.Scale(quadAmount, new Vector2Int(2, 1));

        // now we have to start working on the compute shader, so:
        // get the right compute shader from resources
        computeShader = Resources.Load<ComputeShader>("ComputeShaders/WorldGeneratorComputeShader");

        // fetch kernels indices
        // BiomeVertSelector: selects the biome each vertice is
        int BiomeVertSelector = computeShader.FindKernel("BiomeVertSelector");
        // BiomeTriSelector: selects the biome each triangle is
        int BiomeTriSelector = computeShader.FindKernel("BiomeTriSelector");
        // VertElevation: given vertice biome fetch a height value (only y value)
        int VertElevation = computeShader.FindKernel("VertElevation");
        // TriNormal: given quad calculate both triangle normals of that quad
        int TriNormal = computeShader.FindKernel("TriNormal");
        // VertNormal: given vertice calculate smooth normals from neighbouring triangles
        int VertNormal = computeShader.FindKernel("VertNormal");
        // TriHeight: given quad calculate both triangles midpoints (only y value)
        int TriHeight = computeShader.FindKernel("TriHeight");

        // then create all buffers which are not constant (random read write enabled) will be read back to cpu after dispatch, do not need to be populated
        vertBiomeBuffer = new ComputeBuffer(verticeCountExtended, sizeof(int));
        triBiomeBuffer = new ComputeBuffer(triangleCount, sizeof(int));
        vertElevationBuffer = new ComputeBuffer(verticeCountExtended, sizeof(float));
        triNormalBuffer = new ComputeBuffer(triangleCountExtended, sizeof(float) * 3);
        vertNormalBuffer = new ComputeBuffer(verticeCount, sizeof(float) * 3);
        triHeightBuffer = new ComputeBuffer(triangleCount, sizeof(float));

        // and pass them to the required compute shader kernels
        PassBuffer(ref computeShader, "vertBiomeBuffer", vertBiomeBuffer,
            BiomeVertSelector,
            VertElevation
        );
        PassBuffer(ref computeShader, "triBiomeBuffer", triBiomeBuffer,
            BiomeTriSelector
        );
        PassBuffer(ref computeShader, "vertElevationBuffer", vertElevationBuffer,
            VertElevation,
            TriNormal,
            TriHeight
        );
        PassBuffer(ref computeShader, "triNormalBuffer", triNormalBuffer,
            TriNormal,
            VertNormal
        );
        PassBuffer(ref computeShader, "vertNormalBuffer", vertNormalBuffer,
            VertNormal
        );
        PassBuffer(ref computeShader, "triHeightBuffer", triHeightBuffer,
            TriHeight
        );

        // now its the constant buffers turn :)

        // they need to be populated with cpu data like vertArray2D and triMidpointArray2D that holds xz values for mesh vertices and triangle
        PopulateComputeBuffer(sizeof(float) * 2, out vertExtendedBuffer2D, vertExtendedArray2D);
        PopulateComputeBuffer(sizeof(float) * 2, out triMidpointBuffer2D, triMidpointArray2D);

        // then pass them
        PassBuffer(ref computeShader, "vertExtendedBuffer2D", vertExtendedBuffer2D,
            BiomeVertSelector,
            VertElevation,
            TriNormal,
            VertNormal

        );
        PassBuffer(ref computeShader, "triMidpointBuffer2D", triMidpointBuffer2D,
            BiomeTriSelector,
            TriHeight
        );

        // then lastly we have the how? and where? questions to answer, aka biome spawn settings

        // create a list of spawn values, will hold all biomes values in correct order and rotation
        List<BiomeSpawnSettings.SpawnValues> spawnValues = new List<BiomeSpawnSettings.SpawnValues>();
        // create a list of fnl states, will hold all biomes warp and noise states
        List<NoiseSettings.fnl_state> warpStates = new List<NoiseSettings.fnl_state>();
        
        // itterate rings in reverse
        foreach (RingSettings ring in worldGenerationSettings.rings.Reverse())
        {
            // the random rotation we should rotate this ring by
            float randomRotation = Random.Range(0f, Mathf.PI * 2f);

            // get this regions start rotation
            float regionRotation = 0f;

            // itterate regions randomly
            foreach (RegionSettings region in ring.regions.OrderBy(x => worldGenerationSettings.dotNetRandom.Next()))
            {
                // get the comming biomes start rotations
                float biomeRotation = 0f;

                // itterate the regions layers in reverse
                foreach (LayerSettings layer in region.layers.Reverse())
                {
                    // reset the biomes start rotations
                    biomeRotation = 0f;

                    // itterate biomes randomly
                    foreach (BiomeSettings biome in layer.biomes.OrderBy(x => worldGenerationSettings.dotNetRandom.Next()))
                    {
                        // set min angle to current angle
                        biome.spawn.spawnValues.inscribedAngleMin = regionRotation + biomeRotation;
                        // add this biomes inscribed angle to layer rotation
                        biomeRotation += biome.spawn.spawnValues.inscribedAngle;
                        // and set max angle
                        biome.spawn.spawnValues.inscribedAngleMax = regionRotation + biomeRotation;
                        // and lastly random angle
                        biome.spawn.spawnValues.inscribedAngleRandom = randomRotation;

                        // convert all ranges of arbritrary number to final resolution
                        biome.spawn.spawnValues.FromFractionToPixel(worldVerticeResolution);

                        // set from, to current count of warpStates
                        biome.spawn.spawnValues.warpFrom = warpStates.Count();
                        // add its warp states to warpStates
                        warpStates.AddRange(biome.spawn.warp.ToFnlState(true));
                        // and set to, to new count of warpStates
                        biome.spawn.spawnValues.warpTo = warpStates.Count();

                        // then add it to spawnValues
                        spawnValues.Add(biome.spawn.spawnValues);
                    }
                }

                // add this regions inscribed angle (which is biomeRotation)
                regionRotation += biomeRotation;
            }
        }

        // are the angle delta between the two given angles less than diff?
        bool CompareAangles(float a, float b, float diff)
        {
            float difference = Mathf.Atan2(Mathf.Sin(a - b), Mathf.Cos(a - b));
            return Mathf.Abs(difference) <= diff || Mathf.Approximately(a, b);
        }

        // create an array of spawnValues, because we need to change values during loop
        BiomeSpawnSettings.SpawnValues[] spawnValuesArray = spawnValues.ToArray();
        // remove first biome from spawnValues if we have multiple rings
        if (worldGenerationSettings.rings.Count() > 1)
            spawnValues.RemoveAt(0);
        // then check what start index we are at, if we removed a value we have 1, otherwise 0
        int startIndex = System.Convert.ToInt32(worldGenerationSettings.rings.Count() > 1);

        // now for each spawn value in spawnValuesArray that is not the main biome
        for (int i = startIndex; i < spawnValuesArray.Length; i++)
        {
            // find all biomes with near same inscribedAngleMin of this spawnValue (can only be inscribedAngleMax)
            var nearMin = spawnValues.Where(value => CompareAangles(spawnValuesArray[i].inscribedAngleMin + spawnValuesArray[i].inscribedAngleRandom, value.inscribedAngleMax + value.inscribedAngleRandom, 0.01f)).ToList();

            // next find all biomes with near same inscribedAngleMax of this spawnValue (can only be inscribedAngleMin)
            var nearMax = spawnValues.Where(value => CompareAangles(spawnValuesArray[i].inscribedAngleMax + spawnValuesArray[i].inscribedAngleRandom, value.inscribedAngleMin + value.inscribedAngleRandom, 0.01f)).ToList();

            // remove current value from collections, even though it should not be in any of them, just to be shure
            nearMin.Remove(spawnValuesArray[i]);
            nearMax.Remove(spawnValuesArray[i]);

            // now for both of those collections, find the one with the closest average areaWidth and areaHeight
            float areaAverage = (spawnValuesArray[i].areaWidth + spawnValuesArray[i].areaHeight) * 0.5f;
            BiomeSpawnSettings.SpawnValues nearMinBiome = nearMin.OrderBy(value => Mathf.Abs(areaAverage - ((value.areaHeight + value.areaWidth) * 0.5f))).First();
            BiomeSpawnSettings.SpawnValues nearMaxBiome = nearMax.OrderBy(value => Mathf.Abs(areaAverage - ((value.areaHeight + value.areaWidth) * 0.5f))).First();

            // then set the spawn value's spawnValuesIndexLeft and spawnValuesIndexRight to the index of nearMinMaxBiome in spawnValues, making shure to take the removed biome into account when getting index (+1)
            spawnValuesArray[i].indexLeft = spawnValues.IndexOf(nearMinBiome) + startIndex;
            spawnValuesArray[i].indexRight = spawnValues.IndexOf(nearMaxBiome) + startIndex;
        }

        // get warp in fnl_state structs
        List<NoiseSettings.fnl_state> globalWarp = worldGenerationSettings.globalWarp.ToFnlStates(true);

        // now we have allllll the data on the cpu, so pass it to the gpu :), by first populating
        PopulateComputeBuffer(NoiseSettings.fnl_state.size, out warpStatesBuffer, warpStates);
        PopulateComputeBuffer(NoiseSettings.fnl_state.size, out globalWarpStatesBuffer, globalWarp);
        PopulateComputeBuffer(BiomeSpawnSettings.SpawnValues.size, out biomeSpawnValuesBuffer, spawnValuesArray);

        // then passing
        PassBuffer(ref mapComputeShader, 0,
            new KeyValuePair<string, ComputeBuffer>("globalWarpStatesBuffer", globalWarpStatesBuffer),
            new KeyValuePair<string, ComputeBuffer>("biomeSpawnValuesBuffer", biomeSpawnValuesBuffer),
            new KeyValuePair<string, ComputeBuffer>("warpStatesBuffer", warpStatesBuffer),
            new KeyValuePair<string, ComputeBuffer>("noiseStatesBuffer", noiseStatesBuffer)
        );
    }

    /// <summary>
    /// Clears gpu buffers/textures
    /// </summary>
    public static void Destroy()
    {
        vertBiomeBuffer?.DisposeOf();
        triBiomeBuffer?.DisposeOf();
        vertElevationBuffer?.DisposeOf();
        triNormalBuffer?.DisposeOf();
        vertNormalBuffer?.DisposeOf();
        triHeightBuffer?.DisposeOf();

        vertExtendedBuffer2D?.DisposeOf();
        triMidpointBuffer2D?.DisposeOf();
    }

    /// <summary>
    /// Creates everything for a singluar chunk
    /// </summary>
    public static void Generate(Chunk chunk)
    {
        // create the map

        // we now know the biome and position for each vertex, but we choose to create the meshes on runtime        
    }
}