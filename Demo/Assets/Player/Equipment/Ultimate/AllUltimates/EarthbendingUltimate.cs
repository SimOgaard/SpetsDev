﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script is child component of the Equipment hierarchy.
/// Holds specific functionallity and uppgrades for EarthbendingUltimate Equipment.
/// </summary>
public class EarthbendingUltimate : MonoBehaviour, Ultimate.IUltimate
{
    private MousePoint mouse_point;

    public bool stach_pillars = false;
    public bool follow_mouse = false;
    public bool start_at_mouse = false;

    public int pillar_amount = 10;
    public int current_pillar_amount = 10;

    public float traverse_time = 0.1f;
    public float distance_between = 3.25f;
    public float extra_distance_first_spawn = 3.25f;

    public float alive_time = 5f;

    public float ability_cooldown = 2f;
    public float current_cooldown = 0f;
    private bool is_casting = false;

    public float pillar_height = 7f;
    public float pillar_width = 2f;
    public float pillar_growth_speed = 20f;

    private IEnumerator ult_coroutine;

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
    /// Manages cooldowns
    /// </summary>
    private void Update()
    {
        current_cooldown -= Time.deltaTime;
        if(current_cooldown < 0f && current_pillar_amount < pillar_amount && !is_casting)
        {
            current_pillar_amount++;
            current_cooldown = ability_cooldown / pillar_amount;
        }
    }

    /// <summary>
    /// Starts to use this ultimate.
    /// </summary>
    public void UsePrimary()
    {
        if((current_pillar_amount >= pillar_amount) || stach_pillars && !is_casting)
        {
            is_casting = true;
            ult_coroutine = FollowMouse();
            StartCoroutine(ult_coroutine);
        }
    }

    /// <summary>
    /// Stops this ultimate.
    /// </summary>
    public void StopPrimary()
    {
        if (is_casting && stach_pillars)
        {
            is_casting = false;
            StopCoroutine(ult_coroutine);
        }
    }

    private IEnumerator LineTwordsMouse(Vector3 mouse_point)
    {
        Vector3 starting_point = transform.position;
        Vector3 direction = (mouse_point - transform.position).normalized;

        WaitForSeconds wait = new WaitForSeconds(0.1f);

        for (int i = 1; i <= 8; i++)
        {
            Vector3 spawn_point = starting_point + direction * i * 4f;

            OLDSpawnPillar(spawn_point, 20f, 15f, 0.5f, 50f);
            yield return wait;
        }
    }

    private IEnumerator FollowMouse()
    {
        Vector3 mouse_point_pos = mouse_point.GetWorldPoint();
        Vector3 last_point = start_at_mouse ? mouse_point_pos : transform.position;
        Vector3 direction_straight = (mouse_point_pos - transform.position).normalized;

        WaitForSeconds wait = new WaitForSeconds(traverse_time);

        bool first_spawn = true;

        while (current_pillar_amount > 0)
        {
            Vector3 dir = direction_straight;
            if (follow_mouse && !first_spawn)
            {
                Vector3 diff = mouse_point.GetWorldPoint() - last_point;
                if (diff.sqrMagnitude < distance_between)
                {
                    // ska pillarna försvinna fastän de inte spawnas?
                    // de kanske endast ska försvinna om du inte har "stach uppgrade"
                    // current_pillar_amount--;

                    yield return wait;
                    continue;
                }
                dir = diff.normalized;
            }

            Vector3 new_point = last_point + dir * distance_between;
            if (first_spawn)
            {
                if (!start_at_mouse)
                {
                    new_point = last_point + dir * (distance_between + extra_distance_first_spawn);
                }
                else
                {
                    new_point = last_point;
                }
            }

            SpawnPillar(new_point);
            last_point = new_point;
            current_pillar_amount--;
            first_spawn = false;
            yield return wait;
        }

        is_casting = false;
    }

    private IEnumerator Wave()
    {
        Vector3 mouse_direction = this.mouse_point.transform.forward;
        Vector3 wave_point = transform.position;

        WaitForSeconds wait = new WaitForSeconds(0.1f);
        for (int i = 0; i < 5; i++)
        {
            wave_point += mouse_direction * 4f;

            Vector3 tangent;
            Vector3 t1 = Vector3.Cross(mouse_direction, Vector3.forward);
            Vector3 t2 = Vector3.Cross(mouse_direction, Vector3.up);
            if (t1.magnitude > t2.magnitude)
            {
                tangent = t1;
            }
            else
            {
                tangent = t2;
            }

            for (float q = -i*0.5f; q < i*0.5f + 1; q++)
            {
                Vector3 spawn_point_left = wave_point - tangent * 4f * q;
                Vector3 spawn_point_right = wave_point + tangent * 4f * q;

                OLDSpawnPillar(spawn_point_left, 20f, 15f, 10f, 50f);
                OLDSpawnPillar(spawn_point_right, 20f, 15f, 10f, 50f);
            }
            yield return wait;
        }
    }

    /// <summary>
    /// Creates pillar game object instanciated with EarthbendingPillar component and right values.
    /// </summary>
    private void SpawnPillar(Vector3 point)
    {
        GameObject pillar_game_object = GameObject.CreatePrimitive(PrimitiveType.Cube);
        EarthbendingPillar earthbending_pillar = pillar_game_object.AddComponent<EarthbendingPillar>();
        earthbending_pillar.NEWSpawnPillar(point, pillar_height, alive_time, pillar_growth_speed, pillar_width);
    }

    private void OLDSpawnPillar(Vector3 point, float under_ground_dist, float over_ground_dist, float time, float speed)
    {
        GameObject pillar_game_object = GameObject.CreatePrimitive(PrimitiveType.Cube);
        EarthbendingPillar earthbending_pillar = pillar_game_object.AddComponent<EarthbendingPillar>();
        earthbending_pillar.SpawnPillar(point, under_ground_dist, over_ground_dist, time, speed);
    }

    /// <summary>
    /// Returns earthbending icon for ui element.
    /// </summary>
    public Sprite GetIconSprite()
    {
        return icon_sprite;
    }

    private Sprite icon_sprite;
    private void Start()
    {
        icon_sprite = Resources.Load<Sprite>("Sprites/UI/earthbending");
        mouse_point = GameObject.Find("MouseRot").GetComponent<MousePoint>();
    }
}