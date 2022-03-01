using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script is child component of the Equipment hierarchy.
/// Holds specific functionallity and uppgrades for EarthbendingUltimate Equipment.
/// </summary>
public class TelekinesisUltimate : Ultimate
{
    public override void UsePrimary()
    {
        Debug.Log("TelekinesisUltimate.UsePrimary");
    }
    public override void StopPrimary()
    {
        Debug.Log("TelekinesisUltimate.StopPrimary");
    }

    public override void UpdateUI()
    {
        base.UpdateUI();
        UIInventory.currentUltimate_UIImage.sprite = iconSprite;
    }

    public override void Awake()
    {
        base.Awake();
        iconSprite = Resources.Load<Sprite>("Sprites/UI/earthbending");
    }
}