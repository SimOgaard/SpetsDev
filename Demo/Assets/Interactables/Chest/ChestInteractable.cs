using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChestInteractable : MonoBehaviour
{
    private EquipmentSpawner equipment_spawner;

    private bool is_open = false;

    public void InteractWith()
    {
        if (!is_open)
        {
            OpenChest();
        }
    }

    private void OpenChest()
    {
        //is_open = true;
        equipment_spawner.SpawnEquipment(Equipment.Type.test, transform.position, 90);
        ChestOpeningAnimation();
    }

    private void ChestOpeningAnimation()
    {

    }

    private void Start()
    {
        equipment_spawner = gameObject.AddComponent<EquipmentSpawner>();
    }
}
