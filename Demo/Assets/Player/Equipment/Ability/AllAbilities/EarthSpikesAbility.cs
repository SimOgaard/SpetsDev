using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script is child component of the Equipment hierarchy.
/// Holds specific functionallity and uppgrades for EarthSpikesAbility Equipment.
/// </summary>
public class EarthSpikesAbility : MonoBehaviour, Ability.IAbility
{
    public bool upgrade = false;

    /// <summary>
    /// Used to get mouse position in world space.
    /// </summary>
    private MousePoint mouse_point;

    /// <summary>
    /// All variables that can be changed on runtime to effect how this ability should behave.
    /// </summary>
    public float structure_radius = 7.5f;

    public float pillar_height = 8f;
    public float pillar_height_offset = -2f;
    public float pillar_width = 1f;
    public float pillar_width_offset = -0.1f;

    public float pillar_speed = 100f;
    public float structure_build_time = 0.025f;
    public float pillar_alive_time = 0.1f;

    public float ability_cooldown = 1f;
    public float _current_cooldown = 0f;
    public float current_cooldown { get { return _current_cooldown; } set { _current_cooldown = Mathf.Max(0f, value); } }

    public float damage = 10f;
    public float min_damage = 4f;
    public float damage_falloff_by_radius = 2f;

    /// <summary>
    /// All variables that when changed need to run Upgrade().
    /// </summary>
    [Header("Variables underneath need to check 'Upgrade' for effects to work. Note console log to see if it worked")]
    public float structure_angle = 75f;
    public float pillar_recursive_angle = 10f;
    public float tumble_time = 3f;

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
        int index = 0;

        Vector3 shield_point = player_pos + Quaternion.Euler(0f, 0f, 0f) * mouse_direction * structure_radius;
        Quaternion rotation = new Quaternion();
        rotation.SetFromToRotation(Vector3.up, Quaternion.Euler(0f, 0f, 0f) * mouse_direction + new Vector3(0, 0.57735f, 0));

        SpawnPillar(earthbending_pillar_array[index++], shield_point, pillar_height + pillar_height_offset * itter, pillar_width + pillar_width_offset * itter, rotation, itter, true);
        Vector3 ground_point = earthbending_pillar_array[0].ground_point;
        //earthbending_pillar_list[0].DealDamageByTrigger(new Vector3(0f, 0.2f, 0f), 0.4f);
        itter++;

        for (float angle = pillar_recursive_angle; itter <= structure_angle / (pillar_recursive_angle * 2f); angle += pillar_recursive_angle)
        {
            yield return wait;

            Vector3 shield_point_left = player_pos + Quaternion.Euler(0f, -angle, 0f) * mouse_direction * structure_radius;
            shield_point_left.y = ground_point.y;
            Vector3 shield_point_right = player_pos + Quaternion.Euler(0f, angle, 0f) * mouse_direction * structure_radius;
            shield_point_right.y = ground_point.y;

            Quaternion rotation_left = new Quaternion();
            rotation_left.SetFromToRotation(Vector3.up, Quaternion.Euler(0f, -angle, 0f) * mouse_direction + new Vector3(0, 0.57735f, 0));

            Quaternion rotation_right = new Quaternion();
            rotation_right.SetFromToRotation(Vector3.up, Quaternion.Euler(0f, angle, 0f) * mouse_direction + new Vector3(0, 0.57735f, 0));

            SpawnPillar(earthbending_pillar_array[index++], shield_point_left, pillar_height + pillar_height_offset * itter, pillar_width + pillar_width_offset * itter, rotation_left, itter, false);
            SpawnPillar(earthbending_pillar_array[index++], shield_point_right, pillar_height + pillar_height_offset * itter, pillar_width + pillar_width_offset * itter, rotation_right, itter, false);
            itter++;
        }

        yield return new WaitForSeconds(pillar_alive_time);

        for (int i = 0; i < pillar_amount; i++)
        {
            earthbending_pillar_array[i].move_state = EarthbendingPillar.MoveStates.down;
        }
    }

    /// <summary>
    /// Creates pillar game object instanciated with EarthbendingPillar component and right values.
    /// </summary>
    private EarthbendingPillar SpawnPillar(EarthbendingPillar earthbending_pillar, Vector3 point, float height, float width, Quaternion rotation, int index, bool ignore)
    {
        earthbending_pillar.gameObject.SetActive(true);
        rotation *= Quaternion.Euler(Vector3.up * 45f);
        earthbending_pillar.damage = Mathf.Max(damage - damage_falloff_by_radius * index, min_damage);
        earthbending_pillar.InitEarthbendingPillar(height, width, rotation, Mathf.Infinity, pillar_speed);
        earthbending_pillar.PlacePillar(point, 2.5f, ignore);

        return earthbending_pillar;
    }

    /// <summary>
    /// Returns earthspikes icon for ui element.
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

    private EarthbendingPillar[] earthbending_pillar_array;
    private int pillar_amount;

    /// <summary>
    /// Starts object pooling when ability is in inventory.
    /// </summary>
    public void ObjectPool()
    {
        System.Guid guid = System.Guid.NewGuid();

        DeleteObjectPool();
        pillar_amount = Mathf.FloorToInt(structure_angle / pillar_recursive_angle);
        earthbending_pillar_array = new EarthbendingPillar[pillar_amount];

        for (int i = 0; i < pillar_amount; i++)
        {
            GameObject pillar_game_object = CreateMesh.CreatePrimitive(CreateMesh.CubeMesh(), material, "earth_spike_object");
            EarthbendingPillar earthbending_pillar = pillar_game_object.AddComponent<EarthbendingPillar>();
            earthbending_pillar.SetDamageId(guid.ToString());
            earthbending_pillar.DealDamageByTrigger(tumble_time);
            earthbending_pillar_array[i] = earthbending_pillar;
            pillar_game_object.SetActive(false);
        }
    }
    /// <summary>
    /// Delets pooled objects when ability is dropped.
    /// </summary>
    public void DeleteObjectPool()
    {
        if (earthbending_pillar_array == null)
        {
            return;
        }
        for (int i = 0; i < earthbending_pillar_array.Length; i++)
        {
            Destroy(earthbending_pillar_array[i].gameObject);
        }
        earthbending_pillar_array= null;
    }

    public void Upgrade()
    {
        DeleteObjectPool();
        ObjectPool();
    }
}
