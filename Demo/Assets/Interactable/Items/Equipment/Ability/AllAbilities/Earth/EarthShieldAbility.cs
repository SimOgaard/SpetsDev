using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script is child component of the Equipment hierarchy.
/// Holds specific functionallity and uppgrades for EarthShieldAbility Equipment.
/// </summary>
public class EarthShieldAbility : Ability
{
    public override void UsePrimary()
    {
        Debug.Log("EarthShieldAbility.UsePrimary");
    }
    public override void StopPrimary()
    {
        Debug.Log("EarthShieldAbility.StopPrimary");
    }

    public override void UpdateUI()
    {
        base.UpdateUI();
        UIInventory.current_ability_UI_image.sprite = icon_sprite;
    }

    public override void Awake()
    {
        icon_sprite = Resources.Load<Sprite>("Sprites/UI/fireball");
    }
}