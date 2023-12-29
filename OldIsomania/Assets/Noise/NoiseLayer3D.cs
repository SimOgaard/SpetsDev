using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Noise
{
    public class NoiseLayer3D : MonoBehaviour
    {
        public bool Tilable = true;

        public int Width = 256;
        public int Height = 256;
        public int Depth = 2;

        public TextureFormat Format = TextureFormat.RHalf;

        public NoiseLayerSettings NoiseLayerSettings;

        public Texture3D Noise;

        public NoiseLayer3D(NoiseLayerSettings noiseLayerSettings, bool tilable = true, int width = 256, int height = 256, int depth = 2, TextureFormat format = TextureFormat.RHalf)
        {
            NoiseLayerSettings = noiseLayerSettings;
            Tilable = tilable;
            Width = width;
            Height = height;
            Depth = depth;
            Format = format;
        }


    }
}
