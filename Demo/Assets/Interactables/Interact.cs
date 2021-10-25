using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interact : MonoBehaviour
{
    [SerializeField] private bool awake = true;

    private void Awake()
    {
        if (!awake)
        {
            return;
        }
        GameObject interactables = new GameObject("Interactables");
        GameObject items = new GameObject("Items");
        GameObject used_interactables = new GameObject("UsedInteractables");

        interactables.transform.parent = transform;
        items.transform.parent = transform;
        used_interactables.transform.parent = transform;

        interactables.AddComponent<InteractableEventHandler>();

        GameObject items_in_air = new GameObject("ItemsInAir");
        GameObject items_on_ground = new GameObject("ItemsOnGround");
        items_in_air.transform.parent = items.transform;
        items_on_ground.transform.parent = items.transform;
    }
}
