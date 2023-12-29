using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script is parent component of the Equipment hierarchy.
/// Is one of the three Equipments and routes further all calls regarding its child component (currentWeapon).
/// Holds all shared functions for Equipment "Weapon".
/// </summary>
public class Weapon : Equipment
{
    #region interact
    public override void InteractWith()
    {
        base.InteractWith();
        if (PlayerInventory.weapon != null)
        {
            PlayerInventory.weapon.Drop(Global.playerTransform.position, PlayerInventory.weapon.Thrust(360f, Vector3.zero));
        }
        PlayerInventory.weapon = this as Equipment.IEquipment;
    }
    #endregion

    public override void UpdateUI()
    {
        UIInventory.currentWeapon_UIImage.color = Color.white;
    }

    public new static System.Type[] equipmentTypes = { typeof(HammerWeapon) };
    public new static System.Type RandomEquipment()
    {
        return equipmentTypes[Random.Range(0, equipmentTypes.Length)];
    }
}