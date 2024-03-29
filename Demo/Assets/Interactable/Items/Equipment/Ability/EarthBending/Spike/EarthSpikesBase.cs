﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script is child component of the Equipment hierarchy.
/// Holds specific functionallity and uppgrades for EarthSpikesAbility Equipment.
/// </summary>
public class EarthSpikesBase : Ability, Equipment.IEquipment
{
    public override void UsePrimary()
    {
        Debug.Log("EarthSpikesAbility.UsePrimary");
    }
    public override void StopPrimary()
    {
        Debug.Log("EarthSpikesAbility.StopPrimary");
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