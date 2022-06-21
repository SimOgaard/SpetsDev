using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    /// <summary>
    /// Calculates the best pixelSize values for given resolution.
    /// </summary>
    /// <param name="resolution"></param>
    /// <returns>Dictionary<pixelSize, divider>();</returns>
    private static Dictionary<int, int> Aspects(int resolution, bool negativeRemainder = false)
    {
        Dictionary<int, int> aspects = new Dictionary<int, int>();

        for (int i = 1; i <= resolution; i++)
        {
            float pixelDensity = (float)resolution / (float)i;
            int pixelSize = (negativeRemainder ? Mathf.RoundToInt(pixelDensity) : Mathf.FloorToInt(pixelDensity));
            int remainder = Mathf.Abs(resolution - pixelSize * i);

            if (aspects.ContainsKey(pixelSize))
            {
                int oldRemainder = Mathf.Abs(resolution - pixelSize * aspects[pixelSize]);
                if (remainder < oldRemainder)
                {
                    aspects[pixelSize] = i;
                }
            }
            else
            {
                aspects[pixelSize] = i;
            }
        }

        return aspects;
    }

    /// <summary>
    /// Returns dictionary of all capable pixelsize in aspects
    /// </summary>
    /// <param name="resolution"></param>
    /// <returns>Dictionary<widthDivider, heightDivider>();</returns>
    private static Dictionary<int, int> GetCapable(Dictionary<int, int> aspectsWidth, Dictionary<int, int> aspectsHeight)
    {
        Dictionary<int, int> finalAspects = new Dictionary<int, int>();

        foreach (KeyValuePair<int, int> aspect in aspectsWidth)
        {
            if (aspectsHeight.ContainsKey(aspect.Key))
            {
                finalAspects.Add(aspectsWidth[aspect.Key], aspectsHeight[aspect.Key]);
            }
        }

        return finalAspects;
    }

    /// <summary>
    /// The screen or window resolution width of game
    /// </summary>
    public static int screenWidth = 1;
    /// <summary>
    /// The screen or window resolution height of game
    /// </summary>
    public static int screenHeight = 1;

    /// <summary>
    /// Game resolution width we are striving for
    /// </summary>
    public static int targetWidth = 512;
    /// <summary>
    /// Game resolution height we are striving for
    /// </summary>
    public static int targetHeight = 288;
    /// <summary>
    /// Amount of pixels in targetResolution (targetWidth * targetHeight)
    /// </summary>
    public static int targetPixelDensity { get { return targetWidth * targetHeight; } }

    /// <summary>
    /// The calculated game resolution (width) that the player will see
    /// </summary>
    public static int renderWidth = 1;
    /// <summary>
    /// The calculated game resolution (height) that the player will see
    /// </summary>
    public static int renderHeight = 1;
    /// <summary>
    /// The resolution (width) of the render textures that all cameras render to
    /// </summary>
    public static int renderWidthExtended = 1;
    /// <summary>
    /// The resolution (height) of the render textures that all cameras render to
    /// </summary>
    public static int renderHeightExtended = 1;

    public static int remainderWidth;
    public static int remainderHeight;

    public static float cameraScaleWidth { get { return ((float)renderWidth / (float)renderWidthExtended) /** (1f - remainderWidth / screenWidth)*/; } }
    public static float cameraScaleHeight { get { return ((float)renderHeight / (float)renderHeightExtended) /** (1f - remainderHeight / screenHeight)*/; } }
    public static float cameraScaleWidthOffset { get { return (1f - cameraScaleWidth) * 0.5f; } }
    public static float cameraScaleHeightOffset { get { return (1f - cameraScaleHeight) * 0.5f; } }

    public static float pixelsPerUnit = 5f;
    public static float unitsPerPixelWorld { get { return 1f / pixelsPerUnit; } }
    public static float unitsPerPixelCamera { get { return ((float)renderHeight / pixelsPerUnit) / (float)renderHeightExtended; } }

    public void SetResolution()
    {
        if (!CalculateResolution())
            return;

        if (planarReflectionManager != null)
            planarReflectionManager.UpdateRenderTexture();
        if (dayNightCycle != null)
            dayNightCycle.UpdateRenderTexture();
        if (cameraNormal != null)
            cameraNormal.UpdateRenderTexture();

        SetRenderTextureAspect(ref nCamera);
        SetRenderTextureAspect(ref rCamera);
    }

    /// <summary>
    /// Initilizes game resolution from given screen
    /// </summary>
    public static bool CalculateResolution()
    {
        // if the resolution has changed
        if (screenWidth == Screen.width && screenHeight == Screen.height)
            return false;

        // remember current game render resolution
        screenWidth = Screen.width;
        screenHeight = Screen.height;

        bool negativeRemainders = false;
        bool keepAspect = false; // will require black bars on some monitors

        Dictionary<int, int> aspectsWidth = Aspects(screenWidth, negativeRemainders);
        Dictionary<int, int> aspectsHeight = Aspects(screenHeight, negativeRemainders);

        // gets a dictionary containing all possible width, heigth divisions
        Dictionary<int, int> finalAspects = GetCapable(aspectsWidth, aspectsHeight);

        // sort the dictionary to values closest to targetPixelDensity;
        KeyValuePair<int, int>[] sortedAspects = finalAspects.OrderBy(x => Mathf.Abs(x.Key * x.Value - targetPixelDensity)).ToArray();

        KeyValuePair<int, int> bestAspect = sortedAspects.First();

        renderWidth = bestAspect.Key;
        renderHeight = bestAspect.Value;

        renderWidthExtended = renderWidth;
        renderHeightExtended = renderHeight;

        float sizeWidth = (float)screenWidth / (float)renderWidth;
        float sizeHeight = (float)screenHeight / (float)renderHeight;

        int pixelSizeWidth = Mathf.RoundToInt(sizeWidth);
        int pixelSizeHeight = Mathf.RoundToInt(sizeHeight);

        remainderWidth = screenWidth - renderWidth * pixelSizeWidth;
        remainderHeight = screenHeight - renderHeight * pixelSizeHeight;

        Debug.Log(
            $"Given a screen resolution of {screenWidth}x{screenHeight} with a target resolution of {targetWidth}x{targetHeight}.\n" +
            $"Found resolution {bestAspect.Key}x{bestAspect.Value} by making each rendered pixel represent {pixelSizeWidth}x{pixelSizeHeight} pixels on screen.\n" +
            $"With {remainderWidth}x{remainderHeight} screen pixels as remainders"
        );

        return true;
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

    private NormalsReplacementShader cameraNormal;

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
        float xDivider = pixelsPerUnit / renderWidthExtended * zoom;
        float yDivider = pixelsPerUnit / renderHeightExtended * zoom;
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

        if (!Application.isPlaying)
        {
            return;
        }
#endif

        CalculateResolution();

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

        planarReflectionManager = GameObject.FindObjectOfType<PlanarReflectionManager>();
        dayNightCycle = GameObject.FindObjectOfType<DayNightCycle>();
        cameraNormal = GameObject.FindObjectOfType<NormalsReplacementShader>();

        SetResolution();
    }

    private void UpdateZoomValues(float scrollOffset = 0f)
    {
        float scroll = PlayerInput.mouseScrollValue - scrollOffset;
        if (scroll == 0f)
        {
            return;
        }
        /*
        zoom += scroll;
        if (sunCloudShadows != null)
        {
            sunCloudShadows.UpdateLightProperties(zoom);
        }
        if (moonCloudShadows != null)
        {
            moonCloudShadows.UpdateLightProperties(zoom);
        }
        */
        _cameraDistance = cameraDistance / zoom;
        _cameraFarClippingPlane = cameraFarClippingPlane / zoom;
        QualitySettings.shadowDistance = shadowDistance / zoom;
    }

    private void ZoomCamera(ref Camera camera, float farClipPlaneFactor = 1f)
    {
        camera.orthographicSize = (renderHeightExtended / (pixelsPerUnit * 2f)) / zoom;
        camera.farClipPlane = _cameraFarClippingPlane * farClipPlaneFactor;
    }

    private void Update()
    {
        SetResolution();
        UpdateZoomValues(Mathf.NegativeInfinity);

        if (GameTime.isPaused)
        {
            return;
        }
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
        Ray botRightRay = MainCamera.mCamera.ViewportPointToRay(new Vector3(1, 0, 0));
        Ray botLeftRay = MainCamera.mCamera.ViewportPointToRay(new Vector3(0, 0, 0));
        
        // Move camera after main camera
        planarReflectionManager.ConstructMatrix4X4Ortho(botRightRay, botLeftRay, 2f * mCamera.orthographicSize, transform.rotation * Quaternion.Euler(-60f, 0f, 0f));
        planarReflectionManager.SetCameraNearClippingPlane();

#if UNITY_EDITOR
        if (dayNightCycle == null)
        {
            return;
        }
#endif
        //dayNightCycle.UpdatePos();
    }

    public static Vector3 CameraRayHitPlane(float x = 0.5f, float y = 0.5f)
    {
        Plane plane = new Plane(Vector3.up, 0f);
        Ray cameraRay = MainCamera.mCamera.ViewportPointToRay(new Vector3(x, y, 0));

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

    private static void SetScreenAspect(ref Camera camera)
    {
        if (camera == null)
            return;

        camera.aspect = (float)screenWidth / (float)screenHeight;
        camera.orthographicSize = ((float)screenHeight / (pixelsPerUnit * 2f));
    }

    private static void SetRenderTextureAspect(ref Camera camera)
    {
        if (camera == null)
            return;

        camera.aspect = (float)renderWidthExtended / (float)renderHeightExtended;
        camera.orthographicSize = ((float)renderHeightExtended / (pixelsPerUnit * 2f));
    }

    private void OnPreCull()
    {
        //SetRenderTextureAspect(ref mCamera);
    }

    /// <summary>
    /// Before render set camera render texture to temporary low res render texture. Bigger than actuall resolution to account for border pixel stretch.
    /// </summary>
    private void OnPreRender()
    {
        rt = RenderTexture.GetTemporary(renderWidthExtended, renderHeightExtended, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default, 1);
        mCamera.targetTexture = rt;
    }

    /// <summary>
    /// After render clear camera render texture.
    /// </summary>
    private void OnPostRender()
    {
        mCamera.targetTexture = null;
        RenderTexture.active = null;

        //SetTrueAspect(ref mCamera);
    }

    /// <summary>
    /// When we render account for none integer offsets using blits to get smooth camera movement. And scaled to right resolution in pixels 384x216 px.
    /// </summary>
    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        src.filterMode = FilterMode.Point;

        Graphics.Blit(src, dest, new Vector2(cameraScaleWidth, cameraScaleHeight), cameraOffset);

        RenderTexture.ReleaseTemporary(rt);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // Construct plane representing the water level
        Plane plane = new Plane(Vector3.up, -Water.waterLevel);

        // Create rays for bottom corners of the camera
        Ray botRightRay = MainCamera.mCamera.ViewportPointToRay(new Vector3(1, 0, 0));
        Ray botLeftRay = MainCamera.mCamera.ViewportPointToRay(new Vector3(0, 0, 0));

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
