using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Upgrade : MonoBehaviour
{
    private DropItem drop_item;

    public string cached_upgrade_parrent_name = "";

    /// <summary>
    /// Initializes upgrade to be an upgrade to given weapon.
    /// </summary>
    public void Init(Weapon.IWeapon weapon)
    {
        cached_upgrade_parrent_name = weapon.GetType().Name;
    }

    /// <summary>
    /// Initializes upgrade to be an upgrade to given ability.
    /// </summary>
    public void Init(Ability.IAbility ability)
    {
        cached_upgrade_parrent_name = ability.GetType().Name;
    }

    /// <summary>
    /// Initializes upgrade to be an upgrade to given ultimate.
    /// </summary>
    public void Init(Ultimate.IUltimate ultimate)
    {
        cached_upgrade_parrent_name = ultimate.GetType().Name;
    }

    /// <summary>
    /// Drops upgrade from given position to world pos.
    /// </summary>
    public void DropUpgrade(Vector3 position, float selected_rotation, float force = 5750f)
    {
        drop_item = gameObject.AddComponent<DropItem>();
        drop_item.InitDrop(position, GetDroppedItemShaderStruct(), OnGround);
        drop_item.Drop(selected_rotation, force);
    }

    /// <summary>
    /// Drops upgrade from given position to world pos.
    /// </summary>
    public void DropUpgrade(Vector3 position, float selected_rotation, Vector3 direction, float force = 5750f)
    {
        drop_item = gameObject.AddComponent<DropItem>();
        drop_item.InitDrop(position, GetDroppedItemShaderStruct(), OnGround);
        drop_item.Drop(selected_rotation, force, direction);
    }

    /// <summary>
    /// Signals that upgrade has hit ground.
    /// </summary>
    public void OnGround()
    {
        GetComponent<MeshRenderer>().material.SetColor("_Color", new Color(240f / 255f, 230f / 255f, 140f / 255f));
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
        shader_struct.color = new Color[2] { new Color(218f / 255f, 165f / 255f, 32f / 255f), new Color(238f / 255f, 232f / 255f, 170f / 255f) };
        shader_struct.alpha = new float[2] { 1f, 0.1f };
        shader_struct.trail_material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
        shader_struct.material = new Material(Shader.Find(DropItem.drop_item_shader_tag));
        shader_struct.material.SetColor("_Color", new Color(250f / 255f, 250f / 255f, 210f / 255f));
        return shader_struct;
    }

    /// <summary>
    /// Removes gameobject of upgrade and upgrades equipment.
    /// </summary>
    public void Pickup()
    {
        if (PlayerInventory.weapon != null && cached_upgrade_parrent_name == PlayerInventory.weapon.GetType().Name)
        {
            PlayerInventory.weapon.Upgrade();
            Destroy(gameObject);
        }
        else if (PlayerInventory.ability != null && cached_upgrade_parrent_name == PlayerInventory.ability.GetType().Name)
        {
            PlayerInventory.ability.Upgrade();
            Destroy(gameObject);
        }
        else if (PlayerInventory.ultimate != null && cached_upgrade_parrent_name == PlayerInventory.ultimate.GetType().Name)
        {
            PlayerInventory.ultimate.Upgrade();
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("You are not holding any equipment that can use this upgrade");
        }
    }

    /// <summary>
    /// Shows weather or not this upgrade can be taken
    /// </summary>
    public bool CanInteractWith()
    {
        if (PlayerInventory.weapon != null && cached_upgrade_parrent_name == PlayerInventory.weapon.GetType().Name)
        {
            return true;
        }
        else if (PlayerInventory.ability != null && cached_upgrade_parrent_name == PlayerInventory.ability.GetType().Name)
        {
            return true;
        }
        else if (PlayerInventory.ultimate != null && cached_upgrade_parrent_name == PlayerInventory.ultimate.GetType().Name)
        {
            return true;
        }
        return false;
    }
}