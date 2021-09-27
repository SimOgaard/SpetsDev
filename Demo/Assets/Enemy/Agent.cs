using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// How the enemy should interact with the worlds physics.
/// </summary>
public class Agent : MonoBehaviour
{
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

    public bool is_grounded = true;
    private RaycastHit slope_hit;

    [SerializeField] private float ground_raycast_length;
    public Rigidbody enemy_rigidbody;
    private float gravity = 2500f;

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
        Physics.Raycast(transform.position, Vector3.down, out slope_hit, ground_raycast_length);
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
}
