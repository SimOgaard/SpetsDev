using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script is child component of the Equipment hierarchy.
/// Holds specific functionallity and uppgrades for EarthQuakeAbility Equipment.
/// </summary>
public class EarthquakeAbility : MonoBehaviour, Ability.IAbility
{
    /// <summary>
    /// Used to get mouse position in world space.
    /// </summary>
    private MousePoint mouse_point;

    // Calculate area of circle
    // Calculate amount of points to spawn (circle_area*point_amount_per_unit)
    // Itterate over that amount.
    // Pick random point in circle around point or player.
    // Dependent on distance from circle_mid_point get probability of spawning pillar theere.
    // If probability is meet: spawn pillar after an amount of time dependent on radius.

    // All variables could be changed by different uppgrades.

    public float point_amount_per_unit = 0.1f; // amount of points per unit inside circle
    public float density_loss_magnitude = 0.25f; // goes from 1-0 over radius. thinning out probability of spawn over radius increese

    public float circumference_angle = 90f; // earthquake circumfrence
    public float max_radius = 20f; // max radius of earthquake
    public float player_safe_zone_radius = 5f; // safezone for player
    public float player_lock_on_radius = 12.5f; // set to valid radius for ability to stop locking on player

    public int wave_recursion = 2; // amount of times the earth goes up and down;
    public float earthquake_recursion_time_wait = -0.3f;

    public float flow_speed = 1f; // time for quake to flow to max radius, use distance from start_point to calculate time
    public float pillar_speed = 5f; // speed of pillar translation
    public float pillar_alive_time = 0f; // time for pillar to be static at highest point 

    public float pillar_height = 1f; // height of pillar (private const)
    public float max_pillar_height_offset = 0f; // height goes down over radius increese
    public float pillar_width = 0.5f; // width of pillar (private const)
    public float max_pillar_width_offset = 0f; // width goes down over radius increese (private const)

    public float ability_cooldown = 1f; // cooldown for ability
    public float current_cooldown = 0f; // keeps time of cooldown

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
            Debug.Log("vibin");
            current_cooldown = ability_cooldown;
            StartEarthquake(mouse_point.GetWorldPoint(), transform.position, mouse_point.transform.rotation.eulerAngles);
        }
    }

    /// <summary>
    /// Stops this ultimate.
    /// </summary>
    public void StopPrimary()
    {

    }

    /// <summary>
    /// 
    /// </summary>
    private void StartEarthquake(Vector3 mouse_pos, Vector3 player_pos, Vector3 mouse_rotation)
    {
        float circle_area = Mathf.PI * max_radius * max_radius;
        int spawn_amount = Mathf.RoundToInt(circle_area * point_amount_per_unit);

        Vector3 start_point;
        bool is_around_player = (mouse_pos - player_pos).magnitude < player_lock_on_radius;
        if (is_around_player)
        {
            start_point = player_pos;
        }
        else
        {
            start_point = mouse_pos;
        }

        for (int earthquake_recursion_index = 0; earthquake_recursion_index < wave_recursion; earthquake_recursion_index++)
        {
            for (int spawn_index = 0; spawn_index < spawn_amount; spawn_index++)
            {
                Vector2 circle_point = Random.insideUnitCircle * max_radius;
                float pillar_point_magnitude = circle_point.magnitude;
                float distance_remap01 = pillar_point_magnitude / max_radius;

                float rotation = Mathf.Rad2Deg * Mathf.Atan2(circle_point.x, circle_point.y);
                Debug.Log(circumference_angle * 0.5f + mouse_rotation.y);
                Debug.Log(-circumference_angle * 0.5f + mouse_rotation.y);
                if (true)
                {
                    if (Random.Range(0f, 1f) < (1 - (distance_remap01 * density_loss_magnitude)) && (pillar_point_magnitude > player_safe_zone_radius || !is_around_player))
                    {
                        Vector3 pillar_point = new Vector3(circle_point.x, 0f, circle_point.y) + start_point;
                        StartCoroutine(
                            SpawnPillar(
                                pillar_point,
                                pillar_height + max_pillar_height_offset * distance_remap01,
                                pillar_width + max_pillar_width_offset * distance_remap01,
                                pillar_alive_time,
                                (distance_remap01 / flow_speed) + earthquake_recursion_index * ((1f / flow_speed) + (((pillar_height + max_pillar_height_offset * distance_remap01) * 0.5f) / pillar_speed) + earthquake_recursion_time_wait)
                            )
                        );
                    }
                }
            }
        }
    }

    /// <summary>
    /// Creates pillar game object instanciated with EarthbendingPillar component and right values.
    /// </summary>
    private IEnumerator SpawnPillar(Vector3 point, float height, float width, float time, float structure_build_time)
    {
        yield return new WaitForSeconds(structure_build_time);
        GameObject pillar_game_object = GameObject.CreatePrimitive(PrimitiveType.Cube);
        EarthbendingPillar earthbending_pillar = pillar_game_object.AddComponent<EarthbendingPillar>();
        earthbending_pillar.NEWSpawnPillar(point, height, time, pillar_speed, width);
    }

    /// <summary>
    /// Returns earthquake icon for ui element.
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
