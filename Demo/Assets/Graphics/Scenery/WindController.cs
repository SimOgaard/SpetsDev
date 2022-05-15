using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// controlls the wind
/// </summary>
public class WindController : MonoBehaviour
{
    private WindSettings windSettings;
    private FastNoiseLite windSpeedNoise;
    private FastNoiseLite windSpeedWarpNoise;

    private Vector2 windSpeedNoisePos = Vector2.zero;

    private Vector3 windScroll = Vector3.zero;

    /// <summary>
    /// updates windsettings and noiselayers
    /// </summary>
    public void UpdateSettings(WindSettings windSettings)
    {
        this.windSettings = windSettings;
        this.windSpeedNoise = windSettings.noiseScrollNoise.ToFNLStateCS(0, false);
        this.windSpeedWarpNoise = windSettings.noiseScrollNoise.ToFNLStateCS(0, true);
    }

    /// <summary>
    /// updates cloudSpeed 
    /// </summary>
    public void Update()
    {
        // add 
        float x = Time.realtimeSinceStartup;
        float y = Time.realtimeSinceStartup;

        // warp it
        windSpeedWarpNoise.DomainWarp(ref x, ref y);

        // scale speed of wind by noise
        Vector3 cloudSpeed = windSettings.cloudSpeed;
        cloudSpeed.Scale(new Vector3(
            windSpeedNoise.remap01(windSpeedNoise.SampleNoise2D(x, y)),
            windSpeedNoise.remap01(windSpeedNoise.SampleNoise2D(-x, y)),
            windSpeedNoise.remap01(windSpeedNoise.SampleNoise2D(x, -y))
        ));

        // get new windscroll value
        windScroll += cloudSpeed * Time.deltaTime;
        // expose it to all shaders
        Shader.SetGlobalVector("_WindScroll", windScroll);
    }
}
