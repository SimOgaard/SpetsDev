using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanarReflectionManager : MonoBehaviour
{
    private Camera reflectionCamera;

    [SerializeField] private float cameraFarClippingPlane = 150f;

    private void Awake()
    {
        reflectionCamera = GetComponent<Camera>();
        reflectionCamera.farClipPlane = cameraFarClippingPlane;
    }

    public void SetCameraNearClippingPlane()
    {
        Vector4 clipPlaneWorldSpace = new Vector4(0f, 1f, 0f, -Water.waterLevel);
        Vector4 clipPlaneCameraSpace = Matrix4x4.Transpose(Matrix4x4.Inverse(reflectionCamera.worldToCameraMatrix)) * clipPlaneWorldSpace;
        reflectionCamera.projectionMatrix = MainCamera.mCamera.CalculateObliqueMatrix(clipPlaneCameraSpace);
    }
  
    public void ConstructMatrix4X4Ortho(Ray botRightRay, Ray botLeftRay, float cameraOrthographicHeight, Quaternion cameraRotation)
    {
        // Construct plane representing the water level
        Plane plane = new Plane(Vector3.up, -Water.waterLevel);

        // Assign floats of ray distance for bottom corners
        float botRightDistance;
        float botLeftDistance;

        // Raycast to plain
        plane.Raycast(botRightRay, out botRightDistance);
        plane.Raycast(botLeftRay, out botLeftDistance);

        // Get each position of all four camera corners where they hit the water plain
        Vector3 topRightPosition = botRightRay.GetPoint(botRightDistance);
        Vector3 topLeftPosition = botLeftRay.GetPoint(botLeftDistance);

        // Get sidebar of new camera
        Vector3 cameraSidebarDirection = cameraRotation * Vector3.down;
        Vector3 cameraSidebar = cameraSidebarDirection * cameraOrthographicHeight;

        // Create the vectors representing bottom corners of our new worldToCameraMatrix.
        Vector3 botRightPosition = topRightPosition + cameraSidebar;
        Vector3 botLeftPosition = topLeftPosition + cameraSidebar;

        // Get middle point of our four points
        Vector3 middle = (topRightPosition + topLeftPosition + botRightPosition + botLeftPosition) * 0.25f;

        // Assign that point to the camera position and rotate camera
        reflectionCamera.transform.position = middle;
        reflectionCamera.transform.rotation = cameraRotation;

        //Matrix4x4 worldToCameraMatrix = Matrix4x4.Ortho(-10, 10, -10, 10, 0, 10); // You could probably create an ortho matrix from theese cords but i do not now how
    }

    public void UpdateRenderTexture()
    {
        if (reflectionCamera.targetTexture != null)
        {
            reflectionCamera.targetTexture.Release();
        }

        RenderTexture renderTextureTarget = new RenderTexture(PixelPerfectCameraRotation.renderWidthExtended, PixelPerfectCameraRotation.renderHeightExtended, 24, RenderTextureFormat.Default);
        renderTextureTarget.filterMode = FilterMode.Point;
        Shader.SetGlobalTexture("_WaterReflectionTexture", renderTextureTarget);
        reflectionCamera.targetTexture = renderTextureTarget;
    }
}
