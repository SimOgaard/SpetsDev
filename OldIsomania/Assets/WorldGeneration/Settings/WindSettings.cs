using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controlls the global wind
/// </summary>
[CreateAssetMenu(menuName = "WorldGeneration/Climate/Wind", order = 30)]
[System.Serializable]
public class WindSettings : Settings
{
    //public Texture2D WindDistortionMap;
    //public Vector4 WindDistortionMap_ST;

    //public Vector2 WindFrequency;
    //public float WindStrength;

    public Vector3 cloudScroll;
    public NoiseSettings cloudScrollNoise;

    [ContextMenu("Rename", false, 500)]
    public override void Rename()
    {
        base.Rename();
    }

    [ContextMenu("Update", false, -1000)]
    public override void Update()
    {
        //Shader.SetGlobalTexture("_WindDistortionMap", WindDistortionMap);
        //Shader.SetGlobalVector("_WindDistortionMap_ST", WindDistortionMap_ST);

        //Shader.SetGlobalVector("_WindFrequency", WindFrequency);
        //Shader.SetGlobalFloat("_WindStrength", WindStrength);

        Global.windController.UpdateSettings(this);
    }
}