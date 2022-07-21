using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class WorldMapGenerationManager : MonoBehaviour
{
    [Header("Settings")]
    public WorldGenerationSettings worldGenerationSettings;

    [Header("Compute Shaders")]
    public ComputeShader computeShader;

    public ComputeBuffer biomeSpawnValuesBuffer;
    /*
    public ComputeBuffer angleBuffer;
    public ComputeBuffer biomeIndexBuffer;
    public ComputeBuffer blendFactorsBuffer;

    public ComputeBuffer widthBuffer;
    public ComputeBuffer heightBuffer;
    public ComputeBuffer roundingBuffer;
    */

    [Header("Renders")]
    public RenderTexture biomeBinary;
    public WorldMapRenderer biomeBinaryRenderer;

    public RenderTexture biomeBinaryBlend;
    public WorldMapRenderer biomeBinaryBlendRenderer;

    public RenderTexture map;
    public WorldMapRenderer mapRenderer;

    public RenderTexture mapBlend;
    public WorldMapRenderer mapBlendRenderer;
    
    /*
        public RenderTexture edgeBiomeBinary;
        public WorldMapRenderer edgeBiomeBinaryRenderer;

        public RenderTexture edgeBiomeBlend;
        public WorldMapRenderer edgeBiomeBlendRenderer;
    */
    /// TODO: WE WANT A REGION AND BIOME TEXTURE SEPERATLY
    /// THIS IS SO THAT WE CAN LATER DRAW THE MAP INTO DIFFERENT DIVIDED REGIONS

    /*
    /// <summary>
    /// Resets the textures
    /// </summary>
    private void Reset()
    {
        computeShader.Dispatch(0, 280 / 10, 280 / 10, 1);
    }

    private void StandardNormalize(ref List<float> listToNormalize, float returningSum)
    {
        float sum = listToNormalize.Sum();
        listToNormalize = listToNormalize.Select(x => (x * returningSum) / sum).ToList();
    }

    /// <summary>
    /// Writes to binary biome a circular of biomes
    /// </summary>
    private void CircularBiome(int start, int amount, float blend)
    {
        // now we want to create this: https://cdn.discordapp.com/attachments/884159095500836914/997561204908228668/Namnlost-1.jpg 
        // so first take a random angle between 0 and 360
        float angleInit = Mathf.Deg2Rad * Random.Range(0f, 360f);

        // then ask: how many circular sectors do we want?
        computeShader.SetInt("bufferLengths", amount);
        computeShader.SetInt("bufferStart", start);
        // and create the neccesary arrays for keeping the resulting random angles that will be passed to the gpu, and random floats between selected arbitrary numbers
        float[] randomAngles = new float[amount]; // actual angles that will be passed to gpu
        float[] randomValues = new float[amount]; // arbitrary numbers
        float sum = 0f;
        // for each circular sector
        for (int i = 0; i < amount; i++)
        {
            // get a random arbitrary number
            randomValues[i] = Random.Range(.2f, 1f);
            // and add to sum to later calculate the standard normalization
            sum += randomValues[i];
        }
        // multiply sum by a value lesser than 1 so that the sum of randomAngles are larger than 360deg
        sum *= 0.9999f;
        // for each circular sector
        for (int i = 0; i < amount; i++)
        {
            // calculate the "softmax" (in reality it isnt the softmax because we just divide by the sum but whatever, its an standard normalization anyways)
            float normalization = randomValues[i] / sum;
            // then convert to angle and save to array
            randomAngles[i] = normalization * (360f * Mathf.Deg2Rad);
        }

        // pass newly calculated angleInit and randomAngles to compute shader to draw our circular sectors to the created image
        computeShader.SetFloat("angleInit", angleInit);

        angleBuffer = new ComputeBuffer(
            amount,
            sizeof(float)
        );
        angleBuffer.SetData(randomAngles);
        computeShader.SetBuffer(1, "angleBuffer", angleBuffer);

        // next pass the biome indices to the compute shader
        // create the indices array
        int[] biomeIndices = new int[amount];

        // for each circular sector
        for (int i = 0; i < amount; i++)
        {
            // set the indice
            biomeIndices[i] = start + i;
            Debug.Log(start + i);
        }
        biomeIndexBuffer = new ComputeBuffer(
            amount,
            sizeof(int)
        );
        biomeIndexBuffer.SetData(biomeIndices);
        computeShader.SetBuffer(1, "biomeIndexBuffer", biomeIndexBuffer);

        // next calculate the blend factor for each biome
        float[] blendFactors = new float[amount];
        // for each circular sector
        for (int i = 0; i < amount; i++)
        {
            // get a random number for how many pixels we should blend this biome with
            blendFactors[i] = Random.Range(1f, 1.25f) * blend;
        }

        // pass newly calculated blendFactors to compute shader to blend our circular sectors
        blendFactorsBuffer = new ComputeBuffer(
            amount,
            sizeof(float)
        );
        blendFactorsBuffer.SetData(blendFactors);
        computeShader.SetBuffer(1, "blendFactorsBuffer", blendFactorsBuffer);

        // this is everything neccesarry for the second kernel so dispatch the compute shader
        computeShader.Dispatch(1, 280 / 10, 280 / 10, 1);
    }

    /// <summary>
    /// 
    /// </summary>
    private void RingBiome(float radius, float rounding, int amount)
    {
        // now we want to create this: https://cdn.discordapp.com/attachments/884159095500836914/997585904854106162/Namnlost-2.jpg
        // to do so calculate random widths and heights that the edge biomes will be constrained too
        // by first knowing how many edge biomes there are (from parameter)
        // and then create the neccesary arrays for keeping the resulting random widhts and heights and roundings that will be passed to the gpu
        float[] randomWidths = new float[amount];
        float[] randomHeights = new float[amount];
        float[] randomRounding = new float[amount];

        // for each edge biome
        for (int i = 0; i < amount; i++)
        {
            // get a random number specifying the pixel width
            randomWidths[i] = Random.Range(1f, 1.2f) * radius;
            // get a random number specifying the pixel height
            randomHeights[i] = Random.Range(1f, 1.2f) * radius;
            // get a random number specifying the rounding of square
            randomRounding[i] = Random.Range(1f, 1.2f) * rounding;
        }

        // pass buffers to compute shader
        widthBuffer = new ComputeBuffer(
            amount,
            sizeof(float)
        );
        widthBuffer.SetData(randomWidths);
        computeShader.SetBuffer(2, "widthBuffer", widthBuffer);

        heightBuffer = new ComputeBuffer(
            amount,
            sizeof(float)
        );
        heightBuffer.SetData(randomHeights);
        computeShader.SetBuffer(2, "heightBuffer", heightBuffer);

        roundingBuffer = new ComputeBuffer(
            amount,
            sizeof(float)
        );
        roundingBuffer.SetData(randomRounding);
        computeShader.SetBuffer(2, "roundingBuffer", roundingBuffer);

        computeShader.SetBuffer(2, "biomeIndexBuffer", biomeIndexBuffer);

        // now the compute buffer has everything to start the third kernel
        computeShader.Dispatch(2, 280 / 10, 280 / 10, 1);
    }

    private void MakeRenderable(int biomeCount)
    {
        // all values neccesary for this kernel
        computeShader.SetFloat("biomeCount", biomeCount);
        computeShader.SetTexture(3, "biomeBinary", biomeBinary);
        computeShader.SetTexture(3, "biomeBlend", biomeBlend);

        // now the compute buffer has everything to start the fourth kernel
        computeShader.Dispatch(3, 280 / 10, 280 / 10, 1);

        // and lastly render the textures to display
        biomeBinaryRenderer.renderTexture = biomeBinary;
        biomeBlendRenderer.renderTexture = biomeBlend;
    }


    private struct BiomeSpawnValues
    {
        public float arcValue;
        public float widthValue;
        public float heightValue;
        public float roundingValue;
        public float blendingValue;
        public int biomeIndex;
        public Color biomeColor;

        public BiomeSpawnValues(float arcValue, float widthValue, float heightValue, float roundingValue, float blendingValue, int biomeIndex, Color biomeColor)
        {
            this.arcValue = arcValue;
            this.widthValue = widthValue;
            this.heightValue = heightValue;
            this.roundingValue = roundingValue;
            this.blendingValue = blendingValue;
            this.biomeIndex = biomeIndex;
            this.biomeColor = biomeColor;
        }
    }

    private void StandardNormalize(ref List<BiomeSpawnValues> biomeSpawnValuesToNormalize, float returningSum)
    {
        float sum = biomeSpawnValuesToNormalize.Sum(biome => biome.arcValue);
        BiomeSpawnValues[] biomeSpawnValues = new BiomeSpawnValues[biomeSpawnValuesToNormalize.Count];
        for (int i = 0; i < biomeSpawnValuesToNormalize.Count; i++)
        {
            biomeSpawnValues[i] = biomeSpawnValuesToNormalize[i];
            biomeSpawnValues[i].arcValue *= (returningSum/sum);
        }
        biomeSpawnValuesToNormalize = biomeSpawnValues.ToList();
    }
    */

    private void Render()
    {
        biomeBinaryRenderer.renderTexture = biomeBinary;
        biomeBinaryBlendRenderer.renderTexture = biomeBinaryBlend;

        mapRenderer.renderTexture = map;
        mapBlendRenderer.renderTexture = mapBlend;
    }

    private void Start()
    {
        // update settings
        worldGenerationSettings.Update();

        // get map resolution
        Vector2Int quadAmount = Vector2Int.Scale(worldGenerationSettings.chunk._worldSize, worldGenerationSettings.chunk.quadAmount);
        Vector2Int textureResolution = quadAmount + Vector2Int.one;

        // set it
        computeShader.SetInts("imageResolutions", textureResolution.x, textureResolution.y);

        // set screen resolution
        Screen.SetResolution(textureResolution.x, textureResolution.y, false);
        Debug.Log($"Map resolution is: {textureResolution}");

        // create texture from resolution
        biomeBinary = new RenderTexture(textureResolution.x, textureResolution.y, 0, RenderTextureFormat.RFloat, 1);
        biomeBinary.enableRandomWrite = true;
        biomeBinary.Create();

        biomeBinaryBlend = new RenderTexture(textureResolution.x, textureResolution.y, 0, RenderTextureFormat.RFloat, 1);
        biomeBinaryBlend.enableRandomWrite = true;
        biomeBinaryBlend.Create();

        map = new RenderTexture(textureResolution.x, textureResolution.y, 0, RenderTextureFormat.ARGB32, 1);
        map.enableRandomWrite = true;
        map.Create();

        mapBlend = new RenderTexture(textureResolution.x, textureResolution.y, 0, RenderTextureFormat.ARGB32, 1);
        mapBlend.enableRandomWrite = true;
        mapBlend.Create();

        // pass the textures to the compute shader
        computeShader.SetTexture(0, "biomeBinary", biomeBinary);
        computeShader.SetTexture(0, "biomeBinaryBlend", biomeBinaryBlend);
        computeShader.SetTexture(0, "map", map);
        computeShader.SetTexture(0, "mapBlend", mapBlend);

        // create a random object that is used to randomly itterate array
        System.Random random = new System.Random();

        // create a list of spawn values, will hold all biomes values in correct order and rotation
        List<BiomeSpawnSettings.SpawnValues> spawnValues = new List<BiomeSpawnSettings.SpawnValues>();

        // itterate rings in reverse
        foreach (RingSettings ring in worldGenerationSettings.rings.Reverse())
        {
            // the random rotation we should rotate this ring by
            float randomRotation = Random.Range(0f, Mathf.PI * 2f);

            // get this regions start rotation
            float regionRotation = 0f;

            // itterate regions randomly
            foreach (RegionSettings region in ring.regions.OrderBy(x => random.Next()))
            {
                // get the comming biomes start rotations
                float biomeRotation = 0f;

                // itterate the regions layers in reverse
                foreach (LayerSettings layer in region.layers.Reverse())
                {
                    // reset the biomes start rotations
                    biomeRotation = 0f;

                    // itterate biomes randomly
                    foreach (BiomeSettings biome in layer.biomes.OrderBy(x => random.Next()))
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
                        biome.spawn.spawnValues.FromFractionToPixel(textureResolution);

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

        // create an array of spawnValues, because we need to change it during loop
        BiomeSpawnSettings.SpawnValues[] spawnValuesArray = spawnValues.ToArray();

        // now for each spawn value in spawnValuesArray
        for (int i = 0; i < spawnValuesArray.Length; i++)
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

            // then set the spawn value's spawnValuesIndexLeft and spawnValuesIndexRight to the index of nearMinMaxBiome in spawnValues
            spawnValuesArray[i].spawnValuesIndexLeft = spawnValues.IndexOf(nearMinBiome);
            spawnValuesArray[i].spawnValuesIndexRight = spawnValues.IndexOf(nearMaxBiome);
        }

        // pass theese values to compute shader
        biomeSpawnValuesBuffer = new ComputeBuffer(
                spawnValuesArray.Length,
                BiomeSpawnSettings.SpawnValues.SizeOf()
            );
        biomeSpawnValuesBuffer.SetData(spawnValuesArray);
        computeShader.SetBuffer(0, "biomeSpawnValuesBuffer", biomeSpawnValuesBuffer);
        computeShader.SetInt("bufferLengths", spawnValuesArray.Length);

        // start compute shader
        computeShader.Dispatch(0, 6400 / 10, 5400 / 10, 1);
        
        // and render
        Render();
        return;
    }

    private void Update()
    {
        Start();
    }
}
