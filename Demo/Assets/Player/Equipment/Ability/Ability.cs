using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script is parent component of the Equipment hierarchy.
/// Is one of the three Equipments and routes further all calls regarding its child component (current_ability).
/// Holds all shared functions for Equipment "Ability".
/// </summary>
public class Ability : MonoBehaviour, Equipment.IEquipment
{
    // Current Ability this component controlls.
    public IAbility current_ability;

    /// <summary>
    /// Rulesets for all children of Ability to follow.
    /// </summary>
    public interface IAbility
    {
        void Destroy();
        void OnGround();
        void UsePrimary();
        void StopPrimary();
        void ObjectPool();
        void DeleteObjectPool();
        Sprite GetIconSprite();
        void Upgrade();
        float GetCurrentCooldown();
        float GetCooldown();
    }

    /// <summary>
    /// Further transmits Destroy function from Master to child and then destroys itself.
    /// </summary>
    public void Destroy()
    {
        current_ability.Destroy();
        Destroy(this);
    }

    /// <summary>
    /// Unique shader data holding colors, time and width for when object of this type is dropped.
    /// </summary>
    public DropItem.DroppedItemShaderStruct GetDroppedItemShaderStruct()
    {
        DropItem.DroppedItemShaderStruct shader_struct;
        shader_struct.time = 0.2f;
        shader_struct.start_width = 0.75f;
        shader_struct.end_width = 0.1f;
        shader_struct.color = new Color[2] { Color.magenta, Color.red };
        shader_struct.alpha = new float[2] { 1f, 0.1f };
        shader_struct.trail_material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
        shader_struct.material = new Material(Shader.Find("Custom/ItemTestShader"));
        shader_struct.material.SetColor("_Color", new Color(1f, 0.85f, 1f));
        return shader_struct;
    }

    /// <summary>
    /// Further transmits OnGround function from Master to child.
    /// </summary>
    public void OnGround()
    {
        current_ability.OnGround();
    }

    /// <summary>
    /// Further transmits UsePrimary function from Master to child.
    /// </summary>
    public void UsePrimary()
    {
        current_ability.UsePrimary();
    }

    /// <summary>
    /// Gets icon for ui element of ability.
    /// </summary>
    public Sprite GetIconSprite()
    {
        return current_ability.GetIconSprite();
    }

    /// <summary>
    /// Returns current cooldown of equipment.
    /// </summary>
    public float GetCurrentCooldown()
    {
        return current_ability.GetCurrentCooldown();
    }
    /// <summary>
    /// Returns cooldown of equipment.
    /// </summary>
    public float GetCooldown()
    {
        return current_ability.GetCooldown();
    }

    /// <summary>
    /// Further transmits ObjectPool and DeleteObjectPool function from Master to child.
    /// </summary>
    public void ObjectPool()
    {
        current_ability.ObjectPool();
    }
    public void DeleteObjectPool()
    {
        current_ability.DeleteObjectPool();
    }
}
