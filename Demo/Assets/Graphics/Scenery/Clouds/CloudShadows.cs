using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CloudShadows : MonoBehaviour
{
    [SerializeField] private Light _light;
    [SerializeField] private RenderTexture shadow_render_texture;
    [SerializeField] private Material cloud_shadow_material;

    private const float cookie_size = 175;
    private const int shadow_render_texutre_resolution = 512;

    private void Awake()
    {
        _light = GetComponent<Light>();
        shadow_render_texture = CreateShadowRenderTexture(shadow_render_texutre_resolution);
        _light.cookie = shadow_render_texture;
        cloud_shadow_material.SetFloat("_CookieSize", cookie_size);
        UpdateLightProperties(1f);
    }

    private RenderTexture CreateShadowRenderTexture(int resolution)
    {
        RenderTexture render_texture = new RenderTexture(resolution, resolution, 0);
        render_texture.wrapMode = TextureWrapMode.Clamp;
        render_texture.filterMode = FilterMode.Bilinear;

        return render_texture;
    }

    public static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
    {
        return RotatePointAroundPivot(point, pivot, Quaternion.Euler(angles));
    }

    public static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Quaternion rotation)
    {
        return rotation * (point - pivot) + pivot;
    }

    [SerializeField] private Vector3 inverse_light_pos;
    [SerializeField] private float angleToHorizon;
    public void UpdatePos()
    {
        // Calculate the angle between the lights direction and the horizon.
        angleToHorizon = Vector3.Angle(Vector3.up, transform.forward) - 90;

        // Set remaining material properties.
        cloud_shadow_material.SetFloat("_AngleToHorizon", angleToHorizon);

        inverse_light_pos = RotatePointAroundPivot(transform.position, Vector3.zero, Quaternion.Inverse(transform.rotation));

        cloud_shadow_material.SetVector("_LightPosition", inverse_light_pos);

        Vector3 cloud_stretch_offset = (transform.up + transform.right);
        cloud_stretch_offset = Vector3.Scale(cloud_stretch_offset, cloud_stretch_offset);
        cloud_shadow_material.SetVector("_CloudStrechOffset", cloud_stretch_offset);

        // Blit using material.
        Graphics.Blit(null, shadow_render_texture, cloud_shadow_material);
    }

    public void UpdateLightProperties(float zoom)
    {
        _light.cookieSize = cookie_size / zoom;
        cloud_shadow_material.SetFloat("_Zoom", zoom);
    }
}