using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Holds all universal player inputs
/// </summary>
public class PlayerInput
{
    public static KeyCode sprint_key { get; set; } = KeyCode.LeftShift;

    public static KeyCode crouch_key { get; set; } = KeyCode.LeftControl;

    public static KeyCode interact_key { get; set; } = KeyCode.F;

    public static KeyCode use_weapon { get; set; } = KeyCode.Mouse0;

    public static KeyCode use_ability { get; set; } = KeyCode.Mouse1;

    public static KeyCode use_ultimate { get; set; } = KeyCode.R;

    public static float horizontal { get { return GameTime.is_paused ? 0f : Input.GetAxisRaw("Horizontal"); } }

    public static float vertical { get { return GameTime.is_paused ? 0f : Input.GetAxisRaw("Vertical"); } }

    public static KeyCode left_rotation { get; set; } = KeyCode.Q;

    public static KeyCode right_rotation { get; set; } = KeyCode.E;

    public static float mouse_scroll_value { get { return GameTime.is_paused ? 0f : Input.GetAxis("Mouse ScrollWheel") * 1f; } }

    public static bool GetKeyDown(KeyCode key)
    {
        return !GameTime.is_paused && Input.GetKeyDown(key);
    }
    public static bool GetKeyUp(KeyCode key)
    {
        return !GameTime.is_paused && Input.GetKeyUp(key);
    }
    public static bool GetKey(KeyCode key)
    {
        return !GameTime.is_paused && Input.GetKey(key);
    }
}
