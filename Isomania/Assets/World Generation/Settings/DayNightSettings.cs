using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// How the day night cylcle should be defined
/// </summary>
[CreateAssetMenu(menuName = "WorldGeneration/Climate/DayNight", order = 10)]
[System.Serializable]
public class DayNightSettings : ScriptableObject
{
    [Header("Ambience")]
    public Color ambientDay;
    public Color ambientNight;
    public AnimationCurve ambientLerp;

    public CloudSettings cloudSettingsDay;
    public CloudSettings cloudSettingsNight;

    [Header("Rotation")]
    public Vector3 rotationSpeedDay;
    public Vector3 rotationSpeedNight;
    public AnimationCurve rotationSpeedLerp;

    public Vector3 rotationSnap;

    [Header("Horizon")]
    public float horizonAngleThreshold = 7.5f;
    public float horizonAngleFade = 7.5f;

    /// <summary>
    /// Assigns this setting to daynightcycle game object
    /// </summary>
    [ContextMenu("Update", false, -1000)]
    public void Update()
    {
        Destroy();

        // add horizon values as global shader variables
        Shader.SetGlobalFloat("_HorizonAngleThreshold", horizonAngleThreshold);
        Shader.SetGlobalFloat("_HorizonAngleFade", horizonAngleFade);

        // 123 both should be updated or mby just the one active, ie day or night one
        cloudSettingsDay.Update();
        Global.dayNight.UpdateSettings(this);
    }

    public void Destroy()
    {
        cloudSettingsDay.Destroy();
    }
}
