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
    private Vector3 move_direction_normalized;
    private RaycastHit slope_hit;
    private PlayerInput player_input;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        player_input = GetComponent<PlayerInput>();
        controller.material.dynamicFriction = 0f;
        controller.material.staticFriction = 0f;
    }

    /// <summary>
    /// Retrieves input and normal of plain under player and liniarly interpolates move_speed between sprinting and walking speed dependent on acceleration.
    /// </summary>
    private void Update()
    {
        move_direction_normalized = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical")).normalized;
        Physics.Raycast(transform.position, Vector3.down, out slope_hit, player_height / 2f);
        slope_move_direction_normalized = Vector3.ProjectOnPlane(move_direction_normalized, slope_hit.normal).normalized;
        move_speed = Mathf.Lerp(move_speed, Input.GetKey(player_input.sprint_key) ? sprint_speed : walk_speed, acceleration * Time.deltaTime);
    }

    /// <summary>
    /// Moves controller in slope_move_direction_normalized direction dependent on move_speed.
    /// Applies gravity and checks for ground.
    /// Is in FixedUpdate because it is physics based.
    /// </summary>
    private void FixedUpdate()
    {
        is_grounded = (controller.Move(slope_move_direction_normalized * move_speed * movement_multiplier * (is_grounded ? 1f : air_multiplier) * Time.deltaTime) & CollisionFlags.Below) != 0;
        if (!is_grounded)
        {
            controller.Move(Vector3.down * gravity * Time.deltaTime);
        }
    }
}
