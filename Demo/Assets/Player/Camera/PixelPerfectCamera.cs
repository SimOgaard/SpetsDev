using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script renders our scene at 384x216 resolution and upscales the result to the screen.
/// At render the camera is snapped to nearest pixel in pixelgrid and the snap offset is corrected for when we blits the render texture to game view.
/// </summary>
[RequireComponent(typeof(Camera))]
public class PixelPerfectCamera : MonoBehaviour
{
    [SerializeField]
    private float camera_distance_origo_z = -100f;
    [SerializeField]
    private float camera_distance = 100f;

    private float camera_distance_to_y = Mathf.Sin(Mathf.Atan(Mathf.Sin(30f * Mathf.Deg2Rad)));
    private float camera_distance_to_z = Mathf.Cos(Mathf.Atan(Mathf.Sin(30f * Mathf.Deg2Rad)));
    private float camera_focus_point_y_to_z = 1f / Mathf.Sin(30f * Mathf.Deg2Rad);

    private float units_per_pixel = 40f / 216f;
    private float camera_rotation = Mathf.Atan(Mathf.Sin(30f * Mathf.Deg2Rad));
    private Quaternion camera_quaternion_rotation = Quaternion.Euler(Mathf.Rad2Deg * Mathf.Atan(Mathf.Sin(30f * Mathf.Deg2Rad)), 0f, 0f);
    private float camera_rotation_inverse = Mathf.Atan(Mathf.Sin(330f * Mathf.Deg2Rad));
    private Quaternion camera_quaternion_rotation_inverse = Quaternion.Euler(Mathf.Rad2Deg * Mathf.Atan(Mathf.Sin(330f * Mathf.Deg2Rad)), 0f, 0f);

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
        result.x = Mathf.Round(position.x / units_per_pixel) * units_per_pixel;
        result.y = Mathf.Round(position.y / units_per_pixel) * units_per_pixel;
        result.z = Mathf.Round(position.z / units_per_pixel) * units_per_pixel;

        return result;
    }

    /// <summary>
    /// Snap camera position to pixel grid using Camera.worldToCameraMatrix. 
    /// For camera fixed in Y space multiply (m_camera.transform.position) with:
    ///     Quaternion.Euler(Mathf.Rad2Deg * Mathf.Atan(Mathf.Sin(330f * Mathf.Deg2Rad)), 0f, 0f);
    /// For camera fixed in Z space multiply (m_camera.transform.position) with:
    ///     Quaternion.Euler(Mathf.Rad2Deg * Mathf.Atan(Mathf.Sin(30f * Mathf.Deg2Rad)), 0f, 0f);
    /// </summary>
    private void PixelSnap()
    {
        Vector3 camera_position = camera_quaternion_rotation_inverse * m_camera.transform.position;
        Vector3 rounded_camera_position = RoundToPixel(camera_position);
        offset = rounded_camera_position - camera_position;
        offset.z = -offset.z;
        Matrix4x4 offset_matrix = Matrix4x4.TRS(-offset, Quaternion.identity, new Vector3(1.0f, 1.0f, -1.0f));

        m_camera.worldToCameraMatrix = offset_matrix * m_camera.transform.worldToLocalMatrix;
    }

    /// <summary>
    /// Sets camera rotation to isometric view.
    /// 30 degrees does not achieve the 2 pixel across 1 pixel down look so we set rotation to approximately 26.565 degrees.
    /// </summary>
    private void SetCameraRotation()
    {
        m_camera.transform.rotation = camera_quaternion_rotation;
    }

    /// <summary>
    /// Sets camera near clipping plane to be tangent to world 2D plane.
    /// Objects rendered close to camera will be cut vertically.
    /// 
    /// OBS! When player moves in Y, camera moves in Z. Camera projection matrix should not be changed from character moving in Y.
    /// 
    /// </summary>
    private void SetCameraNearClippingPlane()
    {
        Vector4 clipPlaneWorldSpace = new Vector4(0f, 0f, 1f, 0f);
        Vector4 clipPlaneCameraSpace = Matrix4x4.Transpose(m_camera.cameraToWorldMatrix) * clipPlaneWorldSpace;
        m_camera.projectionMatrix = m_camera.CalculateObliqueMatrix(clipPlaneCameraSpace);
    }

    /// <summary>
    /// Moves camera to given focus point to reduce flickering
    /// For camera fixed in Y space:
    ///     Keeps the camera Y position so that the camera only snaps to X and Z dimensions.
    ///     Set camera_pos to (new Vector3(camera_focus_point.position.x, camera_distance * Mathf.Sin(Mathf.Atan(Mathf.Sin(30f * Mathf.Deg2Rad))), camera_focus_point.position.z - (camera_distance * Mathf.Cos(Mathf.Atan(Mathf.Sin(30f * Mathf.Deg2Rad)))) + camera_focus_point.position.y / Mathf.Sin(30f * Mathf.Deg2Rad)))
    /// 
    /// For camera fixed in Z space:
    ///     Keeps camera the same distance from (0,0,0) in Z dimension so that the camera only snaps to X and Y dimensions.
    ///     Set camera_pos to (new Vector3(camera_focus_point.position.x, (camera_focus_point.position.z - camera_distance_origo_z) * Mathf.Sin(30f * Mathf.Deg2Rad) + camera_focus_point.position.y, camera_distance_origo_z))
    /// </summary>
    private void MoveCamera()
    {
        Vector3 camera_pos = new Vector3(
            camera_focus_point.position.x,
            camera_distance * camera_distance_to_y,
            camera_focus_point.position.z - camera_distance * camera_distance_to_z + camera_focus_point.position.y * camera_focus_point_y_to_z
        );
        m_camera.transform.position = camera_pos;
    }

    private void Awake()
    {
        m_camera = GetComponent<Camera>();
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
    /// Before render set camera render texture to temporary low res render texture.
    /// </summary>
    void OnPreRender()
    {
        int width = 384;
        int height = 216;
        rt = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default, 1);
        m_camera.targetTexture = rt;
    }

    /// <summary>
    /// When we render account for none integer offsets using blits to get smooth camera movement.
    /// </summary>
    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        src.filterMode = FilterMode.Point;

        // Translate offset.xy to render texture uv cordinates.
        float to_positive = units_per_pixel * 0.5f;
        float x_divider = 9f / 640f; //(1f * 216f) / (384f * 40f);
        float y_divider = 1f / 40f;  //(1f * 384f) / (384f * 40f);

        Vector2 camera_offset = new Vector2((-offset.x + to_positive) * x_divider, (-offset.y + to_positive) * y_divider);
        Vector2 camera_scale = new Vector2(1f, 1f);

        Graphics.Blit(src, dest, camera_scale, camera_offset);

        RenderTexture.ReleaseTemporary(rt);
    }

    /// <summary>
    /// After render clear camera render texture.
    /// </summary>
    void OnPostRender()
    {
        m_camera.targetTexture = null;
        RenderTexture.active = null;
    }
}
