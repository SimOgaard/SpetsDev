using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurveCreator : MonoBehaviour
{
    [SerializeField] private MaterialSettings.Curve curve;
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

    public static Texture2D CreateTexture(Vector2Int resolution)
    {
        Texture2D curveTexture = new Texture2D(resolution.x, resolution.y, TextureFormat.R8, false);
        curveTexture.filterMode = FilterMode.Point;
        curveTexture.wrapMode = TextureWrapMode.Clamp;
        return curveTexture;
    }

    public static Texture2D CreateCurveTexture(MaterialSettings.Curve curve)
    {
        Texture2D curveTexture = CreateTexture(new Vector2Int(curve.textureResolution, 1));
        for (int x = 0; x < curve.textureResolution; x++)
        {
            float curveValue = Mathf.Clamp01(curve.curve.Evaluate((float) x / (float) curve.textureResolution));
            curveTexture.SetPixel(x, 0, new Color(curveValue, curveValue, curveValue));
        }
        curveTexture.Apply();
        return curveTexture;
    }

    public static void AddCurveTexture(ref Material material, MaterialSettings.Curve curve, string textureName = "_ColorShading")
    {
        Texture2D curveTexture = CreateTexture(new Vector2Int(curve.textureResolution, 1));
        curveTexture.filterMode = FilterMode.Point;

        float oneDiv = 1f / curve.textureResolution;
        for (int x = 0; x <= curve.textureResolution; x++)
        {
            float curveValue = Mathf.Clamp01(curve.curve.Evaluate(x * oneDiv));
            float curveColor = curveValue + curve.curveOffset;
            curveTexture.SetPixel(x, 0, new Color(curveColor, curveColor, curveColor));
        }
        curveTexture.Apply();

        material.SetTexture(textureName, curveTexture);
    }

    public static void AddCurveTextures(ref MaterialSettings materialWithCurve)
    {
        for (int i = 0; i < materialWithCurve.materialCurveTextures.Length; i++)
        {
            AddCurveTexture(ref materialWithCurve.shaderMaterial, materialWithCurve.materialCurveTextures[i], materialWithCurve.materialCurveTextures[i].textureName);
        }
    }
}
