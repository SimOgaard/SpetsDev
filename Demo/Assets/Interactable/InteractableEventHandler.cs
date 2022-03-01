using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script handles all game objects that player can interact with.
/// </summary>
public class InteractableEventHandler : MonoBehaviour
{
    private Interactable nearestInteractable = null;

    /// <summary>
    /// Iterates all interactable game objects and returns closest object to player within distance threshold.
    /// </summary>
    private Interactable GetNearestInteractable()
    {
        Interactable closestInteractable = null;
        Interactable closestNotInteractable = null;
        float minDistanceInteractable = Mathf.Infinity;
        float minDistanceNotInteractable = Mathf.Infinity;

        foreach (Interactable interactable in Interactable.interactables)
        {
            Transform interactableTransform = interactable.transform;
            float currentDistance = (interactableTransform.position - Global.playerTransform.position).magnitude;
            if (currentDistance > interactable.playerMinDistance)
            {
                continue;
            }
            if (interactable.allowsInteraction && currentDistance < minDistanceInteractable)
            {
                closestInteractable = interactable;
                minDistanceInteractable = currentDistance;
            }
            else if (currentDistance < minDistanceNotInteractable)
            {
                closestNotInteractable = interactable;
                minDistanceNotInteractable = currentDistance;
            }
        }

        if (closestInteractable != null)
        {
            return closestInteractable;
        }
        else if (closestNotInteractable != null)
        {
            return closestNotInteractable;
        }
        return null;
    }

    /// <summary>
    /// Waits for input that is used to interact with game objects.
    /// </summary>
    private void Update()
    {
        Interactable newNearestInteractable = GetNearestInteractable();
        if (nearestInteractable != newNearestInteractable)
        {
            if (nearestInteractable != null)
            {
                nearestInteractable.ChangeInteractingSprite(Global.notInteractingWithSprite);
            }
            if (newNearestInteractable != null)
            {
                newNearestInteractable.ChangeInteractingSprite(Global.interactingWithSprite);
            }
            nearestInteractable = newNearestInteractable;
        }

        if (nearestInteractable != null)
        {
            if (PlayerInput.GetKeyDown(PlayerInput.interactKey))
            {
                if (nearestInteractable.CanInteractWith())
                {
                    nearestInteractable.InteractWith();
                }
            }
        }
    }

    private void Awake()
    {
         Item.droppedShaderPrefab = Resources.Load<GameObject>("DroppedShader/Default") as GameObject;
    }
}
