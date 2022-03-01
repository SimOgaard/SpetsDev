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
    public float walkSpeed;
    public float sprintSpeed;

    [Header("Rigid Body")]
    [SerializeField] private float explosionUpModifier;

    public void AddExplosionForce(float force, Vector3 explosionPos, float explosionRadius, float explosionUpModifier, ForceMode forceMode)
    {
        _rigidbody.AddExplosionForce(force, explosionPos, explosionRadius, this.explosionUpModifier * explosionUpModifier, forceMode);
    }

    public void AddForce(Vector3 directionAndMagnitude, ForceMode forceMode)
    {
        _rigidbody.AddForce(750f * transform.up, forceMode);
    }

    /// <summary>
    /// Moves controller in slopeMoveDirectionNormalized direction dependent on moveSpeed.
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
        Vector3 groundVelocity = Hover();
        Move(groundVelocity);
    }
}
