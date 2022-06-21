using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// How the day night cylcle should be defined
/// </summary>
[CreateAssetMenu(menuName = "WorldGeneration/Climate/DayNightCycle", order = 10)]
[System.Serializable]
public class DayNightCycleSettings : ScriptableObject
{
    public Color ambientDay;
    public Color ambientNight;
    public AnimationCurve ambientLerp;

    public Vector3 rotationSpeed;
    public Vector3 rotationSnap;

    /// <summary>
    /// Assigns this setting to daynightcycle game object
    /// </summary>
    public void Update()
    {
        Global.dayNightCycle.UpdateSettings(this);
    }

    public void UpdateRender()
    {
        // calculate smallest neccesary resolution for cookie when light is perpendicular to ground and extended camera should fit given any rotation.
        CloudSettings.shadowRenderTexutreResolution = PixelPerfect.NearestBiggerInt(
            Mathf.CeilToInt(2.0f * Mathf.Sin(45f * Mathf.Deg2Rad) * Mathf.Max(PixelPerfect.renderWidthExtended, PixelPerfect.renderHeightExtended))
        );

        // calculate a cookie size so that when light is perpendicular to ground, each pixel is its own light ray
        CloudSettings.cookieSize = PixelPerfect.unitsPerPixelWorld * CloudSettings.shadowRenderTexutreResolution;
    }
}
