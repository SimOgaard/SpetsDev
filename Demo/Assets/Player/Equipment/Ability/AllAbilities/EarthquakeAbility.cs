using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script is child component of the Equipment hierarchy.
/// Holds specific functionallity and uppgrades for EarthQuakeAbility Equipment.
/// </summary>
public class EarthquakeAbility : MonoBehaviour, Ability.IAbility
{
    public bool upgrade = false;

    /// <summary>
    /// Used to get mouse position in world space.
    /// </summary>
    private MousePoint mouse_point;

    /// <summary>
    /// All variables that can be changed on runtime to effect how this ability should behave.
    /// </summary>
    public float angle_const_recursive = 1250f;

    public float density_loss_magnitude = 0f;

    public float player_safe_zone_radius = 8f;
    public float player_lock_on_radius = 5f;

    public int wave_recursion = 5;
    public float earthquake_recursion_time_wait = -0.3f;

    public float flow_speed = 0.5f;
    public float pillar_speed = 2f;
    public float pillar_alive_time = 0f;

    public float ability_cooldown = 1f;
    public float _current_cooldown = 0f;
    public float current_cooldown { get { return _current_cooldown; } set { _current_cooldown = Mathf.Max(0f, value); } }

    public float damage = 20f;
    public float min_damage = 5f;

    public bool ignore_player = true;

    /// <summary>
    /// All variables that when changed need to clear earthbending_pillars_for_each_ring and re run ObjectPool().
    /// </summary>
    [Header("Variables underneath need to check 'Upgrade' for effects to work. Note console log to see if it worked")]
    public float radius_increment = 3f;
    public float max_radius = 20f;

    public float pillar_height = 1.5f;
    public float max_pillar_height_offset = -0.25f;
    public float pillar_width = 1.5f;
    public float max_pillar_width_offset = -0.25f;

    /// <summary>
    /// Destroys itself.
    /// </summary>
    public void Destroy()
    {
        Destroy(this);
    }

    /// <summary>
    /// Visualizes that transmission of this function reached this child component.
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
        if (upgrade)
        {
            Debug.Log("Uppgraded to new variables on " + GetType().Name);
            upgrade = false;
            Upgrade();
        }

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
            StartStructuredEarthquake(mouse_point.GetWorldPoint(), transform.position, mouse_point.transform.rotation.eulerAngles);
        }
    }

    /// <summary>
    /// Stops this ability.
    /// </summary>
    public void StopPrimary()
    {

    }

    /// <summary>
    /// Snaps center to player if close enough and calculates amount of earthquake circles as well as which circles should be neglected to not hit player.
    /// Creates new list with one element of merged earthbending pillars for each circle
    /// For each merged element move coresponding cached pillar in earthbending_pillars_for_each_ring around circumfrance of circle and merging its mesh.
    /// Then for each wave_recursion move merged circle of pillars.
    /// </summary>
    private void StartStructuredEarthquake(Vector3 mouse_pos, Vector3 player_pos, Vector3 mouse_rotation)
    {
        int ammount_of_circles = Mathf.RoundToInt((max_radius) / radius_increment);
        int circle_index_start;
        Vector3 start_point;
        bool is_around_player = (mouse_pos - player_pos).magnitude < player_lock_on_radius;
        if (is_around_player)
        {
            start_point = player_pos;
            circle_index_start = Mathf.RoundToInt(player_safe_zone_radius / radius_increment);
        }
        else
        {
            start_point = mouse_pos;
            circle_index_start = 1;
        }

        if (ignore_player)
        {
            circle_index_start = 1;
        }

        EarthbendingPillar[] all_circles_merged_pillars = new EarthbendingPillar[ammount_of_circles];

        for (int circle_index = circle_index_start; circle_index <= ammount_of_circles; circle_index++)
        {
            float current_radius = radius_increment * circle_index;
            float distance_remap01 = current_radius / max_radius;
            float diameter = 2f * current_radius * Mathf.PI;
            float radian_recursive = (angle_const_recursive / diameter) * Mathf.Deg2Rad;

            GameObject merged_circle_pillars_game_object = new GameObject();
            EarthbendingPillar merged_circle_pillars = merged_circle_pillars_game_object.AddComponent<EarthbendingPillar>();
            if (ignore_player)
            {
                merged_circle_pillars_game_object.layer = Layer.ignore_player_collision;
            }

            all_circles_merged_pillars[circle_index-1] = merged_circle_pillars;

            for (float radian = 0; radian <= 2f * Mathf.PI; radian += radian_recursive)
            {
                if (Random.Range(0f, 1f) < (1 - (distance_remap01 * density_loss_magnitude)))
                {
                    Vector3 pillar_point = new Vector3(start_point.x + Mathf.Cos(radian) * current_radius, start_point.y, start_point.z + Mathf.Sin(radian) * current_radius);
                    earthbending_pillars_for_each_ring[circle_index].PlacePillar(pillar_point);
                    merged_circle_pillars.InitEarthbendingPillar(earthbending_pillars_for_each_ring[circle_index].gameObject);
                    merged_circle_pillars.should_be_deleted = 1 == wave_recursion;
                }
            }
            merged_circle_pillars.SetSharedValues(pillar_alive_time, pillar_speed, pillar_height + max_pillar_height_offset * distance_remap01, material);
            StartCoroutine(
                SpawnPillarShared(
                    merged_circle_pillars,
                    (distance_remap01 / flow_speed),
                    1 == wave_recursion,
                    start_point,
                    current_radius + radius_increment * 0.5f,
                    circle_index == 1 ? 0f : current_radius - radius_increment * 0.5f
                )
            );
        }

        for (int earthquake_recursion_index = 1; earthquake_recursion_index < wave_recursion; earthquake_recursion_index++)
        {
            for (int circle_index = circle_index_start; circle_index <= ammount_of_circles; circle_index++)
            {
                float current_radius = radius_increment * circle_index;
                float distance_remap01 = current_radius / max_radius;

                StartCoroutine(
                    SpawnPillarShared(
                        all_circles_merged_pillars[circle_index - 1],
                        (distance_remap01 / flow_speed) + earthquake_recursion_index * ((1f / flow_speed) + (((pillar_height + max_pillar_height_offset * distance_remap01) * 0.5f) / pillar_speed) + earthquake_recursion_time_wait),
                        earthquake_recursion_index == wave_recursion - 1,
                        start_point,
                        current_radius + radius_increment * 0.5f,
                        circle_index == 1 ? 0f : current_radius - radius_increment * 0.5f
                    )
                );
            }
        }
    }

    /// <summary>
    /// Controlls activation of merged ring and controlls weather or not it should be reused (disabled) or not (deleted).
    /// </summary>
    private IEnumerator SpawnPillarShared(EarthbendingPillar merged_circle_pillars, float wait, bool should_be_deleted, Vector3 mid_point, float max_radius, float min_radius)
    {
        merged_circle_pillars.gameObject.SetActive(false);
        yield return new WaitForSeconds(wait);
        merged_circle_pillars.should_be_deleted = should_be_deleted;
        merged_circle_pillars.gameObject.SetActive(true);
        yield return new WaitForSeconds(pillar_height * 0.5f / pillar_speed);
        Damage(mid_point, max_radius, min_radius);
    }

    private string damage_id = System.Guid.NewGuid().ToString();
    /// <summary>
    /// Calculates distance from enemy to mid point of ability cast ignoring y values.
    /// Damages enemy liniarly by distance.
    /// </summary>
    private void Damage(Vector3 mid_point, float max_radius, float min_radius)
    {
        mid_point.y = 0f;
        foreach (EnemyAI enemy_ai in Enemies.all_enemy_ais)
        {
            Vector3 enemy_pos = enemy_ai.transform.position;
            enemy_pos.y = 0f;

            float current_radius = (enemy_pos - mid_point).magnitude;

            if (current_radius < max_radius && current_radius > min_radius)
            {
                float distance_remap01 = current_radius / (this.max_radius + radius_increment * 0.5f);
                float damage_by_liniar_function = -(damage - min_damage) * distance_remap01 + damage;

                enemy_ai.Damage(damage_by_liniar_function, damage_id, 0.1f);
            }
        }
    }

    /// <summary>
    /// Returns earthquake icon for ui element.
    /// </summary>
    public Sprite GetIconSprite()
    {
        return icon_sprite;
    }

    private Material material;
    private Sprite icon_sprite;
    private void Start()
    {
        icon_sprite = Resources.Load<Sprite>("Sprites/UI/fireball");
        mouse_point = GameObject.Find("MouseRot").GetComponent<MousePoint>();
        material = Resources.Load<Material>("Materials/Stone Material");
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

    private EarthbendingPillar[] earthbending_pillars_for_each_ring;

    /// <summary>
    /// Starts object pooling one pillar for each ring of earthquake when ability is in inventory.
    /// Each pillar in each ring is identical to each others which can be used to merge meshes.
    /// </summary>
    public void ObjectPool()
    {
        DeleteObjectPool();
        int amount_of_circles = Mathf.RoundToInt((max_radius) / radius_increment);
        earthbending_pillars_for_each_ring = new EarthbendingPillar[amount_of_circles+1];
        for (int circle_index = 0; circle_index <= amount_of_circles; circle_index++)
        {
            float current_radius = radius_increment * circle_index;
            float distance_remap01 = current_radius / max_radius;
            float diameter = 2f * current_radius * Mathf.PI;

            GameObject pillar_game_object = CreateMesh.CreatePrimitive(CreateMesh.CubeMesh(), material, "earth_quake_object");
            EarthbendingPillar earthbending_pillar = pillar_game_object.AddComponent<EarthbendingPillar>();

            earthbending_pillar.InitEarthbendingPillar(pillar_height + max_pillar_height_offset * distance_remap01, pillar_width + max_pillar_width_offset * distance_remap01, Quaternion.Euler(0f, 45f, 0f), pillar_alive_time, pillar_speed);
            earthbending_pillars_for_each_ring[circle_index] = earthbending_pillar;
        }
    }

    /// <summary>
    /// Delets pooled objects when ability is dropped.
    /// </summary>
    public void DeleteObjectPool()
    {
        if (earthbending_pillars_for_each_ring == null)
        {
            return;
        }

        for (int i = 0; i < earthbending_pillars_for_each_ring.Length; i++)
        {
            Destroy(earthbending_pillars_for_each_ring[i].gameObject);
        }
        earthbending_pillars_for_each_ring = null;
    }

    public void Upgrade()
    {
        DeleteObjectPool();
        ObjectPool();
    }
}
