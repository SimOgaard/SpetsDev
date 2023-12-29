using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// How the day night cylcle should be defined
/// </summary>
[CreateAssetMenu(menuName = "WorldGeneration/Climate/DayNight", order = 10)]
[System.Serializable]
public class DayNightSettings : Settings
{
    [Header("Ambient Lightning")]
    public Color ambientLightDay;
    public Color ambientLightNight;
    public AnimationCurve ambientLightLerp;

    [Header("Horizon")]
    public float horizonAngleThresholdLight = 7.5f;
    public float horizonAngleFadeLight = 7.5f;

    [Header("Rotation")]
    public Vector3 rotationStart;

    public Vector3 rotationSpeedDay;
    public Vector3 rotationSpeedNight;
    public AnimationCurve rotationSpeedLerp;

    public Vector3 rotationSnap;

    [Header("Clouds")]
    public CloudSettings cloudsDay;
    public CloudSettings cloudsNight;

    [ContextMenu("Rename", false, 500)]
    public override void Rename()
    {
        base.Rename();
    }

    /// <summary>
    /// Assigns this setting to daynightcycle game object
    /// </summary>
    [ContextMenu("Update", false, -1000)]
    public override void Update()
    {
        Destroy();

        // add horizon values as global shader variables
        Shader.SetGlobalFloat("_HorizonAngleThreshold", horizonAngleThresholdLight);
        Shader.SetGlobalFloat("_HorizonAngleFade", horizonAngleFadeLight);

        // 123 both should be updated or mby just the one active, ie day or night one
        cloudsDay.Update();
        Global.dayNight.UpdateSettings(this);
    }

    public void Destroy()
    {
        cloudsDay.Destroy();
    }
}
