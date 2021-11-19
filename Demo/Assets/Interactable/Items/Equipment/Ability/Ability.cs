using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script is parent component of the Equipment hierarchy.
/// Is one of the three Equipments and routes further all calls regarding its child component (current_ability).
/// Holds all shared functions for Equipment "Ability".
/// </summary>
public class Ability : Equipment
{
    #region interact
    public override void InteractWith()
    {
        base.InteractWith();
        if (PlayerInventory.ability != null)
        {
            PlayerInventory.ability.Drop(Global.player_transform.position, PlayerInventory.ability.Thrust(360f, Vector3.zero));
        }
        PlayerInventory.ability = this as Equipment.IEquipment;
    }
    #endregion

    public override void UpdateUI()
    {
        UIInventory.current_ability_UI_image.color = Color.white;
    }

    public new static System.Type[] equipment_types = { typeof(EarthBendingPillarBase) };
    public new static System.Type RandomEquipment()
    {
        return equipment_types[Random.Range(0, equipment_types.Length)];
    }
}
