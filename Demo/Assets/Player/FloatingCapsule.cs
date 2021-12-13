using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingCapsule : MonoBehaviour
{
    public Rigidbody _rigidbody;

    [Header("Ground")]
    public bool grounded = false;
    [SerializeField] private Vector3 ground_raycast_offset;
    [SerializeField] private float ground_distance;
    private RaycastHit ground_raycast_hit;
    [Range(0f, 90f)] [SerializeField] private float max_slope_angle = 90f;
    private float max_slope_angle_cos;

    [Header("Hover")]
    [SerializeField] private float hover_height;
    [SerializeField] private float ride_spring_strenght;
    [SerializeField] private float ride_spring_damper;

    [Header("Upright")]
    [SerializeField] private float upright_joint_spring_strength;
    [SerializeField] private float upright_joint_spring_damper;
    [SerializeField] private float xy_rotation_scale;

    [Header("Movement")]
    public Vector3 desired_heading = Vector3.forward;
    public float desired_speed;

    [SerializeField] private float speed_factor;
    [SerializeField] private float acceleration;
    [SerializeField] private AnimationCurve acceleration_factor_from_dot;
    [SerializeField] private float max_acceleration_force;
    [SerializeField] private AnimationCurve max_acceleration_force_factor_from_dot;
    [SerializeField] private Vector3 force_scale;
    [SerializeField] private float gravity_scale_drop;

    [SerializeField] private float max_velocity = Mathf.Infinity;
    [SerializeField] private float max_angular_velocity = Mathf.Infinity;

    [Header("Rigidbody")]
    [SerializeField] private float rigidbody_max_angular_velocity = 10f;

    public virtual void Awake()
    {
        max_slope_angle_cos = Mathf.Cos(max_slope_angle * Mathf.Deg2Rad);
        _rigidbody.maxAngularVelocity = rigidbody_max_angular_velocity;
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

    public virtual void FixedUpdate()
    {
        if (Tumbling())
        {
            return;
        }
        Upright();
        Vector3 ground_velocity = Hover();
        if (GameTime.is_paused)
        {
            desired_speed = 0f;
        }
        Move(ground_velocity);
    }

    public virtual bool Tumbling()
    {
        bool hit = Physics.Raycast(transform.position + ground_raycast_offset, Vector3.down, out ground_raycast_hit, Layer.Mask.ground_enemy);

        if (!hit)
        {
            return true;
        }

        float ground_dot = Vector3.Dot(ground_raycast_hit.normal, transform.up);
        grounded = ground_raycast_hit.distance < ground_distance && ground_dot >= max_slope_angle_cos;

        if (_rigidbody.velocity.magnitude > max_velocity || _rigidbody.angularVelocity.magnitude > max_angular_velocity || !grounded)
        {
            return true;
        }
        return false;
    }

    public virtual void Upright()
    {
        // upright
        Quaternion character_current = transform.rotation;
        Quaternion to_goal = ShortestRotation(Quaternion.LookRotation(desired_heading, Vector3.up), character_current);

        Vector3 rotation_axis;
        float rotation_degrees;

        to_goal.ToAngleAxis(out rotation_degrees, out rotation_axis);
        float rotation_radians = rotation_degrees * Mathf.Deg2Rad;

        _rigidbody.AddTorque((Vector3.Scale(rotation_axis, Vector3.one + transform.up * xy_rotation_scale) * (rotation_radians * upright_joint_spring_strength)) - (_rigidbody.angularVelocity * upright_joint_spring_damper));
    }

    public virtual Vector3 Hover()
    {
        // hover
        Vector3 velocity = _rigidbody.velocity; // unit_velocity
        Vector3 ray_direction = Vector3.down;

        Vector3 other_velocity = Vector3.zero;
        Rigidbody hit_rigid_body = ground_raycast_hit.rigidbody;
        if (hit_rigid_body != null)
        {
            other_velocity = hit_rigid_body.velocity;
        }

        float direction_velocity = Vector3.Dot(ray_direction, velocity);
        float other_direction_velocity = Vector3.Dot(ray_direction, other_velocity);

        float relative_velocity = direction_velocity - other_direction_velocity;
        float x = ground_raycast_hit.distance - hover_height;
        float spring_force = (x * ride_spring_strenght) - (relative_velocity * ride_spring_damper);

        Vector3 force = ray_direction * spring_force;
        _rigidbody.AddForce(force);
        Vector3 ground_velocity = Vector3.zero;
        if (hit_rigid_body != null)
        {
            hit_rigid_body.AddForceAtPosition(-force, ground_raycast_hit.point);
            ground_velocity = hit_rigid_body.velocity;
        }

        return ground_velocity;
    }

    public virtual void Move(Vector3 ground_velocity)
    {
        // move
        Vector3 velocity = _rigidbody.velocity;
        velocity.y = 0f;
        float velocity_dot = Vector3.Dot(desired_heading, velocity.normalized);
        float accel = acceleration * acceleration_factor_from_dot.Evaluate(velocity_dot);

        Vector3 goal_velocity = desired_heading * desired_speed * speed_factor;
        velocity = Vector3.MoveTowards(velocity, goal_velocity + ground_velocity, accel * Time.fixedDeltaTime);

        Vector3 needed_acceleration = (velocity - _rigidbody.velocity) / Time.fixedDeltaTime;

        float max_acceleration = max_acceleration_force * max_acceleration_force_factor_from_dot.Evaluate(velocity_dot) * max_acceleration_force;
        needed_acceleration = Vector3.ClampMagnitude(needed_acceleration, max_acceleration);

        _rigidbody.AddForce(Vector3.Scale(needed_acceleration * _rigidbody.mass, force_scale));
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position + ground_raycast_offset, Vector3.down * 100f);
    }
#endif
}
