using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurveCreator : MonoBehaviour
{
    public static void AddCurveTexture(ref Material material, NoiseLayerSettings.Curve curve)
    {
        Texture2D curve_texture = new Texture2D(curve.resolution, 1, TextureFormat.R8, false);
        curve_texture.filterMode = FilterMode.Point;
        for (int x = 0; x < curve.resolution; x++)
        {
            float curve_value = curve.light_curve.Evaluate((float) x / (float) curve.resolution);

            Debug.Log(curve_value);

            curve_texture.SetPixel(x, 0, new Color(curve_value, curve_value, curve_value));
        }
        curve_texture.Apply();

        material.SetInt("_ColDiff", curve.col_diff);
        material.SetTexture("_CurveTexture", curve_texture);
    }
}
