using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode, RequireComponent(typeof(Camera))]
public class PixelCamera : MonoBehaviour
{
    public Material material;
    public Vector2 resolution = new Vector2(640, 360);

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Vector2 pixel_division = new Vector2(Screen.width / resolution.x, Screen.height / resolution.y);
    
        material.SetVector("_PixelDivision", resolution);
        Graphics.Blit(source, destination, material);
    }
}