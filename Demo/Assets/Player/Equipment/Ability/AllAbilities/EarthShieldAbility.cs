using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script is child component of the Equipment hierarchy.
/// Holds specific functionallity and uppgrades for EarthShieldAbility Equipment.
/// </summary>
public class EarthShieldAbility : MonoBehaviour, Ability.IAbility
{
    /// <summary>
    /// Used to get mouse position in world space.
    /// </summary>
    private MousePoint mouse_point;

    // All variables could be changed by different uppgrades.
    // where pillars can spawn around the player
    public float structure_radius = 10f; // private const (dependent on feels)
    public float structure_angle = 180f;
    public float pillar_recursive_angle = 15f; // private const (dependent on pillar width)

    // pillar looks
    public float pillar_height = 7f;
    public float pillar_height_offset = -0.5f; // private const (dependent on looks)
    public float pillar_width = 2f; // private const (dependent on looks)
    public float pillar_width_offset = -0.2f; // private const (dependent on looks)

    // pillar timed
    public float pillar_speed = 15f;
    public float structure_build_time = 0.15f;
    public float pillar_alive_time = 5f;

    // Ability cooldown
    public float ability_cooldown = 10f;
    public float current_cooldown = 0f;

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
        transform.GetComponent<MeshRenderer>().material.SetColor("_Color", Color.magenta);
    }

    /// <summary>
    /// Manages cooldowns
    /// </summary>
    private void Update()
    {
        current_cooldown -= Time.deltaTime;
    }

    /// <summary>
    /// Starts to use this ability.
    /// </summary>
    public void UsePrimary()
    {
        if (current_cooldown < 0f)
        {
            current_cooldown = ability_cooldown;
            StartCoroutine(RecursivePillarSpawn(mouse_point.transform.forward, transform.position));
        }
    }

    /// <summary>
    /// Stops this ultimate.
    /// </summary>
    public void StopPrimary()
    {

    }

    /// <summary>
    /// Recursivly selects points around player and spawns pillar using variables above.
    /// </summary>
    private IEnumerator RecursivePillarSpawn(Vector3 mouse_direction, Vector3 player_pos)
    {
        WaitForSeconds wait = new WaitForSeconds(structure_build_time);
        int itter = 0;
        for (float angle = pillar_recursive_angle; itter <= structure_angle / (pillar_recursive_angle * 2f); angle += pillar_recursive_angle)
        {
            Vector3 shield_point_left = player_pos + Quaternion.Euler(0f, -angle + pillar_recursive_angle * 0.5f, 0f) * mouse_direction * structure_radius;
            Vector3 shield_point_right = player_pos + Quaternion.Euler(0f, angle - pillar_recursive_angle * 0.5f, 0f) * mouse_direction * structure_radius;

            // smootly rotates cubes instead of 45 degrees
            // Quaternion rotation_left = Quaternion.LookRotation((shield_point_left - player_pos), Vector3.up);
            // Quaternion rotation_right = Quaternion.LookRotation((shield_point_right - player_pos), Vector3.up);

            SpawnPillar(shield_point_left, pillar_height + pillar_height_offset * itter, pillar_width + pillar_width_offset * itter, pillar_alive_time - structure_build_time * itter);
            SpawnPillar(shield_point_right, pillar_height + pillar_height_offset * itter, pillar_width + pillar_width_offset * itter, pillar_alive_time - structure_build_time * itter);
            itter++;

            yield return wait;
        }
    }

    /// <summary>
    /// Creates pillar game object instanciated with EarthbendingPillar component and right values.
    /// </summary>
    private void SpawnPillar(Vector3 point, float height, float width, float time)
    {
        GameObject pillar_game_object = GameObject.CreatePrimitive(PrimitiveType.Cube);
        EarthbendingPillar earthbending_pillar = pillar_game_object.AddComponent<EarthbendingPillar>();
        earthbending_pillar.NEWSpawnPillar(point, height, time, pillar_speed, width);
    }

    /// <summary>
    /// Returns earthshield icon for ui element.
    /// </summary>
    public Sprite GetIconSprite()
    {
        return icon_sprite;
    }

    private Sprite icon_sprite;
    private void Start()
    {
        icon_sprite = Resources.Load<Sprite>("Sprites/UI/fireball");
        mouse_point = GameObject.Find("MouseRot").GetComponent<MousePoint>();
    }
}
