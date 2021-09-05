using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanarReflectionInit : MonoBehaviour
{
    private void Awake()
    {
        Camera this_camera = GetComponent<Camera>();
        RenderTexture render_texture_target = new RenderTexture(400, 225, 24, RenderTextureFormat.Default);
        render_texture_target.filterMode = FilterMode.Point;
        RenderTexture render_texture_target_depth = new RenderTexture(400, 225, 24, RenderTextureFormat.Depth);
        render_texture_target_depth.filterMode = FilterMode.Point;

        Shader.SetGlobalTexture("_WaterReflectionTexture", render_texture_target);
        Shader.SetGlobalTexture("_WaterReflectionTextureDepth", render_texture_target_depth);

        Camera reflection_camera = NormalsReplacementShader.CopyCamera(this_camera, transform.parent, "ReflectionCamera", 1);
        //reflection_camera.clearFlags = CameraClearFlags.Nothing;
        reflection_camera.SetTargetBuffers(render_texture_target.colorBuffer, render_texture_target_depth.depthBuffer);

        reflection_camera.gameObject.AddComponent<PlanarReflectionManager>();
    }
}
