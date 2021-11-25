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
    private Transform camera_focus_transform;
    [SerializeField] private float crouched_speed;
    [SerializeField] private float walk_speed;
    [SerializeField] private float sprint_speed;

    [Header("Sound")]
    [SerializeField] private float base_sound = 1f;
    [SerializeField] private float sound_amplifier = 1f;
    [SerializeField] private float max_sound = 1f;

    [Header("Vision")]
    [SerializeField] private float base_vision = 1f;
    [SerializeField] private float vision_amplifier = 1f;
    [SerializeField] private float max_vision = 1f;
    [SerializeField] private float min_vision = 0.001f;

    public override void Awake()
    {
        base.Awake();
        camera_focus_transform = GameObject.Find("camera_focus_point").transform;
    }

    /// <summary>
    /// Retrieves input and normal of plain under player and liniarly interpolates move_speed between sprinting and walking speed dependent on acceleration.
    /// </summary>
    private void Update()
    {
        Vector3 new_desired_heading = (camera_focus_transform.right * PlayerInput.horizontal + camera_focus_transform.forward * PlayerInput.vertical).normalized;
        if (new_desired_heading != Vector3.zero)
        {
            desired_heading = new_desired_heading;
        }
        else
        {
            desired_speed = 0f;
            return;
        }

        if (Input.GetKey(PlayerInput.crouch_key))
        {
            desired_speed = crouched_speed;
        }
        else if (Input.GetKey(PlayerInput.sprint_key))
        {
            desired_speed = sprint_speed;
        }
        else
        {
            desired_speed = walk_speed;
        }
    }

    private void MakeSound()
    {
        if (desired_speed > (crouched_speed + 0.1f))
        {
            float sound = Mathf.Max(_rigidbody.velocity.magnitude * sound_amplifier * base_sound, max_sound);
            Enemies.Sound(transform, sound, Time.fixedDeltaTime);
        }
    }

    /// <summary>
    /// Moves controller in slope_move_direction_normalized direction dependent on move_speed.
    /// Applies gravity and checks for ground.
    /// Is in FixedUpdate because it is physics based.
    /// </summary>
    public override void FixedUpdate()
    {
        base.FixedUpdate();
        MakeSound();
        CheckEnemyVision();
    }

    private void CheckEnemyVision()
    {
        float min = min_vision;
        float max = max_vision;
        float vision = Mathf.Min(Mathf.Max(_rigidbody.velocity.magnitude * vision_amplifier * base_vision, min), max);
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
