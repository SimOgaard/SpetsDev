using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// How the day night cylcle should be defined
/// </summary>
[CreateAssetMenu()]
[System.Serializable]
public class DayNightCycleSettings : ScriptableObject
{
    [HideInInspector] public float ambient;
    public AnimationCurve ambientLight;

    [HideInInspector] public float darkest;
    public AnimationCurve darkestValue;

    [HideInInspector] public float waterOffset;
    public AnimationCurve waterColOffset;

    public Vector3 rotationSpeed;
    public Vector3 rotationSnap;

    [Header("Sun")]
    public CloudSettings sun;

    [Header("Moon")]
    public CloudSettings moon;

    /// <summary>
    /// Assigns this setting to daynightcycle game object
    /// </summary>
    public void Update()
    {
        Global.dayNightCycle.UpdateSettings(this);

        sun.Update();
        moon.Update();
    }

    /// <summary>
    /// clears all data that has no gc collect like computebuffers
    /// </summary>
    public void Destroy()
    {
        sun.Destroy();
        moon.Destroy();
    }
}
