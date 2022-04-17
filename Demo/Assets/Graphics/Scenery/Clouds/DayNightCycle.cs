using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    [SerializeField] private float ambient;
    [SerializeField] private AnimationCurve ambientLight;

    [SerializeField] private float darkest;
    [SerializeField] private AnimationCurve darkestValue;

    [SerializeField] private float waterOffset;
    [SerializeField] private AnimationCurve waterColOffset;

    [SerializeField] private Vector3 rotationSpeed;
    [SerializeField] private Vector3 rotationSnap;
    public Vector3 currentRotationEuler;

    private GameObject sun;
    private CloudShadows sunCloudShadows;
    private GameObject moon;
    private CloudShadows moonCloudShadows;

    private void Start()
    {
        sun = transform.GetChild(0).gameObject;
        sunCloudShadows = sun.GetComponent<CloudShadows>();
        moon = transform.GetChild(1).gameObject;
        moonCloudShadows = moon.GetComponent<CloudShadows>();
    }

    public static Vector3 DivideVector3(Vector3 numerator, Vector3 denominator)
    {
        return new Vector3(numerator.x / denominator.x, numerator.y / denominator.y, numerator.z / denominator.z);
    }

    public void UpdatePos()
    {
        currentRotationEuler += rotationSpeed * Time.deltaTime;
        if (rotationSnap != Vector3.zero)
        {
            Vector3 roundedRotation = Vector3.Scale(Vector3Int.RoundToInt(DivideVector3(currentRotationEuler, rotationSnap)), rotationSnap);
            transform.rotation = Quaternion.Euler(roundedRotation);
        }
        else
        {
            transform.rotation = Quaternion.Euler(currentRotationEuler);
        }

        // Reposition directional light to be over player to keep shadows
        transform.position = MousePoint.PositionRayPlane(Vector3.zero, -transform.forward, Global.cameraFocusPointTransform.position, transform.forward);

        float x = Vector3.Dot(transform.forward, Vector3.up);
        ambient = ambientLight.Evaluate(x);
        Shader.SetGlobalFloat("_Ambient", ambient);

        darkest = darkestValue.Evaluate(x);
        Shader.SetGlobalFloat("_Darkest", darkest);

        //Debug.Log($"ambient {ambient}");
        //Debug.Log($"darkest {darkest}");

        waterOffset = waterColOffset.Evaluate(x);
        Global.Materials.waterMaterial.SetFloat("_WaterColOffset", waterOffset);

        if (sun.transform.forward.y < 0)
        {
            sunCloudShadows.UpdatePos();

            if (!sun.activeInHierarchy)
            {
                sun.SetActive(true);
                moon.SetActive(false);
            }
        }
        else if (moon.transform.forward.y < 0)
        {
            moonCloudShadows.UpdatePos();

            if (!moon.activeInHierarchy)
            {
                moon.SetActive(true);
                sun.SetActive(false);
            }
        }

    }

    internal void UpdateRenderTexture()
    {
        throw new NotImplementedException();
    }
}
