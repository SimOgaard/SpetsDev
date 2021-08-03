using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script is child component of the Equipment hierarchy.
/// Holds specific functionallity and uppgrades for FireballAbility Equipment.
/// </summary>
public class FireballAbility : MonoBehaviour, Ability.IAbility
{
    private Material fireball_burned_out_material;
    private Material fireball_material;
    private DamageByFire damage_by_fire;
    private SetFire set_fire;
    private MousePoint mouse_point;

    /// <summary>
    /// All variables that can be changed on runtime to effect how this ability should behave.
    /// </summary>
    public float velocity = 100f;

    public float ability_cooldown = 1f;
    public float _current_cooldown = 0f;
    public float current_cooldown { get { return _current_cooldown; } set { _current_cooldown = Mathf.Max(0f, value); } }

    /// <summary>
    /// All variables that when changed need to delete all fireballs in all fireball lists so they are reinstanciated.
    /// </summary>
    public float ground_fire_radius = 4f;
    public float ground_fire_time_min = 2.5f;
    public float ground_fire_time_max = 7.5f;
    public float fireball_fire_time_min = 3.5f;
    public float fireball_fire_time_max = 5f;
    public float on_collision_damage = 20f;
    public float fire_damage = 2f;

    public bool penetrate_enemies = false;
    public bool explode_on_first_hit = true;

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
            ThrowFireball();
        }
    }

    /// <summary>
    /// Stops this ability.
    /// </summary>
    public void StopPrimary()
    {

    }

    /// <summary>
    /// Calculates roots of a given quadratic equation and returns arctangent of most valid root.
    /// </summary>
    public float CalculateQuadraticEquation(float a, float b, float c)
    {
        float disc, deno, x1, x2;
        if (a == 0)
        {
            // The roots are Linear
            x1 = Mathf.Atan(-c / b);
            return x1;
        }
        else
        {
            disc = (b * b) - (4 * a * c);
            deno = 2 * a;
            if (disc > 0)
            {
                // The roots are real and distinct roots
                x1 = Mathf.Atan((-b / deno) + (Mathf.Sqrt(disc) / deno));
                x2 = Mathf.Atan((-b / deno) - (Mathf.Sqrt(disc) / deno));

                if (Mathf.PI / 4f >= x1 && x1 >= -Mathf.PI / 2f)
                {
                    return x1;
                }
                if (Mathf.PI / 4f >= x2 && x2 >= -Mathf.PI / 2f)
                {
                    return x2;
                }
                return Mathf.PI / 4f; // wat???
                Debug.Log("Should be clamped?");
            }
            else if (disc == 0)
            {
                // The roots are repeated roots
                x1 = Mathf.Atan(-b / deno);
                return x1;
            }
            else
            {
                // The roots are imaginary;
                return Mathf.PI / 4f;
            }
        }
    }

    /// <summary>
    /// Checks if there are allready instanciated fireball game objects that are inactive, if not add InstantiateFireball() to pool. Activate fireball game object and apply calculated velocity to reach given mouse to world position.
    /// </summary>
    private void ThrowFireball()
    {
        // UpdateFire();

        if (current_pool_index == fireball_projectiles.Count)
        {
            if (!fireball_projectiles[0].gameObject.activeSelf)
            {
                current_pool_index = 0;
            }
            else
            {
                fireball_projectiles.Add(InstanciateFireballProjectile());
            }
        }
        else if (fireball_projectiles[current_pool_index].gameObject.activeSelf)
        {
            fireball_projectiles.Insert(current_pool_index, InstanciateFireballProjectile());
        }

        fireball_projectiles[current_pool_index].gameObject.SetActive(true);
        fireball_projectiles[current_pool_index].Reset(transform.position + new Vector3(0f, 3f, 0f));
        fireball_projectiles[current_pool_index].StartBurning();
        
        Vector3 aim_point = mouse_point.GetWorldPointAndEnemyMid();
        Vector3 start_point = fireball_projectiles[current_pool_index].transform.position;

        Vector3 force_direction = (aim_point - start_point);
        Vector3 force_direction_normal = force_direction.normalized;

        float distance_y = -force_direction.y;
        float distance_x = new Vector2(force_direction.x, force_direction.z).magnitude;

        float a = -200f * 0.5f * Mathf.Pow(distance_x / velocity, 2);
        float rad = CalculateQuadraticEquation(a, distance_x, a + distance_y);

        Vector2 rotate_around = new Vector2(force_direction.x, force_direction.z).normalized;
        float angle_y = Mathf.Rad2Deg * Mathf.Atan2(rotate_around.x, rotate_around.y) - 90f;

        Quaternion rotation = Quaternion.Euler(0f, angle_y, Mathf.Rad2Deg * rad);

        fireball_projectiles[current_pool_index].ApplyVelocity(rotation * Vector3.right * velocity);

        current_pool_index++;
    }

    /// <summary>
    /// Returns fireball icon for ui element.
    /// </summary>
    public Sprite GetIconSprite()
    {
        return icon_sprite;
    }

    private Sprite icon_sprite;
    private void Start()
    {
        icon_sprite = Resources.Load<Sprite>("Sprites/UI/fireball");
        fireball_material = Resources.Load<Material>("Prefabs/FireballMaterial");
        mouse_point = GameObject.Find("MouseRot").GetComponent<MousePoint>();
        fireball_burned_out_material = Resources.Load<Material>("Prefabs/AshballMaterial");
        set_fire = GameObject.Find("Fire").GetComponent<SetFire>();
        damage_by_fire = GameObject.Find("Flammable").GetComponent<DamageByFire>();
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
    /// Creates new fireball game object instanciated with FireballProjectile component and all other necessary components and returns the newly created FireballProjectile component.
    /// </summary>
    private FireballProjectile InstanciateFireballProjectile()
    {
        GameObject fireball_game_object = new GameObject("fireball");

        GameObject fireball_trigger_game_object = new GameObject("fireball_trigger");
        fireball_trigger_game_object.transform.parent = fireball_game_object.transform;
        fireball_trigger_game_object.transform.position = Vector3.zero;
        fireball_trigger_game_object.layer = 14;
        SphereCollider fireball_trigger_sphere_collider = fireball_trigger_game_object.AddComponent<SphereCollider>();
        FireballProjectileTrigger fireball_projectile_trigger = fireball_trigger_game_object.AddComponent<FireballProjectileTrigger>();
        fireball_trigger_sphere_collider.isTrigger = true;
        fireball_trigger_sphere_collider.radius = 1f;

        fireball_game_object.layer = 14;
        fireball_game_object.transform.position = Vector3.zero;
        fireball_game_object.transform.localScale = new Vector3(2f, 2f, 2f);

        SphereCollider fireball_collider = fireball_game_object.AddComponent<SphereCollider>();
        fireball_collider.radius = 0.5f;

        Rigidbody fireball_rigidbody = fireball_game_object.AddComponent<Rigidbody>();
        fireball_rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        fireball_rigidbody.angularDrag = 1000f;

        MeshRenderer fireball_mesh_renderer = fireball_game_object.AddComponent<MeshRenderer>();
        MeshFilter fireball_mesh_filter = fireball_game_object.AddComponent<MeshFilter>();
        fireball_mesh_filter.mesh = DropItem.GetLowPolySphereMesh();

        FireballProjectile fireball_projectile = fireball_game_object.AddComponent<FireballProjectile>();
        fireball_projectile_trigger.Init(fireball_projectile);
        fireball_projectile.InitVar(set_fire, fireball_burned_out_material, fireball_material, fireball_collider, fireball_mesh_renderer, fireball_rigidbody);
        fireball_projectile.UpgradeVar(ground_fire_time_min, ground_fire_time_max, fireball_fire_time_min, fireball_fire_time_max, explode_on_first_hit, penetrate_enemies, on_collision_damage);
        damage_by_fire.UpdateFireDamage(fire_damage);

        fireball_game_object.SetActive(false);
        return fireball_projectile;
    }

    /// <summary>
    /// Updates ground fire visuals and damage radius.
    /// </summary>
    private void UpdateFire()
    {
        damage_by_fire.UpdateFireDistance(ground_fire_radius);
        Debug.Log("Updating fire visuals are not yet implemented lamao");
        return;
        set_fire.UpdateFire(ground_fire_radius);
    }

    private List<FireballProjectile> fireball_projectiles;
    private int current_pool_index = 0;

    /// <summary>
    /// Starts object pooling when ability is in inventory.
    /// </summary>
    public void ObjectPool()
    {
        DeleteObjectPool();
        fireball_projectiles = new List<FireballProjectile>();
        fireball_projectiles.Add(InstanciateFireballProjectile());
    }

    /// <summary>
    /// Delets pooled objects when ability is dropped.
    /// </summary>
    public void DeleteObjectPool()
    {
        if (fireball_projectiles == null)
        {
            return;
        }
        for (int i = 0; i < fireball_projectiles.Count; i++)
        {
            Destroy(fireball_projectiles[i].gameObject);
        }
        fireball_projectiles = null;
        current_pool_index = 0;
    }

    public void Upgrade()
    {
        damage_by_fire.UpdateFireDamage(fire_damage);

        foreach (FireballProjectile fireball_projectile in fireball_projectiles)
        {
            fireball_projectile.UpgradeVar(ground_fire_time_min, ground_fire_time_max, fireball_fire_time_min, fireball_fire_time_max, explode_on_first_hit, penetrate_enemies, on_collision_damage);
        }
    }
}
