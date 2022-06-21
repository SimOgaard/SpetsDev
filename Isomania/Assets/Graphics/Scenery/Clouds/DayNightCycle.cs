using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controlls the day night cycle
/// </summary>
public class DayNightCycle : MonoBehaviour
{
    private DayNightCycleSettings dayNightCycleSettings;

    public Vector3 currentRotationEuler;

    public static Vector3 DivideVector3(Vector3 numerator, Vector3 denominator)
    {
        return new Vector3(numerator.x / denominator.x, numerator.y / denominator.y, numerator.z / denominator.z);
    }

    private void LateUpdate()
    {
        currentRotationEuler += dayNightCycleSettings.rotationSpeed * Time.deltaTime;
        if (dayNightCycleSettings.rotationSnap != Vector3.zero)
        {
            Vector3 roundedRotation = Vector3.Scale(Vector3Int.RoundToInt(DivideVector3(currentRotationEuler, dayNightCycleSettings.rotationSnap)), dayNightCycleSettings.rotationSnap);
            transform.rotation = Quaternion.Euler(roundedRotation);
        }
        else
        {
            transform.rotation = Quaternion.Euler(currentRotationEuler);
        }

        // Reposition directional light to be over player to keep shadows
        //transform.position = MousePoint.PositionRayPlane(Vector3.zero, Vector3.up, MainCamera.mCamera.transform.position, MainCamera.mCamera.transform.forward);

        float time = Vector3.Dot(transform.forward, Vector3.up);

        float ambientLerp = dayNightCycleSettings.ambientLerp.Evaluate(time);
        Color currentAmbientColor = Color.Lerp(dayNightCycleSettings.ambientDay, dayNightCycleSettings.ambientNight, ambientLerp);
        Shader.SetGlobalColor("_AmbientColor", currentAmbientColor);
    }

    public void UpdateSettings(DayNightCycleSettings dayNightCycleSettings)
    {
        this.dayNightCycleSettings = dayNightCycleSettings;
        //GameObject.Find("Sun").GetComponent<CloudShadows>().UpdateSettings(dayNightCycleSettings.sun);
        //GameObject.Find("Moon").GetComponent<CloudShadows>().UpdateSettings(dayNightCycleSettings.moon);
    }

    public void UpdateRenderTexture()
    {
        //throw new NotImplementedException();
    }
}
