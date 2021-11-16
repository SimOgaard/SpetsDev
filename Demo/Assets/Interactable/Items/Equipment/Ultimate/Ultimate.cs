using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script is parent component of the Equipment hierarchy.
/// Is one of the three Equipments and routes further all calls regarding its child component (current_ultimate).
/// Holds all shared functions for Equipment "Ultimate".
/// </summary>
public class Ultimate : Equipment
{
    #region interact
    public override void InteractWith()
    {
        base.InteractWith();
        if (PlayerInventory.ultimate != null)
        {
            PlayerInventory.ultimate.Drop(Global.player_transform.position, PlayerInventory.ultimate.Thrust(360f, Vector3.zero));
        }
        PlayerInventory.ultimate = this as Equipment.IEquipment;
    }
    #endregion

    public override void UpdateUI()
    {
        UIInventory.current_ultimate_UI_image.color = Color.white;
    }

    public new static System.Type[] equipment_types = { typeof(EarthbendingUltimate) };
    public new static System.Type RandomEquipment()
    {
        return equipment_types[Random.Range(0, equipment_types.Length)];
    }
}
