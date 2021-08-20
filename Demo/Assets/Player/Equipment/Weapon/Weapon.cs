﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script is parent component of the Equipment hierarchy.
/// Is one of the three Equipments and routes further all calls regarding its child component (current_weapon).
/// Holds all shared functions for Equipment "Weapon".
/// </summary>
public class Weapon : MonoBehaviour, Equipment.IEquipment
{
    // Current Weapon this component controlls.
    public IWeapon current_weapon;

    /// <summary>
    /// Rulesets for all children of Weapon to follow.
    /// </summary>
    public interface IWeapon
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
        current_weapon.Destroy();
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
        shader_struct.color = new Color[2] { Color.red, Color.yellow };
        shader_struct.alpha = new float[2] { 1f, 0.1f };
        shader_struct.trail_material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
        shader_struct.material = new Material(Shader.Find(DropItem.drop_item_shader_tag));
        shader_struct.material.SetColor("_Color", new Color(1f, 0.85f, 0.85f));
        return shader_struct;
    }

    /// <summary>
    /// Further transmits OnGround function from Master to child.
    /// </summary>
    public void OnGround()
    {
        current_weapon.OnGround();
    }

    /// <summary>
    /// Further transmits UsePrimary function from Master to child.
    /// </summary>
    public void UsePrimary()
    {
        current_weapon.UsePrimary();
    }

    /// <summary>
    /// Gets icon for ui element of weapon.
    /// </summary>
    public Sprite GetIconSprite()
    {
        return current_weapon.GetIconSprite();
    }

    /// <summary>
    /// Returns current cooldown of equipment.
    /// </summary>
    public float GetCurrentCooldown()
    {
        return current_weapon.GetCurrentCooldown();
    }
    /// <summary>
    /// Returns cooldown of equipment.
    /// </summary>
    public float GetCooldown()
    {
        return current_weapon.GetCooldown();
    }

    /// <summary>
    /// Further transmits ObjectPool and DeleteObjectPool function from Master to child.
    /// </summary>
    public void ObjectPool()
    {
        current_weapon.ObjectPool();
    }
    public void DeleteObjectPool()
    {
        current_weapon.DeleteObjectPool();
    }
}
