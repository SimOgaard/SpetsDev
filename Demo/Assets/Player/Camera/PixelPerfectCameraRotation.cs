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
    [SerializeField] private Vector3 cameraRotationInit = new Vector3(30f, 0f, 0f);
    [SerializeField] private float cameraDistance = 100f;
    [SerializeField] private float cameraFarClippingPlane = 300f;
    [SerializeField] private float shadowDistance = 300f;
    private float _cameraFarClippingPlane;
    private float _cameraDistance;

    public static Vector2 resolution
    {
        get { return new Vector2(480f, 270f); }
    }
    public static Vector2 resolutionExtended
    {
        get { return new Vector2(512f, 288f); }
    }

    public const float unitsPerPixelWorld = 1f / 5f;
    public const float unitsPerPixelCamera = (270f / 5f) / 288f;
    public const float pixelsPerUnit = 5f;

    private Vector3 offset;

    private CameraMovement cameraFocusPointScript;
    [HideInInspector] public Camera mCamera;
    [HideInInspector] public Camera nCamera;
    [HideInInspector] public Camera rCamera;
    private RenderTexture rt;

    public static Vector2 cameraOffset;

    private PlanarReflectionManager planarReflectionManager;

    [SerializeField] private DayNightCycle dayNightCycle;
    [SerializeField] private CloudShadows sunCloudShadows;
    [SerializeField] private CloudShadows moonCloudShadows;

    private static float _zoom = 0.5f;
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
        float zoomedUnitsPerPixelWorld = unitsPerPixelWorld / zoom;

        Vector3 result;
        result.x = Mathf.Round(position.x / zoomedUnitsPerPixelWorld) * zoomedUnitsPerPixelWorld;
        result.y = Mathf.Round(position.y / zoomedUnitsPerPixelWorld) * zoomedUnitsPerPixelWorld;
        result.z = Mathf.Round(position.z / zoomedUnitsPerPixelWorld) * zoomedUnitsPerPixelWorld;

        return result;
    }

    /// <summary>
    /// Snap camera position to pixel grid using Camera.worldToCameraMatrix. 
    /// </summary>
    public static Vector2 PixelSnap(ref Camera mCamera)
    {
        Vector3 cameraPosition = Quaternion.Inverse(mCamera.transform.rotation) * mCamera.transform.position;
        Vector3 roundedCameraPosition = RoundToPixel(cameraPosition);
        Vector3 offset = roundedCameraPosition - cameraPosition;
        offset.z = -offset.z;
        Matrix4x4 offsetMatrix = Matrix4x4.TRS(-offset, Quaternion.identity, new Vector3(1.0f, 1.0f, -1.0f));

        mCamera.worldToCameraMatrix = offsetMatrix * mCamera.transform.worldToLocalMatrix;

        // Translate offset.xy to render texture uv cordinates.
        float toPositive = (unitsPerPixelCamera * 0.5f) / zoom;
        float xDivider = pixelsPerUnit / resolution.x * zoom;
        float yDivider = pixelsPerUnit / resolution.y * zoom;
        Vector2 cameraOffset = new Vector2((-offset.x + toPositive) * xDivider, (-offset.y + toPositive) * yDivider);

        return cameraOffset;
    }

    /// <summary>
    /// Sets camera near clipping plane to be tangent to world 2D plane.
    /// Objects rendered close to camera will be cut vertically.
    /// </summary>
    private void SetCameraNearClippingPlane()
    {
        Vector3 direction = mCamera.transform.forward;
        direction.y = 0f;
        Vector4 clipPlaneWorldSpace = new Vector4(direction.x, direction.y, direction.z, Vector3.Dot(mCamera.transform.position, -direction));
        Vector4 clipPlaneCameraSpace = Matrix4x4.Transpose(mCamera.cameraToWorldMatrix) * clipPlaneWorldSpace;
        mCamera.projectionMatrix = mCamera.CalculateObliqueMatrix(clipPlaneCameraSpace);
    }

    /// <summary>
    /// Moves camera to given focus point to reduce flickering
    /// </summary>
    public static void MoveCamera(ref Camera mCamera, Transform cameraFocusPoint, Vector3 xRot, float dist)
    {
        mCamera.transform.rotation = cameraFocusPoint.rotation * Quaternion.Euler(xRot);
        mCamera.transform.position = cameraFocusPoint.position - mCamera.transform.forward * dist;
    }

    private void Awake()
    {
#if UNITY_EDITOR
        if (GameObject.Find("DEBUG"))
        {
            Debug.Log("Debug mode");
        }
#endif

        Shader.SetGlobalFloat("pixelsPerUnit", pixelsPerUnit);
        Shader.SetGlobalFloat("yScale", SpriteInitializer.yScale);

        cameraFocusPointScript = Global.cameraFocusPointTransform.GetComponent<CameraMovement>();
        mCamera = GetComponent<Camera>();
    }

    private void Start()
    {
        UpdateZoomValues(Mathf.NegativeInfinity);
        ZoomCamera(ref mCamera);

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            return;
        }
#endif
        planarReflectionManager = GameObject.Find("ReflectionCamera").GetComponent<PlanarReflectionManager>();
        dayNightCycle = GameObject.Find("DayNightCycle").GetComponent<DayNightCycle>();
    }

    private void UpdateZoomValues(float scrollOffset = 0f)
    {
        float scroll = PlayerInput.mouseScrollValue - scrollOffset;
        if (scroll == 0f)
        {
            return;
        }

        zoom += scroll;
        if (sunCloudShadows != null)
        {
            sunCloudShadows.UpdateLightProperties(zoom);
        }
        if (moonCloudShadows != null)
        {
            moonCloudShadows.UpdateLightProperties(zoom);
        }
        _cameraDistance = cameraDistance / zoom;
        _cameraFarClippingPlane = cameraFarClippingPlane / zoom;
        QualitySettings.shadowDistance = shadowDistance / zoom;
    }
    private void ZoomCamera(ref Camera camera, float farClipPlaneFactor = 1f)
    {
        camera.orthographicSize = (resolutionExtended.y / (5f * 2f)) / zoom;
        camera.farClipPlane = _cameraFarClippingPlane * farClipPlaneFactor;
    }

    private void Update()
    {
        if (GameTime.isPaused)
        {
            return;
        }

        UpdateZoomValues(Mathf.NegativeInfinity);
    }

    private void LateUpdate()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            return;
        }
#endif

        ZoomCamera(ref mCamera);
        ZoomCamera(ref nCamera);
        ZoomCamera(ref rCamera, 0.75f);

        // Check if camera sees an unloaded chunk
        GameTime.isPaused = SeesUnloaded();
        if (GameTime.isPaused)
        {
            return;
        }

        cameraFocusPointScript.SmoothPosition();

        MoveCamera(ref mCamera, Global.cameraFocusPointTransform, cameraRotationInit, _cameraDistance);
        Shader.SetGlobalVector("_CameraOffset", new Vector4(cameraOffset.x, cameraOffset.y, 0f, 0f));
        cameraOffset = PixelSnap(ref mCamera);
        SetCameraNearClippingPlane();

        // Create rays for bottom corners of the camera
        Ray botRightRay = Camera.main.ViewportPointToRay(new Vector3(1, 0, 0));
        Ray botLeftRay = Camera.main.ViewportPointToRay(new Vector3(0, 0, 0));
        
        // Move camera after main camera
        planarReflectionManager.ConstructMatrix4X4Ortho(botRightRay, botLeftRay, 2f * mCamera.orthographicSize, transform.rotation * Quaternion.Euler(-60f, 0f, 0f));
        planarReflectionManager.SetCameraNearClippingPlane();

        dayNightCycle.UpdatePos();
    }

    private bool SeesUnloaded()
    {
#if UNITY_EDITOR
        if (GameObject.Find("DEBUG"))
        {
            return false;
        }
#endif
        Plane plane = new Plane(Vector3.up, -Water.waterLevel);

        for (float x = -0.25f; x < 2f; x += 1.5f)
        {
            for (float y = -0.25f; y < 2f; y += 1.5f)
            {
                Ray cameraCornerRay = Camera.main.ViewportPointToRay(new Vector3(x, y, 0));

                float distance;
                plane.Raycast(cameraCornerRay, out distance);
                Vector3 planePoint = cameraCornerRay.GetPoint(distance);
                Chunk chunk = WorldGenerationManager.ReturnNearestChunk(planePoint);
                
                if (chunk == null || !chunk.isLoaded)
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
        rt = RenderTexture.GetTemporary((int) resolutionExtended.x, (int) resolutionExtended.y, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default, 1);
        mCamera.targetTexture = rt;
    }

    /// <summary>
    /// When we render account for none integer offsets using blits to get smooth camera movement. And scaled to right resolution in pixels 384x216 px.
    /// </summary>
    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        src.filterMode = FilterMode.Point;

        Vector2 cameraScale = new Vector2(resolution.x / resolutionExtended.x, resolution.y / resolutionExtended.y);

        Graphics.Blit(src, dest, cameraScale, cameraOffset);
        //Graphics.Blit(src, dest, cameraScale, Vector2.zero);

        RenderTexture.ReleaseTemporary(rt);
    }

    /// <summary>
    /// After render clear camera render texture.
    /// </summary>
    private void OnPostRender()
    {
        mCamera.targetTexture = null;
        RenderTexture.active = null;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // Construct plane representing the water level
        Plane plane = new Plane(Vector3.up, -Water.waterLevel);

        // Create rays for bottom corners of the camera
        Ray botRightRay = Camera.main.ViewportPointToRay(new Vector3(1, 0, 0));
        Ray botLeftRay = Camera.main.ViewportPointToRay(new Vector3(0, 0, 0));

        // Assign floats of ray distance for bottom corners
        float botRightDistance;
        float botLeftDistance;

        // Raycast to plain
        plane.Raycast(botRightRay, out botRightDistance);
        plane.Raycast(botLeftRay, out botLeftDistance);

        // Get each position of all four camera corners where they hit the water plain
        Vector3 topRightPosition = botRightRay.GetPoint(botRightDistance);
        Vector3 topLeftPosition = botLeftRay.GetPoint(botLeftDistance);

        // Draw red raycasts twords those points
        Gizmos.color = Color.red;
        Gizmos.DrawRay(botRightRay.origin, botRightRay.direction * botRightDistance);
        Gizmos.DrawRay(botLeftRay.origin, botLeftRay.direction * botLeftDistance);

        // Draw a yellow sphere at the hit location
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(topRightPosition, 1);
        Gizmos.DrawSphere(topLeftPosition, 1);

        // Get height of orthographic camera
        float cameraHeight = 2f * mCamera.orthographicSize;

        // Get sidebar of new camera
        Vector3 cameraSidebarDirection = transform.rotation * Quaternion.Euler(-60f, 0f, 0f) * Vector3.down;
        Vector3 cameraSidebar = cameraSidebarDirection * cameraHeight;

        // Continue drawing magenta raycast that represent the bottom screen edges
        Gizmos.color = Color.magenta;
        Gizmos.DrawRay(topRightPosition, cameraSidebar);
        Gizmos.DrawRay(topLeftPosition, cameraSidebar);

        // Create the vectors representing each corner of our new worldToCameraMatrix.
        Vector3 botRightPosition = topRightPosition + cameraSidebar;
        Vector3 botLeftPosition = topLeftPosition + cameraSidebar;

        // Draw 
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(botRightPosition, 1);
        Gizmos.DrawSphere(botLeftPosition, 1);

        // Yellow points are now representing the top right and top left corners of our new camera whilst the green points represent bottom right and bottom left corners.
    }
#endif
}
