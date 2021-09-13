using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CloudShadows : MonoBehaviour
{
    [SerializeField] private Light light;
    [SerializeField] private RenderTexture shadow_render_texture;
    [SerializeField] private Material cloud_shadow_material;
    [SerializeField] private AnimationCurve curve;
    [SerializeField] private int resolution = 10;

    private void Start()
    {
        light = GetComponent<Light>();
        shadow_render_texture = CreateShadowRenderTexture(1024);
        CurveCreator.AddCurveTexture(ref cloud_shadow_material, curve);
        UpdateLightProperties();
    }

    private RenderTexture CreateShadowRenderTexture(int resolution)
    {
        RenderTexture render_texture = new RenderTexture(resolution, resolution, 0);
        render_texture.wrapMode = TextureWrapMode.Repeat;
        render_texture.filterMode = FilterMode.Bilinear;

        return render_texture;
    }

    private void Update()
    {
        // Calculate the angle between the lights direction and the horizon.
        float angleToHorizon = Vector3.Angle(Vector3.up, transform.forward) - 90;

        // Set remaining material properties.
        cloud_shadow_material.SetFloat("_AngleToHorizon", angleToHorizon);
        // DO NOT HAVE IN THE END 123
        CurveCreator.AddCurveTexture(ref cloud_shadow_material, curve, resolution);

        // Blit using material.
        Graphics.Blit(null, shadow_render_texture, cloud_shadow_material);

        UpdateLightProperties();
    }

    private void UpdateLightProperties()
    {
        light.cookie = shadow_render_texture;
        light.cookieSize = 250;
    }
}