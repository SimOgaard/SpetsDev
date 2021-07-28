using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script is child component of the Equipment hierarchy.
/// Holds specific functionallity and uppgrades for EarthbendingUltimate Equipment.
/// </summary>
public class EarthbendingUltimate : MonoBehaviour, Ultimate.IUltimate
{
    private MousePoint mouse_point;

    /// <summary>
    /// All variables that can be changed on runtime to effect how this ultimate should behave.
    /// </summary>
    public bool stach_pillars = false;
    public bool follow_mouse = false;
    public bool start_at_mouse = false;

    public int pillar_amount = 10;
    public int current_pillar_amount = 10;

    public float traverse_time = 0.1f;
    public float distance_between = 3.25f;
    public float extra_distance_first_spawn = 3.25f;

    public float alive_time = 5f;

    public float ultimate_cooldown = 2f;
    public float _current_cooldown = 0f;
    public float current_cooldown { get { return _current_cooldown; } set { _current_cooldown = Mathf.Max(0f, value); } }
    private bool is_casting = false;

    /// <summary>
    /// All variables that when changed need to reinstanciate all pillars in earthbending_pillars with earthbending_pillars[i].ChangePillar().
    /// </summary>
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
        if(current_cooldown <= 0f && current_pillar_amount < pillar_amount && !is_casting)
        {
            current_pillar_amount++;
            current_cooldown = ultimate_cooldown / pillar_amount;
        }
    }

    /// <summary>
    /// Starts to use this ultimate.
    /// </summary>
    public void UsePrimary()
    {
        if((current_pillar_amount >= pillar_amount) && current_cooldown <= 0f || stach_pillars && !is_casting)
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

    private IEnumerator FollowMouse()
    {
        Vector3 mouse_point_pos = mouse_point.GetWorldPoint();
        Vector3 last_point = start_at_mouse ? mouse_point_pos : transform.position;
        Vector3 direction_straight = (new Vector3(mouse_point_pos.x, 0f, mouse_point_pos.z) - new Vector3(transform.position.x, 0f, transform.position.z)).normalized;

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

            last_point = new_point;
            current_pillar_amount--;
            first_spawn = false;
            SpawnPillar(new_point);
            yield return wait;
        }

        is_casting = false;
    }

    /// <summary>
    /// Old ultimate concept that can be used in other abilities.
    /// </summary>
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

                continue;
                Debug.Log("This is where you were to spawn two pillars on spawn_point_left and spawn_point_right");
            }
            yield return wait;
        }
    }

    /// <summary>
    /// Checks if there are allready instanciated pillar game objects that are inactive, if not add InstantiatePillar() to pool. Activate pillar game object and place it at given point.
    /// </summary>
    private void SpawnPillar(Vector3 point)
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

    /// <summary>
    /// Returns current cooldown of equipment.
    /// </summary>
    public float GetCurrentCooldown()
    {
        return ultimate_cooldown - (current_pillar_amount * (ultimate_cooldown / pillar_amount)) + current_cooldown;
    }
    /// <summary>
    /// Returns cooldown of equipment.
    /// </summary>
    public float GetCooldown()
    {
        return ultimate_cooldown;
    }

    /// <summary>
    /// Creates new pillar game object instanciated with EarthbendingPillar component and right values and returns the newly created EarthbendingPillar component.
    /// </summary>
    private EarthbendingPillar InstantiatePillar()
    {
        GameObject pillar_game_object = GameObject.CreatePrimitive(PrimitiveType.Cube);
        EarthbendingPillar earthbending_pillar = pillar_game_object.AddComponent<EarthbendingPillar>();
        earthbending_pillar.InitEarthbendingPillar(pillar_height, pillar_width, Quaternion.Euler(0f, 45f, 0f), alive_time, pillar_growth_speed);
        pillar_game_object.SetActive(false);
        return earthbending_pillar;
    }

    /// <summary>
    /// List of all instanciated EarthbendingPillar of this game object.
    /// </summary>
    private List<EarthbendingPillar> earthbending_pillars;
    private int current_pool_index = 0;

    /// <summary>
    /// Starts object pooling when ultimate is in inventory.
    /// </summary>
    public void ObjectPool()
    {
        earthbending_pillars = new List<EarthbendingPillar>();
        for (int i = 0; i < pillar_amount; i++)
        {
            earthbending_pillars.Add(InstantiatePillar());
        }
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
        foreach(EarthbendingPillar earthbending_pillar in earthbending_pillars)
        {
            Destroy(earthbending_pillar.gameObject);
        }
        earthbending_pillars = null;
    }

    public void Upgrade()
    {

    }
}
