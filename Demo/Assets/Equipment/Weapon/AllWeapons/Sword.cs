using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script is child component of the Equipment hierarchy.
/// Holds specific functionallity and uppgrades for Sword Equipment.
/// </summary>
public class Sword : MonoBehaviour, Weapon.IWeapon
{
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
    /// Example of how this specific equipments can be used with character interactions.
    /// Should be called from master Equipment component.
    /// </summary>
    public void UsePrimary()
    {
        Debug.Log("attacked using a sword");
    }
}
