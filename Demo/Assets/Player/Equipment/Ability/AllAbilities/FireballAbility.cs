using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script is child component of the Equipment hierarchy.
/// Holds specific functionallity and uppgrades for FireballAbility Equipment.
/// </summary>
public class FireballAbility : MonoBehaviour, Ability.IAbility
{
    private Material fireball_material;
    private SetFire set_fire;
    private Material fireball_burned_out_material;

    private MousePoint mouse_point;

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
    /// Starts to use this ability.
    /// </summary>
    public void UsePrimary()
    {
        InstantiateFireball();
    }

    /// <summary>
    /// Stops this ultimate.
    /// </summary>
    public void StopPrimary()
    {

    }

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
                Debug.Log("WHAT");
                return Mathf.PI / 4f; // wat???
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

    private void InstantiateFireball()
    {
        GameObject fireball_game_object = new GameObject("fireball");
        FireballProjectile fireball_projectile = fireball_game_object.AddComponent<FireballProjectile>();
        SphereCollider fireball_collider = fireball_game_object.AddComponent<SphereCollider>();
        Rigidbody fireball_rigidbody = fireball_game_object.AddComponent<Rigidbody>();
        MeshRenderer fireball_mesh_renderer = fireball_game_object.AddComponent<MeshRenderer>();
        MeshFilter fireball_mesh_filter = fireball_game_object.AddComponent<MeshFilter>();
        fireball_projectile.InitVar(set_fire, fireball_burned_out_material, fireball_collider, fireball_mesh_renderer, fireball_rigidbody);
        fireball_game_object.transform.position = transform.position + new Vector3(0f, 3f, 0f);
        fireball_game_object.transform.localScale = new Vector3(2f, 2f, 2f);
        fireball_mesh_renderer.material = fireball_material;
        fireball_mesh_filter.mesh = Equipment.GetLowPolySphereMesh();


        Vector3 aim_point = mouse_point.GetWorldPoint();
        Vector3 start_point = fireball_game_object.transform.position;
        float velocity = 100f;

        // EWWWWWW, but works so whatever

        Vector3 force_direction = (aim_point - start_point);
        Vector3 force_direction_normal = force_direction.normalized;

        float distance_y = -force_direction.y;
        float distance_x = new Vector2(force_direction.x, force_direction.z).magnitude;

        float a = -200f * 0.5f * Mathf.Pow(distance_x / velocity, 2);
        float rad = CalculateQuadraticEquation(a, distance_x, a + distance_y);

        Vector2 rotate_around = new Vector2(force_direction.x, force_direction.z).normalized;
        float angle_y = Mathf.Rad2Deg * Mathf.Atan2(rotate_around.x, rotate_around.y) - 90f;

        Quaternion rotation = Quaternion.Euler(0f, angle_y, Mathf.Rad2Deg * rad);

        fireball_rigidbody.velocity = rotation * Vector3.right * velocity;
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
    }
}
