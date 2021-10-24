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

    public const float units_per_pixel_world = 40f / 216f;
    public const float units_per_pixel_camera = 40f / 225f;
    private Vector3 offset;

    [SerializeField] private Transform camera_focus_point;
    private Camera m_camera;
    private RenderTexture rt;

    public static Vector2 camera_offset;

    private PlanarReflectionManager planar_reflection_manager;

    /// <summary>
    /// Rounds given Vector3 position to pixel grid.
    /// </summary>
    public static Vector3 RoundToPixel(Vector3 position)
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
    public static Vector2 PixelSnap(ref Camera m_camera)
    {
        Vector3 camera_position = Quaternion.Inverse(m_camera.transform.rotation) * m_camera.transform.position;
        Vector3 rounded_camera_position = RoundToPixel(camera_position);
        Vector3 offset = rounded_camera_position - camera_position;
        offset.z = -offset.z;
        Matrix4x4 offset_matrix = Matrix4x4.TRS(-offset, Quaternion.identity, new Vector3(1.0f, 1.0f, -1.0f));

        m_camera.worldToCameraMatrix = offset_matrix * m_camera.transform.worldToLocalMatrix;

        // Translate offset.xy to render texture uv cordinates.
        const float to_positive = units_per_pixel_camera * 0.5f;
        const float x_divider = 9f / 640f; //(1f * 216f) / (384f * 40f);
        const float y_divider = 1f / 40f;  //(1f * 384f) / (384f * 40f);
        Vector2 camera_offset = new Vector2((-offset.x + to_positive) * x_divider, (-offset.y + to_positive) * y_divider);

        return camera_offset;
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
    public static void MoveCamera(ref Camera m_camera, Transform camera_focus_point, Vector3 x_rot, float dist)
    {
        m_camera.transform.rotation = camera_focus_point.rotation * Quaternion.Euler(x_rot);
        m_camera.transform.position = camera_focus_point.position - m_camera.transform.forward * dist;
    }

    private void Awake()
    {
        camera_focus_point = GameObject.Find("camera_focus_point").transform;
        m_camera = GetComponent<Camera>();
        m_camera.orthographicSize = (225f / 216f) * 20f;
        SetCameraNearClippingPlane();
    }

    private void Start()
    {
        if (Application.isPlaying)
        {
            planar_reflection_manager = GameObject.Find("ReflectionCamera").GetComponent<PlanarReflectionManager>();

            /*
            GameObject fucking_bitch_ass_god_cube_game_object = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fucking_bitch_ass_god_cube_game_object.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Nature/Terrain/Diffuse"));
            fucking_bitch_ass_god_cube_game_object.name = "fucking bitch ass god cube";
            Destroy(fucking_bitch_ass_god_cube_game_object.GetComponent<BoxCollider>());
            fucking_bitch_ass_god_cube = fucking_bitch_ass_god_cube_game_object.transform;
            fucking_bitch_ass_god_cube.localScale = Vector3.zero;
            */
        }
    }

    [SerializeField] private Transform fucking_bitch_ass_god_cube;
    private void Update()
    {
        if (Application.isPlaying)
        {
            //fucking_bitch_ass_god_cube.transform.position = m_camera.transform.position + m_camera.transform.forward * 25f;
        }
    }

    private void LateUpdate()
    {
        MoveCamera(ref m_camera, camera_focus_point, camera_rotation_init, camera_distance);
        Shader.SetGlobalVector("_CameraOffset", new Vector4(camera_offset.x, camera_offset.y, 0f, 0f));
        camera_offset = PixelSnap(ref m_camera);

        if (!Application.isPlaying)
        {
            return;
        }

        // Create rays for bottom corners of the camera
        Ray bot_right_ray = Camera.main.ViewportPointToRay(new Vector3(1, 0, 0));
        Ray bot_left_ray = Camera.main.ViewportPointToRay(new Vector3(0, 0, 0));
        
        // Move camera after main camera
        planar_reflection_manager.ConstructMatrix4X4Ortho(bot_right_ray, bot_left_ray, 2f * m_camera.orthographicSize, transform.rotation * Quaternion.Euler(-60f, 0f, 0f));
        planar_reflection_manager.SetCameraNearClippingPlane();
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

        Vector2 camera_scale = new Vector2(384f / 400f, 216f / 225f);

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
