using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script is child component of the Equipment hierarchy.
/// Holds specific functionallity and uppgrades for EarthShieldAbility Equipment.
/// </summary>
public class EarthShieldAbility : MonoBehaviour, Ability.IAbility
{
    public bool upgrade = false;

    /// <summary>
    /// Used to get mouse position in world space.
    /// </summary>
    private MousePoint mouse_point;

    /// <summary>
    /// All variables that can be changed on runtime to effect how this ability should behave.
    /// </summary>
    public float pillar_speed = 2.0f;
    public float structure_build_time = 0.1f;
    public float pillar_alive_time = 5f;

    public float ability_cooldown = 1f;
    public float _current_cooldown = 0f;
    public float current_cooldown { get { return _current_cooldown; } set { _current_cooldown = Mathf.Max(0f, value); } }

    [HideInInspector] public bool is_allowed_to_remove_shield = false;
    public float shield_cooldown_refund_coefficient = 0.25f;

    /// <summary>
    /// All variables that when changed need to clear half_of_shield and re run ObjectPool().
    /// </summary>
    [Header("Variables underneath need to check 'Upgrade' for effects to work. Note console log to see if it worked")]
    public float pillar_height = 7f;
    public float pillar_height_offset = -0.5f;
    public float pillar_width = 2f;
    public float pillar_width_offset = -0.2f;

    public float structure_radius = 10f;
    public float structure_angle = 180f;
    public float pillar_recursive_angle = 15f;

    public float sound_amplifier = 750f;
    public float max_sound = 1.5f;
    public float hearing_threshold_change = 1f;

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
            StartCoroutine(RecursivePillarSpawn(mouse_point.transform.forward, transform.position));
        }
        else
        {
            PullDownShield();
        }
    }

    /// <summary>
    /// Stops this ability.
    /// </summary>
    public void StopPrimary()
    {

    }

    /// <summary>
    /// Pull down shield and returns some cooldown.
    /// </summary>
    private void PullDownShield()
    {
        if (is_allowed_to_remove_shield)
        {
            bool shield_is_fully_up = true;
            float shield_up_time = 0f;
            for (int i = 0; i < half_of_shield_pillar_amount; i++)
            {
                if (merged_mirrored_shield[i] == null)
                {
                    shield_is_fully_up = false;
                    break;
                }

                if (merged_mirrored_shield[i].move_state == EarthbendingPillar.MoveStates.still)
                {
                    shield_up_time += merged_mirrored_shield[i].current_sleep_time;
                }
                else
                {
                    shield_is_fully_up = false;
                }
            }
            if (shield_is_fully_up)
            {
                current_cooldown -= (shield_up_time / half_of_shield_pillar_amount) * shield_cooldown_refund_coefficient;
                for (int i = 0; i < half_of_shield_pillar_amount; i++)
                {
                    merged_mirrored_shield[i].move_state = EarthbendingPillar.MoveStates.down;
                }
            }
        }
    }

    /// <summary>
    /// Recursivly selects points around player and moves pillars to that point merging the mesh to a new EarthbendingPillar component.
    /// </summary>
    private IEnumerator RecursivePillarSpawn(Vector3 mouse_direction, Vector3 player_pos)
    {
        WaitForSeconds wait = new WaitForSeconds(structure_build_time);
        int itter = 0;
        for (float angle = pillar_recursive_angle; itter <= structure_angle / (pillar_recursive_angle * 2f); angle += pillar_recursive_angle)
        {
            GameObject merged_circle_pillars_game_object = new GameObject();
            EarthbendingPillar merged_circle_pillars = merged_circle_pillars_game_object.AddComponent<EarthbendingPillar>();

            Vector3 shield_point_left = player_pos + Quaternion.Euler(0f, -angle + pillar_recursive_angle * 0.5f, 0f) * mouse_direction * structure_radius;
            Vector3 shield_point_right = player_pos + Quaternion.Euler(0f, angle - pillar_recursive_angle * 0.5f, 0f) * mouse_direction * structure_radius;

            half_of_shield[itter].PlacePillar(shield_point_left);
            merged_circle_pillars.InitEarthbendingPillar(half_of_shield[itter].gameObject);
            half_of_shield[itter].PlacePillar(shield_point_right);
            merged_circle_pillars.InitEarthbendingPillar(half_of_shield[itter].gameObject);
            merged_circle_pillars.should_be_deleted = true;

            merged_circle_pillars.SetSharedValues(pillar_alive_time - structure_build_time * itter, pillar_speed, pillar_height + pillar_height_offset * itter, material);
            merged_circle_pillars.SetSound(sound_amplifier, max_sound, hearing_threshold_change);

            // smootly rotates cubes instead of restricting/snapping it to 45 degrees
            // Quaternion rotation_left = Quaternion.LookRotation((shield_point_left - player_pos), Vector3.up);
            // Quaternion rotation_right = Quaternion.LookRotation((shield_point_right - player_pos), Vector3.up);

            merged_circle_pillars.gameObject.SetActive(false);
            yield return wait;
            merged_circle_pillars.gameObject.SetActive(true);
            merged_mirrored_shield[itter] = merged_circle_pillars;
            itter++;
        }
    }

    /// <summary>
    /// Returns earthshield icon for ui element.
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

    private EarthbendingPillar[] merged_mirrored_shield;
    private EarthbendingPillar[] half_of_shield;
    private int half_of_shield_pillar_amount;

    /// <summary>
    /// Starts object pooling half of your shield when ability is in inventory.
    /// Half of your shield is identical to other half so can be used to merge meshes into a new shield.
    /// </summary>
    public void ObjectPool()
    {
        DeleteObjectPool();
        half_of_shield_pillar_amount = 0;
        for (float angle = pillar_recursive_angle; half_of_shield_pillar_amount <= structure_angle / (pillar_recursive_angle * 2f); angle += pillar_recursive_angle)
        {
            half_of_shield_pillar_amount++; // AAAAAH MATHF.CEIL DO NOT WORK WTF IS TIHSIA DPKSA DOASD JANSKDAJS (180/15) != 12 ????? FUKC OJAS
        }

        half_of_shield = new EarthbendingPillar[half_of_shield_pillar_amount];
        merged_mirrored_shield = new EarthbendingPillar[half_of_shield_pillar_amount];
        for (int i = 0; i < half_of_shield_pillar_amount; i++)
        {
            GameObject pillar_game_object = CreateMesh.CreatePrimitive(CreateMesh.CubeMesh(), material, "earth_shield_object");
            EarthbendingPillar earthbending_pillar = pillar_game_object.AddComponent<EarthbendingPillar>();

            earthbending_pillar.InitEarthbendingPillar(pillar_height + pillar_height_offset * i, pillar_width + pillar_width_offset * i, Quaternion.Euler(0f, 45f, 0f), pillar_alive_time, pillar_speed);
            pillar_game_object.SetActive(false);
            half_of_shield[i] = earthbending_pillar;
        }
    }

    /// <summary>
    /// Delets pooled objects when ability is dropped.
    /// </summary>
    public void DeleteObjectPool()
    {
        if (half_of_shield == null)
        {
            return;
        }
        for (int i = 0; i < half_of_shield_pillar_amount; i++)
        {
            Destroy(half_of_shield[i].gameObject);
        }
        half_of_shield = null;
    }

    public void Upgrade()
    {
        DeleteObjectPool();
        ObjectPool();
    }
}
