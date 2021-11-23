using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CloudShadows : MonoBehaviour
{
    [SerializeField] private bool invert;
    [SerializeField] private Light _light;
    [SerializeField] private RenderTexture shadow_render_texture;
    [SerializeField] private Material cloud_shadow_material;
    [SerializeField] private NoiseLayerSettings.Curve curve;
    private Transform camera_focus_point_transform;
    private float cookie_size = 175;

    private void Awake()
    {
        camera_focus_point_transform = GameObject.Find("camera_focus_point").transform;
        _light = GetComponent<Light>();
        shadow_render_texture = CreateShadowRenderTexture(896);
        _light.cookie = shadow_render_texture;
        CurveCreator.AddCurveTexture(ref cloud_shadow_material, curve);
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

    private void LateUpdate()
    {
        // Reposition directional light to be over player to keep
        Vector3 new_pos = camera_focus_point_transform.position;
        new_pos.y = 0f;
        transform.position = new_pos;

        // Calculate the angle between the lights direction and the horizon.
        float angleToHorizon = Vector3.Angle(Vector3.up, transform.forward) - 90;

        // Set remaining material properties.
        cloud_shadow_material.SetFloat("_AngleToHorizon", angleToHorizon);
        cloud_shadow_material.SetVector("_LightPosition", transform.position * (invert ? -1f : 1f));
        
        /*
        #if UNITY_EDITOR
        CurveCreator.AddCurveTexture(ref cloud_shadow_material, curve);
        UpdateLightProperties(_light.cookieSize);
        #endif
        */

        // Blit using material.
        Graphics.Blit(null, shadow_render_texture, cloud_shadow_material);
    }

    public void UpdateLightProperties(float zoom)
    {
        _light.cookieSize = cookie_size / zoom;
        cloud_shadow_material.SetFloat("_Zoom", zoom);
    }
}