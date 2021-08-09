using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controlls fireball game objects.
/// </summary>
public class FireballProjectile : MonoBehaviour
{
    private FireballAbility fireball_ability;
    private Rigidbody rigid_body;
    private MeshRenderer mesh_renderer;

    [SerializeField] private Vector3 fire_heat_lift_force = new Vector3(0f, 25f, 0f);
    [SerializeField] private float air_effect_magnitude = 0.1f;
    public bool is_burned_out = false;

    private Vector3 last_burn_contact_point = Vector3.zero;

    private bool has_exploded = false;

    /// <summary>
    /// Returns ability on hit damage.
    /// </summary>
    public float OnHitDamage()
    {
        return fireball_ability.on_collision_damage;
    }

    /// <summary>
    /// Set variables when initializing fireball in FireballAbility.
    /// </summary>
    public void InitVar(FireballAbility fireball_ability, Rigidbody rigid_body, MeshRenderer mesh_renderer)
    {
        this.fireball_ability = fireball_ability;
        this.rigid_body = rigid_body;
        this.mesh_renderer = mesh_renderer;
    }

    /// <summary>
    /// Updates collisions.
    /// </summary>
    public void UpgradeVar()
    {
        gameObject.layer = fireball_ability.penetrate_enemies ? 15 : 14;
        rigid_body.angularDrag = fireball_ability.rigidbody_angular_drag;
        rigid_body.mass = fireball_ability.rigidbody_mass;
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
        mesh_renderer.material = fireball_ability.fireball_material;
        is_burned_out = false;
        StartCoroutine(BurnOut(Random.Range(fireball_ability.fireball_fire_time_min, fireball_ability.fireball_fire_time_max)));
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

    /// <summary>
    /// Deals damage around fireball and applies force to enemies.
    /// </summary>
    public void Explode()
    {
        if (!has_exploded && fireball_ability.explode_on_first_hit)
        {
            Vector3 fireball_pos = transform.position;
            Collider[] all_collisions = Physics.OverlapSphere(fireball_pos, fireball_ability.explosion_radius);
            foreach (Collider collider in all_collisions)
            {
                if (collider.gameObject == gameObject || collider.gameObject.layer == 11)
                {
                    continue;
                }
                if (collider.gameObject.layer == 16)
                {
                    collider.GetComponent<EnemyAI>().Damage(fireball_ability.explosion_damage);
                    collider.attachedRigidbody.AddExplosionForce(fireball_ability.explosion_force, fireball_pos, 0f, 1f, ForceMode.Impulse);
                }
                else if (collider.TryGetComponent(out Rigidbody rigid_body))
                {
                    rigid_body.AddExplosionForce(fireball_ability.explosion_force, fireball_pos, 0f, 1f, ForceMode.Impulse);
                }
            }
            fireball_ability.explosion_prefab.transform.position = transform.position;
            fireball_ability.explosion_prefab.SetActive(false);
            fireball_ability.explosion_prefab.SetActive(true);
            has_exploded = true;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!is_burned_out)
        {
            Explode();
        }
    }

    /// <summary>
    /// While colliding with ground check collision tag and update fire in SetFire.
    /// </summary>
    private void OnCollisionStay(Collision collision)
    {
        if (is_burned_out || (MousePoint.layer_mask_world.value & 1 << collision.gameObject.layer) == 0)
        {
            return;
        }

        ContactPoint contact_point = collision.GetContact(0);
        Vector3 new_burn_contact_point = contact_point.point;
        Vector3 normal = contact_point.normal;
        if ((last_burn_contact_point - new_burn_contact_point).sqrMagnitude > 3f)
        {
            if (collision.gameObject.tag == "Flammable")
            {
                fireball_ability.set_fire.UpdateFlammableFire(new_burn_contact_point, normal, Random.Range(fireball_ability.ground_fire_time_min, fireball_ability.ground_fire_time_max));
            }
            else
            {
                fireball_ability.set_fire.UpdateNonFlammableFire(new_burn_contact_point, normal, Random.Range(fireball_ability.ground_fire_time_min / 10f, fireball_ability.ground_fire_time_max / 10f));
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
        mesh_renderer.material = fireball_ability.fireball_burned_out_material;
        yield return new WaitForSeconds(time * 0.5f);
        gameObject.SetActive(false);
    }
}
