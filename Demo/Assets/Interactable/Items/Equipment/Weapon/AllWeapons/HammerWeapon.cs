using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script is child component of the Equipment hierarchy.
/// Holds specific functionallity and uppgrades for HammerWeapon Equipment.
/// </summary>
public class HammerWeapon : Weapon, Equipment.IEquipment
{
    public override void UsePrimary()
    {
        Debug.Log("HammerWeapon.UsePrimary");
    }
    public override void StopPrimary()
    {
        Debug.Log("HammerWeapon.StopPrimary");
    }

    public override void UpdateUI()
    {
        base.UpdateUI();
        UIInventory.current_weapon_UI_image.sprite = icon_sprite;
    }

    public override void Awake()
    {
        icon_sprite = Resources.Load<Sprite>("Sprites/UI/hammer");
    }
}
