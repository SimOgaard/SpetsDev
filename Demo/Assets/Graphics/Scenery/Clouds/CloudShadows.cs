using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudShadows : MonoBehaviour
{
    private CloudSettings cloudSettings;

    private void Start()
    {
        cloudSettings.light = GetComponent<Light>();
        cloudSettings.light.cookie = cloudSettings.cloudRenderTexture;
        cloudSettings.light.cookieSize = cloudSettings.cookieSize;
    }

    public static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
    {
        return RotatePointAroundPivot(point, pivot, Quaternion.Euler(angles));
    }

    public static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Quaternion rotation)
    {
        return rotation * (point - pivot) + pivot;
    }

    [SerializeField] private Vector3 inverseLightPos;
    [SerializeField] private float angleToHorizon;
    private void LateUpdate()
    {
        if (transform.forward.y > 0)
        {
            cloudSettings.light.enabled = false;
            return;
        }
        cloudSettings.light.enabled = true;

        // Calculate the angle between the lights direction and the horizon.
        angleToHorizon = Vector3.Angle(Vector3.up, transform.forward) - 90;

        // Set remaining material properties.
        cloudSettings.material.material.SetFloat("_AngleToHorizon", angleToHorizon);

        inverseLightPos = RotatePointAroundPivot(transform.position, Vector3.zero, Quaternion.Inverse(transform.rotation));

        cloudSettings.material.material.SetVector("_LightPosition", inverseLightPos);

        Vector3 cloudStretchOffset = (transform.up + transform.right);
        cloudStretchOffset = Vector3.Scale(cloudStretchOffset, cloudStretchOffset);
        cloudSettings.material.material.SetVector("_CloudStrechOffset", cloudStretchOffset);

        // Blit using material.
        Graphics.Blit(null, cloudSettings.cloudRenderTexture, cloudSettings.material.material);
    }

    public void UpdateSettings(CloudSettings cloudSettings)
    {
        this.cloudSettings = cloudSettings;
    }
}