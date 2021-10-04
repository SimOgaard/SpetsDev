using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// How the enemy should interact with the worlds physics.
/// </summary>
public class Agent : MonoBehaviour
{
    public bool tumbling = false;

    public Vector3 destination;

    private Vector3 move_direction_normalized;
    private Vector3 slope_move_direction_normalized;
    private Vector3 look_direction_normalized;

    public bool is_grounded = true;
    private RaycastHit slope_hit;

    [SerializeField] private float ground_raycast_length;
    public Rigidbody enemy_rigidbody;

    [Header("Movement")]
    [SerializeField] private float max_move_speed = 5f;
    [SerializeField] private float acceleration = 2.5f;
    [SerializeField] private float current_move_speed = 0f;
    private float wanted_move_speed;

    [Header("Rotation")]
    [SerializeField] private VectorPid angularVelocityController = new VectorPid(25f, 0.5f, 30f);
    [SerializeField] private VectorPid headingController = new VectorPid(25f, 0.5f, 30f);
    [SerializeField] private float max_slope_rotation = 20f;
    private float max_slope_rotation_cos;

    [Header("Rigid Body")]
    [SerializeField] private float angular_drag;
    [SerializeField] private float max_angular_velocity;
    [SerializeField] private float drag;
    [SerializeField] private float max_depenetration_velocity;
    [SerializeField] private float mass;

    [Header("Tumble")]
    [SerializeField] private float tumble_time_modifier;
    [SerializeField] private float slope_tumble_time;
    [SerializeField] private float un_grounded_tumble_time;

    private IEnumerator tumble_coroutine;

    public void AddRigidBody(Rigidbody rigid_body)
    {
        enemy_rigidbody = rigid_body;
        enemy_rigidbody.angularDrag = angular_drag;
        enemy_rigidbody.drag = drag;
        enemy_rigidbody.mass = mass;
        enemy_rigidbody.maxAngularVelocity = max_angular_velocity;
        enemy_rigidbody.maxDepenetrationVelocity = max_depenetration_velocity;
        enemy_rigidbody.isKinematic = false;
        enemy_rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        enemy_rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        enemy_rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        max_slope_rotation_cos = Mathf.Cos(max_slope_rotation * Mathf.Deg2Rad);

        wanted_move_speed = max_move_speed;

        tumble_coroutine = Tumble(0f);
    }

    public void StopMoving()
    {
        wanted_move_speed = 0f;
    }

    public void StartMoving()
    {
        wanted_move_speed = max_move_speed;
    }

    public void StopRotating()
    {
    }

    public void StartRotating()
    {
    }

    public void AddExplosionForce(float force, Vector3 explosion_pos, float explosion_radius, float explosion_up_modifier, ForceMode force_mode, float tumble_time)
    {
        StartTumble(tumble_time);
        enemy_rigidbody.AddExplosionForce(force, explosion_pos, explosion_radius, explosion_up_modifier, force_mode);
    }

    public void AddForce(Vector3 direction_and_magnitude, ForceMode force_mode, float tumble_time)
    {
        StartTumble(tumble_time);
        enemy_rigidbody.AddForce(750f * transform.up, ForceMode.Impulse);
    }

    public void StartTumble(float tumble_time)
    {
        StopCoroutine(tumble_coroutine);
        tumble_coroutine = Tumble(tumble_time);
        StartCoroutine(tumble_coroutine);
    }

    /// <summary>
    /// 
    /// </summary>
    public IEnumerator Tumble(float tumble_time)
    {
        if (tumble_time_modifier == 0f)
        {
            yield break;
        }

        enemy_rigidbody.constraints = RigidbodyConstraints.None;
        tumbling = true;

        yield return new WaitForSeconds(tumble_time * tumble_time_modifier);

        enemy_rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        tumbling = false;
        transform.rotation = Quaternion.identity;
    }

    /// <summary>
    /// Moves controller in slope_move_direction_normalized direction dependent on move_speed.
    /// Applies gravity and checks for ground.
    /// Is in FixedUpdate because it is physics based.
    /// </summary>
    private void FixedUpdate()
    {
        if (tumbling)
        {
            return;
        }
        else
        {
            is_grounded = Physics.Raycast(transform.position + Vector3.up * ground_raycast_length * 0.5f, Vector3.down, out slope_hit, ground_raycast_length, Layer.Mask.ground_enemy);

            if (Vector3.Dot(slope_hit.normal, Vector3.up) < max_slope_rotation_cos)
            {
                StartTumble(slope_tumble_time);
                return;
            }

            if (is_grounded)
            {
                Vector3 angularVelocityError = -enemy_rigidbody.angularVelocity;
                Debug.DrawRay(transform.position, enemy_rigidbody.angularVelocity * 10, Color.black);

                Vector3 angularVelocityCorrection = angularVelocityController.Update(angularVelocityError, Time.fixedDeltaTime);
                Debug.DrawRay(transform.position, angularVelocityCorrection, Color.green);

                enemy_rigidbody.AddTorque(angularVelocityCorrection);

                Vector3 desiredHeading = destination - transform.position;
                Debug.DrawRay(transform.position, desiredHeading, Color.magenta);

                Vector3 currentHeading = transform.forward;
                Debug.DrawRay(transform.position, currentHeading * 15, Color.blue);

                Vector3 headingError = Vector3.Cross(currentHeading, desiredHeading);
                Vector3 headingCorrection = headingController.Update(headingError, Time.fixedDeltaTime);

                enemy_rigidbody.AddTorque(headingCorrection);


                Vector3 norm_desired_heading = (new Vector3(desiredHeading.x, 0f, desiredHeading.z)).normalized;
                float angular_difference = (Vector3.Dot(transform.forward, norm_desired_heading) + 1f) * 0.5f;

                slope_move_direction_normalized = Vector3.ProjectOnPlane(transform.forward, slope_hit.normal).normalized;
                current_move_speed = Mathf.Lerp(current_move_speed, wanted_move_speed * angular_difference, acceleration * Time.deltaTime);
                //enemy_rigidbody.velocity = transform.forward * current_move_speed;
                //enemy_rigidbody.MovePosition(slope_hit.point + slope_move_direction_normalized * current_move_speed * Time.deltaTime);
            }
            else
            {
                StartTumble(un_grounded_tumble_time);
                return;
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(transform.position + Vector3.up * ground_raycast_length * 0.5f, transform.position + Vector3.down * ground_raycast_length * 0.5f);
    }
}
