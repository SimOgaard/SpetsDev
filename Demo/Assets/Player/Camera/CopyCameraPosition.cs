using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CopyCameraPosition : MonoBehaviour
{
    private Camera camera_to_copy;
    private Camera this_camera;

    private void Start()
    {
        camera_to_copy = Camera.main;
        this_camera = GetComponent<Camera>();
    }

    private void OnPreCull()
    {
        this_camera.worldToCameraMatrix = camera_to_copy.worldToCameraMatrix;
    }

    /*
    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        Graphics.Blit(src, dest, new Vector2(1f, 1f), PixelPerfectCameraRotation.camera_offset);
    }
    */
}
