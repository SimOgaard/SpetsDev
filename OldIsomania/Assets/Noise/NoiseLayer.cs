using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Noise
{
    /// <summary>
    /// 
    /// </summary>
    public class NoiseLayer : MonoBehaviour
    {
        public bool Tilable = true;

        public int Width = 256;
        public int Height = 256;

        public TextureFormat Format = TextureFormat.RHalf;

        public NoiseLayerSettings NoiseLayerSettings;

        public Texture2D Noise;

        public NoiseLayer(NoiseLayerSettings noiseLayerSettings, bool tilable = true, int width = 256, int height = 256, TextureFormat format = TextureFormat.RHalf)
        {
            NoiseLayerSettings = noiseLayerSettings;
            Tilable = tilable;
            Width = width;
            Height = height;
            Format = format;
        }

        /// <summary>
        /// Creates the texture using class parameters
        /// </summary>
        public void Create()
        {
            Noise = new Texture2D(Width, Height, Format, false);
        }
    }
}
