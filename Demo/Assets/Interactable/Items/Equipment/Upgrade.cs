using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Upgrade : Item
{
    public interface IUpgrade
    {
        void Apply();
        string AppliesTo();
        int amount { get; set; }

        void Drop(Vector3 position, Vector3 thrust);
        Vector3 Thrust(float selected_rotation, Vector3 forward_vector, float force = 5750f);
    }

    #region initialize
    public static (GameObject, IUpgrade) CreateRandomUpgrade()
    {
        System.Type equipment_type = Equipment.RandomEquipment();
        return CreateRandomUpgrade(equipment_type);
    }
    public static (GameObject, IUpgrade) CreateRandomUpgrade(System.Type equipment_type)
    {
        GameObject equipment_game_object = new GameObject(equipment_type.Name, equipment_type);
        IUpgrade equipment = equipment_game_object.AddComponent(equipment_type) as IUpgrade;
        return (equipment_game_object, equipment);
    }
    #endregion
}