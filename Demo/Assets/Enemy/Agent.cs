using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent : MonoBehaviour
{
    private bool is_dead = false;
    private bool complete_stop = false;
    public bool is_stopped = false;
    public Vector3 destination;
    public float wanted_move_speed = 0f;

    [SerializeField] private float angular_speed = 80f;
    [SerializeField] private float acceleration = 8f;
    private float move_speed = 0f;

    private Vector3 move_direction_normalized;
    private Vector3 slope_move_direction_normalized;
    private Vector3 look_dir;

    public int trigger_count;
    public bool is_grounded;
    private RaycastHit slope_hit;

    [SerializeField] private float height;
    private Rigidbody enemy_rigidbody;
    private float gravity = 25000f;

    private RigidbodyConstraints rigidbody_constraints_grounded = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    private RigidbodyConstraints rigidbody_constraints_airborn = RigidbodyConstraints.None;

    /// <summary>
    /// Completely stops agent from moving and removes any angular velocity / velocity. AAAAAH WHYYYY DO I NEED TO REMOVE RIGIDBODY
    /// </summary>
    public void CompleteStop()
    {
        DestroyImmediate(enemy_rigidbody);
        enemy_rigidbody = gameObject.AddComponent<Rigidbody>();
        enemy_rigidbody.isKinematic = true;
        complete_stop = true;
    }

    /// <summary>
    /// Stops agent from moving and removes any rigidbody restraints.
    /// </summary>
    public void Die()
    {
        is_dead = true;
        is_stopped = true;
        enemy_rigidbody.constraints = RigidbodyConstraints.None;
    }

    private void Start()
    {
        height *= 0.5f;
        enemy_rigidbody = GetComponent<Rigidbody>();
    }

    /// <summary>
    /// Retrieves input and normal of plain under enemy and liniarly interpolates move_speed to wanted_move_speed dependent on acceleration.
    /// </summary>
    private void Update()
    {
        if (complete_stop || is_stopped)
        {
            return;
        }

        move_direction_normalized = (destination - transform.position).normalized;
        look_dir = new Vector3(move_direction_normalized.x, 0f, move_direction_normalized.z);
        Physics.Raycast(transform.position, Vector3.down, out slope_hit, height);
        slope_move_direction_normalized = Vector3.ProjectOnPlane(move_direction_normalized, slope_hit.normal).normalized;
        move_speed = Mathf.Lerp(move_speed, wanted_move_speed, acceleration * Time.deltaTime);
    }

    /// <summary>
    /// Moves controller in slope_move_direction_normalized direction dependent on move_speed.
    /// Applies gravity and checks for ground.
    /// Is in FixedUpdate because it is physics based.
    /// </summary>
    private void FixedUpdate()
    {
        if (complete_stop)
        {
            return;
        }

        if (is_grounded && !is_stopped)
        {
            enemy_rigidbody.MovePosition(transform.position + slope_move_direction_normalized * move_speed * Time.deltaTime);

            Vector3 new_dir = Vector3.RotateTowards(transform.forward, look_dir, angular_speed * Time.deltaTime, 0f);
            transform.rotation = Quaternion.LookRotation(new_dir);
        }
        enemy_rigidbody.AddForce(Vector3.down * gravity * Time.deltaTime, ForceMode.Acceleration);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (complete_stop)
        {
            return;
        }
        if (IsGround(other.gameObject.layer) && !is_dead)
        {
            trigger_count++;
            if (trigger_count == 1)
            {
                if (!is_dead)
                {
                    enemy_rigidbody.constraints = rigidbody_constraints_grounded;
                    transform.rotation = Quaternion.LookRotation(look_dir);
                }
                enemy_rigidbody.drag = 10f;
                is_grounded = true;
            }
        }
        return;
    }

    private void OnTriggerExit(Collider other)
    {
        if (complete_stop)
        {
            return;
        }
        if (IsGround(other.gameObject.layer))
        {
            trigger_count--;
            if (trigger_count == 0)
            {
                if (!is_dead)
                {
                    enemy_rigidbody.constraints = rigidbody_constraints_airborn;
                }
                enemy_rigidbody.drag = 1f;
                is_grounded = false;
            }
        }
        return;
    }

    /// <summary>
    /// Returns true if given layer is in layer_mask.
    /// </summary>
    private bool IsGround(int layer)
    {
        return (MousePoint.layer_mask_world_colliders_2.value & 1 << layer) != 0;
    }
}
