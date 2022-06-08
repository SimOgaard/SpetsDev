using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CopyCameraPosition : MonoBehaviour
{
    private Camera cameraToCopy;
    private Camera thisCamera;

    private void Start()
    {
        cameraToCopy = MainCamera.mCamera;
        thisCamera = GetComponent<Camera>();
    }

    private void OnPreCull()
    {
        transform.localRotation = cameraToCopy.transform.rotation;
        transform.position = cameraToCopy.transform.position;
        thisCamera.worldToCameraMatrix = cameraToCopy.worldToCameraMatrix;
    }

    /*
    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        Graphics.Blit(src, dest, new Vector2(1f, 1f), PixelPerfectCameraRotation.cameraOffset);
    }
    */
}
