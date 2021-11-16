using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script is master component of Equipment hierarchy.
/// Initializes equipment in game world and routes all specific Equipment calls to current_equipment (Equipment parent).
/// </summary>
public class Equipment : Item
{
    public interface IEquipment
    {
        void UsePrimary();
        void StopPrimary();

        void Drop(Vector3 position, Vector3 thrust);
        Vector3 Thrust(float selected_rotation, Vector3 forward_vector, float force = 5750f);
        
        void InteractWith();
        bool allows_interaction { get; }

        void UpdateUI();

        float cooldown { get; set; }
        float current_cooldown { get; set; }
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
    private float _current_cooldown;
    public float current_cooldown
    {
        get { return _current_cooldown; }
        set { _current_cooldown = Mathf.Max(0f, value); }
    }
    #endregion

    #region interact
    public override void InteractWith()
    {
        base.InteractWith();
        transform.parent = Global.equipments_in_inventory;
        UpdateUI();
    }
    #endregion

    #region UI
    public virtual void UpdateUI()
    {
    }
    #endregion

    #region initialize
    public static System.Type[] equipment_types = { typeof(Weapon), typeof(Ability), typeof(Ultimate) };
    public static System.Type RandomEquipment()
    {
        System.Type equipment_type = equipment_types[Random.Range(0, equipment_types.Length)];
        return RandomEquipment(equipment_type);
    }
    public static System.Type RandomEquipment(System.Type equipment_type)
    {
        return equipment_type.GetMethod("RandomEquipment").Invoke(null, null) as System.Type;
    }

    public static (GameObject, IEquipment) CreateRandomEquipment()
    {
        System.Type equipment_type = RandomEquipment();
        return CreateRandomEquipment(equipment_type);
    }
    public static (GameObject, IEquipment) CreateRandomEquipment(System.Type equipment_type)
    {
        GameObject equipment_game_object = new GameObject(equipment_type.Name);
        IEquipment equipment = equipment_game_object.AddComponent(equipment_type) as IEquipment;
        return (equipment_game_object, equipment);
    }
    #endregion
}
