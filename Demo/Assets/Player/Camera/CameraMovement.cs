using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Moves point that camera is focused on.
/// </summary>
public class CameraMovement : MonoBehaviour
{
    private MousePoint mouse_point;
    private PlayerMovement player_movement;
    private Transform player_transform;
    private Vector3 smoothed_position;

    [Header("Focus Point Position")]
    [SerializeField] private float max_distance;
    [SerializeField] private float smooth_speed;
    [SerializeField] [Range(0f, 1f)] private float player_position_diff_amplitude;
    [SerializeField] private float player_heading_xz_amplitude;
    [SerializeField] private float player_heading_y_amplitude;
    [SerializeField] [Range(0f, 0.2f)] private float player_looking_plane_amplitude;
    [SerializeField] [Range(0f, 0.2f)] private float player_looking_3d_amplitude;
    [SerializeField] private float player_y_offset;

    [Header("Focus Point Rotation")]
    [SerializeField] private float input_rotation_speed;
    [SerializeField] private float lerp_rotation_speed;
    [SerializeField] private float snap_increment;
    private float lerp_value_1 = 0f;
    private float lerp_value_2 = 0f;
    private float _lerp_time = 0f;
    private float lerp_time { get { return _lerp_time; } set { _lerp_time = Mathf.Clamp01(value); } }
    private int rotation_input;
    private float new_rotation;

    /// <summary>
    /// Calculates difference between actual player position and camera position, where player is moving twords and where mouse is pointing on a 2d plane and in 3d world and finally returns sum of all positions multiplied by their amplification.
    /// </summary>
    private Vector3 GetLookPoint()
    {
        Vector3 player_position_diff = (transform.position - player_transform.position + new Vector3(0f, player_y_offset, 0f)) * player_position_diff_amplitude;
        Vector3 player_heading = new Vector3(player_movement.controller.velocity.x * player_heading_xz_amplitude, player_movement.controller.velocity.y * player_heading_y_amplitude, player_movement.controller.velocity.z * player_heading_xz_amplitude);
        Vector3 player_looking_plane = (mouse_point.MousePosition2D() - player_transform.position) * player_looking_plane_amplitude;
        Vector3 player_looking_3d = (mouse_point.GetWorldPoint() - player_transform.position) * player_looking_3d_amplitude;

        return Vector3.ClampMagnitude(player_position_diff + player_heading + player_looking_plane + player_looking_3d, max_distance);
    }

    /// <summary>
    /// Liniarly interpolates camera focus point with player position.
    /// </summary>
    private Vector3 SmoothMovementToPoint(Vector3 focus_point)
    {
        Vector3 smoothed_position = Vector3.Lerp(transform.position, (player_transform.position + focus_point), smooth_speed * Time.fixedDeltaTime);
        return smoothed_position;
    }

    private void ClosestRotationSnap()
    {
        lerp_time += Time.fixedDeltaTime * lerp_rotation_speed;
        float t = lerp_time * lerp_time * (3f - 2f * lerp_time);
        new_rotation = Mathf.Lerp(lerp_value_1, lerp_value_2, t);

        transform.rotation = Quaternion.Euler(0f, new_rotation, 0f);
    }

    private void RotateWithInputFloat()
    {
        float input_value = rotation_input * input_rotation_speed * Time.fixedDeltaTime;
        float current_rotation = transform.rotation.eulerAngles.y;
        new_rotation = current_rotation + input_value;
        lerp_value_1 = new_rotation;
        lerp_value_2 = Mathf.RoundToInt(current_rotation / snap_increment) * snap_increment;
        lerp_time = 0f;
        transform.rotation = Quaternion.Euler(0f, new_rotation, 0f);
    }

    private void Start()
    {
        mouse_point = GameObject.Find("MouseRot").GetComponent<MousePoint>();
        player_transform = GameObject.Find("Player").transform;
        player_movement = player_transform.GetComponent<PlayerMovement>();
        transform.position = player_transform.position;
    }

    /*
    /// <summary>
    /// Rotate with input binary.
    /// </summary>
    private void Update()
    {
        bool left_down = Input.GetKeyDown(PlayerInput.left_rotation);
        bool right_down = Input.GetKeyDown(PlayerInput.right_rotation);
        if (left_down)
        {
            lerp_time = 0f;
            lerp_value_1 = new_rotation;
            lerp_value_2 += snap_increment;
        }
        if (right_down)
        {
            lerp_time = 0f;
            lerp_value_1 = new_rotation;
            lerp_value_2 -= snap_increment;
        }
    }

    /// <summary>
    /// Rotate with input binary;
    /// </summary>
    private void FixedUpdate()
    {
        ClosestRotationSnap();
        smoothed_position = SmoothMovementToPoint(GetLookPoint());
        transform.position = smoothed_position;
    }
    */

    /// <summary>
    /// Rotate with input float.
    /// </summary>
    private void Update()
    {
        bool left = Input.GetKey(PlayerInput.left_rotation);
        bool right = Input.GetKey(PlayerInput.right_rotation);

        if (left && right || !left && !right)
        {
            rotation_input = 0;
        }
        else if (left)
        {
            rotation_input = 1;
        }
        else
        {
            rotation_input = -1;
        }
    }
    
    /// <summary>
    /// Rotate With input float
    /// </summary>
    private void FixedUpdate()
    {
        if (rotation_input != 0)
        {
            RotateWithInputFloat();
        }
        else
        {
            ClosestRotationSnap();
        }
        smoothed_position = SmoothMovementToPoint(GetLookPoint());
        transform.position = smoothed_position;
    }
}
