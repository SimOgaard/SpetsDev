using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Holds all universal player inputs
/// </summary>
public class PlayerInput : MonoBehaviour
{
    public KeyCode sprint_key { get; set; } = KeyCode.LeftShift;

    public KeyCode interact_key { get; set; } = KeyCode.E;

    public KeyCode use_weapon { get; set; } = KeyCode.Mouse0;

    public KeyCode use_ability { get; set; } = KeyCode.Mouse1;

    public KeyCode use_ultimate { get; set; } = KeyCode.Q;
}
