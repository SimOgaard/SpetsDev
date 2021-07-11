using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script handles how chests respond with player interactions.
/// </summary>
public class ChestInteractable : MonoBehaviour
{
    // Specifies what Equipment this chest contains.
    [SerializeField]
    private Equipment.EEquipment chest_type;

    private GameObject equipment_game_object;
    private Equipment equipment;
    private SpriteInitializer sprite_initializer;

    private bool is_open = false;

    /// <summary>
    /// Handles InteractWith function call that is received from InteractableEventHandler.
    /// </summary>
    public void InteractWith()
    {
        if (!is_open)
        {
            OpenChest();
        }
    }

    /// <summary>
    /// Signals Equipment to spawn in game world and disables InteractWith function.
    /// </summary>
    private void OpenChest()
    {
        is_open = true;
        transform.parent = GameObject.Find("UsedInteractables").transform;
        sprite_initializer.Destroy();
        equipment.InitEquipment(chest_type);
        equipment.DropEquipment(transform.position, 90);
        ChestOpeningAnimation();

        // debugging purposes, allows infinite chest openings
        Start();
    }

    /// <summary>
    /// visually shows that the chest is used.
    /// </summary>
    private void ChestOpeningAnimation()
    {

    }

    /// <summary>
    /// Loads sprites and creates Equipment game object.
    /// </summary>
    private void Start()
    {
        is_open = false;
        transform.parent = GameObject.Find("Interactables").transform;
        equipment_game_object = new GameObject("equipment_game_object");
        equipment_game_object.transform.parent = GameObject.Find("EquipmentsInChests").transform;
        equipment = equipment_game_object.AddComponent<Equipment>();
        Sprite not_interacting_with_sprite = Resources.Load<Sprite>("Interactables/not_interacting_with_sprite");
        sprite_initializer = gameObject.AddComponent<SpriteInitializer>();
        sprite_initializer.Initialize(not_interacting_with_sprite, Vector3.zero);
    }
}
