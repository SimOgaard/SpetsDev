using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanarReflectionInit : MonoBehaviour
{
    private void Awake()
    {
        PixelPerfectCameraRotation thisCamera = GetComponent<PixelPerfectCameraRotation>();
        RenderTexture renderTextureTarget = new RenderTexture((int) PixelPerfectCameraRotation.resolutionExtended.x,(int) PixelPerfectCameraRotation.resolutionExtended.y, 32, RenderTextureFormat.Default);
        renderTextureTarget.filterMode = FilterMode.Point;

        Shader.SetGlobalTexture("_WaterReflectionTexture", renderTextureTarget);

        Camera reflectionCamera = NormalsReplacementShader.CopyCamera(thisCamera, transform.parent, "ReflectionCamera", 1);
        //reflectionCamera.clearFlags = CameraClearFlags.Nothing;
        reflectionCamera.targetTexture = renderTextureTarget;

        reflectionCamera.nearClipPlane = 0f;

        reflectionCamera.gameObject.AddComponent<PlanarReflectionManager>();
        thisCamera.rCamera = reflectionCamera;
    }
}
