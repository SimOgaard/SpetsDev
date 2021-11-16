using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Holds all current equipments in player inventory and all its child components for easy calling of type:
/// <para>Equipment, Weapon, Weapon.IWeapon, Ability, Ability.IAbility, Ultimate, Ultimate.IUltimate.</para>
/// 
/// Example use:
/// <para>if (PlayerInventory.weapon_equipment != null) PlayerInventory.weapon_equipment.DropEquipment();</para>
/// <para>if (PlayerInventory.weapon_parrent != null) PlayerInventory.weapon_parrent.GetDroppedItemShaderStruct();</para>
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
}
