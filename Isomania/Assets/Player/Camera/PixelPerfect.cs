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
    public static int screenWidth = 1;
    /// <summary>
    /// The screen or window resolution height of game
    /// </summary>
    public static int screenHeight = 1;

    /// <summary>
    /// Game resolution width we are striving for
    /// </summary>
    public static int targetWidth = 592;
    /// <summary>
    /// Game resolution height we are striving for
    /// </summary>
    public static int targetHeight = 333;
    /// <summary>
    /// The aspect of the game resolution wer are striving for
    /// </summary>
    public static float targetAspect = 16f / 9f;
    /// <summary>
    /// Amount of pixels in targetResolution (targetWidth * targetHeight)
    /// </summary>
    public static int targetPixelDensity { get { return targetWidth * targetHeight; } }
    /// <summary>
    /// Amount of pixels in targetResolution without aspect (targetWidth * targetHeight) / targetAspect
    /// </summary>
    public static float targetPixelDensityNoAspect { get { return (float) (targetWidth * targetHeight) / targetAspect; } }

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

    /// <summary>
    /// The amount of screen pixels that are not included in renderWidth
    /// </summary>
    public static int remainderWidth = 0;
    /// <summary>
    /// The amount of screen pixels that are not included in renderHeight
    /// </summary>
    public static int remainderHeight = 0;

    /// <summary>
    /// The scale.x value required when calculating mouse pixel position
    /// </summary>
    public static float cameraScaleWidth = 1f;
    /// <summary>
    /// The scale.y value required when calculating mouse pixel position
    /// </summary>
    public static float cameraScaleHeight = 1f;
    /// <summary>
    /// The scale.x value required for blit when rendering the main camera render texture to screen
    /// </summary>
    public static float screenScaleWidth = 1f;
    /// <summary>
    /// The scale.y value required for blit when rendering the main camera render texture to screen
    /// </summary>
    public static float screenScaleHeight = 1f;
    /// <summary>
    /// The offset.x blit value required for blit when rendering the main camera render texture to screen
    /// </summary>
    public static float screenScaleWidthOffset = 1f;
    /// <summary>
    /// The offset.y blit value required for blit when rendering the main camera render texture to screen
    /// </summary>
    public static float screenScaleHeightOffset = 1f;

    /// <summary>
    /// The game ppu value
    /// </summary>
    public static float pixelsPerUnit = 5f;
    /// <summary>
    /// FOR SOME REASON SOME SHADERS NEED THIS
    /// </summary>
    public static float pixelsPerUnit3 { get { return pixelsPerUnit * 3.0f; } }
    /// <summary>
    /// The game ppu value translated to world
    /// </summary>
    public static float unitsPerPixelWorld { get { return 1f / pixelsPerUnit; } }
    /// <summary>
    /// FOR SOME REASON SOME SHADERS NEED THIS
    /// </summary>
    public static float unitsPerPixelWorld3 { get { return 1f / pixelsPerUnit3; } }
    /// <summary>
    /// The game ppu value translated to camera
    /// </summary>
    public static float unitsPerPixelCamera { get { return ((float)renderHeight / pixelsPerUnit) / (float)renderHeightExtended; } }

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

        float aspect0 = (float) Screen.width / (float) Screen.height;
        float aspect1 = (float) Screen.height / (float) Screen.width;
        float highestAspect = aspect0 > aspect1 ? aspect0 : aspect1;

        // sort the dictionary to values closest to targetPixelDensity;
        KeyValuePair<int, int>[] sortedAspectsByPixelDensity = finalAspects.OrderBy(x => Mathf.Abs(((x.Key * x.Value) / highestAspect) - targetPixelDensityNoAspect)).ToArray();

        // get the best suited aspect
        KeyValuePair<int, int> bestAspect = sortedAspectsByPixelDensity.First();

        // set our best attempt of a target resolution
        renderWidth = bestAspect.Key;
        renderHeight = bestAspect.Value;

        // set extended resolution for later render offsets
        renderWidthExtended = NearestBiggerInt(renderWidth, diff: 2);
        renderHeightExtended = NearestBiggerInt(renderHeight, diff: 2);

        // get the size of a game pixel
        float sizeWidth = (float)screenWidth / (float)renderWidth;
        float sizeHeight = (float)screenHeight / (float)renderHeight;

        // get the real screen pixel size of a game pixel
        int pixelSizeWidth = Mathf.RoundToInt(sizeWidth);
        int pixelSizeHeight = Mathf.RoundToInt(sizeHeight);

        // get the remaining pixels on screen that do not take up a whole game pixel
        remainderWidth = screenWidth - renderWidth * pixelSizeWidth;
        remainderHeight = screenHeight - renderHeight * pixelSizeHeight;

        cameraScaleWidth = ((float)renderWidth / (float)renderWidthExtended) + (float)remainderWidth / (float)renderWidthExtended;
        cameraScaleHeight = ((float)renderHeight / (float)renderHeightExtended) + (float)remainderHeight / (float)renderHeightExtended;

        screenScaleWidth = ((float)renderWidth / (float)renderWidthExtended) + ((float)remainderWidth / ((float)renderWidthExtended * ((float)screenWidth / (float)renderWidth)));
        screenScaleHeight = ((float)renderHeight / (float)renderHeightExtended) + ((float)remainderHeight / ((float)renderHeightExtended * ((float)screenHeight / (float)renderHeight)));

        screenScaleWidthOffset = (1f - screenScaleWidth) * 0.5f;
        screenScaleHeightOffset = (1f - screenScaleWidth) * 0.5f;

        Debug.Log(
            $"Given a screen resolution of {screenWidth}x{screenHeight} with a target resolution of {targetWidth}x{targetHeight}.\n" +
            $"Found resolution {bestAspect.Key}x{bestAspect.Value} by making each rendered pixel represent {pixelSizeWidth}x{pixelSizeHeight} pixels on screen.\n" +
            $"With {remainderWidth}x{remainderHeight} screen pixels as remainders"
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

        Shader.SetGlobalVector("renderResolutionExtended", new Vector2(renderWidthExtended, renderHeightExtended));
        Shader.SetGlobalVector("renderResolution", new Vector2(renderWidth, renderHeight));
    }

    /// <summary>
    /// Creates a render texture that is applicaple to game resolution
    /// </summary>
    public static RenderTexture CreateRenderTexture(int depth = 24)
    {
        RenderTexture rt = new RenderTexture(renderWidthExtended, renderHeightExtended, depth, RenderTextureFormat.ARGB32, 1);
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
        // get position without rotation
        Vector3 cameraPosition = Quaternion.Inverse(camera.transform.rotation) * camera.transform.position;
        // snap it to grid
        Vector3 roundedCameraPosition = RoundToPixel(cameraPosition);

        // set camera position to snapped position by adding the rotation
        camera.transform.position = camera.transform.rotation * roundedCameraPosition;

        // get camera position offset from snapped and original position
        Vector3 offset = roundedCameraPosition - cameraPosition;
        // translate offset.xy to render texture uv cordinates.
        float toPositive = (unitsPerPixelCamera * 0.5f);
        float xDivider = pixelsPerUnit / renderWidthExtended;
        float yDivider = pixelsPerUnit / renderHeightExtended;
        Vector2 cameraOffset = new Vector2(
            (-offset.x + toPositive) * xDivider + screenScaleWidthOffset,
            (-offset.y + toPositive) * yDivider + screenScaleHeightOffset
        );
        // return the new blit offset
        return cameraOffset;
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
        Shader.SetGlobalFloat("pixelsPerUnit3", pixelsPerUnit3);
        Shader.SetGlobalFloat("unitsPerPixelWorld", unitsPerPixelWorld);
        Shader.SetGlobalFloat("unitsPerPixelWorld", unitsPerPixelWorld3);
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
