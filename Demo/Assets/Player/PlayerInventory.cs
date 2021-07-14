using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Holds all current equipments in player inventory and all its child components for easy calling of type:
/// <para>Equipment, Weapon, Weapon.IWeapon, Ability, Ability.IAbility, Ultimate, Ultimate.IUltimate.</para>
/// 
/// Example use:
/// <para>if (weapon_equipment != null) weapon_equipment.DropEquipment();</para>
/// <para>if (weapon_parrent != null) weapon_parrent.GetDroppedItemShaderStruct();</para>
/// <para>if (weapon != null) weapon.UsePrimary();</para>
/// </summary>
public class PlayerInventory : MonoBehaviour
{
    /// <summary>
    /// Call to execute functions which encompasses all Equipments no matter Equipment.IEquipment type.
    /// <para>Is reference to Equipment component in inventory that is parrent of weapon_equipment.</para>
    /// </summary>
    public Equipment weapon_equipment;
    /// <summary>
    /// Call to execute functions which encompasses all Weapons no matter Weapon.IWeapon type.
    /// <para>Is reference to Weapon component in inventory that is parrent of weapon.</para>
    /// </summary>
    public Weapon weapon_parrent;
    /// <summary>
    /// Current weapon in Inventory.
    /// <para>Is of type Weapon.IWeapon. Example: SledgeHammer, Sword, Bow.</para>
    /// </summary>
    public Weapon.IWeapon weapon;

    /// <summary>
    /// Call to execute functions which encompasses all Equipments no matter Equipment.IEquipment type.
    /// <para>Is reference to Equipment component in inventory that is parrent of ability_equipment.</para>
    /// </summary>
    public Equipment ability_equipment;
    /// <summary>
    /// Call to execute functions which encompasses all Abilities no matter Ability.IAbility type.
    /// <para>Is reference to Ability component in inventory that is parrent of ability.</para>
    /// </summary>
    public Ability ability_parrent;
    /// <summary>
    /// Current ability in Inventory.
    /// <para>Is of type Ability.IAbility. Example: Fireball, Lightening, Teleport.</para>
    /// </summary>
    public Ability.IAbility ability;

    /// <summary>
    /// Call to execute functions which encompasses all Equipments no matter Equipment.IEquipment type.
    /// <para>Is reference to Equipment component in inventory that is parrent of ultimate_equipment.</para>
    /// </summary>
    public Equipment ultimate_equipment;
    /// <summary>
    /// Call to execute functions which encompasses all Ultimates no matter Ultimate.IUltimate type.
    /// <para>Is reference to Ultimate component in inventory that is parrent of ultimate.</para>
    /// </summary>
    public Ultimate ultimate_parrent;
    /// <summary>
    /// Current ultimate in Inventory.
    /// <para>Is of type Ultimate.IUltimate. Example: Earth bender, Vats, Telekinesis.</para>
    /// </summary>
    public Ultimate.IUltimate ultimate;
}
