using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CurveCreator : MonoBehaviour
{
    [SerializeField] private NoiseLayerSettings.Curve curve;
    [SerializeField] private Material material;

    private void Start()
    {
        material = GetComponent<MeshRenderer>().sharedMaterial;
        AddCurveTexture(ref material, curve);
    }

    private void Update()
    {
        /*
        #if UNITY_EDITOR
        AddCurveTexture(ref material, curve);
        #endif
        */
    }

    private static Texture2D CreateTexture(Vector2Int resolution)
    {
        Texture2D curve_texture = new Texture2D(resolution.x, resolution.y, TextureFormat.R8, false);
        curve_texture.filterMode = FilterMode.Point;
        curve_texture.wrapMode = TextureWrapMode.Clamp;
        return curve_texture;
    }

    public static Texture2D CreateCurveTexture(NoiseLayerSettings.Curve curve)
    {
        Texture2D curve_texture = CreateTexture(new Vector2Int(curve.resolution, 1));
        for (int x = 0; x < curve.resolution; x++)
        {
            float curve_value = Mathf.Clamp01(curve.light_curve.Evaluate((float) x / (float) curve.resolution));
            curve_texture.SetPixel(x, 0, new Color(curve_value, curve_value, curve_value));
        }
        curve_texture.Apply();
        return curve_texture;
    }

    public static void AddCurveTexture(ref Material material, NoiseLayerSettings.Curve curve, string texture_name = "_ColorShading")
    {
        Texture2D curve_texture = CreateTexture(new Vector2Int(curve.resolution, 1));
        curve_texture.filterMode = FilterMode.Point;

        float one_div = 1f / curve.resolution;
        for (int x = 0; x <= curve.resolution; x++)
        {
            float curve_value = Mathf.Clamp01(curve.light_curve.Evaluate(x * one_div));
            float curve_color = curve_value + curve.col_diff;
            curve_texture.SetPixel(x, 0, new Color(curve_color, curve_color, curve_color));
        }
        curve_texture.Apply();

        material.SetTexture(texture_name, curve_texture);
    }

    public static void AddCurveTextures(ref NoiseLayerSettings.MaterialWithCurve material_with_curve)
    {
        for (int i = 0; i < material_with_curve.curves.Length; i++)
        {
            AddCurveTexture(ref material_with_curve.material, material_with_curve.curves[i], material_with_curve.curves[i].texture_name);
        }
    }
}
