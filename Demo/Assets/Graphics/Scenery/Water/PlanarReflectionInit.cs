using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanarReflectionInit : MonoBehaviour
{
    private void Awake()
    {
        PixelPerfectCameraRotation thisCamera = GetComponent<PixelPerfectCameraRotation>();
        Camera reflectionCamera = NormalsReplacementShader.CopyCamera(thisCamera, transform.parent, "ReflectionCamera", 1);
        reflectionCamera.nearClipPlane = 0f;

        PlanarReflectionManager planarReflectionManager = reflectionCamera.gameObject.AddComponent<PlanarReflectionManager>();
        thisCamera.rCamera = reflectionCamera;
    }
}
