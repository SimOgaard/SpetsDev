using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldMapRenderer : MonoBehaviour
{
    public RenderTexture renderTexture;

    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        // render that texture
        Graphics.Blit(renderTexture, dest);
    }
}
