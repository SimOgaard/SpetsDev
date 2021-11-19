using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script is earthbening pillar "uppgrade" 1 (only spawn in a straight line)
/// </summary>
public class EarthBendingPillarBase : Ability, Equipment.IEquipment
{
    private float _current_cooldown;
    public new float current_cooldown
    {
        get { return _current_cooldown; }
        set { _current_cooldown = Mathf.Max(0f, value); }
    }

    public float traverse_time = 0.1f;
    public float pillar_gap = 3f;

    public int max_pillars = 5;
    public int ready_pillars = 5;
    public Vector3 scale = new Vector3(1.5f, 6f, 1.5f);

    [SerializeField] private float pillar_distance_from_player = 5f;

    public override void UsePrimary()
    {
        if (ready_pillars >= max_pillars)
        {
            Vector3 player_pos = Global.player_transform.position;
            Vector3 mouse_pos = MousePoint.MousePositionWorldAndEnemy();

            Vector3 direction = (mouse_pos - player_pos);
            direction.y = 0f;
            direction = direction.normalized;

            Vector3 start_point = player_pos + direction * pillar_distance_from_player;

            StartCoroutine(SpawnStraight(start_point, direction));
        }
    }

    public virtual void Update()
    {
        if (ready_pillars < max_pillars)
        {
            current_cooldown -= Time.deltaTime;

            if (current_cooldown == 0f)
            {
                ready_pillars++;
                current_cooldown = cooldown;
            }
            Debug.Log(current_cooldown);
            Debug.Log(ready_pillars);
        }
    }

    public IEnumerator SpawnStraight(Vector3 start_point, Vector3 direction)
    {
        WaitForSeconds wait = new WaitForSeconds(traverse_time);

        while (ready_pillars > 0)
        {
            Vector3 new_point = start_point + direction * pillar_gap;
            SpawnRock(new_point);

            start_point = new_point;
            ready_pillars--;
            yield return wait;
        }
    }

    public EarthBendingRock SpawnRock(Vector3 position)
    {
        GameObject earth_bending_rock_game_object = new GameObject("pillar");
        EarthBendingRock earth_bending_rock_script = earth_bending_rock_game_object.AddComponent<EarthBendingRockScale>();
        earth_bending_rock_script.growth_speed = 3f;
        earth_bending_rock_script.sleep_time = 2f;
        earth_bending_rock_script.PlacePillar(position, Quaternion.identity, scale);
        return earth_bending_rock_script;
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
        cooldown = 0.4f;
        icon_sprite = Resources.Load<Sprite>("Sprites/UI/earthbending");
    }
}
