using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CloudShadows : MonoBehaviour
{
    [SerializeField] private Light light;
    [SerializeField] private RenderTexture shadow_render_texture;
    [SerializeField] private Material cloud_shadow_material;

    private void Start()
    {
        light = GetComponent<Light>();
        shadow_render_texture = CreateShadowRenderTexture(1024);
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

        Debug.Log(angleToHorizon);

        // Set remaining material properties.
        cloud_shadow_material.SetFloat("_AngleToHorizon", angleToHorizon);

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