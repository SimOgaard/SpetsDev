using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionTest : MonoBehaviour
{
    private Material explosion_material;
    private float time01 = 1f;
    private float Time01 {
        get { return time01; }
        set { time01 = value < 0.5f ? 1f : value; }
    }

    private void Start()
    {
        explosion_material = GetComponent<MeshRenderer>().material;
    }

    private void Update()
    {
        Time01 -= Time.deltaTime;

        //explosion_material.SetFloat("_SmokeFireRatio", Time01 * Time01);
        //explosion_material.SetFloat("_SmokeFireBlend", Time01 * Time01);
        explosion_material.SetFloat("_AlphaClip", Time01);
    }
}
