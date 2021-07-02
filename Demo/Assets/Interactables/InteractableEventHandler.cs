using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script handles all game objects that player can interact with.
/// </summary>
public class InteractableEventHandler : MonoBehaviour
{
    [SerializeField]
    private Transform player_transform;

    [SerializeField]
    private float player_min_distance = 5f;
    private float player_min_distance_squared;

    private Sprite interacting_with_sprite;
    private Sprite not_interacting_with_sprite;

    private GameObject nearest_interactable = null;
    private GameObject last_nearest_interactable = null;

    /// <summary>
    /// Iterates all interactable game objects and returns closest object to player within distance threshold.
    /// </summary>
    private GameObject GetNearestInteractable()
    {
        Transform closest_interactable = null;
        float min_squared_distance = float.MaxValue;
        float current_squared_distance = 0f;

        foreach (Transform interactable_child in transform)
        {
            current_squared_distance = (interactable_child.position - player_transform.position).sqrMagnitude;
            if (current_squared_distance < min_squared_distance)
            {
                closest_interactable = interactable_child;
                min_squared_distance = current_squared_distance;
            }
        }

        foreach (Transform interactable_child in GameObject.Find("EquipmentsOnGround").transform)
        {
            current_squared_distance = (interactable_child.position - player_transform.position).sqrMagnitude;
            if (current_squared_distance < min_squared_distance)
            {
                closest_interactable = interactable_child;
                min_squared_distance = current_squared_distance;
            }
        }

        return closest_interactable == null || min_squared_distance > player_min_distance_squared ? null : closest_interactable.gameObject;
    }

    /// <summary>
    /// Changes sprite that shows if player can interact with object.
    /// </summary>
    private void ChangeInteractableUserInterfaceState(GameObject interactable_game_object, GameObject last_interactable_game_object)
    {
        if (interactable_game_object != null)
        {
            SpriteInitializer interactable_game_object_sprite_initializer = interactable_game_object.GetComponent<SpriteInitializer>();
            interactable_game_object_sprite_initializer.ChangeRender(interacting_with_sprite);
        }

        if (last_interactable_game_object != null)
        {
            if (last_interactable_game_object.TryGetComponent(out SpriteInitializer last_interactable_game_object_sprite_initializer))
            {
                last_interactable_game_object_sprite_initializer.ChangeRender(not_interacting_with_sprite);
            }
        }
    }

    /// <summary> AAAAAAAH
    /// Finds specific component on nearest interactable game object that satisfies .
    /// </summary>
    private void InteractWith(GameObject interactable_game_object) // USE INTERFACE?
    {
        if (interactable_game_object.TryGetComponent(out ChestInteractable chest_interactable))
        {
            chest_interactable.InteractWith();
        }
        else if (interactable_game_object.TryGetComponent(out Equipment equipment_interactable))
        {
            equipment_interactable.Pickup();
        }
    }

    /// <summary>
    /// Loads sprites and squares player_min_distance to reduce compute.
    /// </summary>
    private void Start()
    {
        player_min_distance_squared = player_min_distance * player_min_distance;
        interacting_with_sprite = Resources.Load<Sprite>("Interactables/interacting_with_sprite");
        not_interacting_with_sprite = Resources.Load<Sprite>("Interactables/not_interacting_with_sprite");
    }

    /// <summary>
    /// Waits for input that is used to interact with game objects.
    /// </summary>
    private void Update()
    {
        nearest_interactable = GetNearestInteractable();

        if (nearest_interactable != null)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                InteractWith(nearest_interactable);
            }
        }

        if (nearest_interactable != last_nearest_interactable)
        {
            ChangeInteractableUserInterfaceState(nearest_interactable, last_nearest_interactable);
            last_nearest_interactable = nearest_interactable;
        }
    }
}
