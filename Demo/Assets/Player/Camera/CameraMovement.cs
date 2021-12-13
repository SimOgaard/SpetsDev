using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Moves point that camera is focused on.
/// </summary>
public class CameraMovement : MonoBehaviour
{
    private Rigidbody player_rigidbody;
    private Vector3 smoothed_position;

    [Header("Focus Point Position")]
    [SerializeField] private float max_distance;
    [SerializeField] private float smooth_speed;
    [SerializeField] [Range(0f, 1f)] private float player_position_diff_amplitude;
    [SerializeField] private float player_heading_xz_amplitude;
    [SerializeField] private float player_heading_y_amplitude;
    [SerializeField] [Range(0f, 0.2f)] private float player_looking_plane_amplitude;
    [SerializeField] private Vector2 player_looking_plane_amplitude_xy;
    [SerializeField] [Range(0f, 0.2f)] private float player_looking_3d_amplitude;
    [SerializeField] private float player_y_offset;

    [Header("Focus Point Rotation")]
    [SerializeField] private AnimationCurve rotation_curve;
    [SerializeField] private float y_axis_rotation;
    [SerializeField] private float x_axis_time;
    [SerializeField] private float snap_increment;
    //[SerializeField] [Range(0.5f, 1f)] private float reccursive_rotation_threshold;
    //[SerializeField] private float last_stopped_rotation;
    
    private void Start()
    {
        player_rigidbody = Global.player_transform.GetComponent<PlayerMovement>()._rigidbody;
        transform.position = Global.player_transform.position;
    }

    /// <summary>
    /// Calculates difference between actual player position and camera position, where player is moving twords and where mouse is pointing on a 2d plane and in 3d world and finally returns sum of all positions multiplied by their amplification.
    /// </summary>
    private Vector3 GetLookPoint()
    {
        Vector3 diff = transform.position - Global.player_transform.position;
        Vector3 player_position_diff = (diff) * player_position_diff_amplitude;
        //Vector3 player_heading = Vector3.Scale(player_movement.controller.velocity, new Vector3(player_heading_xz_amplitude, player_heading_y_amplitude, player_heading_xz_amplitude));
        //Vector3 player_heading = Vector3.Scale(diff, new Vector3(-player_heading_xz_amplitude, -player_heading_y_amplitude, -player_heading_xz_amplitude));
        Vector3 player_heading = Vector3.Scale(player_rigidbody.velocity, new Vector3(player_heading_xz_amplitude, player_heading_y_amplitude, player_heading_xz_amplitude));
        Vector3 player_looking_plane = (MousePoint.MousePositionPlayerPlane(player_looking_plane_amplitude_xy.x, player_looking_plane_amplitude_xy.y) - Global.player_transform.position) * player_looking_plane_amplitude;
        Vector3 player_looking_3d = (MousePoint.MousePositionWorld() - Global.player_transform.position) * player_looking_3d_amplitude;

        Vector3 clamped_look_point = Vector3.ClampMagnitude(player_position_diff + player_heading + player_looking_plane + player_looking_3d, max_distance);
        Vector3 scaled_forward = Vector3.Scale(clamped_look_point, Vector3.one + transform.forward * (1f - player_looking_plane_amplitude_xy.y));
        Vector3 scaled_sideways = Vector3.Scale(clamped_look_point, Vector3.one + transform.right * (1f - player_looking_plane_amplitude_xy.x));

        return scaled_sideways + new Vector3(0f, player_y_offset, 0f);
    }

    /// <summary>
    /// Liniarly interpolates camera focus point with player position.
    /// </summary>
    private Vector3 SmoothMovementToPoint(Vector3 focus_point)
    {
        Vector3 smoothed_position = Vector3.Lerp(transform.position, focus_point, smooth_speed * Time.deltaTime);
        return smoothed_position;
    }

    [SerializeField] private float current_rotation;
    [SerializeField] private float wanted_rotation;
    private float disjointment;
    private float offset;
    private float f(float x)
    {
        return (x - y_axis_rotation * (wanted_rotation + 0.5f)) / y_axis_rotation;
    }
    private float SampleRotationCurve(float x, float y_offset = 0.5f)
    {
        return (Mathf.Abs(x - offset) - Mathf.Abs(x + offset) * 0.5f) + x + y_offset;
    }
    private float SampleDerivative(float x)
    {
        const float h = 0.001f;
        float rotation_curve_value_1 = SampleRotationCurve(x + h);
        float rotation_curve_value_2 = SampleRotationCurve(x - h);

        float derivative = (rotation_curve_value_1 - rotation_curve_value_2) / (h * 2f);
        return derivative;
    }

    /// <summary>
    /// Rotate with input float.
    /// </summary>
    private void Update()
    {
        /*
        float derivative = SampleDerivative(current_rotation);
        Debug.Log(derivative);            
        return;
        */
        if (PlayerInput.GetKeyDown(PlayerInput.left_rotation) || (PlayerInput.GetKeyUp(PlayerInput.right_rotation) && PlayerInput.GetKey(PlayerInput.left_rotation)))
        {
            //wanted_rotation += y_axis_rotation;
            StopAllCoroutines();
            StartCoroutine(RotateCamera(1));
        }
        else if (PlayerInput.GetKeyDown(PlayerInput.right_rotation) || (PlayerInput.GetKeyUp(PlayerInput.left_rotation) && PlayerInput.GetKey(PlayerInput.right_rotation)))
        {
            //wanted_rotation -= y_axis_rotation;
            StopAllCoroutines();
            StartCoroutine(RotateCamera(-1));
        }
    }

    [Header("ttesting")]
    [SerializeField] private float oval_size_large = 1f;
    [SerializeField] private float oval_size_small = 1f;
    [SerializeField] private bool move = false;
    public void SmoothPosition()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            return;
        }
#endif
        
        if (false)
        {
            // find mid pixel of player
            Vector2 player_middle = MousePoint.WorldToViewportPoint(Global.player_transform.position) - new Vector3(0.5f, 0.5f, 0f);

            Debug.Log(player_middle);

            // check if it is outside of squarcircle
            float squarcircle_value = Mathf.Pow(player_middle.x, 4) + Mathf.Pow(player_middle.y, 4);
            if (squarcircle_value > Mathf.Pow(oval_size_large, 4))
            {
                move = true;
            }
            else if (squarcircle_value < Mathf.Pow(oval_size_small, 4))
            {
                move = false;
            }

            if (move)
            {
                smoothed_position = SmoothMovementToPoint(Global.player_transform.position);
                transform.position = smoothed_position;
            }

            // if it is

            // find mid pixel of player
            // check if it is inside of smaller oval
            // if it is
            // stop moving camera
        }
        else
        {
            smoothed_position = SmoothMovementToPoint(Global.player_transform.position + GetLookPoint());
            transform.position = smoothed_position;
        }

        //smoothed_position = SmoothMovementToPoint(player_transform.position + GetLookPoint());
        //Vector3 screenPos = cam.WorldToScreenPoint(target.position);
    }

    [SerializeField] private float reccursive_rotation_threshold = 0.5f;
    [SerializeField] private float last_stopped_rotation;
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

                bool left = PlayerInput.GetKey(PlayerInput.left_rotation);
                bool right = PlayerInput.GetKey(PlayerInput.right_rotation);
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
}
