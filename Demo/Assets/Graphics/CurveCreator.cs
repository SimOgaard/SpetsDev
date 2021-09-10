using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurveCreator : MonoBehaviour
{
    public static void AddCurveTexture(ref Material material, AnimationCurve curve, int x_resolution = 255)
    {
        Texture2D curve_texture = new Texture2D(x_resolution, 1, TextureFormat.R8, false);
        curve_texture.filterMode = FilterMode.Point;
        for (int x = 0; x < x_resolution; x++)
        {
            float curve_value = curve.Evaluate((float) x /(float) x_resolution);

            curve_texture.SetPixel(x, 0, new Color(curve_value, curve_value, curve_value));
        }
        curve_texture.Apply();

        material.SetTexture("_CurveTexture", curve_texture);
    }
}
