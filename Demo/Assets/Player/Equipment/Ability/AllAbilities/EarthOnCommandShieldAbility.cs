using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script is child component of the Equipment hierarchy.
/// Holds specific functionallity and uppgrades for EarthShieldAbility Equipment.
/// </summary>
public class EarthOnCommandShieldAbility : MonoBehaviour, Ability.IAbility
{
    public bool upgrade = false;

    /// <summary>
    /// Used to get mouse position in world space.
    /// </summary>
    private MousePoint mouse_point;

    /// <summary>
    /// All variables that can be changed on runtime to effect how this ability should behave.
    /// </summary>
    public float pillar_speed = 85f;
    public float structure_build_time = 0.05f;
    public float pillar_alive_time = 1f;

    public float ability_cooldown = 3f;
    public float _current_cooldown = 0f;
    public float current_cooldown { get { return _current_cooldown; } set { _current_cooldown = Mathf.Max(0f, value); } }
    public float shield_cooldown_refund_coefficient = 0.75f;
    public bool allow_pull_down_shield = true;

    /// <summary>
    /// All variables that when changed need to clear half_of_shield and re run ObjectPool().
    /// </summary>
    [Header("Variables underneath need to check 'Upgrade' for effects to work. Note console log to see if it worked")]
    public float pillar_height = 8f;
    public float pillar_height_offset = -1f;
    public float pillar_width = 1f;
    public float pillar_width_offset = -0.1f;

    public float structure_radius = 8f;
    public float structure_angle = 45f;
    public float pillar_recursive_angle = 9f;

    [Header("Time that gets refunded")]
    [SerializeField] private float _time_left_for_shield;
    private float time_left_for_shield
    {
        get { return _time_left_for_shield; }
        set { _time_left_for_shield = Mathf.Max(0f, value); }
    }

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
        time_left_for_shield -= Time.deltaTime;
    }

    /// <summary>
    /// Starts to use this ability.
    /// </summary>
    public void UsePrimary()
    {
        if (current_cooldown <= 0f)
        {
            current_cooldown = ability_cooldown;
            PullDownShield();
            PullUpShield(mouse_point.transform.forward, transform.position);
        }
        else
        {
            if (allow_pull_down_shield)
            {
                PullDownShield();
            }
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
        StopAllCoroutines();
        for (int i = 0; i < half_of_shield_pillar_amount; i++)
        {
            if (merged_mirrored_shield[i] != null)
            {
                merged_mirrored_shield[i].move_state = EarthbendingPillar.MoveStates.down;
            }
        }
        current_cooldown -= time_left_for_shield * shield_cooldown_refund_coefficient;
        time_left_for_shield = 0f;
    }

    /// <summary>
    /// Recursivly selects points around player and moves pillars to that point merging the mesh to a new EarthbendingPillar component.
    /// </summary>
    private void PullUpShield(Vector3 mouse_direction, Vector3 player_pos)
    {
        // Store all Quaternion.Euler(0f, angle + pillar_recursive_angle * 0.5f, 0f) * mouse_direction * structure_radius;
        // Store player position
        // Get difference of old and current Player position
        // If diff is large
        // Set state to EarthbendingPillar.MoveStates.down and create new pillars at the new points

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

            merged_circle_pillars.SetSharedValues(Mathf.Infinity, pillar_speed, pillar_height + pillar_height_offset * itter, material);

            // smootly rotates cubes instead of restricting/snapping it to 45 degrees
            // Quaternion rotation_left = Quaternion.LookRotation((shield_point_left - player_pos), Vector3.up);
            // Quaternion rotation_right = Quaternion.LookRotation((shield_point_right - player_pos), Vector3.up);

            StartCoroutine(EnablePillarAfter(structure_build_time * itter, merged_circle_pillars_game_object));

            merged_mirrored_shield[itter] = merged_circle_pillars;
            itter++;
        }
        StartCoroutine(PullDownShieldAfter(structure_build_time * itter + pillar_alive_time));
    }

    /// <summary>
    /// Makes it so that every pillar is spawned in so that PullDownShield() effects all pillars.
    /// </summary>
    private IEnumerator EnablePillarAfter(float time, GameObject pillar)
    {
        pillar.SetActive(false);
        WaitForSeconds wait = new WaitForSeconds(time);
        yield return wait;
        pillar.SetActive(true);
    }

    private IEnumerator PullDownShieldAfter(float time)
    {
        time_left_for_shield = time;
        WaitForSeconds wait = new WaitForSeconds(time);
        yield return wait;
        PullDownShield();
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
            half_of_shield_pillar_amount++;
        }

        half_of_shield = new EarthbendingPillar[half_of_shield_pillar_amount];
        merged_mirrored_shield = new EarthbendingPillar[half_of_shield_pillar_amount];
        for (int i = 0; i < half_of_shield_pillar_amount; i++)
        {
            GameObject pillar_game_object = CreateMesh.CreatePrimitive(CreateMesh.CubeMesh(), material, "earth_shield_object");
            EarthbendingPillar earthbending_pillar = pillar_game_object.AddComponent<EarthbendingPillar>();

            earthbending_pillar.InitEarthbendingPillar(pillar_height + pillar_height_offset * i, pillar_width + pillar_width_offset * i, Quaternion.Euler(0f, 45f, 0f), Mathf.Infinity, pillar_speed);
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
        PullDownShield();
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

/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script is child component of the Equipment hierarchy.
/// Holds specific functionallity and uppgrades for EarthShieldAbility Equipment.
/// </summary>
public class EarthOnCommandShieldAbility : MonoBehaviour, Ability.IAbility
{
    /// <summary>
    /// Used to get mouse position in world space.
    /// </summary>
    private MousePoint mouse_point;

    /// <summary>
    /// All variables that when changed need to clear half_of_shield and re run ObjectPool().
    /// </summary>
    public float pillar_height = 8f;
    public float shield_arch_magnitude = 2f; // private const (dependent on looks)
    public float pillar_width = 1; // private const (dependent on looks)
    public float pillar_width_arch_offset = -0.1f; // private const (dependent on looks)

    public float structure_radius = 12.5f; // private const (dependent on feels)
    public float structure_angle = 45f;
    public float pillar_recursive_angle = 1f; // private const

    /// <summary>
    /// All variables that can be changed on runtime to effect how this ability should behave.
    /// </summary>
    public float pillar_speed = 50f;
    public float pillar_alive_time = 0f;

    public float ability_cooldown = 1f;
    public float _current_cooldown = 0f;
    public float current_cooldown { get { return _current_cooldown; } set { _current_cooldown = Mathf.Max(0f, value); } }
    public float shield_cooldown_refund_coefficient = 0.75f;

    public float shield_time = 2f;

    public bool follow_player = true;

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
        if (follow_player)
        {
            MoveShieldToPoint();
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
            PullDownShield();
            PullUpShield(mouse_point.transform.forward, transform.position);
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
        StopAllCoroutines();
        foreach (EarthbendingPillar earthbending_pillar in earthbending_pillars)
        {
            earthbending_pillar.move_state = EarthbendingPillar.MoveStates.down;
        }
        current_shield_direction = Vector3.zero;
    }

    private IEnumerator StopShieldAfter()
    {
        yield return new WaitForSeconds(shield_time);
        PullDownShield();
    }

    /// <summary>
    /// Recursivly selects points around player and moves pillars to that point merging the mesh to a new EarthbendingPillar component.
    /// </summary>
    Vector3 current_shield_direction = Vector3.zero;
    private void PullUpShield(Vector3 mouse_direction, Vector3 player_pos)
    {
        current_shield_direction = mouse_direction;
        MoveShieldToPoint();
        StartCoroutine(StopShieldAfter());

        // DENNA FUNKTION ANVÄNDS FÖR ATT SPARA MOUSE_DIRECTION


        // Store all Quaternion.Euler(0f, angle + pillar_recursive_angle * 0.5f, 0f) * mouse_direction * structure_radius;
        // Store player position
        // Get difference of old and current Player position
        // If diff is large and no points are overlapping
        // Set state to EarthbendingPillar.MoveStates.down and create new pillars at the new points
    }

    private Vector3 SnapToGrid(Vector3 position)
    {
        Vector3 result;
        result.x = Mathf.Round(position.x / (pillar_width * 1.5f)) * pillar_width * 1.5f;
        result.y = Mathf.Round(position.y / (pillar_width * 1.5f)) * pillar_width * 1.5f;
        result.z = Mathf.Round(position.z / (pillar_width * 1.5f)) * pillar_width * 1.5f;

        return result;
    }

    private void MoveShieldToPoint()
    {

        // shield should be represented by a palabra
        // pillars should snap to a grid:
        // pillars should spawn on palabra end points
        // pillars should strive for palabra height: pillar_rigidbody.MovePosition(palabra height);

        // du har bågen på en cirkelsektor med hjälp av "for (float angle = pillar_recursive_angle; itter <= structure_angle / (pillar_recursive_angle * 2f); angle += pillar_recursive_angle)"
        // för alla punkter på cirkelsektorn SnapToGrid(punkter)
        // kolla om några av grid punkterna representeras av en EarthbendingPillar
        // om de gör det, ändra ground_point till parabeln och move_state till EarthbendingPillar.MoveStates.up.
        // om de inte gör det spawna en ny EarthbendingPillar.
        // kolla om det finns EarthbendingPillars som är under dig, isåfall delete.


        if (earthbending_pillars == null || current_shield_direction == Vector3.zero)
        {
            return;
        }

        List<Vector3> points = new List<Vector3>();
        for (float angle = -structure_angle * 0.5f; angle <= structure_angle * 0.5f; angle += pillar_recursive_angle)
        {
            Quaternion rotation = Quaternion.Euler(0f, angle, 0f);
            Vector3 point = transform.position + rotation * current_shield_direction * structure_radius;
            Vector3 grid_point = SnapToGrid(point);
            Vector3 grid_point_in_xz_plane = new Vector3(grid_point.x, 0f, grid_point.z);
            bool contains_point = points.Contains(grid_point_in_xz_plane);
            if (!contains_point)
            {
                points.Add(grid_point_in_xz_plane);

                bool pillar_exists = false;
                foreach (EarthbendingPillar earthbending_pillar in earthbending_pillars)
                {
                    Vector3 pillar_position = earthbending_pillar.transform.position;
                    Vector3 pillar_position_in_xz_plane = new Vector3(pillar_position.x, 0f, pillar_position.z);
                    if ((grid_point_in_xz_plane - pillar_position_in_xz_plane).sqrMagnitude < 0.1f)
                    {
                        earthbending_pillar.gameObject.SetActive(true);
                        earthbending_pillar.move_state = EarthbendingPillar.MoveStates.up;
                        //earthbending_pillar.ground_point.y += Mathf.Cos(shield_arch_magnitude * angle * Mathf.Deg2Rad) * pillar_height; // this point should be 
                        pillar_exists = true;
                        break;
                    }
                }
                if (!pillar_exists)
                {
                    Debug.Log(pillar_exists);
                    SpawnPillar(grid_point);
                }
            }
        }
    }

    /// <summary>
    /// Checks if there are allready instanciated pillar game objects that are inactive, if not add InstantiatePillar() to pool. Activate pillar game object and place it at given point.
    /// </summary>
    private EarthbendingPillar SpawnPillar(Vector3 point)
    {
        if (current_pool_index == earthbending_pillars.Count)
        {
            if (!earthbending_pillars[0].gameObject.activeSelf)
            {
                current_pool_index = 0;
            }
            else
            {
                earthbending_pillars.Add(InstantiatePillar());
            }
        }
        else if (earthbending_pillars[current_pool_index].gameObject.activeSelf)
        {
            earthbending_pillars.Insert(current_pool_index, InstantiatePillar());
        }

        earthbending_pillars[current_pool_index].gameObject.SetActive(true);
        earthbending_pillars[current_pool_index].PlacePillar(point);
        current_pool_index++;
        return earthbending_pillars[current_pool_index - 1];
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
    /// Creates new pillar game object instanciated with EarthbendingPillar component and right values and returns the newly created EarthbendingPillar component.
    /// </summary>
    private EarthbendingPillar InstantiatePillar()
    {
        GameObject pillar_game_object = GameObject.CreatePrimitive(PrimitiveType.Cube);
        EarthbendingPillar earthbending_pillar = pillar_game_object.AddComponent<EarthbendingPillar>();
        earthbending_pillar.InitEarthbendingPillar(pillar_height, pillar_width, Quaternion.Euler(0f, 45f, 0f), pillar_alive_time, pillar_speed);
        pillar_game_object.SetActive(false);
        return earthbending_pillar;
    }

    /// <summary>
    /// List of all instanciated EarthbendingPillar of this game object.
    /// </summary>
    private List<EarthbendingPillar> earthbending_pillars = null;
    private int current_pool_index = 0;

    /// <summary>
    /// Starts object pooling when ultimate is in inventory.
    /// </summary>
    public void ObjectPool()
    {
        earthbending_pillars = new List<EarthbendingPillar>();
        earthbending_pillars.Add(InstantiatePillar());
    }

    /// <summary>
    /// Delets pooled objects when ultimate is dropped.
    /// </summary>
    public void DeleteObjectPool()
    {
        if (earthbending_pillars == null)
        {
            return;
        }
        foreach (EarthbendingPillar earthbending_pillar in earthbending_pillars)
        {
            Destroy(earthbending_pillar.gameObject);
        }
        earthbending_pillars = null;
    }

    public void Upgrade()
    {

    }
}
*/