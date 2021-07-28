using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script handles all game objects that player can interact with.
/// </summary>
public class InteractableEventHandler : MonoBehaviour
{
    [SerializeField] private Transform player_transform;

    [SerializeField] private float player_min_distance = 10f;
    private float player_min_distance_squared;

    private Sprite interacting_with_sprite;
    private Sprite not_interacting_with_sprite;

    private GameObject nearest_interactable = null;
    private GameObject last_nearest_interactable = null;

    private Transform items_on_ground;

    /// <summary>
    /// Iterates all interactable game objects and returns closest object to player within distance threshold.
    /// </summary>
    private GameObject GetNearestInteractable()
    {
        Transform closest_interactable = null;
        Transform closest_not_interactable = null;
        float min_squared_distance_interactable = player_min_distance_squared;
        float min_squared_distance_not_interactable = player_min_distance_squared;
        float current_squared_distance = 0f;

        foreach (Transform interactable_child in transform)
        {
            current_squared_distance = (interactable_child.position - player_transform.position).sqrMagnitude;
            bool can_interact_with = TryInteractWith(interactable_child.gameObject);
            if (current_squared_distance <= min_squared_distance_interactable && can_interact_with)
            {
                closest_interactable = interactable_child;
                min_squared_distance_interactable = current_squared_distance;
            }
            if (current_squared_distance <= min_squared_distance_not_interactable && !can_interact_with)
            {
                closest_not_interactable = interactable_child;
                min_squared_distance_not_interactable = current_squared_distance;
            }
        }

        foreach (Transform interactable_child in items_on_ground)
        {
            current_squared_distance = (interactable_child.position - player_transform.position).sqrMagnitude;
            bool can_interact_with = TryInteractWith(interactable_child.gameObject);
            if (current_squared_distance <= min_squared_distance_interactable && can_interact_with)
            {
                closest_interactable = interactable_child;
                min_squared_distance_interactable = current_squared_distance;
            }
            if (current_squared_distance <= min_squared_distance_not_interactable && !can_interact_with)
            {
                closest_not_interactable = interactable_child;
                min_squared_distance_not_interactable = current_squared_distance;
            }
        }

        if (closest_interactable != null)
        {
            return closest_interactable.gameObject;
        }
        else if (closest_not_interactable != null)
        {
            return closest_not_interactable.gameObject;
        }
        return null;
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

    /// <summary>
    /// Finds specific component on nearest interactable game object that satisfies .
    /// </summary>
    private void InteractWith(GameObject interactable_game_object)
    {
        if (interactable_game_object.TryGetComponent(out ChestInteractable chest_interactable))
        {
            chest_interactable.InteractWith();
        }
        else if (interactable_game_object.TryGetComponent(out Equipment equipment_interactable))
        {
            equipment_interactable.Pickup();
        }
        else if (interactable_game_object.TryGetComponent(out Upgrade upgrade_interactable))
        {
            upgrade_interactable.Pickup();
        }
    }

    /// <summary>
    /// Returns boolean of if you are able to interact with given game object
    /// </summary>
    private bool TryInteractWith(GameObject interactable_game_object)
    {
        if (interactable_game_object.TryGetComponent(out ChestInteractable chest_interactable))
        {
            return chest_interactable.CanInteractWith();
        }
        else if (interactable_game_object.TryGetComponent(out Equipment equipment_interactable))
        {
            return equipment_interactable.CanInteractWith();
        }
        else if (interactable_game_object.TryGetComponent(out Upgrade upgrade_interactable))
        {
            return upgrade_interactable.CanInteractWith();
        }
        return false;
    }

    /// <summary>
    /// Loads sprites and squares player_min_distance to reduce compute.
    /// </summary>
    private void Start()
    {
        player_min_distance_squared = player_min_distance * player_min_distance;
        interacting_with_sprite = Resources.Load<Sprite>("Interactables/interacting_with_sprite");
        not_interacting_with_sprite = Resources.Load<Sprite>("Interactables/not_interacting_with_sprite");
        items_on_ground = GameObject.Find("ItemsOnGround").transform;
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
