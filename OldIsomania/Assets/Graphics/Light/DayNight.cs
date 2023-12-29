using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controlls the day night cycle
/// </summary>
public class DayNight : MonoBehaviour
{
    private DayNightSettings dayNightCycleSettings;

    public Vector3 currentRotationEuler;

    public static Vector3 DivideVector3(Vector3 numerator, Vector3 denominator)
    {
        return new Vector3(numerator.x / denominator.x, numerator.y / denominator.y, numerator.z / denominator.z);
    }

    private void LateUpdate()
    {
        float time = Vector3.Dot(transform.forward, Vector3.up);

        float rotationLerp = dayNightCycleSettings.ambientLightLerp.Evaluate(time);
        Vector3 rotationSpeed = Vector3.Lerp(dayNightCycleSettings.rotationSpeedDay, dayNightCycleSettings.rotationSpeedNight, rotationLerp);

        currentRotationEuler += rotationSpeed * Time.deltaTime;
        if (dayNightCycleSettings.rotationSnap != Vector3.zero)
        {
            Vector3 roundedRotation = Vector3.Scale(Vector3Int.RoundToInt(DivideVector3(currentRotationEuler, dayNightCycleSettings.rotationSnap)), dayNightCycleSettings.rotationSnap);
            transform.rotation = Quaternion.Euler(roundedRotation);
        }
        else
        {
            transform.rotation = Quaternion.Euler(currentRotationEuler);
        }

        float ambientLerp = dayNightCycleSettings.ambientLightLerp.Evaluate(time);
        Color currentAmbientColor = Color.Lerp(dayNightCycleSettings.ambientLightDay, dayNightCycleSettings.ambientLightNight, ambientLerp);
        Shader.SetGlobalColor("_AmbientColor", currentAmbientColor);

        // Calculate the angle between the lights direction and the horizon.
        float angleToHorizon = Vector3.Angle(Vector3.up, transform.forward) - 90;

        // Set remaining material properties.
        Shader.SetGlobalFloat("_AngleToHorizon", angleToHorizon);
    }

    public void UpdateSettings(DayNightSettings dayNightCycleSettings)
    {
        this.dayNightCycleSettings = dayNightCycleSettings;
        currentRotationEuler = dayNightCycleSettings.rotationStart;
    }
}
