using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireballProjectile : MonoBehaviour
{
    private SphereCollider sphere_collider;
    private MeshRenderer mesh_renderer;
    private Rigidbody rigid_body;
    private Vector3 seed;
    [SerializeField] private Vector3 fire_heat_lift_force = new Vector3(0f, 25f, 0f);
    [SerializeField] private float air_effect_magnitude = 0.1f;

    private Vector3 last_burn_contact_point = Vector3.zero;

    private SetFire fire;
    private bool is_burned_out;
    private Material burned_out_material;

    public void InitVar(SetFire set_fire, Material burned_out_material, SphereCollider sphere_collider, MeshRenderer mesh_renderer, Rigidbody rigid_body)
    {
        fire = set_fire;
        this.burned_out_material = burned_out_material;
        this.sphere_collider = sphere_collider;
        this.mesh_renderer = mesh_renderer;
        this.rigid_body = rigid_body;
    }

    private void Start()
    {
        gameObject.layer = 14;
        sphere_collider.radius = 0.5f;

        //rigid_body.angularDrag = 10000f;

        StartCoroutine(BurnOut(10f));

        seed = new Vector3(Random.Range(-100f, 100f), Random.Range(-100f, 100f), Random.Range(-100f, 100f));
    }

    private void Update()
    {
        mesh_renderer.material.SetVector("_FireBallCentre", transform.position + seed);
        mesh_renderer.material.SetVector("_FireDirection", rigid_body.velocity * air_effect_magnitude - fire_heat_lift_force);
    }

    private void OnCollisionStay(Collision collision)
    {
        if (is_burned_out)
        {
            return;
        }

        Vector3 new_burn_contact_point = transform.position;
        new_burn_contact_point.y -= 1f;
        if ((last_burn_contact_point - new_burn_contact_point).sqrMagnitude > 3f)
        {
            if (collision.gameObject.tag == "Flammable")
            {
                fire.UpdateFlammableFire(new_burn_contact_point, Random.Range(2f, 8f));
            }
            else
            {
                fire.UpdateNonFlammableFire(new_burn_contact_point, Random.Range(0.55f, 0.6f));
            }
            last_burn_contact_point = new_burn_contact_point;
        }
    }

    private IEnumerator BurnOut(float time)
    {
        yield return new WaitForSeconds(time);
        is_burned_out = true;
        mesh_renderer.material = burned_out_material;
        Destroy(gameObject, time * 0.5f);
    }
}
