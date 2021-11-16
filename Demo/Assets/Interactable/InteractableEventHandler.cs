using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script handles all game objects that player can interact with.
/// </summary>
public class InteractableEventHandler : MonoBehaviour
{
    private Interactable nearest_interactable = null;

    /// <summary>
    /// Iterates all interactable game objects and returns closest object to player within distance threshold.
    /// </summary>
    private Interactable GetNearestInteractable()
    {
        Interactable closest_interactable = null;
        Interactable closest_not_interactable = null;
        float min_distance_interactable = Mathf.Infinity;
        float min_distance_not_interactable = Mathf.Infinity;

        foreach (Interactable interactable in Interactable.interactables)
        {
            Transform interactable_transform = interactable.transform;
            float current_distance = (interactable_transform.position - Global.player_transform.position).magnitude;
            if (current_distance > interactable.player_min_distance)
            {
                continue;
            }
            if (interactable.allows_interaction && current_distance < min_distance_interactable)
            {
                closest_interactable = interactable;
                min_distance_interactable = current_distance;
            }
            else if (current_distance < min_distance_not_interactable)
            {
                closest_not_interactable = interactable;
                min_distance_not_interactable = current_distance;
            }
        }

        if (closest_interactable != null)
        {
            return closest_interactable;
        }
        else if (closest_not_interactable != null)
        {
            return closest_not_interactable;
        }
        return null;
    }

    /// <summary>
    /// Waits for input that is used to interact with game objects.
    /// </summary>
    private void Update()
    {
        Interactable new_nearest_interactable = GetNearestInteractable();
        if (nearest_interactable != new_nearest_interactable)
        {
            if (nearest_interactable != null)
            {
                nearest_interactable.ChangeInteractingSprite(Global.not_interacting_with_sprite);
            }
            if (new_nearest_interactable != null)
            {
                new_nearest_interactable.ChangeInteractingSprite(Global.interacting_with_sprite);
            }
            nearest_interactable = new_nearest_interactable;
        }

        if (nearest_interactable != null)
        {
            if (Input.GetKeyDown(PlayerInput.interact_key))
            {
                if (nearest_interactable.CanInteractWith())
                {
                    nearest_interactable.InteractWith();
                }
            }
        }
    }

    private void Awake()
    {
         Item.dropped_shader_prefab = Resources.Load<GameObject>("DroppedShader/Default") as GameObject;
    }
}
