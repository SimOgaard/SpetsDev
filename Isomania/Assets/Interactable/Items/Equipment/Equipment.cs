using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script is master component of Equipment hierarchy.
/// Initializes equipment in game world and routes all specific Equipment calls to currentEquipment (Equipment parent).
/// </summary>
public class Equipment : Item
{
    public interface IEquipment
    {
        void UsePrimary();
        void StopPrimary();

        void Drop(Vector3 position, Vector3 thrust);
        Vector3 Thrust(float selectedRotation, Vector3 forwardVector, float force = 5750f);
        
        void InteractWith();
        bool allowsInteraction { get; }

        void UpdateUI();

        float cooldown { get; set; }
        float currentCooldown { get; set; }
    }

    #region use
    public virtual void UsePrimary()
    {
    }
    public virtual void StopPrimary()
    {
    }
    #endregion

    #region cooldown
    private float _cooldown;
    public float cooldown
    {
        get { return _cooldown; }
        set { _cooldown = Mathf.Max(0f, value); }
    }
    private float _currentCooldown;
    public float currentCooldown
    {
        get { return _currentCooldown; }
        set { _currentCooldown = Mathf.Max(0f, value); }
    }
    #endregion

    #region interact
    public override void InteractWith()
    {
        base.InteractWith();
        transform.parent = Global.equipmentsInInventoryTransform;
        UpdateUI();
    }
    #endregion

    #region UI
    public virtual void UpdateUI()
    {
    }
    #endregion

    #region initialize
    public static System.Type[] equipmentTypes = { typeof(Weapon), typeof(Ability), typeof(Ultimate) };
    public static System.Type RandomEquipment()
    {
        System.Type equipmentType = equipmentTypes[Random.Range(0, equipmentTypes.Length)];
        return RandomEquipment(equipmentType);
    }
    public static System.Type RandomEquipment(System.Type equipmentType)
    {
        return equipmentType.GetMethod("RandomEquipment").Invoke(null, null) as System.Type;
    }

    public static (GameObject, IEquipment) CreateRandomEquipment()
    {
        System.Type equipmentType = RandomEquipment();
        return CreateRandomEquipment(equipmentType);
    }
    public static (GameObject, IEquipment) CreateRandomEquipment(System.Type equipmentType)
    {
        GameObject equipmentGameObject = new GameObject(equipmentType.Name);
        IEquipment equipment = equipmentGameObject.AddComponent(equipmentType) as IEquipment;
        return (equipmentGameObject, equipment);
    }
    #endregion
}
