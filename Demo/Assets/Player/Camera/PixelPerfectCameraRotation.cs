using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script renders our scene at 384x216 resolution and upscales the result to the screen.
/// At render the camera is snapped to nearest pixel in pixelgrid and the snap offset is corrected for when we blits the render texture to game view.
/// </summary>
[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class PixelPerfectCameraRotation : MonoBehaviour
{
    [SerializeField] private Vector3 camera_rotation_init = new Vector3(30f, 0f, 0f);
    [SerializeField] private float camera_distance = 100f;
    [SerializeField] private float camera_far_clipping_plane = 300f;
    [SerializeField] private float shadow_distance = 300f;
    private float _camera_far_clipping_plane;
    private float _camera_distance;

    public static Vector2 resolution
    {
        get { return new Vector2(480f, 270f); }
    }
    public static Vector2 resolution_extended
    {
        get { return new Vector2(512f, 288f); }
    }

    public const float units_per_pixel_world = 1f / 5f;
    public const float units_per_pixel_camera = (270f / 5f) / 288f;
    public const float pixels_per_unit = 5f;

    private Vector3 offset;

    [SerializeField] private Transform camera_focus_point;
    private CameraMovement camera_focus_point_script;
    [HideInInspector] public Camera m_camera;
    [HideInInspector] public Camera n_camera;
    [HideInInspector] public Camera r_camera;
    private RenderTexture rt;

    public static Vector2 camera_offset;

    private PlanarReflectionManager planar_reflection_manager;

    private static float _zoom = 1f;
    public static float zoom
    {
        get { return _zoom; }
        set { _zoom = Mathf.Clamp(value, 0.5f, 1f); }
    }

    /// <summary>
    /// Rounds given Vector3 position to pixel grid.
    /// </summary>
    public static Vector3 RoundToPixel(Vector3 position)
    {
        float zoomed_units_per_pixel_world = units_per_pixel_world / zoom;

        Vector3 result;
        result.x = Mathf.Round(position.x / zoomed_units_per_pixel_world) * zoomed_units_per_pixel_world;
        result.y = Mathf.Round(position.y / zoomed_units_per_pixel_world) * zoomed_units_per_pixel_world;
        result.z = Mathf.Round(position.z / zoomed_units_per_pixel_world) * zoomed_units_per_pixel_world;

        return result;
    }

    /// <summary>
    /// Snap camera position to pixel grid using Camera.worldToCameraMatrix. 
    /// </summary>
    public static Vector2 PixelSnap(ref Camera m_camera)
    {
        Vector3 camera_position = Quaternion.Inverse(m_camera.transform.rotation) * m_camera.transform.position;
        Vector3 rounded_camera_position = RoundToPixel(camera_position);
        Vector3 offset = rounded_camera_position - camera_position;
        offset.z = -offset.z;
        Matrix4x4 offset_matrix = Matrix4x4.TRS(-offset, Quaternion.identity, new Vector3(1.0f, 1.0f, -1.0f));

        m_camera.worldToCameraMatrix = offset_matrix * m_camera.transform.worldToLocalMatrix;

        // Translate offset.xy to render texture uv cordinates.
        float to_positive = (units_per_pixel_camera * 0.5f) / zoom;
        float x_divider = pixels_per_unit / resolution.x * zoom;
        float y_divider = pixels_per_unit / resolution.y * zoom;
        Vector2 camera_offset = new Vector2((-offset.x + to_positive) * x_divider, (-offset.y + to_positive) * y_divider);

        return camera_offset;
    }

    /// <summary>
    /// Sets camera near clipping plane to be tangent to world 2D plane.
    /// Objects rendered close to camera will be cut vertically.
    /// </summary>
    private void SetCameraNearClippingPlane()
    {
        Vector3 direction = m_camera.transform.forward;
        direction.y = 0f;
        Vector4 clipPlaneWorldSpace = new Vector4(direction.x, direction.y, direction.z, Vector3.Dot(m_camera.transform.position, -direction));
        Vector4 clipPlaneCameraSpace = Matrix4x4.Transpose(m_camera.cameraToWorldMatrix) * clipPlaneWorldSpace;
        m_camera.projectionMatrix = m_camera.CalculateObliqueMatrix(clipPlaneCameraSpace);
    }

    /// <summary>
    /// Moves camera to given focus point to reduce flickering
    /// </summary>
    public static void MoveCamera(ref Camera m_camera, Transform camera_focus_point, Vector3 x_rot, float dist)
    {
        m_camera.transform.rotation = camera_focus_point.rotation * Quaternion.Euler(x_rot);
        m_camera.transform.position = camera_focus_point.position - m_camera.transform.forward * dist;
    }

    private void Awake()
    {
        Shader.SetGlobalFloat("pixels_per_unit", pixels_per_unit);
        Shader.SetGlobalFloat("y_scale", SpriteInitializer.y_scale);

        camera_focus_point = GameObject.Find("camera_focus_point").transform;
        camera_focus_point_script = camera_focus_point.GetComponent<CameraMovement>();
        m_camera = GetComponent<Camera>();

        UpdateZoomValues();
        ZoomCamera(ref m_camera);

        //SetCameraNearClippingPlane();
    }

    private void Start()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            return;
        }
#endif
        planar_reflection_manager = GameObject.Find("ReflectionCamera").GetComponent<PlanarReflectionManager>();
    }

    private void UpdateZoomValues()
    {
        float scroll = PlayerInput.mouse_scroll_value;
        zoom += scroll;
        _camera_distance = camera_distance / zoom;
        _camera_far_clipping_plane = camera_far_clipping_plane / zoom;
        QualitySettings.shadowDistance = shadow_distance / zoom;
    }
    private void ZoomCamera(ref Camera camera)
    {
        camera.orthographicSize = (resolution_extended.y / (5f * 2f)) / zoom;
        camera.farClipPlane = _camera_far_clipping_plane;
    }

    private void Update()
    {
        if (GameTime.is_paused)
        {
            return;
        }

        UpdateZoomValues();
    }

    private void LateUpdate()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            return;
        }
#endif

        ZoomCamera(ref m_camera);
        ZoomCamera(ref n_camera);
        ZoomCamera(ref r_camera);

        // Check if camera sees an unloaded chunk
        GameTime.is_paused = SeesUnloaded();
        if (GameTime.is_paused)
        {
            return;
        }

        camera_focus_point_script.SmoothPosition();

        MoveCamera(ref m_camera, camera_focus_point, camera_rotation_init, _camera_distance);
        Shader.SetGlobalVector("_CameraOffset", new Vector4(camera_offset.x, camera_offset.y, 0f, 0f));
        camera_offset = PixelSnap(ref m_camera);
        SetCameraNearClippingPlane();

        // Create rays for bottom corners of the camera
        Ray bot_right_ray = Camera.main.ViewportPointToRay(new Vector3(1, 0, 0));
        Ray bot_left_ray = Camera.main.ViewportPointToRay(new Vector3(0, 0, 0));
        
        // Move camera after main camera
        planar_reflection_manager.ConstructMatrix4X4Ortho(bot_right_ray, bot_left_ray, 2f * m_camera.orthographicSize, transform.rotation * Quaternion.Euler(-60f, 0f, 0f));
        planar_reflection_manager.SetCameraNearClippingPlane();
    }

    private bool SeesUnloaded()
    {
        Plane plane = new Plane(Vector3.up, -Water.water_level);

        for (float x = -0.25f; x < 2f; x += 1.5f)
        {
            for (float y = -0.25f; y < 2f; y += 1.5f)
            {
                Ray camera_corner_ray = Camera.main.ViewportPointToRay(new Vector3(x, y, 0));

                float distance;
                plane.Raycast(camera_corner_ray, out distance);
                Vector3 plane_point = camera_corner_ray.GetPoint(distance);
                Chunk chunk = WorldGenerationManager.ReturnNearestChunk(plane_point);
                
                if (chunk == null || !chunk.is_loaded)
                {
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Before render set camera render texture to temporary low res render texture. Bigger than actuall resolution to account for border pixel stretch.
    /// </summary>
    private void OnPreRender()
    {
        rt = RenderTexture.GetTemporary((int) resolution_extended.x, (int) resolution_extended.y, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default, 1);
        m_camera.targetTexture = rt;
    }

    /// <summary>
    /// When we render account for none integer offsets using blits to get smooth camera movement. And scaled to right resolution in pixels 384x216 px.
    /// </summary>
    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        src.filterMode = FilterMode.Point;

        Vector2 camera_scale = new Vector2(resolution.x / resolution_extended.x, resolution.y / resolution_extended.y);

        Graphics.Blit(src, dest, camera_scale, camera_offset);
        //Graphics.Blit(src, dest, camera_scale, Vector2.zero);

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

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // Construct plane representing the water level
        Plane plane = new Plane(Vector3.up, -Water.water_level);

        // Create rays for bottom corners of the camera
        Ray bot_right_ray = Camera.main.ViewportPointToRay(new Vector3(1, 0, 0));
        Ray bot_left_ray = Camera.main.ViewportPointToRay(new Vector3(0, 0, 0));

        // Assign floats of ray distance for bottom corners
        float bot_right_distance;
        float bot_left_distance;

        // Raycast to plain
        plane.Raycast(bot_right_ray, out bot_right_distance);
        plane.Raycast(bot_left_ray, out bot_left_distance);

        // Get each position of all four camera corners where they hit the water plain
        Vector3 top_right_position = bot_right_ray.GetPoint(bot_right_distance);
        Vector3 top_left_position = bot_left_ray.GetPoint(bot_left_distance);

        // Draw red raycasts twords those points
        Gizmos.color = Color.red;
        Gizmos.DrawRay(bot_right_ray.origin, bot_right_ray.direction * bot_right_distance);
        Gizmos.DrawRay(bot_left_ray.origin, bot_left_ray.direction * bot_left_distance);

        // Draw a yellow sphere at the hit location
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(top_right_position, 1);
        Gizmos.DrawSphere(top_left_position, 1);

        // Get height of orthographic camera
        float camera_height = 2f * m_camera.orthographicSize;

        // Get sidebar of new camera
        Vector3 camera_sidebar_direction = transform.rotation * Quaternion.Euler(-60f, 0f, 0f) * Vector3.down;
        Vector3 camera_sidebar = camera_sidebar_direction * camera_height;

        // Continue drawing magenta raycast that represent the bottom screen edges
        Gizmos.color = Color.magenta;
        Gizmos.DrawRay(top_right_position, camera_sidebar);
        Gizmos.DrawRay(top_left_position, camera_sidebar);

        // Create the vectors representing each corner of our new worldToCameraMatrix.
        Vector3 bot_right_position = top_right_position + camera_sidebar;
        Vector3 bot_left_position = top_left_position + camera_sidebar;

        // Draw 
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(bot_right_position, 1);
        Gizmos.DrawSphere(bot_left_position, 1);

        // Yellow points are now representing the top right and top left corners of our new camera whilst the green points represent bottom right and bottom left corners.
    }
#endif
}
