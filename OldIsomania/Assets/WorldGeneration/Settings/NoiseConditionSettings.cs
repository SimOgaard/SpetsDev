using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Defines multiple conditions that returns a binary yes or no.
/// Mainly from FastNoiseLite but also added functionality.
/// </summary>
[CreateAssetMenu(menuName = "Noise/NoiseAdditiveActivation", order = 3)]
[System.Serializable]
public class NoiseConditionSettings : Settings
{
    /// <summary>
    /// A seperate noise layer representing a probability
    /// </summary>
    public NoiseActivationSettings spawnCondition;

    /// <summary>
    /// Comunal noise layers representing the probability of this biome spawning there
    /// All noiselayers gets added before evaluation
    /// </summary>
    public NoiseAdditiveActivationSettings spawnConditionAddative;
}
