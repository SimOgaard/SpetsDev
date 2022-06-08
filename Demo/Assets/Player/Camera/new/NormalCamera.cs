using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class NormalCamera : MonoBehaviour
{
    /// <summary>
    /// The reflection camera
    /// </summary>
    private Camera normalCamera;

    /// <summary>
    /// The render texture this camera renders to
    /// </summary>
    private RenderTexture normalCameraRenderTexture;

    /// <summary>
    /// The normals replacement shader for camera
    /// </summary>
    [SerializeField] private Shader normalsShader;

    /// <summary>
    /// Gets the camera component and initilizes it
    /// </summary>
    private void Awake()
    {
        // get the camera
        normalCamera = GetComponent<Camera>();
        UpdateProjection();

        // deligate this UpdateRender to updateRenders
        PixelPerfect.updateRenders += UpdateRender;
    }

    /// <summary>
    /// Dispose of render texture
    /// </summary>
    private void OnDestroy()
    {
        if (normalCameraRenderTexture != null)
        {
            RenderTexture.active = null;
            normalCamera.targetTexture = null;
            DestroyImmediate(normalCameraRenderTexture);
        }
    }

    private void UpdateProjection()
    {
        // make it similar to the one of MainCamera
        normalCamera.CopyFrom(MainCamera.mCamera);
        // but with other render (first render normal, then reflection and lastly main camera)
        normalCamera.depth = -2;
        normalCamera.depthTextureMode = DepthTextureMode.None;
        normalCamera.SetReplacementShader(normalsShader, "RenderType");
    }

    /// <summary>
    /// Deligate to run after new game resolution (frees and creates new render texture)
    /// </summary>
    private void UpdateRender()
    {
        OnDestroy();
        normalCameraRenderTexture = PixelPerfect.CreateRenderTexture(0);
        UpdateProjection();

        Shader.SetGlobalTexture("_CameraNormalsTexture", normalCameraRenderTexture);
        normalCamera.targetTexture = normalCameraRenderTexture;
    }

    private void OnPreCull()
    {
        transform.localRotation = MainCamera.mCamera.transform.rotation;
        transform.position = MainCamera.mCamera.transform.position;
        //normalCamera.worldToCameraMatrix = MainCamera.mCamera.worldToCameraMatrix;
    }
}
