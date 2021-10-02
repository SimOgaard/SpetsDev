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
        material = GetComponent<MeshRenderer>().material;
    }

    private void Update()
    {
        AddCurveTexture(ref material, curve);
    }

    public static void AddCurveTexture(ref Material material, NoiseLayerSettings.Curve curve)
    {
        Texture2D curve_texture = new Texture2D(curve.resolution, 1, TextureFormat.R8, false);
        curve_texture.filterMode = FilterMode.Point;

        float one_div = 1f / curve.resolution;
        float one_div_half = 1f / (curve.resolution * 2f);
        float one_low = curve.resolution - 0.0001f;

        // find maximum and minimum to know how you should devide into x discrete points
        float min = float.MaxValue;
        float max = float.MinValue;
        for (int x = 0; x < curve.resolution; x++)
        {
            float curve_value = Mathf.Clamp01(curve.light_curve.Evaluate(((float)x + one_div_half) * one_div));
            min = Mathf.Min(min, curve_value);
            max = Mathf.Max(max, curve_value);
        }
        float steps = (max - min) * one_div;

        for (int x = 0; x < curve.resolution; x++)
        {
            float curve_value = Mathf.Clamp01(curve.light_curve.Evaluate(((float) x + one_div_half) * one_div));
            //float curve_value_discrete = Mathf.Floor(curve_value / steps) * steps; // is 0.05, 0.15 0.25 ... 0.95
            float curve_step = (float) curve.col_diff * steps; // shifts range to side

            float curve_color = curve_value/*_discrete*/ + curve_step;
            curve_texture.SetPixel(x, 0, new Color(curve_color, curve_color, curve_color));
        }
        curve_texture.Apply();

        material.SetTexture("_CurveTexture", curve_texture);
    }
}
