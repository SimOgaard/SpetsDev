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
    [SerializeField] private AnimationCurve rotation_curve;
    [SerializeField] private float y_axis_rotation;
    [SerializeField] private float x_axis_time;
    [SerializeField] private float snap_increment;
    [SerializeField] [Range(0.5f, 1f)] private float reccursive_rotation_threshold;

    [SerializeField] private float last_stopped_rotation;
    [SerializeField] private float current_rotation;
    [SerializeField] private float wanted_rotation;

    [SerializeField] private AnimationCurve test_curve;
    
    private void Start()
    {
        mouse_point = GameObject.Find("MouseRot").GetComponent<MousePoint>();
        player_transform = GameObject.Find("Player").transform;
        player_movement = player_transform.GetComponent<PlayerMovement>();
        transform.position = player_transform.position;

        //StartCoroutine(RotateCameraAfterCurve(test_curve));
    }

    /// <summary>
    /// Calculates difference between actual player position and camera position, where player is moving twords and where mouse is pointing on a 2d plane and in 3d world and finally returns sum of all positions multiplied by their amplification.
    /// </summary>
    private Vector3 GetLookPoint()
    {
        Vector3 diff = transform.position - player_transform.position;
        Vector3 player_position_diff = (diff) * player_position_diff_amplitude;
        //Vector3 player_heading = Vector3.Scale(player_movement.controller.velocity, new Vector3(player_heading_xz_amplitude, player_heading_y_amplitude, player_heading_xz_amplitude));
        //Vector3 player_heading = Vector3.Scale(diff, new Vector3(-player_heading_xz_amplitude, -player_heading_y_amplitude, -player_heading_xz_amplitude));
        Vector3 player_heading = Vector3.Scale(PlayerMovement.movement, new Vector3(player_heading_xz_amplitude, player_heading_y_amplitude, player_heading_xz_amplitude));
        Vector3 player_looking_plane = (mouse_point.MousePosition2D() - player_transform.position) * player_looking_plane_amplitude;
        Vector3 player_looking_3d = (mouse_point.GetWorldPoint() - player_transform.position) * player_looking_3d_amplitude;

        return Vector3.ClampMagnitude(player_position_diff + player_heading + player_looking_plane + player_looking_3d, max_distance) + new Vector3(0f, player_y_offset, 0f);
    }

    /// <summary>
    /// Liniarly interpolates camera focus point with player position.
    /// </summary>
    private Vector3 SmoothMovementToPoint(Vector3 focus_point)
    {
        Vector3 smoothed_position = Vector3.Lerp(transform.position, (player_transform.position + focus_point), smooth_speed * Time.deltaTime);
        return smoothed_position;
    }

    private float _x_value;
    private float x_value
    {
        get { return _x_value; }
        set { _x_value = Mathf.Clamp01(value); }
    }
    
    private float C = 0f;
    private float old_direction_heading = 0f;
    //private float old_direction = 0f;
    private IEnumerator RotateCamera(float direction)
    {
        /*
        if (old_direction != direction && old_direction != 0f)
        {
            if (direction < 0f)
            {
                wanted_rotation = current_rotation - current_rotation % y_axis_rotation;
            }
            else
            {
                wanted_rotation = current_rotation - current_rotation % y_axis_rotation + y_axis_rotation;
            }
        }
        else
        {
            wanted_rotation += direction * y_axis_rotation;
        }
        old_direction = direction;
        */
        wanted_rotation += direction * y_axis_rotation;

        float direction_heading = current_rotation < wanted_rotation ? 1f : -1f;
        if (old_direction_heading != direction_heading && old_direction_heading != 0f)
        {
            last_stopped_rotation = current_rotation - (current_rotation % y_axis_rotation) - direction_heading * y_axis_rotation;
        }
        old_direction_heading = direction_heading;

        float A = direction_heading * y_axis_rotation * 0.5f + last_stopped_rotation;
        float B = wanted_rotation - direction_heading * y_axis_rotation * 0.5f;

        float h = 0.001f;
        float rate_of_change_middle = ((rotation_curve.Evaluate(0.5f + h) - rotation_curve.Evaluate(0.5f - h)) / (h * 2f * x_axis_time)) * y_axis_rotation;

        WaitForEndOfFrame wait = new WaitForEndOfFrame();
        while (wanted_rotation != current_rotation)
        {
            if (C < A && direction_heading > 0f || C > A && direction_heading < 0f)
            {
                C += direction_heading * ((Time.deltaTime / x_axis_time) * y_axis_rotation);
                float x_time = direction_heading * (C - last_stopped_rotation) / y_axis_rotation;
                float curve_rotation_value = rotation_curve.Evaluate(x_time);
                current_rotation = last_stopped_rotation + direction_heading * curve_rotation_value * y_axis_rotation;
            }
            else if (C > B && direction_heading > 0f || C < B && direction_heading < 0f)
            {
                float b = wanted_rotation - direction_heading * y_axis_rotation;
                C += direction_heading * ((Time.deltaTime / x_axis_time) * y_axis_rotation);
                float x_time = (direction_heading * (C - B) / y_axis_rotation) + 0.5f;
                float curve_rotation_value = rotation_curve.Evaluate(x_time);

                bool left = Input.GetKey(PlayerInput.left_rotation);
                bool right = Input.GetKey(PlayerInput.right_rotation);
                if (left && !right)
                {
                    current_rotation += direction_heading * rate_of_change_middle * Time.deltaTime;
                    C = current_rotation;
                    transform.rotation = Quaternion.Euler(0f, current_rotation, 0f);
                    
                    if (x_time > reccursive_rotation_threshold)
                    {
                        StartCoroutine(RotateCamera(1));
                        yield break;
                    }
                    yield return wait;
                    continue;
                }
                if (right && !left)
                {
                    current_rotation += direction_heading * rate_of_change_middle * Time.deltaTime;
                    C = current_rotation;
                    transform.rotation = Quaternion.Euler(0f, current_rotation, 0f);

                    if (x_time > reccursive_rotation_threshold)
                    {
                        StartCoroutine(RotateCamera(-1));
                        yield break;
                    }
                    yield return wait;
                    continue;
                }

                current_rotation = b + direction_heading * curve_rotation_value * y_axis_rotation;
            }
            else
            {
                current_rotation += direction_heading * rate_of_change_middle * Time.deltaTime;
                C = current_rotation;
            }

            transform.rotation = Quaternion.Euler(0f, current_rotation, 0f);
            yield return wait;
        }
        last_stopped_rotation = current_rotation;
        C = current_rotation;
    }

    private IEnumerator RotateCameraAfterCurve(AnimationCurve curve)
    {
        WaitForEndOfFrame wait = new WaitForEndOfFrame();

        float old_max_distance = max_distance;
        max_distance = 0f;

        float curve_time_end = curve[curve.length - 1].time;
        float curve_time_current = 0f;

        while (curve_time_current < curve_time_end)
        {
            curve_time_current += Time.deltaTime;
            Debug.Log(curve_time_current);
            Debug.Log(curve.Evaluate(curve_time_current));
            transform.rotation = Quaternion.Euler(0f, curve.Evaluate(curve_time_current), 0f);
            yield return wait;
        }
        max_distance = old_max_distance;
    }

    private IEnumerator RotateCameraAfterCurveContinue(AnimationCurve curve)
    {
        WaitForEndOfFrame wait = new WaitForEndOfFrame();

        float curve_time_end = curve[curve.length - 1].time;
        float curve_time_current = 0f;

        float current_rotation = transform.rotation.eulerAngles.y;

        while (curve_time_current < curve_time_end)
        {
            yield return wait;
            curve_time_current += Time.deltaTime;
            current_rotation += curve.Evaluate(curve_time_current);
            transform.rotation = Quaternion.Euler(0f, current_rotation, 0f);
        }
    }

    /// <summary>
    /// Rotate with input float.
    /// </summary>
    private void Update()
    {
        if(Input.GetKeyDown(PlayerInput.left_rotation) || (Input.GetKeyUp(PlayerInput.right_rotation) && Input.GetKey(PlayerInput.left_rotation)))
        {
            StopAllCoroutines();
            StartCoroutine(RotateCamera(1));
        }
        else if (Input.GetKeyDown(PlayerInput.right_rotation) || (Input.GetKeyUp(PlayerInput.left_rotation) && Input.GetKey(PlayerInput.right_rotation)))
        {
            StopAllCoroutines();
            StartCoroutine(RotateCamera(-1));
        }
    }

    public void SmoothPosition()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            return;
        }
#endif
        smoothed_position = SmoothMovementToPoint(GetLookPoint());
        transform.position = smoothed_position;
    }
}
