using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// this script spawns equipments into game world
public class EquipmentSpawner : MonoBehaviour
{
    public GameObject GetEquipmentGameObject(Equipment.Type spawning_equipment_type)
    {
        //GameObject equipment_game_object = new GameObject();
        GameObject equipment_game_object = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Equipment equipment = equipment_game_object.AddComponent<Equipment>();
        equipment.EquipmentInAir(spawning_equipment_type);
        return equipment_game_object;
    }

    public void SpawnEquipment(Equipment.Type equipment_type, Vector3 position, float selected_rotation, float force = 11500f)
    {
        Quaternion rotation = Quaternion.Euler(-Random.Range(72.5f, 82.5f), Random.Range(-selected_rotation * 0.5f + 180f, selected_rotation * 0.5f + 180f), 0);
        Vector3 thrust = rotation * Vector3.forward * force;

        GameObject equipment_game_object = GetEquipmentGameObject(equipment_type);
        equipment_game_object.transform.position = position;
        equipment_game_object.GetComponent<Rigidbody>().AddForce(thrust);
    }
}
