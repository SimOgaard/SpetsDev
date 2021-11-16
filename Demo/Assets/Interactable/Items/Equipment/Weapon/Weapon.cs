using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script is parent component of the Equipment hierarchy.
/// Is one of the three Equipments and routes further all calls regarding its child component (current_weapon).
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
            PlayerInventory.weapon.Drop(Global.player_transform.position, PlayerInventory.weapon.Thrust(360f, Vector3.zero));
        }
        PlayerInventory.weapon = this as Equipment.IEquipment;
    }
    #endregion

    public override void UpdateUI()
    {
        UIInventory.current_weapon_UI_image.color = Color.white;
    }

    public new static System.Type[] equipment_types = { typeof(HammerWeapon) };
    public new static System.Type RandomEquipment()
    {
        return equipment_types[Random.Range(0, equipment_types.Length)];
    }
}