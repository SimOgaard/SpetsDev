using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EarthBendingRockScale : EarthBendingRock
{
    private const float start_scale = 0.01f;
    public float end_scale;

    /// <summary>
    /// Places rock at ground given a point.
    /// </summary>
    public override void PlacePillar(Vector3 point, Quaternion rotation, Vector3 scale)
    {
        transform.rotation = rotation;
        end_scale = scale.y;
        scale.y = start_scale;
        transform.localScale = scale;
        (Vector3 place_point, Vector3 normal) = GetRayHitData(point, rotation, scale, out hit_data);
        transform.position = place_point;
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Controlls all three stages a rock goes through.
    /// Moving up and down and standing still. States can be changed outside of script.
    /// </summary>
    private void FixedUpdate()
    {
        float move_diff;
        float sound;
        switch (move_state)
        {
            case MoveStates.up:
                if (growth_time == 1f)
                {
                    current_sleep_time = sleep_time;
                    rock_rigidbody.Sleep();
                    move_state = MoveStates.still;
                    break;
                }
                move_diff = growth_speed * Time.fixedDeltaTime;
                sound = Mathf.Min(growth_speed * sound_amplifier, max_sound);
                Enemies.Sound(transform, sound, Time.fixedDeltaTime);
                growth_time += move_diff;
                transform.localScale = new Vector3(transform.localScale.x, Mathf.Lerp(start_scale, end_scale, growth_time), transform.localScale.z);
                break;
            case MoveStates.still:
                current_sleep_time -= Time.deltaTime;
                if (current_sleep_time < 0f)
                {
                    rock_rigidbody.WakeUp();
                    move_state = MoveStates.down;
                    break;
                }
                break;
            case MoveStates.down:
                if (growth_time == 0f)
                {
                    move_state = MoveStates.up;
                    if (should_be_deleted)
                    {
                        Destroy(gameObject);
                    }
                    else
                    {
                        rock_rigidbody.Sleep();
                        gameObject.SetActive(false);
                    }
                    break;
                }
                move_diff = growth_speed * Time.fixedDeltaTime;
                sound = Mathf.Min(growth_speed * sound_amplifier, max_sound);
                Enemies.Sound(transform, sound, Time.fixedDeltaTime);
                growth_time -= move_diff;
                transform.localScale = new Vector3(transform.localScale.x, Mathf.Lerp(start_scale, end_scale, growth_time), transform.localScale.z);
                break;
        }
    }

}
