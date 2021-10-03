using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Holds all universal player inputs
/// </summary>
public class PlayerInput : MonoBehaviour
{
    public static KeyCode sprint_key { get; set; } = KeyCode.LeftShift;

    public static KeyCode interact_key { get; set; } = KeyCode.F;

    public static KeyCode use_weapon { get; set; } = KeyCode.Mouse0;

    public static KeyCode use_ability { get; set; } = KeyCode.Mouse1;

    public static KeyCode use_ultimate { get; set; } = KeyCode.R;

    public static float horizontal { get { return Input.GetAxisRaw("Horizontal"); } }

    public static float vertical { get { return Input.GetAxisRaw("Vertical"); } }

    public static KeyCode left_rotation { get; set; } = KeyCode.Q;

    public static KeyCode right_rotation { get; set; } = KeyCode.E;

    public static float mouse_scroll_value { get { return Input.mouseScrollDelta.y; } }
}
