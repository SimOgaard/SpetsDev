using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script is master component of Equipment hierarchy.
/// Initializes equipment in game world and routes all specific Equipment calls to current_equipment (Equipment parent).
/// </summary>
public class Equipment : MonoBehaviour
{
    // Stores all Equipment types globally. Is used to specify what to initialize.
    public enum EEquipment { Weapon, Ability, Ultimate }

    // Current Equipment this component controlls.
    public IEquipment current_equipment;

    public static float spawn_equipment_chance = 0.25f;

    /// <summary>
    /// Rulesets for parent Equipment to follow.
    /// </summary>
    public interface IEquipment
    {
        void Destroy();
        DropItem.DroppedItemShaderStruct GetDroppedItemShaderStruct();
        void OnGround();
        void UsePrimary();
        void ObjectPool();
        void DeleteObjectPool();
        Sprite GetIconSprite();
        float GetCurrentCooldown();
        float GetCooldown();
    }

    /// <summary>
    /// Initializes this Equipment component to random weapon and spawns random amount of upgrades depending on .
    /// </summary>
    public Weapon.IWeapon InitWeapon()
    {
        current_equipment = gameObject.AddComponent<Weapon>();
        Weapon cached_type_of_weapon = gameObject.GetComponent<Weapon>();
        cached_type_of_weapon.current_weapon = gameObject.AddComponent<HammerWeapon>();
        return cached_type_of_weapon.current_weapon;
    }

    /// <summary>
    /// Initializes this Equipment component to random ability with or without uppgrades or uppgrade of current ability in inventory.
    /// </summary>
    public Ability.IAbility InitAbility()
    {
        current_equipment = gameObject.AddComponent<Ability>();
        Ability cached_type_of_ability = gameObject.GetComponent<Ability>();

        int rand_ability = Mathf.RoundToInt(Random.Range(0f, 3f));
        switch (rand_ability)
        {
            case 0:
                cached_type_of_ability.current_ability = gameObject.AddComponent<FireballAbility>();
                break;
            case 1:
                cached_type_of_ability.current_ability = gameObject.AddComponent<EarthquakeAbility>();
                break;
            case 2:
                cached_type_of_ability.current_ability = gameObject.AddComponent<EarthShieldAbility>();
                break;
            case 3:
                cached_type_of_ability.current_ability = gameObject.AddComponent<EarthSpikesAbility>();
                break;
        }
        return cached_type_of_ability.current_ability;
    }

    /// <summary>
    /// Initializes this Equipment component to random ultimate with or without uppgrades or uppgrade of current ultimate in inventory.
    /// </summary>
    public Ultimate.IUltimate InitUltimate()
    {
        current_equipment = gameObject.AddComponent<Ultimate>();
        Ultimate cached_type_of_ultimate = gameObject.GetComponent<Ultimate>();
        cached_type_of_ultimate.current_ultimate = gameObject.AddComponent<EarthbendingUltimate>();
        return cached_type_of_ultimate.current_ultimate;
    }

    /// <summary>
    /// Deletes hierarchy from the bottom up.
    /// </summary>
    public void DestroyEquipment()
    {
        current_equipment.Destroy();
        Destroy(this);
    }

    /// <summary>
    /// Class global values used while transfering Equipment from chest/inventory to game world.
    /// </summary>
    private DropItem drop_item;
    private Transform player_transform;
    private UIInventory ui_inventory;

    /// <summary>
    /// Drops equipment from given position to world pos.
    /// </summary>
    public void DropEquipment(Vector3 position, float selected_rotation, float force = 5750f)
    {
        drop_item = gameObject.AddComponent<DropItem>();
        drop_item.InitDrop(position, selected_rotation, force, current_equipment.GetDroppedItemShaderStruct(), current_equipment.OnGround);
    }

    /// <summary>
    /// Removes visual components of gameObject and transfers gameObject to player inventory.
    /// </summary>
    public void Pickup()
    {
        // Dropps current equipment and sets variable in PlayerInventoryController
        current_equipment.ObjectPool();
        Vector3 spawn_point = player_transform.position;
        switch (current_equipment.GetType().Name)
        {
            case nameof(Weapon):
                ui_inventory.ChangeWeapon(current_equipment);
                if (PlayerInventory.weapon_equipment != null)
                {
                    PlayerInventory.weapon_equipment.DropEquipment(spawn_point, 360f);
                }
                PlayerInventory.weapon_equipment = this;
                PlayerInventory.weapon_parrent = current_equipment as Weapon;
                PlayerInventory.weapon = PlayerInventory.weapon_parrent.current_weapon;
                break;
            case nameof(Ability):
                ui_inventory.ChangeAbility(current_equipment);
                if (PlayerInventory.ability_equipment != null)
                {
                    PlayerInventory.ability_equipment.DropEquipment(spawn_point, 360f);
                }
                PlayerInventory.ability_equipment = this;
                PlayerInventory.ability_parrent = current_equipment as Ability;
                PlayerInventory.ability = PlayerInventory.ability_parrent.current_ability;
                break;
            case nameof(Ultimate):
                ui_inventory.ChangeUltimate(current_equipment);
                if (PlayerInventory.ultimate_equipment != null)
                {
                    PlayerInventory.ultimate_equipment.DropEquipment(spawn_point, 360f);
                }
                PlayerInventory.ultimate_equipment = this;
                PlayerInventory.ultimate_parrent = current_equipment as Ultimate;
                PlayerInventory.ultimate = PlayerInventory.ultimate_parrent.current_ultimate;

                break;
        }

        drop_item.Pickup();
    }

    /// <summary>
    /// Shows that this equipment can be picked up.
    /// </summary>
    public bool CanInteractWith()
    {
        return true;
    }

    /// <summary>
    /// Retrieves necessary transforms and components.
    /// </summary>
    private void Start()
    {
        player_transform = GameObject.Find("Player").transform;
        ui_inventory = GameObject.Find("UIInventory").GetComponent<UIInventory>();
    }
}
