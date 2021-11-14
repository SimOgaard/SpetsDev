using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Density : MonoBehaviour
{
    public const float global_density = 0.001f;
    public enum DensityValues
    {
        ignore = 0,
        stone = 1600,
        metall = 7300
    }

    public DensityValues density = DensityValues.stone;
}
