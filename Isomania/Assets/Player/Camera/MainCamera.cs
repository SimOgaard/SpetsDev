using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCamera : PixelPerfect
{
    #region position
    /// <summary>
    /// The render texture that we render the camera to before blitting to screen to get a larger render of game for "out of bounds" shader rendering
    /// </summary>
    private RenderTexture mainCameraRenderTexture;

    /// <summary>
    /// The main game camera
    /// </summary>
    public static Camera mCamera;

    /// <summary>
    /// The offset of render texture uv coordinates when blitting to screen
    /// </summary>
    public static Vector2 pixelOffset;

    /// <summary>
    /// The distance that shadows are still rendered at
    /// </summary>
    [SerializeField] private float shadowDistance = 300f;

    /// <summary>
    /// Sets camera near clipping plane to be tangent to world 2D plane.
    /// Objects rendered close to camera will be cut vertically.
    /// </summary>
    private void SetCameraNearClippingPlane()
    {
        // get direction of camera without y, normalized
        Vector3 direction = mCamera.transform.forward;
        direction.y = 0f;
        direction = direction.normalized;

        // create a clipplane in world space at camera position with normal as direction
        Vector4 clipPlaneWorldSpace = new Vector4(direction.x, direction.y, direction.z, Vector3.Dot(mCamera.transform.position, -direction));
        // translate it to camera clip space
        Vector4 clipPlaneCameraSpace = Matrix4x4.Transpose(mCamera.cameraToWorldMatrix) * clipPlaneWorldSpace;
        // set it as projection matrix
        mCamera.projectionMatrix = mCamera.CalculateObliqueMatrix(clipPlaneCameraSpace);
    }

    /// <summary>
    /// Returns point where ray from camera at screen cords x and y hits y = 0
    /// </summary>
    public static Vector3 CameraRayHitPlane(float x = 0.5f, float y = 0.5f)
    {
        Plane plane = new Plane(Vector3.up, 0f);
        Ray cameraRay = MainCamera.mCamera.ViewportPointToRay(new Vector3(x, y, 0));

        float distance;
        plane.Raycast(cameraRay, out distance);
        return cameraRay.GetPoint(distance);
    }

    /// <summary>
    /// Returns the chunk the camera ray hit
    /// </summary>
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

    protected override void Awake()
    {
        base.Awake();

        // get camera
        mCamera = GetComponent<Camera>();

        // set depth texture mode
        mCamera.depthTextureMode = DepthTextureMode.Depth;
        QualitySettings.shadowDistance = shadowDistance;

        // deligate this UpdateRender to updateRenders
        PixelPerfect.updateRenders += UpdateRender;
    }

    private void LateUpdate()
    {
        // Check if camera sees an unloaded chunk
        if (GameTime.isPaused)
        {
            GameTime.isPaused = SeesUnloaded();
            return;
        }

        // Snap camera position to grid
        pixelOffset = PixelSnap(ref mCamera);
        // Set offset as global shader vector
        Shader.SetGlobalVector("_PixelOffset", new Vector4(pixelOffset.x, pixelOffset.y, 0f, 0f));
        
        /*
        GameObject lol = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        lol.transform.position = MousePoint.MousePositionWorld();
        lol.GetComponent<Collider>().enabled = false;
        */

        // Set near clip plane to be tangentant to ground
        SetCameraNearClippingPlane();

        // update all main camera matrices on shader side
        Shader.SetGlobalMatrix("MAIN_CAMERA_UNITY_MATRIX_I_VP", mCamera.MATRIX_I_VP());
        Shader.SetGlobalMatrix("MAIN_CAMERA_UNITY_MATRIX_VP", mCamera.MATRIX_VP());

        // do we after moving see any unloaded chunks?
        GameTime.isPaused = SeesUnloaded();
    }
    #endregion

    #region render
    /// <summary>
    /// Dispose of render texture
    /// </summary>
    private void OnDestroy()
    {
        if (mainCameraRenderTexture != null)
        {
            RenderTexture.active = null;
            mCamera.targetTexture = null;
            DestroyImmediate(mainCameraRenderTexture);
        }
    }

    /// <summary>
    /// Deligate to run after new game resolution (frees and creates new render texture)
    /// </summary>
    private void UpdateRender()
    {
        Debug.Log("Update render main");

        OnDestroy();
        mainCameraRenderTexture = CreateRenderTexture(/*screenWidth, screenHeight*/);
        mCamera.aspect = (float)renderWidthExtended / (float)renderHeightExtended;
        mCamera.orthographicSize = ((float)renderHeightExtended / (pixelsPerUnit * 2f));
    }

    /// <summary>
    /// Before culling the environment:
    /// Overwrite global UNITY_MATRIX_I_VP shader matrix to represent this camera.
    /// </summary>
    private void OnPreCull()
    {
        Shader.SetGlobalMatrix("UNITY_MATRIX_I_VP", mCamera.MATRIX_I_VP());
    }

    /// <summary>
    /// Before render set camera render texture to temporary low res render texture. Bigger than actuall resolution to account for border pixel stretch.
    /// </summary>
    private void OnPreRender()
    {
        mCamera.targetTexture = mainCameraRenderTexture;
    }

    /// <summary>
    /// After render clear camera render texture.
    /// </summary>
    private void OnPostRender()
    {
        mCamera.targetTexture = null;
        RenderTexture.active = null;
    }

    /// <summary>
    /// When we render account for none integer offsets using blits to get smooth camera movement. And scaled to right resolution in pixels 384x216 px.
    /// </summary>
    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        src.filterMode = FilterMode.Point;
        //Graphics.Blit(src, dest, new Vector2(screenScaleWidth, screenScaleHeight), pixelOffset);
        //Graphics.Blit(src, dest, Vector2.one, Vector3.zero);
        Graphics.Blit(src, dest);
    }
    #endregion

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            return;
        }

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

        // Create rays for top corners of the camera
        Ray topRightRay = MainCamera.mCamera.ViewportPointToRay(new Vector3(1, 1, 0));
        Ray topLeftRay = MainCamera.mCamera.ViewportPointToRay(new Vector3(0, 1, 0));

        Vector3 botLeftPixelOrigin = Vector3.Lerp(botLeftRay.origin, topLeftRay.origin, 0.5f / renderHeightExtended);
        Vector3 botRightPixelOrigin = Vector3.Lerp(botRightRay.origin, topRightRay.origin, 0.5f / renderHeightExtended);

        for (int i = 0; i < renderWidthExtended; i++)
        {
            Vector3 pixelRayOrigin = Vector3.Lerp(botLeftPixelOrigin, botRightPixelOrigin, (float)i / renderWidthExtended + 0.5f / renderWidthExtended);

            Gizmos.DrawRay(pixelRayOrigin, transform.forward * 300f);
        }
    }
#endif
}
