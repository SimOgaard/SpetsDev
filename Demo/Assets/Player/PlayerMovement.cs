using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controlls player movement.
/// Movement should be ordinary 3D movement, does not need to be translated to 2D isometric movement.
/// </summary>
public class PlayerMovement : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private float player_height;
    [HideInInspector] public CharacterController controller;

    [Header("Movement")]
    [SerializeField] private float crouched_speed = 2f;
    [SerializeField] private float walk_speed = 4f;
    [SerializeField] private float sprint_speed = 6f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float air_multiplier = 0.4f;
    [SerializeField] private float movement_multiplier = 10f;
    [SerializeField] private float gravity = 200f;
    public float move_speed = 4f; // Public float only for debugging, set private.

    [Header("Ground")]
    public bool is_grounded;

    private Vector3 slope_move_direction_normalized;
    public static Vector3 move_direction_normalized;
    private RaycastHit slope_hit;
    private Transform camera_focus_transform;
    public static Vector3 movement;

    [Header("Sound")]
    [SerializeField] private float base_sound = 1f;
    [SerializeField] private float sound_amplifier = 1f;
    [SerializeField] private float max_sound = 1f;

    [Header("Vision")]
    [SerializeField] private float base_vision = 1f;
    [SerializeField] private float vision_amplifier = 1f;
    [SerializeField] private float max_vision = 1f;
    [SerializeField] private float min_vision = 0.001f;

    private void Start()
    {
        camera_focus_transform = GameObject.Find("camera_focus_point").transform;
        controller = GetComponent<CharacterController>();
        controller.material.dynamicFriction = 0f;
        controller.material.staticFriction = 0f;
    }

    /// <summary>
    /// Retrieves input and normal of plain under player and liniarly interpolates move_speed between sprinting and walking speed dependent on acceleration.
    /// </summary>
    private void Update()
    {
        move_direction_normalized = (camera_focus_transform.right * PlayerInput.horizontal + camera_focus_transform.forward * PlayerInput.vertical).normalized;
        Physics.Raycast(transform.position, Vector3.down, out slope_hit, player_height / 2f);
        slope_move_direction_normalized = Vector3.ProjectOnPlane(move_direction_normalized, slope_hit.normal).normalized;
        move_speed = Mathf.Lerp(move_speed, Input.GetKey(PlayerInput.sprint_key) ? sprint_speed : Input.GetKey(PlayerInput.crouch_key) ? crouched_speed : walk_speed, acceleration * Time.deltaTime);
        movement = slope_move_direction_normalized * move_speed * movement_multiplier;

        MakeSound();
    }

    private void MakeSound()
    {
        if (move_speed > (crouched_speed + 0.1f))
        {
            float sound = movement.magnitude * sound_amplifier * base_sound * Time.deltaTime;
            Enemies.Sound(transform, sound, max_sound * Time.deltaTime, Time.deltaTime);
        }
    }

    /// <summary>
    /// Moves controller in slope_move_direction_normalized direction dependent on move_speed.
    /// Applies gravity and checks for ground.
    /// Is in FixedUpdate because it is physics based.
    /// </summary>
    private void FixedUpdate()
    {
        is_grounded = (controller.Move(movement * (is_grounded ? 1f : air_multiplier) * Time.fixedDeltaTime) & CollisionFlags.Below) != 0;
        controller.Move(Vector3.down * gravity * Time.fixedDeltaTime);
        CheckEnemyVision();
    }

    private void CheckEnemyVision()
    {
        float min = min_vision * Time.fixedDeltaTime;
        float vision = Mathf.Max(movement.magnitude * vision_amplifier * base_vision * Time.fixedDeltaTime, min);
        Enemies.Vision(transform, vision, max_vision * Time.fixedDeltaTime, min, Time.fixedDeltaTime);
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (Layer.IsInLayer(Layer.enemy, hit.gameObject.layer))
        {
            hit.gameObject.GetComponent<EnemyAI>().AttendToSound(transform, Mathf.Infinity);
        }
    }
}
