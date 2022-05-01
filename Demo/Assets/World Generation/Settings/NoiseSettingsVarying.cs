using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Hold reference to NoiseLayerSettings but overides/new the functionality
/// to have varying offset on all settings, like asset.seed = 69, this.seed = 420, new seed = 489
/// </summary>
[CreateAssetMenu()]
[System.Serializable]
public class NoiseSettingsVarying : ScriptableObject
{

}
