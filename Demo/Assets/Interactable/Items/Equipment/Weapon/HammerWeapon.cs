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
        UIInventory.currentWeapon_UIImage.sprite = iconSprite;
    }

    public override void Awake()
    {
        base.Awake();
        iconSprite = Resources.Load<Sprite>("Sprites/UI/hammer");
    }
}
