using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script is child component of the Equipment hierarchy.
/// Holds specific functionallity and uppgrades for EarthbendingUltimate Equipment.
/// </summary>
public class EarthbendingAbility : Ability, Equipment.IEquipment
{
    public override void UsePrimary()
    {
        if ((ready_pillars >= max_pillars) && current_cooldown == 0f || bufferable && !casting)
        {
            casting = true;
            pillar_spawn_coroutine = FollowMouse();
            StartCoroutine(pillar_spawn_coroutine);
        }
    }
    public override void StopPrimary()
    {
        Debug.Log("EarthbendingAbility.StopPrimary");
    }

    private void Update()
    {
        current_cooldown -= Time.deltaTime;

        if (current_cooldown == 0f || ready_pillars < max_pillars && !casting)
        {
            ready_pillars++;
            current_cooldown = cooldown;
        }
    }

    /*
    private void Update()
    {
        current_cooldown -= Time.deltaTime;
        if (current_cooldown == 0f && current_pillar_amount < pillar_amount && !is_casting)
        {
            current_pillar_amount++;
            current_cooldown = ultimate_cooldown / pillar_amount;
        }
    }
    */

    /*// alt 2
    private float _cooldown;
    public new float cooldown
    {
        get { return _cooldown / pillar_amount; }
        set { _cooldown = value; }
    }
    private float _current_cooldown;
    public new float current_cooldown
    {
        get { return ultimate_cooldown - (current_pillar_amount * (ultimate_cooldown / pillar_amount)) + current_cooldown; }
        set { _current_cooldown = value; }
    }
    */

    public override void UpdateUI()
    {
        base.UpdateUI();
        UIInventory.current_ability_UI_image.sprite = icon_sprite;
    }

    public override void Awake()
    {
        base.Awake();
        icon_sprite = Resources.Load<Sprite>("Sprites/UI/earthbending");
    }
}
