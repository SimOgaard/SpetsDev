using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Extension of UnityEngine.Material that adds dynamic texture curves for high customizable cartoon/shading etc
/// </summary>
[CreateAssetMenu(menuName = "MaterialSettings/MaterialSetting", order = 1)]
[System.Serializable]
public class MaterialSettings : ScriptableObject
{
    [System.Serializable]
    public class Curve
    {
        public void AddCurveTexture(Material material)
        {
            Texture2D curveTexture = CurveCreator.CreateTexture(new Vector2Int(textureResolution, 1));

            float oneDiv = 1f / textureResolution;
            for (int x = 0; x <= textureResolution; x++)
            {
                float curveValue = Mathf.Clamp01(curve.Evaluate(x * oneDiv));
                float curveColor = curveValue + curveOffset;
                curveTexture.SetPixel(x, 0, new Color(curveColor, curveColor, curveColor));
            }
            curveTexture.Apply();

            material.SetTexture(textureName, curveTexture);
        }

        public void AddGlobalCurveTexture()
        {
            Texture2D curveTexture = CurveCreator.CreateTexture(new Vector2Int(textureResolution, 1));

            float oneDiv = 1f / textureResolution;
            for (int x = 0; x <= textureResolution; x++)
            {
                float curveValue = Mathf.Clamp01(curve.Evaluate(x * oneDiv));
                float curveColor = curveValue + curveOffset;
                curveTexture.SetPixel(x, 0, new Color(curveColor, curveColor, curveColor));
            }
            curveTexture.Apply();

            Shader.SetGlobalTexture(textureName, curveTexture);
        }
        /// <summary>
        /// The property name of affected texture
        /// </summary>
        public string textureName = "_ColorShading";
        /// <summary>
        /// The x resolution of affected texture since the texture represents a gradient; y resolution is keept a 1
        /// </summary>
        public int textureResolution = 256;
        /// <summary>
        /// The curve that should be turned into a texture of size textureResolution with TextureFormat.R8
        /// </summary>
        public AnimationCurve curve;
        /// <summary>
        /// A offset for curve that gets added to each evaluation of the curve
        /// </summary>
        public float curveOffset = 0f;
    }

    /// <summary>
    /// This material reference is static so any changes on this material will effect all other instances this materials references
    /// </summary>
    public Material material;
    /// <summary>
    /// All curvetextures that are static and should be initilized on runtime to the material
    /// </summary>
    public Curve[] materialCurves;

    /// <summary>
    /// Creates curvetextures from all curves of material
    /// Is done once at runtime
    /// </summary>
    [ContextMenu("Update", false, -1000)]
    public void Update()
    {
        for (int i = 0; i < materialCurves.Length; i++)
        {
            materialCurves[i].AddCurveTexture(material);
        }
    }
}
