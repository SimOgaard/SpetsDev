using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script is child component of the Equipment hierarchy.
/// Holds specific functionallity and uppgrades for EarthbendingUltimate Equipment.
/// </summary>
public class TelekinesisUltimate : Ultimate
{
    public override void UsePrimary()
    {
        base.UsePrimary();
        Debug.Log("TelekinesisUltimate.UsePrimary");
    }
}