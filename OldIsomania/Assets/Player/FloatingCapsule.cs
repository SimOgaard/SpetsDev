using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingCapsule : MonoBehaviour
{
    public Rigidbody _rigidbody;

    [Header("Ground")]
    public bool grounded = false;
    [SerializeField] private Vector3 groundRaycastOffset;
    [SerializeField] private float groundDistance;
    private RaycastHit groundRaycastHit;
    [Range(0f, 90f)] [SerializeField] private float maxSlopeAngle = 90f;
    private float maxSlopeAngleCos;

    [Header("Hover")]
    [SerializeField] private float hoverHeight;
    [SerializeField] private float rideSpringStrenght;
    [SerializeField] private float rideSpringDamper;

    [Header("Upright")]
    [SerializeField] private float uprightJointSpringStrength;
    [SerializeField] private float uprightJointSpringDamper;
    [SerializeField] private float xyRotationScale;

    [Header("Movement")]
    public Vector3 desiredHeading = Vector3.forward;
    public float desiredSpeed;

    [SerializeField] private float speedFactor;
    [SerializeField] private float acceleration;
    [SerializeField] private AnimationCurve accelerationFactorFromDot;
    [SerializeField] private float maxAccelerationForce;
    [SerializeField] private AnimationCurve maxAccelerationForceFactorFromDot;
    [SerializeField] private Vector3 forceScale;
    [SerializeField] private float gravityScaleDrop;

    [SerializeField] private float maxVelocity = Mathf.Infinity;
    [SerializeField] private float maxAngularVelocity = Mathf.Infinity;

    [Header("Rigidbody")]
    [SerializeField] private float rigidbodyMaxAngularVelocity = 10f;

    public virtual void Awake()
    {
        maxSlopeAngleCos = Mathf.Cos(maxSlopeAngle * Mathf.Deg2Rad);
        _rigidbody.maxAngularVelocity = rigidbodyMaxAngularVelocity;
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
        Vector3 groundVelocity = Hover();
        if (GameTime.isPaused)
        {
            desiredSpeed = 0f;
        }
        Move(groundVelocity);
    }

    public virtual bool Tumbling()
    {
        bool hit = Physics.Raycast(transform.position + groundRaycastOffset, Vector3.down, out groundRaycastHit, Layer.Mask.groundEnemy);

        if (!hit)
        {
            return true;
        }

        float groundDot = Vector3.Dot(groundRaycastHit.normal, transform.up);
        grounded = groundRaycastHit.distance < groundDistance && groundDot >= maxSlopeAngleCos;

        if (_rigidbody.velocity.magnitude > maxVelocity || _rigidbody.angularVelocity.magnitude > maxAngularVelocity || !grounded)
        {
            return true;
        }
        return false;
    }

    public virtual void Upright()
    {
        // upright
        Quaternion characterCurrent = transform.rotation;
        Quaternion toGoal = ShortestRotation(Quaternion.LookRotation(desiredHeading, Vector3.up), characterCurrent);

        Vector3 rotationAxis;
        float rotationDegrees;

        toGoal.ToAngleAxis(out rotationDegrees, out rotationAxis);
        float rotationRadians = rotationDegrees * Mathf.Deg2Rad;

        _rigidbody.AddTorque((Vector3.Scale(rotationAxis, Vector3.one + transform.up * xyRotationScale) * (rotationRadians * uprightJointSpringStrength)) - (_rigidbody.angularVelocity * uprightJointSpringDamper));
    }

    public virtual Vector3 Hover()
    {
        // hover
        Vector3 velocity = _rigidbody.velocity; // unitVelocity
        Vector3 rayDirection = Vector3.down;

        Vector3 otherVelocity = Vector3.zero;
        Rigidbody hitRigidBody = groundRaycastHit.rigidbody;
        if (hitRigidBody != null)
        {
            otherVelocity = hitRigidBody.velocity;
        }

        float directionVelocity = Vector3.Dot(rayDirection, velocity);
        float otherDirectionVelocity = Vector3.Dot(rayDirection, otherVelocity);

        float relativeVelocity = directionVelocity - otherDirectionVelocity;
        float x = groundRaycastHit.distance - hoverHeight;
        float springForce = (x * rideSpringStrenght) - (relativeVelocity * rideSpringDamper);

        Vector3 force = rayDirection * springForce;
        _rigidbody.AddForce(force);
        Vector3 groundVelocity = Vector3.zero;
        if (hitRigidBody != null)
        {
            hitRigidBody.AddForceAtPosition(-force, groundRaycastHit.point);
            groundVelocity = hitRigidBody.velocity;
        }

        return groundVelocity;
    }

    public virtual void Move(Vector3 groundVelocity)
    {
        // move
        Vector3 velocity = _rigidbody.velocity;
        velocity.y = 0f;
        float velocityDot = Vector3.Dot(desiredHeading, velocity.normalized);
        float accel = acceleration * accelerationFactorFromDot.Evaluate(velocityDot);

        Vector3 goalVelocity = desiredHeading * desiredSpeed * speedFactor;
        velocity = Vector3.MoveTowards(velocity, goalVelocity + groundVelocity, accel * Time.fixedDeltaTime);

        Vector3 neededAcceleration = (velocity - _rigidbody.velocity) / Time.fixedDeltaTime;

        float maxAcceleration = maxAccelerationForce * maxAccelerationForceFactorFromDot.Evaluate(velocityDot) * maxAccelerationForce;
        neededAcceleration = Vector3.ClampMagnitude(neededAcceleration, maxAcceleration);

        _rigidbody.AddForce(Vector3.Scale(neededAcceleration * _rigidbody.mass, forceScale));
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position + groundRaycastOffset, Vector3.down * 100f);
    }
#endif
}
