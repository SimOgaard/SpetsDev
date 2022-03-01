using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Holds all current equipments in player inventory and all its child components for easy calling of type:
/// <para>Equipment, Weapon, Weapon.IWeapon, Ability, Ability.IAbility, Ultimate, Ultimate.IUltimate.</para>
/// 
/// Example use:
/// <para>if (PlayerInventory.weaponEquipment != null) PlayerInventory.weaponEquipment.DropEquipment();</para>
/// <para>if (PlayerInventory.weaponParrent != null) PlayerInventory.weaponParrent.GetDroppedItemShaderStruct();</para>
/// <para>if (PlayerInventory.weapon != null) PlayerInventory.weapon.UsePrimary();</para>
/// </summary>
public class PlayerInventory : MonoBehaviour
{
    /// <summary>
    /// Call to execute functions which encompasses all Weapons no matter Weapon.IWeapon type.
    /// <para>Is reference to Weapon component in inventory that is parrent of weapon.</para>
    /// </summary>
    public static Equipment.IEquipment weapon;

    /// <summary>
    /// Call to execute functions which encompasses all Abilities no matter Ability.IAbility type.
    /// <para>Is reference to Ability component in inventory that is parrent of ability.</para>
    /// </summary>
    public static Equipment.IEquipment ability;

    /// <summary>
    /// Call to execute functions which encompasses all Ultimates no matter Ultimate.IUltimate type.
    /// <para>Is reference to Ultimate component in inventory that is parrent of ultimate.</para>
    /// </summary>
    public static Equipment.IEquipment ultimate;

    private void Start()
    {
        StartWith(typeof(EarthSpikesBase));
    }

    private void StartWith(System.Type equipmentType)
    {
        GameObject gm;
        Equipment.IEquipment eq;
        (gm, eq) = Equipment.CreateRandomEquipment(equipmentType);
        eq.InteractWith();
    }
}
