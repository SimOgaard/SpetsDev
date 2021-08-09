using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script is child component of the Equipment hierarchy.
/// Holds specific functionallity and uppgrades for HammerWeapon Equipment.
/// </summary>
public class HammerWeapon : MonoBehaviour, Weapon.IWeapon
{
    public bool upgrade = false;

    /// <summary>
    /// Destroys itself.
    /// </summary>
    public void Destroy()
    {
        Destroy(this);
    }

    /// <summary>
    /// Visualizes that transmission of this fucntion reached this child component.
    /// </summary>
    public void OnGround()
    {
        transform.GetComponent<MeshRenderer>().material.SetColor("_Color", Color.red);
    }

    /// <summary>
    /// Starts to use this weapon.
    /// </summary>
    public void UsePrimary()
    {
        Debug.Log("hammered the ground");
    }

    /// <summary>
    /// Stops this weapon.
    /// </summary>
    public void StopPrimary()
    {

    }

    private void Update()
    {
        if (upgrade)
        {
            Debug.Log("Uppgraded to new variables on " + GetType().Name);
            upgrade = false;
            Upgrade();
        }
    }

    /// <summary>
    /// Returns hammer icon for ui element.
    /// </summary>
    public Sprite GetIconSprite()
    {
        return icon_sprite;
    }

    private Sprite icon_sprite;
    private void Start()
    {
        icon_sprite = Resources.Load<Sprite>("Sprites/UI/hammer");
    }

    /// <summary>
    /// Returns current cooldown of equipment.
    /// </summary>
    public float GetCurrentCooldown()
    {
        return 0;
    }
    /// <summary>
    /// Returns cooldown of equipment.
    /// </summary>
    public float GetCooldown()
    {
        return 1;
    }

    /// <summary>
    /// Starts object pooling when weapon is in inventory.
    /// </summary>
    public void ObjectPool()
    {

    }
    /// <summary>
    /// Delets pooled objects when weapon is dropped.
    /// </summary>
    public void DeleteObjectPool()
    {
        if ("pooled objects" == null)
        {
            return;
        }
    }

    public void Upgrade()
    {

    }
}
