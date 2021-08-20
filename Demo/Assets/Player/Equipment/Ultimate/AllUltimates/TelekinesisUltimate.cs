using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script is child component of the Equipment hierarchy.
/// Holds specific functionallity and uppgrades for EarthbendingUltimate Equipment.
/// </summary>
public class TelekinesisUltimate : MonoBehaviour, Ultimate.IUltimate
{
    public bool upgrade = false;

    private MousePoint mouse_point;
    private GameObject held_object = null;

    /// <summary>
    /// All variables that can be changed on runtime to effect how this ultimate should behave.
    /// </summary>
    public float attraction_speed = 5f;
    public float attraction_point_y_offset = 3f;

    public float max_throwing_force = 500f;
    public float throwing_force_time_multiplier = 500f;

    public float max_rotation_velocity = 3f;
    public float rotation_acceleration = 3f;

    private float held_object_time = 0f;

    public float sphere_cast_radius = 1.5f;
    public float ultimate_cooldown = 0.25f;
    public float _current_cooldown = 0f;
    public float current_cooldown { get { return _current_cooldown; } set { _current_cooldown = Mathf.Max(0f, value); } }

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
        held_object = null;
        transform.GetComponent<MeshRenderer>().material.SetColor("_Color", Color.cyan);
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

        if (held_object != null)
        {
            RotateObject();
            MoveObjectToPosition();
            held_object_time += Time.deltaTime;
            //MoveObjectToMouse();
        }
    }

    private void RotateObject()
    {
        float x = Random.Range(-1f, 1f);
        float y = Random.Range(-1f, 1f);
        float z = Random.Range(-1f, 1f);

        Vector3 rotate_vector = new Vector3(x, y, z) * Mathf.Min(max_rotation_velocity, held_object_time * rotation_acceleration);

        held_object.transform.Rotate(rotate_vector);
    }

    private void MoveObjectToPosition()
    {
        Vector3 held_object_position = held_object.transform.position;
        Vector3 hold_position = transform.position + Vector3.up * attraction_point_y_offset + (mouse_point.MousePosition2D() - transform.position).normalized * 10f;

        held_object.transform.position = Vector3.Lerp(held_object_position, hold_position, attraction_speed * Time.deltaTime);
    }

    /// <summary>
    /// Starts to use this ultimate.
    /// </summary>
    public void UsePrimary()
    {
        if (current_cooldown <= 0f)
        {
            held_object = mouse_point.GetGameObjectWithRigidbody(sphere_cast_radius);
            if (held_object != null)
            {
                current_cooldown = ultimate_cooldown;
                held_object.GetComponent<Rigidbody>().useGravity = false;
                Debug.Log(held_object);
            }
        }
    }

    /// <summary>
    /// Stops this ultimate.
    /// </summary>
    public void StopPrimary()
    {
        if (held_object != null)
        {
            Rigidbody rigid_body = held_object.GetComponent<Rigidbody>();
            rigid_body.useGravity = true;
            //ApplyForwardForce(rigid_body);
            ApplyMousePointForce(rigid_body);
            held_object = null;
            held_object_time = 0f;
        }
    }

    private void ApplyForwardForce(Rigidbody rigid_body)
    {
        Vector3 force_direction = (mouse_point.MousePosition2D() - transform.position).normalized;

        rigid_body.AddForce(force_direction * Mathf.Min(max_throwing_force, throwing_force_time_multiplier * held_object_time), ForceMode.Impulse);
    }

    private void ApplyMousePointForce(Rigidbody rigid_body)
    {
        Vector3 force_direction = (mouse_point.GetWorldPointAndEnemyMid() - transform.position).normalized;

        rigid_body.AddForce(force_direction * Mathf.Min(max_throwing_force, throwing_force_time_multiplier * held_object_time), ForceMode.Impulse);
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
        return current_cooldown;
    }
    /// <summary>
    /// Returns cooldown of equipment.
    /// </summary>
    public float GetCooldown()
    {
        return ultimate_cooldown;
    }

    /// <summary>
    /// Starts object pooling when ultimate is in inventory.
    /// </summary>
    public void ObjectPool()
    {
        
    }

    /// <summary>
    /// Delets pooled objects when ultimate is dropped.
    /// </summary>
    public void DeleteObjectPool()
    {
        
    }

    public void Upgrade()
    {
        
    }
}
