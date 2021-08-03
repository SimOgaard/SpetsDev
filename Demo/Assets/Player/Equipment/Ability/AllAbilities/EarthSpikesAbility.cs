using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script is child component of the Equipment hierarchy.
/// Holds specific functionallity and uppgrades for EarthSpikesAbility Equipment.
/// </summary>
public class EarthSpikesAbility : MonoBehaviour, Ability.IAbility
{
    /// <summary>
    /// Used to get mouse position in world space.
    /// </summary>
    private MousePoint mouse_point;

    // All variables could be changed by different uppgrades.
    // where pillars can spawn around the player
    public float structure_radius = 7.5f; // private const (dependent on feels)
    public float structure_angle = 75f;
    public float pillar_recursive_angle = 10f; // private const (dependent on pillar width)

    // pillar looks
    public float pillar_height = 8f;
    public float pillar_height_offset = -2f; // private const (dependent on looks)
    public float pillar_width = 1f; // private const (dependent on looks)
    public float pillar_width_offset = -0.1f; // private const (dependent on looks)

    // pillar timed
    public float pillar_speed = 100f;
    public float structure_build_time = 0.025f;
    public float pillar_alive_time = 0.25f;

    // Ability cooldown
    public float ability_cooldown = 0.2f;
    public float _current_cooldown = 0f;
    public float current_cooldown { get { return _current_cooldown; } set { _current_cooldown = Mathf.Max(0f, value); } }

    public float damage = 10f;
    public float min_damage = 4f;
    public float damage_falloff_by_radius = 2f;

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
        if (current_cooldown <= 0f)
        {
            current_cooldown = ability_cooldown;
            StartCoroutine(RecursivePillarSpawn(mouse_point.transform.forward, transform.position));
        }
    }

    /// <summary>
    /// Stops this ability.
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

        List<EarthbendingPillar> earthbending_pillar_list = new List<EarthbendingPillar>();

        Vector3 shield_point = player_pos + Quaternion.Euler(0f, 0f, 0f) * mouse_direction * structure_radius;
        Quaternion rotation = new Quaternion();
        rotation.SetFromToRotation(Vector3.up, Quaternion.Euler(0f, 0f, 0f) * mouse_direction + new Vector3(0, 0.57735f, 0));

        float time = float.MaxValue;
        earthbending_pillar_list.Add(SpawnPillar(shield_point, pillar_height + pillar_height_offset * itter, pillar_width + pillar_width_offset * itter, time, rotation, itter));
        itter++;

        for (float angle = pillar_recursive_angle; itter <= structure_angle / (pillar_recursive_angle * 2f); angle += pillar_recursive_angle)
        {
            yield return wait;

            Vector3 shield_point_left = player_pos + Quaternion.Euler(0f, -angle, 0f) * mouse_direction * structure_radius;
            Vector3 shield_point_right = player_pos + Quaternion.Euler(0f, angle, 0f) * mouse_direction * structure_radius;

            Quaternion rotation_left = new Quaternion();
            rotation_left.SetFromToRotation(Vector3.up, Quaternion.Euler(0f, -angle, 0f) * mouse_direction + new Vector3(0, 0.57735f, 0));

            Quaternion rotation_right = new Quaternion();
            rotation_right.SetFromToRotation(Vector3.up, Quaternion.Euler(0f, angle, 0f) * mouse_direction + new Vector3(0, 0.57735f, 0));

            earthbending_pillar_list.Add(SpawnPillar(shield_point_left, pillar_height + pillar_height_offset * itter, pillar_width + pillar_width_offset * itter, time, rotation_left, itter));
            earthbending_pillar_list.Add(SpawnPillar(shield_point_right, pillar_height + pillar_height_offset * itter, pillar_width + pillar_width_offset * itter, time, rotation_right, itter));
            itter++;
        }

        yield return new WaitForSeconds(pillar_alive_time);

        foreach (EarthbendingPillar earthbending_pillar in earthbending_pillar_list)
        {
            earthbending_pillar.should_be_deleted = true;
            earthbending_pillar.move_state = EarthbendingPillar.MoveStates.down;
        }
    }

    /// <summary>
    /// Creates pillar game object instanciated with EarthbendingPillar component and right values.
    /// </summary>
    private EarthbendingPillar SpawnPillar(Vector3 point, float height, float width, float time, Quaternion rotation, int index)
    {
        rotation *= Quaternion.Euler(Vector3.up * 45f);
        GameObject pillar_game_object = GameObject.CreatePrimitive(PrimitiveType.Cube);
        EarthbendingPillar earthbending_pillar = pillar_game_object.AddComponent<EarthbendingPillar>();
        earthbending_pillar.deal_damage = true;
        earthbending_pillar.damage = Mathf.Max(damage - damage_falloff_by_radius * index, min_damage);
        earthbending_pillar.InitEarthbendingPillar(height, width, rotation, time, pillar_speed);
        earthbending_pillar.PlacePillar(point, transform.position.y, 2f);

        return earthbending_pillar;

        //earthbending_pillar.should_be_deleted = true;
    }

    /// <summary>
    /// Returns earthspikes icon for ui element.
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

    /// <summary>
    /// Returns current cooldown of equipment.
    /// </summary>
    public float GetCurrentCooldown()
    {
        return current_cooldown;
    }
    /// <summary>
    /// Returns cooldown of equipment.
    /// </summary>
    public float GetCooldown()
    {
        return ability_cooldown;
    }

    /// <summary>
    /// Starts object pooling when ability is in inventory.
    /// </summary>
    public void ObjectPool()
    {
        DeleteObjectPool();
    }
    /// <summary>
    /// Delets pooled objects when ability is dropped.
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
