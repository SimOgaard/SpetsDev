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
        return is_burned_out ? fireball_ability.on_collision_damage_burned_out : fireball_ability.on_collision_damage;
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
        gameObject.layer = fireball_ability.penetrate_enemies ? Layer.ignore_enemy_collision : Layer.ignore_player_collision;
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
        if (transform.position.y < Water.water_level && !is_burned_out)
        {
            is_burned_out = true;
            StopAllCoroutines();
            StartCoroutine(BurnOut(0f));
            StartCoroutine(BurnedOut(fireball_ability.fireball_fire_time_min));
        }

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
                if (collider.gameObject == gameObject || Layer.IsInLayer(Layer.ignore_external_forces, collider.gameObject.layer))
                {
                    continue;
                }
                if (Layer.IsInLayer(Layer.enemy, collider.gameObject.layer))
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

            StartCoroutine(RaycastFibonacciSphere(100, 0.75f, 2f, 0.0075f, 0.025f));
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private IEnumerator RaycastFibonacciSphere(int amount, float cutoff, float y_offset, float time_between, float time_begining)
    {
        Vector3 start_point = transform.position + Vector3.up * y_offset;

        float phi = Mathf.PI * (3.0f - Mathf.Sqrt(5.0f));

        yield return new WaitForSeconds(time_begining);
        WaitForSeconds wait_between = new WaitForSeconds(time_between);

        for (int i = amount; i > amount * cutoff; i--)
        {
            yield return wait_between;

            float y = 1f - (i / (amount - 1f)) * 2f;
            float radius = Mathf.Sqrt(1f - y * y);

            float theta = phi * i;

            float x = Mathf.Cos(theta) * radius;
            float z = Mathf.Sin(theta) * radius;

            Vector3 point = new Vector3(x, y, z);

            RaycastHit hit;
            if (Physics.Raycast(start_point, point, out hit, fireball_ability.explosion_radius, Layer.Mask.static_ground))
            {
                if (Tag.IsTaggedWith(hit.collider.tag, Tag.flammable))
                {
                    fireball_ability.set_fire.UpdateFlammableFire(hit.point, hit.normal, Random.Range(fireball_ability.ground_fire_time_min, fireball_ability.ground_fire_time_max));
                }
                else
                {
                    fireball_ability.set_fire.UpdateNonFlammableFire(hit.point, hit.normal, Random.Range(fireball_ability.ground_fire_time_min / 10f, fireball_ability.ground_fire_time_max / 10f));
                }
            }
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
        if (is_burned_out || !Layer.IsInLayer(Layer.Mask.static_ground, collision.gameObject.layer))
        {
            return;
        }

        ContactPoint contact_point = collision.GetContact(0);
        Vector3 new_burn_contact_point = contact_point.point;
        Vector3 normal = contact_point.normal;
        if ((last_burn_contact_point - new_burn_contact_point).sqrMagnitude > 3f)
        {
            if (Tag.IsTaggedWith(collision.gameObject.tag, Tag.flammable))
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
    /// After given time stop adding contact points to SetFire during collisions and change material of fireball.
    /// </summary>
    private IEnumerator BurnOut(float time)
    {
        yield return new WaitForSeconds(time);
        is_burned_out = true;
        mesh_renderer.material = fireball_ability.fireball_burned_out_material;
        StartCoroutine(BurnedOut(fireball_ability.fireball_fire_time_min));
    }

    /// <summary>
    /// Disable game object after given time.
    /// </summary>
    private IEnumerator BurnedOut(float time)
    {
        yield return new WaitForSeconds(time);
        gameObject.SetActive(false);
    }
}
