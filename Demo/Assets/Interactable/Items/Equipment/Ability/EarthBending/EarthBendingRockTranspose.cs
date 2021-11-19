using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EarthBendingRockTranspose : EarthBendingRock
{
    
    public Vector3 ground_point;        // position you should move twords in the beginning, then sleep for some time.
    public Vector3 under_ground_point;  // position you should spawn object in and move twords in the end then delete.
    /*
    /// <summary>
    /// Changes the rock this script controls.
    /// </summary>
    public void InitEarthbendingPillar(float height, float width, Quaternion rotation, float sleep_time, float growth_speed)
    {
        ChangePillar(height, width, rotation, sleep_time, growth_speed);
    }

    /// <summary>
    /// Changes scale, rotation, move speed and sleep time of this rock.
    /// </summary>
    public void ChangePillar(float height, float width, Quaternion rotation, float sleep_time, float growth_speed)
    {
        transform.localScale = new Vector3(width, height * 2f, width);
        transform.rotation = rotation;
        this.sleep_time = sleep_time;
        current_sleep_time = sleep_time;
        this.growth_speed = growth_speed;
    }
    */

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
                rock_rigidbody.MovePosition(Vector3.Lerp(under_ground_point, ground_point, growth_time));
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
                rock_rigidbody.MovePosition(Vector3.Lerp(under_ground_point, ground_point, growth_time));
                break;
        }
    }

}
