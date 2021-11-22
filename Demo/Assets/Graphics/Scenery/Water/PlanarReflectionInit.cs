using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanarReflectionInit : MonoBehaviour
{
    private void Awake()
    {
        PixelPerfectCameraRotation this_camera = GetComponent<PixelPerfectCameraRotation>();
        RenderTexture render_texture_target = new RenderTexture((int) PixelPerfectCameraRotation.resolution_extended.x,(int) PixelPerfectCameraRotation.resolution_extended.y, 32, RenderTextureFormat.Default);
        render_texture_target.filterMode = FilterMode.Point;

        Shader.SetGlobalTexture("_WaterReflectionTexture", render_texture_target);

        Camera reflection_camera = NormalsReplacementShader.CopyCamera(this_camera, transform.parent, "ReflectionCamera", 1);
        //reflection_camera.clearFlags = CameraClearFlags.Nothing;
        reflection_camera.targetTexture = render_texture_target;

        reflection_camera.nearClipPlane = 0f;

        reflection_camera.gameObject.AddComponent<PlanarReflectionManager>();
        this_camera.r_camera = reflection_camera;
    }
}
