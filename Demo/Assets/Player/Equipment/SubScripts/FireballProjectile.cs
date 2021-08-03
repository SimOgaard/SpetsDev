using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controlls fireball game objects.
/// </summary>
public class FireballProjectile : MonoBehaviour
{
    private SetFire set_fire;
    private Rigidbody rigid_body;
    private SphereCollider sphere_collider;
    private MeshRenderer mesh_renderer;

    private Material burned_out_material;
    private Material fireball_material;
    [SerializeField] private Vector3 fire_heat_lift_force = new Vector3(0f, 25f, 0f);
    [SerializeField] private float air_effect_magnitude = 0.1f;
    public bool is_burned_out = false;

    private Vector3 last_burn_contact_point = Vector3.zero;

    private float ground_fire_time_min;
    private float ground_fire_time_max;
    private float fireball_fire_time_min;
    private float fireball_fire_time_max;
    private bool explode_on_first_hit;
    private bool penetrate_enemies;

    private bool has_exploded = false;

    public float on_collision_damage;

    /// <summary>
    /// Set variables when initializing fireball in FireballAbility.
    /// </summary>
    public void InitVar(SetFire set_fire, Material burned_out_material, Material fireball_material, SphereCollider sphere_collider, MeshRenderer mesh_renderer, Rigidbody rigid_body)
    {
        this.set_fire = set_fire;
        this.burned_out_material = burned_out_material;
        this.fireball_material = fireball_material;
        this.sphere_collider = sphere_collider;
        this.mesh_renderer = mesh_renderer;
        this.rigid_body = rigid_body;
    }

    public void UpgradeVar(float ground_fire_time_min, float ground_fire_time_max, float fireball_fire_time_min, float fireball_fire_time_max, bool explode_on_first_hit, bool penetrate_enemies, float on_collision_damage)
    {
        this.ground_fire_time_min = ground_fire_time_min;
        this.ground_fire_time_max = ground_fire_time_max;
        this.fireball_fire_time_min = fireball_fire_time_min;
        this.fireball_fire_time_max = fireball_fire_time_max;
        this.explode_on_first_hit = explode_on_first_hit;
        this.penetrate_enemies = penetrate_enemies;
        this.on_collision_damage = on_collision_damage;

        gameObject.layer = penetrate_enemies ? 15 : 14;
    }

    /// <summary>
    /// Reset position of fireball and stop all coroutines.
    /// </summary>
    public void Reset(Vector3 pos)
    {
        StopAllCoroutines();
        transform.position = pos;
    }

    private void Start()
    {
        StartBurning();
    }

    /// <summary>
    /// Change material to burning fireball and start BurnOut() coroutine.
    /// </summary>
    public void StartBurning()
    {
        mesh_renderer.material = fireball_material;
        is_burned_out = false;
        StartCoroutine(BurnOut(Random.Range(fireball_fire_time_min, fireball_fire_time_max)));
    }

    /// <summary>
    /// Apply velocity to rigidbody
    /// </summary>
    public void ApplyVelocity(Vector3 vel)
    {
        has_exploded = false;
        rigid_body.velocity = vel;
    }

    /// <summary>
    /// Update material properties to animate fireball.
    /// </summary>
    private void Update()
    {
        mesh_renderer.material.SetVector("_FireBallCentre", transform.position);
        mesh_renderer.material.SetVector("_FireDirection", rigid_body.velocity * air_effect_magnitude - fire_heat_lift_force);
    }

    public void Explode()
    {
        if (!has_exploded && explode_on_first_hit)
        {
            Debug.Log("EXPLODE WOW");
            has_exploded = true;
        }
    }

    /// <summary>
    /// While colliding with ground check collision tag and update fire in SetFire.
    /// </summary>
    private void OnCollisionStay(Collision collision)
    {
        if (is_burned_out)
        {
            return;
        }

        Explode();

        Vector3 new_burn_contact_point = transform.position;
        new_burn_contact_point.y -= 1f;
        if ((last_burn_contact_point - new_burn_contact_point).sqrMagnitude > 3f)
        {
            if (collision.gameObject.tag == "Flammable")
            {
                set_fire.UpdateFlammableFire(new_burn_contact_point, Random.Range(ground_fire_time_min, ground_fire_time_max));
            }
            else
            {
                set_fire.UpdateNonFlammableFire(new_burn_contact_point, Random.Range(ground_fire_time_min / 10f, ground_fire_time_max / 10f));
            }
            last_burn_contact_point = new_burn_contact_point;
        }
    }

    /// <summary>
    /// After given time stop adding contact points to SetFire during collisions and change material of fireball. Then destroy game object.
    /// </summary>
    private IEnumerator BurnOut(float time)
    {
        yield return new WaitForSeconds(time);
        is_burned_out = true;
        mesh_renderer.material = burned_out_material;
        yield return new WaitForSeconds(time * 0.5f);
        gameObject.SetActive(false);
    }
}
