using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField]
    private Transform player_transform;
    [SerializeField]
    private float smooth_speed;
    private Vector3 smoothed_position;

    private Vector3 SmoothMovementToPoint(Vector3 focus_point)
    {
        Vector3 smoothed_position = Vector3.Lerp(transform.position, focus_point, smooth_speed * Time.fixedDeltaTime);
        return smoothed_position;
    }

    private void Start()
    {
        transform.position = player_transform.position;
    }

    void FixedUpdate()
    {
        smoothed_position = SmoothMovementToPoint(player_transform.position);
        transform.position = smoothed_position;
    }
}
