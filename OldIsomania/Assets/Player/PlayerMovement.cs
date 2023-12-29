using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controlls player movement.
/// Movement should be ordinary 3D movement, does not need to be translated to 2D isometric movement.
/// </summary>
public class PlayerMovement : FloatingCapsule
{
    [Header("Movement")]
    [SerializeField] private float crouchedSpeed;
    [SerializeField] private float walkSpeed;
    [SerializeField] private float sprintSpeed;

    [Header("Sound")]
    [SerializeField] private float baseSound = 1f;
    [SerializeField] private float soundAmplifier = 1f;
    [SerializeField] private float maxSound = 1f;

    [Header("Vision")]
    [SerializeField] private float baseVision = 1f;
    [SerializeField] private float visionAmplifier = 1f;
    [SerializeField] private float maxVision = 1f;
    [SerializeField] private float minVision = 0.001f;

    public override void Awake()
    {
        base.Awake();
    }

    /// <summary>
    /// Retrieves input and normal of plain under player and liniarly interpolates moveSpeed between sprinting and walking speed dependent on acceleration.
    /// </summary>
    private void Update()
    {
        Vector3 newDesiredHeading = (Global.cameraFocusPointTransform.right * PlayerInput.horizontal + Global.cameraFocusPointTransform.forward * PlayerInput.vertical).normalized;
        if (newDesiredHeading != Vector3.zero)
        {
            desiredHeading = newDesiredHeading;
        }
        else
        {
            desiredSpeed = 0f;
            return;
        }

        if (Input.GetKey(PlayerInput.crouchKey))
        {
            desiredSpeed = crouchedSpeed;
        }
        else if (Input.GetKey(PlayerInput.sprintKey))
        {
            desiredSpeed = sprintSpeed;
        }
        else
        {
            desiredSpeed = walkSpeed;
        }
    }

    private void MakeSound()
    {
        if (desiredSpeed > (crouchedSpeed + 0.1f))
        {
            float sound = Mathf.Max(_rigidbody.velocity.magnitude * soundAmplifier * baseSound, maxSound);
            Enemies.Sound(transform, sound, Time.fixedDeltaTime);
        }
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        MakeSound();
        CheckEnemyVision();
    }

    private void CheckEnemyVision()
    {
        float min = minVision;
        float max = maxVision;
        float vision = Mathf.Min(Mathf.Max(_rigidbody.velocity.magnitude * visionAmplifier * baseVision, min), max);
        Enemies.Vision(transform, vision, Time.fixedDeltaTime);
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (Layer.IsInLayer(Layer.enemy, hit.gameObject.layer))
        {
            hit.transform.parent.GetComponent<EnemyAI>().AttendToSound(transform, Mathf.Infinity);
        }
    }
}
