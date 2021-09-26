using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CloudShadows : MonoBehaviour
{
    [SerializeField] private Light light;
    [SerializeField] private RenderTexture shadow_render_texture;
    [SerializeField] private Material cloud_shadow_material;
    [SerializeField] private NoiseLayerSettings.Curve curve;
    private Transform player_transform;

    private void Start()
    {
        player_transform = GameObject.Find("Player").transform;
        light = GetComponent<Light>();
        shadow_render_texture = CreateShadowRenderTexture(1024);
        CurveCreator.AddCurveTexture(ref cloud_shadow_material, curve);
        UpdateLightProperties(200);
    }

    private RenderTexture CreateShadowRenderTexture(int resolution)
    {
        RenderTexture render_texture = new RenderTexture(resolution, resolution, 0);
        render_texture.wrapMode = TextureWrapMode.Clamp;
        render_texture.filterMode = FilterMode.Bilinear;

        return render_texture;
    }

    private void Update()
    {
        // Reposition directional light to be over player to keep
        transform.position = player_transform.position;

        // Calculate the angle between the lights direction and the horizon.
        float angleToHorizon = Vector3.Angle(Vector3.up, transform.forward) - 90;

        // Set remaining material properties.
        cloud_shadow_material.SetFloat("_AngleToHorizon", angleToHorizon);
        cloud_shadow_material.SetVector("_LightPosition", transform.position);

        #if UNITY_EDITOR
        CurveCreator.AddCurveTexture(ref cloud_shadow_material, curve);
        UpdateLightProperties(light.cookieSize);
        #endif

        // Blit using material.
        Graphics.Blit(null, shadow_render_texture, cloud_shadow_material);
    }

    private void UpdateLightProperties(float cookieSize)
    {
        light.cookie = shadow_render_texture;
        light.cookieSize = cookieSize;
        cloud_shadow_material.SetFloat("_CookieSize", light.cookieSize);
    }
}