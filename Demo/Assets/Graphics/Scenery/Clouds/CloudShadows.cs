using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CloudShadows : MonoBehaviour
{
    [SerializeField] private Light _light;
    [SerializeField] private RenderTexture shadowRenderTexture;
    [SerializeField] private Material cloudShadowMaterial;

    private const float cookieSize = 175;
    private const int shadowRenderTexutreResolution = 512;

    private void Awake()
    {
        _light = GetComponent<Light>();
        shadowRenderTexture = CreateShadowRenderTexture(shadowRenderTexutreResolution);
        _light.cookie = shadowRenderTexture;
        cloudShadowMaterial.SetFloat("_CookieSize", cookieSize);
        UpdateLightProperties(1f);
    }

    private RenderTexture CreateShadowRenderTexture(int resolution)
    {
        RenderTexture renderTexture = new RenderTexture(resolution, resolution, 0);
        renderTexture.wrapMode = TextureWrapMode.Clamp;
        renderTexture.filterMode = FilterMode.Bilinear;

        return renderTexture;
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
    public void UpdatePos()
    {
        // Calculate the angle between the lights direction and the horizon.
        angleToHorizon = Vector3.Angle(Vector3.up, transform.forward) - 90;

        // Set remaining material properties.
        cloudShadowMaterial.SetFloat("_AngleToHorizon", angleToHorizon);

        inverseLightPos = RotatePointAroundPivot(transform.position, Vector3.zero, Quaternion.Inverse(transform.rotation));

        cloudShadowMaterial.SetVector("_LightPosition", inverseLightPos);

        Vector3 cloudStretchOffset = (transform.up + transform.right);
        cloudStretchOffset = Vector3.Scale(cloudStretchOffset, cloudStretchOffset);
        cloudShadowMaterial.SetVector("_CloudStrechOffset", cloudStretchOffset);

        // Blit using material.
        Graphics.Blit(null, shadowRenderTexture, cloudShadowMaterial);
    }

    public void UpdateLightProperties(float zoom)
    {
        _light.cookieSize = cookieSize / zoom;
        cloudShadowMaterial.SetFloat("_Zoom", zoom);
    }
}