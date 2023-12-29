using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Pixel perfect camera base class
/// </summary>
[RequireComponent(typeof(Camera))]
public class PixelPerfect : MonoBehaviour
{
    #region resolution
    /// <summary>
    /// The screen or window resolution width of game
    /// </summary>
    public static int screenResolutionWidth = 1;
    /// <summary>
    /// The screen or window resolution height of game
    /// </summary>
    public static int screenResolutionHeight = 1;
    /// <summary>
    /// The screen or window resolution of game
    /// </summary>
    public static Vector2Int screenResolution { get { return new Vector2Int(screenResolutionWidth, screenResolutionHeight); } }

    /// <summary>
    /// The calculated game resolution (width) that the player will see
    /// </summary>
    public static int renderResolutionWidth = 1;
    /// <summary>
    /// The calculated game resolution (height) that the player will see
    /// </summary>
    public static int renderResolutionHeight = 1;
    /// <summary>
    /// The calculated game resolution that the player will see
    /// </summary>
    public static Vector2Int renderResolution { get { return new Vector2Int(renderResolutionWidth, renderResolutionHeight); } }

    /// <summary>
    /// How many screen pixels are a singular game pixel
    /// </summary>
    public static int gamePixelToScreenPixel { get { return Mathf.RoundToInt(((float)screenResolutionWidth) / ((float)renderResolutionWidth)); } }

    /// <summary>
    /// The resolution (width) of the render textures that all cameras render to
    /// </summary>
    public static int renderResolutionWidthExtended { get { return NearestBiggerInt(renderResolutionWidth, diff: 2); } }
    /// <summary>
    /// The resolution (height) of the render textures that all cameras render to
    /// </summary>
    public static int renderResolutionHeightExtended { get { return NearestBiggerInt(renderResolutionHeight, diff: 2); } }
    /// <summary>
    /// The resolution of the render textures that all cameras render to
    /// </summary>
    public static Vector2Int renderResolutionExtended { get { return new Vector2Int(renderResolutionWidthExtended, renderResolutionHeightExtended); } }

    /// <summary>
    /// The resolution (width) of the screen if the whole render texture would be rendered
    /// </summary>
    public static int screenResolutionWidthExtended { get { return renderResolutionWidthExtended * gamePixelToScreenPixel; } }
    /// <summary>
    /// The resolution (height) of the screen if the whole render texture would be rendered
    /// </summary>
    public static int screenResolutionHeightExtended { get { return renderResolutionHeightExtended * gamePixelToScreenPixel; } }
    /// <summary>
    /// The resolution of the screen if the whole render texture would be rendered
    /// </summary>
    public static Vector2Int screenResolutionExtended { get { return new Vector2Int(screenResolutionWidthExtended, screenResolutionHeightExtended); } }

    /// <summary>
    /// The aspect of the screen/window
    /// </summary>
    public static float screenAspect { get { return (float)screenResolutionWidth / (float)screenResolutionHeight; } }
    /// <summary>
    /// The aspect of the game camera
    /// </summary>
    public static float renderAspect { get { return (float)renderResolutionWidthExtended / (float)renderResolutionHeightExtended; } }

    /// <summary>
    /// The orthographic size of the game camera
    /// </summary>
    public static float renderOrthographicSize { get { return ((float)renderResolutionHeightExtended) / (pixelsPerUnit * 2f); ; } }

    /// <summary>
    /// The amount of render texture pixels that are not included in renderResolutionWidth
    /// </summary>
    public static int renderResolutionWidthRemainder { get { return renderResolutionWidthExtended - renderResolutionWidth; } }
    /// <summary>
    /// The amount of render texture pixels that are not included in renderResolutionHeight
    /// </summary>
    public static int renderResolutionHeightRemainder { get { return renderResolutionHeightExtended - renderResolutionHeight; } }
    /// <summary>
    /// The amount of render texture pixels that are not included in renderResolution
    /// </summary>
    public static Vector2Int renderResolutionRemainder { get { return new Vector2Int(renderResolutionWidthRemainder, renderResolutionHeightRemainder); } }

    /// <summary>
    /// The amount of screen pixels that are not included in screenResolutionWidth
    /// </summary>
    public static int screenResolutionWidthRemainder { get { return screenResolutionWidthExtended - screenResolutionWidth; } }
    /// <summary>
    /// The amount of screen pixels that are not included in screenResolutionHeight
    /// </summary>
    public static int screenResolutionHeightRemainder { get { return screenResolutionHeightExtended - screenResolutionHeight; } }
    /// <summary>
    /// The amount of screen pixels that are not included in screenResolution
    /// </summary>
    public static Vector2Int screenResolutionRemainder { get { return new Vector2Int(screenResolutionWidthRemainder, screenResolutionHeightRemainder); } }

    /// <summary>
    /// Game resolution width we are striving for
    /// </summary>
    public static int targetResolutionWidth { get { return 592; } }
    /// <summary>
    /// Game resolution height we are striving for
    /// </summary>
    public static int targetResolutionHeight { get { return 333; } }
    /// <summary>
    /// Game resolution we are striving for
    /// </summary>
    public static Vector2Int targetResolution { get { return new Vector2Int(targetResolutionWidth, targetResolutionHeight); } }

    /// <summary>
    /// The aspect of the game resolution wer are striving for
    /// </summary>
    public static float targetAspect { get { return 16f / 9f; } }
    /// <summary>
    /// Amount of pixels in targetResolution (targetWidth * targetHeight)
    /// </summary>
    public static int targetPixelDensity { get { return targetResolutionWidth * targetResolutionHeight; } }
    /// <summary>
    /// Amount of pixels in targetResolution without aspect (targetWidth * targetHeight) / targetAspect
    /// </summary>
    public static float targetPixelDensityNoAspect { get { return ((float) targetPixelDensity) / targetAspect; } }

    /// <summary>
    /// The scale width value required for blit when rendering the main camera render texture to screen, is also the scale widht value required for calculating mouse pixel position
    /// </summary>
    public static float scaleWidth { get { return ((float)screenResolutionWidth) / ((float)screenResolutionWidthExtended); } }
    /// <summary>
    /// The scale height value required for blit when rendering the main camera render texture to screen, is also the scale height value required for calculating mouse pixel position
    /// </summary>
    public static float scaleHeight { get { return ((float)screenResolutionHeight) / ((float)screenResolutionHeightExtended); } }
    /// <summary>
    /// The scale value required for blit when rendering the main camera render texture to screen, is also the scale value required for calculating mouse pixel position
    /// </summary>
    public static Vector2 scale { get { return new Vector2(scaleWidth, scaleHeight); } }

    /// <summary>
    /// The scale width value required for blit when rendering the main camera render texture to screen, is also the scale widht value required for calculating mouse pixel position
    /// </summary>
    protected static float _offsetWidth = 0.0f;
    public static float offsetWidth
    {
        get { return ((1f - scaleWidth) * 0.5f) - _offsetWidth; }
        set { _offsetWidth = value; }
    }
    /// <summary>
    /// The scale height value required for blit when rendering the main camera render texture to screen, is also the scale height value required for calculating mouse pixel position
    /// </summary>
    protected static float _offsetHeight = 0.0f;
    public static float offsetHeight
    {
        get { return ((1f - scaleHeight) * 0.5f) - _offsetHeight; }
        set { _offsetHeight = value; }
    }
    /// <summary>
    /// The scale value required for blit when rendering the main camera render texture to screen, is also the scale value required for calculating mouse pixel position
    /// </summary>
    public static Vector2 offset { get { return new Vector2(offsetWidth, offsetHeight); } }

    /// <summary>
    /// The game ppu value
    /// </summary>
    public static float pixelsPerUnit { get { return 5f; } }
    /// <summary>
    /// The game ppu value translated to world
    /// </summary>
    public static float unitsPerPixelWorld { get { return 1f / pixelsPerUnit; } }

    /// <summary>
    /// The size of camera width, OBS! IS HALF
    /// </summary>
    public static float cameraMaxRadiusWidth { get { return unitsPerPixelWorld * ((float)renderResolutionWidthExtended) * 0.5f; } }

    /// <summary>
    /// The size of camera width, OBS! IS HALF
    /// </summary>
    public static float cameraMaxRadiusHeight { get { return unitsPerPixelWorld * ((float)renderResolutionHeightExtended) * 0.5f; } }

    /// <summary>
    /// The max radius of camera
    /// </summary>
    public static float cameraMaxRadius { get { return Mathf.Sqrt(Mathf.Pow(cameraMaxRadiusWidth, 2.0f) + Mathf.Pow(cameraMaxRadiusHeight, 2.0f)); } }

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

    public static int NearestBiggerInt(int n, int divisor = 8, int diff = 1)
    {
        int k = divisor - 1;
        n += k * diff;
        return (n & ~k);
    }

    /// <summary>
    /// Initilizes game resolution from given screen
    /// </summary>
    public static bool CalculateResolution()
    {
        // if the resolution has changed
        if (screenResolutionWidth == Screen.width && screenResolutionHeight == Screen.height)
            return false;

        // remember current game render resolution
        screenResolutionWidth = Screen.width;
        screenResolutionHeight = Screen.height;

        bool negativeRemainders = false;
        // bool keepAspect = false; // will require black bars on some monitors

        Dictionary<int, int> aspectsWidth = Aspects(screenResolutionWidth, negativeRemainders);
        Dictionary<int, int> aspectsHeight = Aspects(screenResolutionHeight, negativeRemainders);

        // gets a dictionary containing all possible width, heigth divisions
        Dictionary<int, int> finalAspects = GetCapable(aspectsWidth, aspectsHeight);

        float aspect0 = (float) Screen.width / (float) Screen.height;
        float aspect1 = (float) Screen.height / (float) Screen.width;
        float highestAspect = aspect0 > aspect1 ? aspect0 : aspect1;

        // sort the dictionary to values closest to targetPixelDensity;
        KeyValuePair<int, int>[] sortedAspectsByPixelDensity = finalAspects.OrderBy(x => Mathf.Abs(((x.Key * x.Value) / highestAspect) - targetPixelDensityNoAspect)).ToArray();

        // get the best suited aspect
        KeyValuePair<int, int> bestAspect = sortedAspectsByPixelDensity.First();

        // set our best attempt of a target resolution
        renderResolutionWidth = bestAspect.Key;
        renderResolutionHeight = bestAspect.Value;
        
        // Set render scale as global shader vector
        Shader.SetGlobalVector("_PixelScale", scale);

        Debug.Log(
            $"Given a screen resolution of {screenResolutionWidth}x{screenResolutionHeight} with a target resolution of {targetResolutionWidth}x{targetResolutionHeight}.\n" +
            $"Found resolution {renderResolutionWidth}x{renderResolutionHeight} by making each rendered pixel represent {gamePixelToScreenPixel}x{gamePixelToScreenPixel} pixels on screen.\n" +
            $"With {screenResolutionWidthRemainder}x{screenResolutionHeightRemainder} screen pixels as remainders.\n" +
            $"This means that we need to scale the full renderTexture by {scaleWidth}x{scaleHeight} and offset by {offsetWidth}x{offsetHeight} before rendering to screen."
        );

        return true;
    }

    /// <summary>
    /// Deligates of functions that updates any render textures or camera aspects etc, after a new game resolution
    /// </summary>
    public delegate void UpdateRenders();
    public static UpdateRenders updateRenders;

    /// <summary>
    /// Applies calculated game resolution for current screen resolution
    /// </summary>
    public void SetResolution(bool ignoreLastResolution = false)
    {
        if (!CalculateResolution() && !ignoreLastResolution)
            return;

        updateRenders();

        Shader.SetGlobalVector("renderResolutionExtended", (Vector2)renderResolutionExtended);
        Shader.SetGlobalVector("renderResolution", (Vector2)renderResolution);
    }

    /// <summary>
    /// Creates a render texture that is applicaple to game resolution
    /// </summary>
    public static RenderTexture CreateRenderTexture(int depth = 24)
    {
        RenderTexture rt = new RenderTexture(renderResolutionWidthExtended, renderResolutionHeightExtended, depth, RenderTextureFormat.ARGB32, 1);
        rt.filterMode = FilterMode.Point;
        return rt;
    }

    /// <summary>
    /// Creates a render texture that is applicaple to game resolution
    /// </summary>
    public static RenderTexture CreateRenderTexture(int width, int height, int depth = 24)
    {
        RenderTexture rt = new RenderTexture(width, height, depth, RenderTextureFormat.ARGB32, 1);
        rt.filterMode = FilterMode.Point;
        return rt;
    }
    #endregion

    #region pixel perfect
    /// <summary>
    /// Rounds given Vector3 position to pixel grid.
    /// </summary>
    public static Vector3 RoundToPixel(Vector3 position)
    {
        Vector3 result;
        result.x = Mathf.Round(position.x / unitsPerPixelWorld) * unitsPerPixelWorld;
        result.y = Mathf.Round(position.y / unitsPerPixelWorld) * unitsPerPixelWorld;
        result.z = Mathf.Round(position.z / unitsPerPixelWorld) * unitsPerPixelWorld;

        return result;
    }

    /// <summary>
    /// Snap camera position to pixel grid using Camera.worldToCameraMatrix. 
    /// </summary>
    public static Vector2 PixelSnap(ref Camera camera)
    {
        // camera.transform is now moved for this frame, but it is not snapped to grid, so:
        // remove rotation from camera world position, so that we can round its position
        Vector3 cameraPosition = Quaternion.Inverse(camera.transform.rotation) * camera.transform.position;
        // snap it to grid
        Vector3 roundedCameraPosition = RoundToPixel(cameraPosition);

        // set camera position to snapped position by adding the rotation
        camera.transform.position = camera.transform.rotation * roundedCameraPosition;

        // get offset of rounded and actual camera position
        Vector3 offset = roundedCameraPosition - cameraPosition;
        // transform offset world xy coordinates to pixel perfect coord using ppu
        Vector3 offsetPPU = offset * pixelsPerUnit;
        // transform from ppu to screen pixel offset
        return new Vector2(offsetPPU.x / ((float)renderResolutionWidthExtended), offsetPPU.y / ((float)renderResolutionHeightExtended));
    }
    #endregion

    protected virtual void Awake()
    {
#if UNITY_EDITOR
        if (GameObject.Find("DEBUG"))
        {
            Debug.Log("Debug mode");
        }
#endif

        CalculateResolution();

        Shader.SetGlobalFloat("pixelsPerUnit", pixelsPerUnit);
        Shader.SetGlobalFloat("unitsPerPixelWorld", unitsPerPixelWorld);
        Shader.SetGlobalFloat("yScale", SpriteInitializer.yScale);
    }

    protected virtual void Start()
    {
        SetResolution(true);
    }

    protected virtual void Update()
    {
        SetResolution();
    }
}
