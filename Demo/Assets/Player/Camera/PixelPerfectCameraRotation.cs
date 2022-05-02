using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// This script renders our scene at 384x216 resolution and upscales the result to the screen.
/// At render the camera is snapped to nearest pixel in pixelgrid and the snap offset is corrected for when we blits the render texture to game view.
/// </summary>
[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class PixelPerfectCameraRotation : MonoBehaviour
{
    // Aspects
    public float pixelScale = 1f;
    //public const int targetPixelCount = (16 * 9) * 32; // https://pacoup.com/2011/06/12/list-of-true-169-resolutions/

    public static int screenResolutionWidth = -1;
    public static int screenResolutionHeight = -1;
    public static float dpi;
    public static float aspect;

    public static int width = 512;
    public static int height = 288;
    public static int widthExtended = width + 16;
    public static int heightExtended = height + 9;
    public static float cameraScaleWidth = (float)width / widthExtended;
    public static float cameraScaleHeight = (float)height / heightExtended;
    public static float cameraScaleWidthOffset = (1f - cameraScaleWidth) * 0.5f;
    public static float cameraScaleHeightOffset = (1f - cameraScaleHeight) * 0.5f;

    public static float unitsPerPixelCamera = (height / 5f) / heightExtended;
    public static float unitsPerPixelWorld = 1f / 5f;
    public static float pixelsPerUnit = 5f;

    /// <summary>
    /// Initilizes game resolution from given screen
    /// </summary>
    public void CalculateResolution()
    {
        // if the resolution has changed
        if (screenResolutionWidth == Screen.width && screenResolutionHeight == Screen.height)
            return;

        // tryget dpi otherwise use common dpi value
        if (Screen.dpi != 0)
        {
            dpi = Screen.dpi;
        }
        else
        {
            dpi = 81f;
            Debug.LogWarning($"We were not able to fetch dpi of screen, using default value: {dpi}");
        }

        void UpdateCamera(ref Camera camera, float aspect)
        {
            if (camera != null)
                camera.aspect = aspect;
        }

        // remember current game render resolution
        screenResolutionWidth = Screen.width;
        screenResolutionHeight = Screen.height;

        // calculate best target resolution from given dpi and screen resolution
        

        width = Mathf.RoundToInt(screenResolutionWidth / pixelScale);
        height = Mathf.RoundToInt(screenResolutionHeight / pixelScale);
        // extend resolution so we can move camera over subpixels in all 4 directions
        widthExtended = width + 2;
        heightExtended = height + 2;
        // add +1 on the sides that need a fraction of a pixel to be rendered

        // get aspect ratio of extended screen resolution
        aspect = (float)widthExtended / (float)heightExtended;


        // we need the scaling to render width * height and not widthExtended * heightExtended

        // we need the constant offset for the +1 pixel so that the fractional pixel is on the right side of screen


        cameraScaleWidth = (float)width / widthExtended;
        cameraScaleHeight = (float)height / heightExtended;
        cameraScaleWidthOffset = (1f - cameraScaleWidth) * 0.5f;
        cameraScaleHeightOffset = (1f - cameraScaleHeight) * 0.5f;
        unitsPerPixelCamera = (height / 5f) / heightExtended;

        if (planarReflectionManager != null)
            planarReflectionManager.UpdateRenderTexture();
        if (dayNightCycle != null)
            dayNightCycle.UpdateRenderTexture();

        UpdateCamera(ref mCamera, aspect);
        UpdateCamera(ref nCamera, aspect);
        UpdateCamera(ref rCamera, aspect);

        //Debug.Log($"{Screen.width} : {Screen.height} = {pixelDensity}");
    }

    [SerializeField] private Vector3 cameraRotationInit = new Vector3(30f, 0f, 0f);
    [SerializeField] private float cameraDistance = 100f;
    [SerializeField] private float cameraFarClippingPlane = 300f;
    [SerializeField] private float shadowDistance = 300f;
    private float _cameraFarClippingPlane;
    private float _cameraDistance;

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
        float xDivider = pixelsPerUnit / width * zoom;
        float yDivider = pixelsPerUnit / height * zoom;
        Vector2 cameraOffset = new Vector2((-offset.x + toPositive) * xDivider + cameraScaleWidthOffset, (-offset.y + toPositive) * yDivider + cameraScaleHeightOffset);

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

        planarReflectionManager = GameObjectFunctions.GetComponentFromGameObjectName<PlanarReflectionManager>("ReflectionCamera");
        //dayNightCycle = GameObjectFunctions.GetComponentFromGameObjectName<DayNightCycle>("DayNightCycle");

        CalculateResolution();
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
        camera.orthographicSize = (heightExtended / (5f * 2f)) / zoom;
        camera.farClipPlane = _cameraFarClippingPlane * farClipPlaneFactor;
    }

    private void Update()
    {
        if (GameTime.isPaused)
        {
            return;
        }

        CalculateResolution();
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

#if UNITY_EDITOR
        if (dayNightCycle == null)
        {
            return;
        }
#endif
        dayNightCycle.UpdatePos();
    }

    public static Vector3 CameraRayHitPlane(float x = 0.5f, float y = 0.5f)
    {
        Plane plane = new Plane(Vector3.up, 0f);
        Ray cameraRay = Camera.main.ViewportPointToRay(new Vector3(x, y, 0));

        float distance;
        plane.Raycast(cameraRay, out distance);
        return cameraRay.GetPoint(distance);
    }

    public static Chunk CameraRayHitChunk(float x, float y)
    {
        Vector3 planePoint = CameraRayHitPlane(x, y);
        return WorldGenerationManager.ReturnNearestChunk(planePoint);
    }

    /// <summary>
    /// Returns true if a unloaded chunk is visible in camera plane
    /// </summary>
    private bool SeesUnloaded()
    {
#if UNITY_EDITOR
        if (GameObject.Find("DEBUG"))
        {
            return false;
        }
#endif

        // raycast uniformaly in a grid around camera
        for (float x = -0.25f; x <= 1.25f; x += 0.75f)
        {
            for (float y = -0.25f; y <= 1.25f; y += 0.75f)
            {
                Chunk rayChunk = CameraRayHitChunk(x, y);

                if (rayChunk == null || !rayChunk.isLoaded || !rayChunk.gameObject.activeInHierarchy)
                {
                    if (!rayChunk.isLoading)
                    {
                        StartCoroutine(rayChunk.LoadChunk(GameObject.FindObjectOfType<WorldGenerationManager>().worldGenerationSettings));
                    }
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
        rt = RenderTexture.GetTemporary(widthExtended, heightExtended, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default, 1);
        mCamera.targetTexture = rt;
    }

    /// <summary>
    /// When we render account for none integer offsets using blits to get smooth camera movement. And scaled to right resolution in pixels 384x216 px.
    /// </summary>
    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        src.filterMode = FilterMode.Point;

        Graphics.Blit(src, dest, new Vector2(cameraScaleWidth, cameraScaleHeight), cameraOffset);
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

        // Draw middle ray;
        Gizmos.DrawRay(transform.position, transform.forward * 300f);
    }
#endif
}
