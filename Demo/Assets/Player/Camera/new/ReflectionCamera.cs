using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class ReflectionCamera : MonoBehaviour
{
    /// <summary>
    /// The reflection camera
    /// </summary>
    private Camera reflectionCamera;

    /// <summary>
    /// The render texture this camera renders to
    /// </summary>
    private RenderTexture reflectionCameraRenderTexture;

    /// <summary>
    /// The far clip plane of reflection camera, can be smaller than the main camera far clip plane
    /// </summary>
    [SerializeField] private float cameraFarClippingPlane = 150f;

    /// <summary>
    /// Gets the camera component and initilizes it
    /// </summary>
    private void Awake()
    {
        // get the camera
        reflectionCamera = GetComponent<Camera>();

        UpdateProjection();

        // deligate this UpdateRender to updateRenders
        PixelPerfect.updateRenders += UpdateRender;
    }

    /// <summary>
    /// Dispose of render texture
    /// </summary>
    private void OnDestroy()
    {
        if (reflectionCameraRenderTexture != null)
        {
            RenderTexture.active = null;
            reflectionCamera.targetTexture = null;
            DestroyImmediate(reflectionCameraRenderTexture);
        }
    }

    private void UpdateProjection()
    {
        // make it similar to the one of MainCamera
        reflectionCamera.CopyFrom(MainCamera.mCamera);
        // but with other far clip plane and render (first render normal, then reflection and lastly main camera)
        reflectionCamera.farClipPlane = cameraFarClippingPlane;
        reflectionCamera.depth = -1;
        reflectionCamera.depthTextureMode = DepthTextureMode.None;
    }

    /// <summary>
    /// Deligate to run after new game resolution (frees and creates new render texture)
    /// </summary>
    private void UpdateRender()
    {
        OnDestroy();
        reflectionCameraRenderTexture = PixelPerfect.CreateRenderTexture();
        UpdateProjection();

        Shader.SetGlobalTexture("_WaterReflectionTexture", reflectionCameraRenderTexture);
        reflectionCamera.targetTexture = reflectionCameraRenderTexture;
    }

    /// <summary>
    /// Sets clip plane to water level
    /// </summary>
    private void SetCameraNearClippingPlane()
    {
        Vector4 clipPlaneWorldSpace = new Vector4(0f, 1f, 0f, -Water.waterLevel);
        Vector4 clipPlaneCameraSpace = Matrix4x4.Transpose(Matrix4x4.Inverse(reflectionCamera.worldToCameraMatrix)) * clipPlaneWorldSpace;
        reflectionCamera.projectionMatrix = MainCamera.mCamera.CalculateObliqueMatrix(clipPlaneCameraSpace);
    }

    /// <summary>
    /// Places 
    /// </summary>
    private void ConstructMatrix4X4Ortho()
    {
        // Construct plane representing the water level
        Plane plane = new Plane(Vector3.up, -Water.waterLevel);

        // Assign floats of ray distance for bottom corners
        float botRightDistance;
        float botLeftDistance;

        // Create rays for bottom corners of the main camera
        Ray botRightRay = MainCamera.mCamera.ViewportPointToRay(new Vector3(1, 0, 0));
        Ray botLeftRay = MainCamera.mCamera.ViewportPointToRay(new Vector3(0, 0, 0));

        // Raycast to plain
        plane.Raycast(botRightRay, out botRightDistance);
        plane.Raycast(botLeftRay, out botLeftDistance);

        // Get each position of all four camera corners where they hit the water plain
        Vector3 topRightPosition = botRightRay.GetPoint(botRightDistance);
        Vector3 topLeftPosition = botLeftRay.GetPoint(botLeftDistance);

        // Get flipped rotation (reflection rotation)
        Quaternion relfectionRotation = MainCamera.mCamera.transform.rotation * Quaternion.Euler(-60f, 0f, 0f);

        // Get sidebar of new camera
        Vector3 cameraSidebarDirection = relfectionRotation * Vector3.down;
        Vector3 cameraSidebar = cameraSidebarDirection * 2f * MainCamera.mCamera.orthographicSize;

        // Create the vectors representing bottom corners of our new worldToCameraMatrix.
        Vector3 botRightPosition = topRightPosition + cameraSidebar;
        Vector3 botLeftPosition = topLeftPosition + cameraSidebar;

        // Get middle point of our four points
        Vector3 middle = (topRightPosition + topLeftPosition + botRightPosition + botLeftPosition) * 0.25f;

        // Assign that point to the camera position and rotate camera
        reflectionCamera.transform.position = middle;
        reflectionCamera.transform.rotation = relfectionRotation;
    }

    /// <summary>
    /// Move reflection camera after main camera (this late update is run after the maincamera each frame)
    /// </summary>
    private void LateUpdate()
    {
        ConstructMatrix4X4Ortho();
        SetCameraNearClippingPlane();
    }
}
