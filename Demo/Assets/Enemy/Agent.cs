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

    [Header("Ground")]
    [SerializeField] private float ground_raycast_length;
    [SerializeField] private float ground_raycast_hover;
    private RaycastHit ground_raycast_hit;
    public bool is_grounded = true;

    [Header("Movement")]
    [SerializeField] private float max_move_speed = 5f;
    [SerializeField] private float movement_acceleration = 2.5f;
    [SerializeField] private AnimationCurve acceleration_factor_from_dot;
    [SerializeField] private float max_acceleration_force = 2f;
    [SerializeField] private float speed_factor;
    [SerializeField] private float max_acceleration_force_factor;
    [SerializeField] private Vector3 force_scale;
    [SerializeField] private AnimationCurve max_acceleration_factor_from_dot;
    private float wanted_move_speed;
    private Vector3 m_goal_velocity;

    [Header("Spring")]
    [SerializeField] private float ride_spring_strenght;
    [SerializeField] private float ride_spring_damper;
    [SerializeField] private float hover_height;

    [Header("Upright Spring")]
    [SerializeField] private float upright_joint_spring_strength;
    [SerializeField] private float upright_joint_spring_damper;

    [Header("Rotation")]
    [SerializeField] private float max_slope_rotation = 20f;
    [SerializeField] private VectorPid angularVelocityController = new VectorPid(25f, 0.5f, 30f);
    [SerializeField] private VectorPid headingController = new VectorPid(25f, 0.5f, 30f);
    private float max_slope_rotation_cos;

    [Header("Rigid Body")]
    [SerializeField] private float angular_drag;
    [SerializeField] private float max_angular_velocity;
    [SerializeField] private float drag;
    [SerializeField] private float max_depenetration_velocity;
    [SerializeField] private float mass;
    [SerializeField] private float explosion_up_modifier_agent;
    public Rigidbody enemy_rigidbody;

    [Header("Tumble")]
    [SerializeField] private float tumble_velocity_magnitude;
    [SerializeField] private float tumble_angular_velocity_magnitude;
    private float ground_dot = 1f;

    private void OnEnable()
    {
        ground_dot = 1f;
        enemy_rigidbody.isKinematic = false;
        enemy_rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    private void OnDisable()
    {
        enemy_rigidbody.isKinematic = true;
        enemy_rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
    }

    public void AddRigidBody(Rigidbody rigid_body)
    {
        enemy_rigidbody = rigid_body;
        enemy_rigidbody.angularDrag = angular_drag;
        enemy_rigidbody.drag = drag;
        enemy_rigidbody.mass = mass;
        enemy_rigidbody.maxAngularVelocity = max_angular_velocity;
        enemy_rigidbody.maxDepenetrationVelocity = max_depenetration_velocity;
        enemy_rigidbody.isKinematic = true;
        enemy_rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        enemy_rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        enemy_rigidbody.useGravity = true;

        max_slope_rotation_cos = Mathf.Cos(max_slope_rotation * Mathf.Deg2Rad);

        wanted_move_speed = max_move_speed;
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

    public void AddExplosionForce(float force, Vector3 explosion_pos, float explosion_radius, float explosion_up_modifier, ForceMode force_mode)
    {
        enemy_rigidbody.AddExplosionForce(force, explosion_pos, explosion_radius, explosion_up_modifier * explosion_up_modifier_agent, force_mode);
    }

    public void AddForce(Vector3 direction_and_magnitude, ForceMode force_mode)
    {
        enemy_rigidbody.AddForce(750f * transform.up, ForceMode.Impulse);
    }

    public static Quaternion ShortestRotation(Quaternion a, Quaternion b)
    {
        if (Quaternion.Dot(a, b) < 0)
        {
            return a * Quaternion.Inverse(Multiply(b, -1));
        }
        else return a * Quaternion.Inverse(b);
    }

    public static Quaternion Multiply(Quaternion input, float scalar)
    {
        return new Quaternion(input.x * scalar, input.y * scalar, input.z * scalar, input.w * scalar);
    }

    /// <summary>
    /// Moves controller in slope_move_direction_normalized direction dependent on move_speed.
    /// Applies gravity and checks for ground.
    /// Is in FixedUpdate because it is physics based.
    /// </summary>
    private void FixedUpdate()
    {
        is_grounded = Physics.Raycast(transform.position + Vector3.up * ground_raycast_hover, Vector3.down, out ground_raycast_hit, ground_raycast_length, Layer.Mask.ground_enemy);
        ground_dot = Vector3.Dot(ground_raycast_hit.normal, Vector3.up);

        if (enemy_rigidbody.velocity.magnitude > tumble_velocity_magnitude || enemy_rigidbody.angularVelocity.magnitude > tumble_angular_velocity_magnitude || ground_dot < max_slope_rotation_cos || !is_grounded)
        {
            tumbling = true;
            return;
        }
        else
        {
            tumbling = false;
            // hover
            Vector3 velocity = enemy_rigidbody.velocity;
            Vector3 ray_direction = Vector3.down;

            Vector3 other_velocity = Vector3.zero;
            Rigidbody hit_rigid_body = ground_raycast_hit.rigidbody;
            if (hit_rigid_body != null)
            {
                other_velocity = hit_rigid_body.velocity;
            }

            float ray_direction_velocity = Vector3.Dot(ray_direction, velocity);
            float other_ray_direction_velocity = Vector3.Dot(ray_direction, other_velocity);

            float relative_velocity = ray_direction_velocity - other_ray_direction_velocity;
            float x = ground_raycast_hit.distance - hover_height;
            float spring_force = (x * ride_spring_strenght) - (relative_velocity * ride_spring_damper);

            //Debug.Log(ray_direction);

            enemy_rigidbody.AddForce(ray_direction * spring_force);
            Vector3 ground_velocity = Vector3.zero;
            if (hit_rigid_body != null)
            {
                hit_rigid_body.AddForceAtPosition(ray_direction * -spring_force, ground_raycast_hit.point);
                ground_velocity = hit_rigid_body.velocity;
            }

            // upright
            Vector3 desiredHeading = destination - transform.position;
            Vector3 norm_desired_heading = (new Vector3(desiredHeading.x, 0f, desiredHeading.z)).normalized;

            Quaternion character_current = transform.rotation;
            Quaternion to_goal = ShortestRotation(Quaternion.LookRotation(norm_desired_heading, Vector3.up), character_current);

            Vector3 rotation_axis;
            float rotation_degrees;
            to_goal.ToAngleAxis(out rotation_degrees, out rotation_axis);
            //Debug.Log(rotation_degrees);
            float rotation_radians = rotation_degrees * Mathf.Deg2Rad;
            enemy_rigidbody.AddTorque((rotation_axis * (rotation_radians * upright_joint_spring_strength)) - (enemy_rigidbody.angularVelocity * upright_joint_spring_damper));

            // move
            float velocity_dot = Vector3.Dot(norm_desired_heading, transform.forward);
            float acceleration = movement_acceleration * acceleration_factor_from_dot.Evaluate(velocity_dot);

            Vector3 goal_velocity = norm_desired_heading * max_move_speed * speed_factor;
            m_goal_velocity = Vector3.MoveTowards(m_goal_velocity, goal_velocity + ground_velocity, acceleration * Time.fixedDeltaTime);

            Vector3 needed_acceleration = (m_goal_velocity - enemy_rigidbody.velocity) / Time.fixedDeltaTime;

            float max_acceleration = max_acceleration_force * max_acceleration_factor_from_dot.Evaluate(velocity_dot) * max_acceleration_force_factor;
            needed_acceleration = Vector3.ClampMagnitude(needed_acceleration, max_acceleration);

            enemy_rigidbody.AddForce(Vector3.Scale(needed_acceleration * enemy_rigidbody.mass, force_scale));

            /*
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
            */
            //enemy_rigidbody.velocity = transform.forward * current_move_speed;
            //enemy_rigidbody.MovePosition(slope_hit.point + slope_move_direction_normalized * current_move_speed * Time.deltaTime);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(transform.position + Vector3.up * ground_raycast_hover, transform.position + Vector3.down * ground_raycast_length + Vector3.up * ground_raycast_hover);
    }
#endif
}
