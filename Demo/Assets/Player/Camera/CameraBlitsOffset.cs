using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBlitsOffset : MonoBehaviour
{
    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        Graphics.Blit(src, dest, new Vector2(1f, 1f), -PixelPerfectCameraRotation.cameraOffset);
    }
}
