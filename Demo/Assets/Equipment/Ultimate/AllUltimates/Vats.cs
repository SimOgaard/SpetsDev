using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script is child component of the Equipment hierarchy.
/// Holds specific functionallity and uppgrades for Vats Equipment.
/// </summary>
public class Vats : MonoBehaviour, Ultimate.IUltimate
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
        transform.GetComponent<MeshRenderer>().material.SetColor("_Color", Color.cyan);
    }

    /// <summary>
    /// Example of how this specific equipments can be used with character interactions.
    /// Should be called from master Equipment component.
    /// </summary>
    public void UsePrimary()
    {
        Debug.Log("stopped time using vats");
    }

    /// <summary>
    /// Returns vats icon for ui element.
    /// </summary>
    public Sprite GetIconSprite()
    {
        return icon_sprite;
    }

    private Sprite icon_sprite;
    private void Start()
    {
        icon_sprite = Resources.Load<Sprite>("Sprites/debugger");
    }
}
