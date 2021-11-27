using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// How the enemy should interact with the worlds physics.
/// </summary>
public class Agent : FloatingCapsule
{
    public bool tumbling = false;

    [Header("Movement")]
    public float walk_speed;
    public float sprint_speed;

    [Header("Rigid Body")]
    [SerializeField] private float explosion_up_modifier;

    public void AddExplosionForce(float force, Vector3 explosion_pos, float explosion_radius, float explosion_up_modifier, ForceMode force_mode)
    {
        _rigidbody.AddExplosionForce(force, explosion_pos, explosion_radius, this.explosion_up_modifier * explosion_up_modifier, force_mode);
    }

    public void AddForce(Vector3 direction_and_magnitude, ForceMode force_mode)
    {
        _rigidbody.AddForce(750f * transform.up, force_mode);
    }

    /// <summary>
    /// Moves controller in slope_move_direction_normalized direction dependent on move_speed.
    /// Applies gravity and checks for ground.
    /// Is in FixedUpdate because it is physics based.
    /// </summary>
    public override void FixedUpdate()
    {
        if (Tumbling())
        {
            return;
        }
        Upright();
        Vector3 ground_velocity = Hover();
        Move(ground_velocity);
    }
}
