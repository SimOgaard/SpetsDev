using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script handles how chests respond with player interactions.
/// </summary>
public class ChestInteractable : MonoBehaviour
{
    // Specifies what Equipment this chest contains.
    [SerializeField] private Equipment.EEquipment chest_type;
    [SerializeField] private bool can_open = true;
    private SpriteInitializer sprite_initializer;
    private bool is_open = false;

    private Equipment equipment;
    private Upgrade upgrade;

    private GameObject chest_top;
    private GameObject chest_bot;

    [SerializeField] private float radius = 0f;
    [SerializeField] private float power = 5750;
    [SerializeField] private float upwards_modifier = 5f;
    [SerializeField] private float explosion_offset = 5f;
    [SerializeField] private float chest_top_static_after = 5f;

    /// <summary>
    /// Handles InteractWith function call that is received from InteractableEventHandler.
    /// </summary>
    public void InteractWith()
    {
        if (!can_open)
        {
            Debug.Log("its locked lamao"); // you can set a System.Action to be for example make boss stand up
        }
        else if (!is_open)
        {
            OpenChest();
        }
    }

    /// <summary>
    /// Shows weather or not this chest can be opened
    /// </summary>
    public bool CanInteractWith()
    {
        return can_open;
    }

    /// <summary>
    /// Signals Equipment to spawn in game world.
    /// </summary>
    private void OpenChest()
    {
        is_open = true;
        transform.parent = GameObject.Find("UsedInteractables").transform;

        SpawnChestItemSpecified(chest_type);

        ChestOpeningAnimation();

        // debugging purposes, allows infinite chest openings
        // Start();
    }

    /// <summary>
    /// Visually shows that the chest is used by unclasping chest lid and applying force to it.
    /// Disables further interaction with object.
    /// </summary>
    private void ChestOpeningAnimation()
    {
        sprite_initializer.Destroy();
        chest_top.isStatic = false;
        Rigidbody chest_top_rigidbody = chest_top.AddComponent<Rigidbody>();
        chest_top_rigidbody.AddExplosionForce(power, chest_top.transform.position + new Vector3(Random.Range(-explosion_offset, explosion_offset), 0f, Random.Range(-explosion_offset, explosion_offset)), radius, upwards_modifier);
        // optinally makes chest lid static after given time to minimize compute.
        // StartCoroutine(StaticTop());
    }

    /// <summary>
    /// Loads sprites and creates Equipment game object.
    /// Rotates chest to be right side up and initializes hitboxes.
    /// </summary>
    private void Start()
    {
        is_open = false;
        transform.parent = GameObject.Find("Interactables").transform;
        Sprite not_interacting_with_sprite = Resources.Load<Sprite>("Interactables/not_interacting_with_sprite");
        sprite_initializer = gameObject.AddComponent<SpriteInitializer>();
        sprite_initializer.Initialize(not_interacting_with_sprite, Vector3.zero);

        chest_top = transform.GetChild(1).gameObject;
        chest_bot = transform.GetChild(0).gameObject;

        chest_top.transform.rotation = Quaternion.Euler(-90, 45, 0);
        chest_bot.transform.rotation = Quaternion.Euler(-90, 45, 0);

        chest_top.AddComponent<BoxCollider>();
        chest_bot.AddComponent<BoxCollider>();

        chest_top.isStatic = true;
        chest_bot.isStatic = true;
    }

    /// <summary>
    /// Makes chest lid static after given time period to minimize compute.
    /// </summary>
    private IEnumerator StaticTop()
    {
        yield return new WaitForSeconds(chest_top_static_after);
        Destroy(chest_top.GetComponent<Rigidbody>());
        chest_top.isStatic = true;
    }

    /// <summary>
    /// Dependent on EEquipment initialize local Equipment to random EEquipment_type with or without uppgrades or uppgrade of current EEquipment_type in inventory.
    /// </summary>
    public void SpawnChestItem(Equipment.EEquipment equipment_type)
    {
        switch (equipment_type)
        {
            case Equipment.EEquipment.Weapon:
                if (PlayerInventory.weapon == null || Random.Range(0f, 1f) < Equipment.spawn_equipment_chance)
                {
                    equipment = InitNewEquipment();
                    Weapon.IWeapon cached_weapon = equipment.InitWeapon();

                    int amount_of_upgrades = 10;
                    for (int i = 0; i < amount_of_upgrades; i++)
                    {
                        Upgrade upgrade = InitNewUpgrade();
                        upgrade.Init(cached_weapon);
                        upgrade.DropUpgrade(transform.position, 90);
                    }

                    equipment.DropEquipment(transform.position, 90);
                    break;
                }
                upgrade = InitNewUpgrade();
                upgrade.Init(PlayerInventory.weapon);
                upgrade.DropUpgrade(transform.position, 90);
                break;
            case Equipment.EEquipment.Ability:
                if (PlayerInventory.ability == null || Random.Range(0f, 1f) < Equipment.spawn_equipment_chance)
                {
                    equipment = InitNewEquipment();
                    Ability.IAbility cached_ability = equipment.InitAbility();

                    int amount_of_upgrades = 10;
                    for (int i = 0; i < amount_of_upgrades; i++)
                    {
                        Upgrade upgrade = InitNewUpgrade();
                        upgrade.Init(cached_ability);
                        upgrade.DropUpgrade(transform.position, 90);
                    }

                    equipment.DropEquipment(transform.position, 90);
                    break;
                }
                upgrade = InitNewUpgrade();
                upgrade.Init(PlayerInventory.ability);
                upgrade.DropUpgrade(transform.position, 90);
                break;
            case Equipment.EEquipment.Ultimate:
                if (PlayerInventory.ultimate == null || Random.Range(0f, 1f) < Equipment.spawn_equipment_chance)
                {
                    equipment = InitNewEquipment();
                    Ultimate.IUltimate cached_ultimate = equipment.InitUltimate();

                    int amount_of_upgrades = 10;
                    for (int i = 0; i < amount_of_upgrades; i++)
                    {
                        Upgrade upgrade = InitNewUpgrade();
                        upgrade.Init(cached_ultimate);
                        upgrade.DropUpgrade(transform.position, 90);
                    }

                    equipment.DropEquipment(transform.position, 90);
                    break;
                }
                upgrade = InitNewUpgrade();
                upgrade.Init(PlayerInventory.ultimate);
                upgrade.DropUpgrade(transform.position, 90);
                break;
        }
    }

    /// <summary>
    /// Initializes local Equipment component to specified Equipment parent with specified child. (for easy debugging)
    /// </summary>
    public void SpawnChestItemSpecified(Equipment.EEquipment equipment_type)
    {
        equipment = InitNewEquipment();
        switch (equipment_type)
        {
            case Equipment.EEquipment.Weapon:
                Weapon cached_weapon = equipment.gameObject.AddComponent<Weapon>();
                equipment.current_equipment = cached_weapon;
                cached_weapon.current_weapon = equipment.gameObject.AddComponent<HammerWeapon>();
                break;
            case Equipment.EEquipment.Ability:
                Ability cached_ability = equipment.gameObject.AddComponent<Ability>();
                equipment.current_equipment = cached_ability;
                cached_ability.current_ability = equipment.gameObject.AddComponent<EarthSpikesAbility>();
                break;
            case Equipment.EEquipment.Ultimate:
                Ultimate cached_ultimate = equipment.gameObject.AddComponent<Ultimate>();
                equipment.current_equipment = cached_ultimate;
                cached_ultimate.current_ultimate = equipment.gameObject.AddComponent<EarthbendingUltimate>();
                break;
        }
        equipment.DropEquipment(transform.position, 90);
    }

    public Equipment InitNewEquipment()
    {
        GameObject equipment_game_object = new GameObject("equipment_game_object");
        return equipment_game_object.AddComponent<Equipment>();
    }

    public Upgrade InitNewUpgrade()
    {
        GameObject upgrade_game_object = new GameObject("upgrade_game_object");
        return upgrade_game_object.AddComponent<Upgrade>();
    }
}
