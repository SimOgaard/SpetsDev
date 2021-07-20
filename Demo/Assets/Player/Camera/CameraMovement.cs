using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    private MousePoint mouse_point;
    private Transform player_transform;
    private PlayerMovement player_movement;

    [SerializeField]
    private float smooth_speed;
    [SerializeField]
    [Range(0f, 1f)]
    private float player_position_diff_amplitude;
    [SerializeField]
    private float player_heading_xz_amplitude;
    [SerializeField]
    private float player_heading_y_amplitude;
    [SerializeField]
    [Range(0f, 0.2f)]
    private float player_looking_plane_amplitude;
    [SerializeField]
    [Range(0f, 0.2f)]
    private float player_looking_3d_amplitude;

    private Vector3 smoothed_position;

    private Vector3 GetLookPoint()
    {
        Vector3 player_position_diff = (transform.position - player_transform.position) * player_position_diff_amplitude;
        Vector3 player_heading = new Vector3(player_movement.controller.velocity.x * player_heading_xz_amplitude, player_movement.controller.velocity.y * player_heading_y_amplitude, player_movement.controller.velocity.z * player_heading_xz_amplitude);
        Vector3 player_looking_plane = (mouse_point.GetTargetHitPoint() - player_transform.position) * player_looking_plane_amplitude;
        Vector3 player_looking_3d = (mouse_point.GetTargetMousePos() - player_transform.position) * player_looking_3d_amplitude;

        return player_position_diff + player_heading + player_looking_plane + player_looking_3d;
    }

    private Vector3 SmoothMovementToPoint(Vector3 focus_point)
    {
        Vector3 smoothed_position = Vector3.Lerp(transform.position, (player_transform.position + focus_point), smooth_speed * Time.fixedDeltaTime);
        return smoothed_position;
    }

    private void Start()
    {
        mouse_point = GameObject.Find("MouseRot").GetComponent<MousePoint>();
        player_transform = GameObject.Find("Player").transform;
        player_movement = player_transform.GetComponent<PlayerMovement>();
        transform.position = player_transform.position;
    }

    private void FixedUpdate()
    {
        smoothed_position = SmoothMovementToPoint(GetLookPoint());
        transform.position = smoothed_position;
    }
}
