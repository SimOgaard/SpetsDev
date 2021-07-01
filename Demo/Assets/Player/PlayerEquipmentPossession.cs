using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEquipmentPossession : MonoBehaviour
{
    private EquipmentSpawner player_equipment_spawner;

    private void Start()
    {
        player_equipment_spawner = gameObject.AddComponent<EquipmentSpawner>();
    }

    private void Update()
    {
    }

    private void DropEquipment()
    {
        // remove from "inventory"
        player_equipment_spawner.SpawnEquipment(Equipment.Type.test, transform.position, 360f);
    }
}
