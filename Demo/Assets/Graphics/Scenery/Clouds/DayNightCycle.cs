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
        transform.position = MousePoint.PositionRayPlane(Vector3.zero, -transform.forward, Global.cameraFocusPointTransform.position, transform.forward);

        float x = Vector3.Dot(transform.forward, Vector3.up);
        dayNightCycleSettings.ambient = dayNightCycleSettings.ambientLight.Evaluate(x);
        Shader.SetGlobalFloat("_Ambient", dayNightCycleSettings.ambient);

        dayNightCycleSettings.darkest = dayNightCycleSettings.darkestValue.Evaluate(x);
        Shader.SetGlobalFloat("_Darkest", dayNightCycleSettings.darkest);

        //Debug.Log($"ambient {ambient}");
        //Debug.Log($"darkest {darkest}");

        dayNightCycleSettings.waterOffset = dayNightCycleSettings.waterColOffset.Evaluate(x);
        Global.waterMaterial.SetFloat("_WaterColOffset", dayNightCycleSettings.waterOffset);
    }

    public void UpdateSettings(DayNightCycleSettings dayNightCycleSettings)
    {
        this.dayNightCycleSettings = dayNightCycleSettings;
        GameObject.Find("Sun").GetComponent<CloudShadows>().UpdateSettings(dayNightCycleSettings.sun);
        GameObject.Find("Moon").GetComponent<CloudShadows>().UpdateSettings(dayNightCycleSettings.moon);
    }

    public void UpdateRenderTexture()
    {
        //throw new NotImplementedException();
    }
}
