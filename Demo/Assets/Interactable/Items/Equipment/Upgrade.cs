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
        Vector3 Thrust(float selectedRotation, Vector3 forwardVector, float force = 5750f);
    }

    #region initialize
    public static (GameObject, IUpgrade) CreateRandomUpgrade()
    {
        System.Type equipmentType = Equipment.RandomEquipment();
        return CreateRandomUpgrade(equipmentType);
    }
    public static (GameObject, IUpgrade) CreateRandomUpgrade(System.Type equipmentType)
    {
        GameObject equipmentGameObject = new GameObject(equipmentType.Name, equipmentType);
        IUpgrade equipment = equipmentGameObject.AddComponent(equipmentType) as IUpgrade;
        return (equipmentGameObject, equipment);
    }
    #endregion
}