﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script is parent component of the Equipment hierarchy.
/// Is one of the three Equipments and routes further all calls regarding its child component (current_ultimate).
/// Holds all shared functions for Equipment "Ultimate".
/// </summary>
public class Ultimate : MonoBehaviour, Equipment.IEquipment
{
    // Current Ultimate this component controlls.
    public IUltimate current_ultimate;

    /// <summary>
    /// Rulesets for all children of Ultimate to follow.
    /// </summary>
    public interface IUltimate
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
        current_ultimate.Destroy();
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
        shader_struct.color = new Color[2] { Color.cyan, Color.blue };
        shader_struct.alpha = new float[2] { 1f, 0.1f };
        shader_struct.trail_material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
        shader_struct.material = new Material(Shader.Find(DropItem.drop_item_shader_tag));
        shader_struct.material.SetColor("_Color", new Color(0.85f, 1f, 1f));
        return shader_struct;
    }

    /// <summary>
    /// Further transmits OnGround function from Master to child.
    /// </summary>
    public void OnGround()
    {
        current_ultimate.OnGround();
    }

    /// <summary>
    /// Further transmits UsePrimary function from Master to child.
    /// </summary>
    public void UsePrimary()
    {
        current_ultimate.UsePrimary();
    }

    /// <summary>
    /// Gets icon for ui element of ultimate.
    /// </summary>
    public Sprite GetIconSprite()
    {
        return current_ultimate.GetIconSprite();
    }

    /// <summary>
    /// Returns current cooldown of equipment.
    /// </summary>
    public float GetCurrentCooldown()
    {
        return current_ultimate.GetCurrentCooldown();
    }
    /// <summary>
    /// Returns cooldown of equipment.
    /// </summary>
    public float GetCooldown()
    {
        return current_ultimate.GetCooldown();
    }

    /// <summary>
    /// Further transmits ObjectPool and DeleteObjectPool function from Master to child.
    /// </summary>
    public void ObjectPool()
    {
        current_ultimate.ObjectPool();
    }
    public void DeleteObjectPool()
    {
        current_ultimate.DeleteObjectPool();
    }
}
