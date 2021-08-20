using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script renders our scene at 384x216 resolution and upscales the result to the screen.
/// At render the camera is snapped to nearest pixel in pixelgrid and the snap offset is corrected for when we blits the render texture to game view.
/// </summary>
[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class PixelPerfectCamera : MonoBehaviour
{
    [SerializeField] private float camera_distance = 100f;

    private float camera_distance_to_y = Mathf.Sin(Mathf.Deg2Rad * 30f);
    private float camera_distance_to_z = Mathf.Cos(Mathf.Deg2Rad * 30f);

    private const float units_per_pixel_world = 40f / 216f;
    private const float units_per_pixel_camera = 40f / 225f;
    private Quaternion camera_quaternion_rotation = Quaternion.Euler(30f, 0f, 0f);
    private Quaternion camera_quaternion_rotation_inverse = Quaternion.Euler(330f, 0f, 0f);

    private Vector3 offset;

    public Transform camera_focus_point;
    private Camera m_camera;
    private RenderTexture rt;

    /// <summary>
    /// Rounds given Vector3 position to pixel grid.
    /// </summary>
    private Vector3 RoundToPixel(Vector3 position)
    {
        Vector3 result;
        result.x = Mathf.Round(position.x / units_per_pixel_world) * units_per_pixel_world;
        result.y = Mathf.Round(position.y / units_per_pixel_world) * units_per_pixel_world;
        result.z = Mathf.Round(position.z / units_per_pixel_world) * units_per_pixel_world;

        return result;
    }

    /// <summary>
    /// Snap camera position to pixel grid using Camera.worldToCameraMatrix. 
    /// </summary>
    private void PixelSnap()
    {
        Vector3 camera_position_inverse = camera_quaternion_rotation_inverse * m_camera.transform.position;
        Vector3 camera_position = camera_quaternion_rotation * m_camera.transform.position;
        camera_position_inverse.z = camera_position.z;

        Vector3 rounded_camera_position = RoundToPixel(camera_position_inverse);
        offset = rounded_camera_position - camera_position_inverse;
        offset.z = -offset.z;
        Matrix4x4 offset_matrix = Matrix4x4.TRS(-offset, Quaternion.identity, new Vector3(1.0f, 1.0f, -1.0f));

        m_camera.worldToCameraMatrix = offset_matrix * m_camera.transform.worldToLocalMatrix;
    }

    /// <summary>
    /// Sets camera rotation to isometric view.
    /// </summary>
    private void SetCameraRotation()
    {
        m_camera.transform.rotation = camera_quaternion_rotation;
    }

    /// <summary>
    /// Sets camera near clipping plane to be tangent to world 2D plane.
    /// Objects rendered close to camera will be cut vertically.
    /// </summary>
    private void SetCameraNearClippingPlane()
    {
        if (Application.isEditor)
        {
            return;
        }
        Vector4 clipPlaneWorldSpace = new Vector4(0f, 0f, 1f, 0f);
        Vector4 clipPlaneCameraSpace = Matrix4x4.Transpose(m_camera.cameraToWorldMatrix) * clipPlaneWorldSpace;
        m_camera.projectionMatrix = m_camera.CalculateObliqueMatrix(clipPlaneCameraSpace);
    }

    /// <summary>
    /// Moves camera to given focus point to reduce flickering
    /// </summary>
    private void MoveCamera()
    {
        Vector3 camera_pos = new Vector3(
            camera_focus_point.position.x,
            camera_focus_point.position.y + camera_distance * camera_distance_to_y,
            camera_focus_point.position.z - camera_distance * camera_distance_to_z
        );
        m_camera.transform.position = camera_pos;
    }

    private void Awake()
    {
        m_camera = GetComponent<Camera>();
        m_camera.orthographicSize = (225f / 216f) * 20f;
        SetCameraRotation();
        SetCameraNearClippingPlane();
    }

    private void LateUpdate()
    {
        MoveCamera();
    }

    private void OnPreCull()
    {
        PixelSnap();
    }

    /// <summary>
    /// Before render set camera render texture to temporary low res render texture. Bigger than actuall resolution to account for border pixel stretch.
    /// </summary>
    private void OnPreRender()
    {
        const int width = 400;
        const int height = 225;
        rt = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default, 1);
        m_camera.targetTexture = rt;
    }

    /// <summary>
    /// When we render account for none integer offsets using blits to get smooth camera movement. And scaled to right resolution in pixels 384x216 px.
    /// </summary>
    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        src.filterMode = FilterMode.Point;

        // Translate offset.xy to render texture uv cordinates.
        const float to_positive = units_per_pixel_camera * 0.5f;
        const float x_divider = 9f / 640f; //(1f * 216f) / (384f * 40f);
        const float y_divider = 1f / 40f;  //(1f * 384f) / (384f * 40f);

        Vector2 camera_offset = new Vector2((-offset.x + to_positive) * x_divider, (-offset.y + to_positive) * y_divider);
        Vector2 camera_scale = new Vector2(384f / 400f, 216f / 225f);

        Graphics.Blit(src, dest, camera_scale, camera_offset);

        RenderTexture.ReleaseTemporary(rt);
    }

    /// <summary>
    /// After render clear camera render texture.
    /// </summary>
    private void OnPostRender()
    {
        m_camera.targetTexture = null;
        RenderTexture.active = null;
    }
}
