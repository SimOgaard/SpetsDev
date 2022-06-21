using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionTest : MonoBehaviour
{
    private Material explosionMaterial;
    private float time01 = 1f;
    private float Time01 {
        get { return time01; }
        set { time01 = value < 0.5f ? 1f : value; }
    }

    private void Start()
    {
        explosionMaterial = GetComponent<MeshRenderer>().material;
    }

    private void Update()
    {
        Time01 -= Time.deltaTime;

        //explosionMaterial.SetFloat("_SmokeFireRatio", Time01 * Time01);
        //explosionMaterial.SetFloat("_SmokeFireBlend", Time01 * Time01);
        explosionMaterial.SetFloat("_AlphaClip", Time01);
    }
}
