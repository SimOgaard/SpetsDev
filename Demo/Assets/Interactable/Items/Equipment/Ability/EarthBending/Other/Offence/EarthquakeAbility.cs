﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script is child component of the Equipment hierarchy.
/// Holds specific functionallity and uppgrades for EarthQuakeAbility Equipment.
/// </summary>
public class EarthquakeAbility : Ability
{
    public override void UsePrimary()
    {
        Debug.Log("EarthquakeAbility.UsePrimary");
    }
    public override void StopPrimary()
    {
        Debug.Log("EarthquakeAbility.StopPrimary");
    }

    public override void UpdateUI()
    {
        base.UpdateUI();
        UIInventory.currentAbility_UIImage.sprite = iconSprite;
    }

    public override void Awake()
    {
        base.Awake();
        iconSprite = Resources.Load<Sprite>("Sprites/UI/fireball");
    }
}
