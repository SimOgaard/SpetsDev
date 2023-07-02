using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "WorldGeneration/Biome/BiomeSpawn", order = 9)]
[System.Serializable]
public class BiomeSpawnSettings : Settings
{
    /// <summary>
    /// Noise layers representing how the biome spawn position should be warped
    /// </summary>
    [NonReorderable]
    public NoiseSettings[] warp;

    /// <summary>
    /// Min and max value for inscribed angle of this biome.
    /// Is arbritrary in the fact that they in the end allways add to 1.
    /// https://cdn.discordapp.com/attachments/884159095500836914/997561204908228668/Namnlost-1.jpg
    /// </summary>
    //[MinTo(0f, 1000f)]
    public Vector2 inscribedAngle;

    /// <summary>
    /// Min and max width and height for biome.
    /// Is in procent of final resolution.
    /// https://cdn.discordapp.com/attachments/884159095500836914/997585904854106162/Namnlost-2.jpg
    /// </summary>
    //[MinTo(0f, 1f)]
    public Vector2 area;

    /// <summary>
    /// How much this biome should be rounded at corners.
    /// 1 is circle 0 is cube.
    /// </summary>
    //[MinTo(0f, 1f)]
    public Vector2 rounding;

    /// <summary>
    /// How much this biome should be blended between neighboring regions.
    /// Is in pixels compared to area.
    /// </summary>
    //[MinTo(0f, 1f)]
    public Vector2 blendingRegion;
    /// <summary>
    /// How much this biome should be blended between neighboring layers.
    /// Is in pixels compared to area.
    /// </summary>
    //[MinTo(0f, 1f)]
    public Vector2 blendingLayer;

    /// <summary>
    /// The actual resulting values of spawn settings. Is set once at world generation. So they keep their values during runtime.
    /// </summary>
    public SpawnValues spawnValues = new SpawnValues();

    /// <summary>
    /// A struct that holds spawn values for a biome
    /// </summary>
    public struct SpawnValues
    {
        public float inscribedAngle;

        public float inscribedAngleMin;
        public float inscribedAngleMax;

        public float inscribedAngleRandom;

        public float areaWidth;
        public float areaHeight;

        public float rounding;

        public float blendingRegion;
        public float blendingLayer;

        public int indexLeft;
        public int indexRight;

        // indices that are warp/noise states for biome warp
        public int warpFrom;
        public int warpTo;

        public static float ConstrainAngle(float angle)
        {
            angle = (angle) % (Mathf.PI * 2f);
            if (angle <= 0)
                angle += Mathf.PI * 2f;
            return angle;
        }

        public static int size
        {
            get 
            {
                return
                    sizeof(float) * 9 + // float values
                    sizeof(int) * 4;    // integers
            }
        }

        /// <summary>
        /// Converts this spawnvalues to no longer be fractional
        /// </summary>
        public void FromFractionToPixel(Vector2Int resolution)
        {
            FromFractionToPixel((Vector2)resolution);
        }

        /// <summary>
        /// Converts this spawnvalues to no longer be 
        /// </summary>
        public void FromFractionToPixel(Vector2 resolution)
        {
            // area should be from 0 (edge of screen) to textureResolution * 0.5f (middle of screen)
            this.areaWidth *= resolution.x * 0.5f;
            this.areaHeight *= resolution.y * 0.5f;

            // rounding should at maximum be half of the smallest value of resolution
            this.rounding *= Mathf.Min(resolution.x, resolution.y) * 0.5f;

            // calculate the maximum (half) base of biome triangle
            // b = 0.5*2*tan(v * 0.5)*h, where h is 
            //this.blendingRegion *= Mathf.Tan((inscribedAngleMin - inscribedAngleMax) * 0.5f) * Mathf.Max(resolution.x, resolution.y);
            this.blendingRegion *= Mathf.Max(resolution.x, resolution.y);
            //Debug.Log(blendingRegion);
        }
    }

    /// <summary>
    /// The sum of all biomes inscribed angle always sums to specified degrees from layer.
    /// </summary>
    /// <param name="inscribedAngleSum">
    /// The sum of all biomes inscribed angles in this biomes layer.
    /// </param>
    /// <param name="inscribedAngleLayer">
    /// The inscribed angle of this layer.
    /// </param>
    public void NormalizeInscribedAngle(float inscribedAngleSum, float inscribedAngleRegion)
    {
        // normalize this biomes inscribedAngle
        spawnValues.inscribedAngle = (spawnValues.inscribedAngle / inscribedAngleSum) * inscribedAngleRegion * 360f * Mathf.Deg2Rad;
    }

    /// <summary>
    /// Updates warp map and sets spawn values to constant randomized value
    /// </summary>
    [ContextMenu("Update", false, -1000)]
    public override void Update()
    {
        for (int i = 0; i < warp.Length; i++)
        {
            warp[i].Update();
        }

        spawnValues.inscribedAngle = Random.Range(inscribedAngle.x, inscribedAngle.y);

        spawnValues.areaWidth = Random.Range(area.x, area.y);
        spawnValues.areaHeight = Random.Range(area.x, area.y);

        spawnValues.rounding = Random.Range(rounding.x, rounding.y);

        spawnValues.blendingRegion = Random.Range(blendingRegion.x, blendingRegion.y);
        spawnValues.blendingLayer = Random.Range(blendingLayer.x, blendingLayer.y);
    }
}
