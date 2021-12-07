using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script is child component of the Equipment hierarchy.
/// Holds specific functionallity and uppgrades for EarthSpikesAbility Equipment.
/// </summary>
public class EarthSpikesBase : Ability, Equipment.IEquipment
{
    // grass cutting simulator
    private Collider grass_cutter;

    public override void UsePrimary()
    {
        //grass_cutter.enabled = true;
        Debug.Log("EarthSpikesAbility.UsePrimary");
    }
    public override void StopPrimary()
    {
        //grass_cutter.enabled = false;
        Debug.Log("EarthSpikesAbility.StopPrimary");
    }

    public override void UpdateUI()
    {
        base.UpdateUI();
        UIInventory.current_ability_UI_image.sprite = icon_sprite;
    }

    public override void Awake()
    {
        base.Awake();
        icon_sprite = Resources.Load<Sprite>("Sprites/UI/fireball");

        grass_cutter = GameObject.Find("TESTING REMOVING GRASS").GetComponent<Collider>();
    }
}