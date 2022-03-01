using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Holds all universal player inputs
/// </summary>
public class PlayerInput
{
    public static KeyCode sprintKey { get; set; } = KeyCode.LeftShift;

    public static KeyCode crouchKey { get; set; } = KeyCode.LeftControl;

    public static KeyCode interactKey { get; set; } = KeyCode.F;

    public static KeyCode useWeapon { get; set; } = KeyCode.Mouse0;

    public static KeyCode useAbility { get; set; } = KeyCode.Mouse1;

    public static KeyCode useUltimate { get; set; } = KeyCode.R;

    public static float horizontal { get { return GameTime.isPaused ? 0f : Input.GetAxisRaw("Horizontal"); } }

    public static float vertical { get { return GameTime.isPaused ? 0f : Input.GetAxisRaw("Vertical"); } }

    public static KeyCode leftRotation { get; set; } = KeyCode.Q;

    public static KeyCode rightRotation { get; set; } = KeyCode.E;

    public static float mouseScrollValue { get { return GameTime.isPaused ? 0f : Input.GetAxis("Mouse ScrollWheel") * 1f; } }

    public static bool GetKeyDown(KeyCode key)
    {
        return !GameTime.isPaused && Input.GetKeyDown(key);
    }
    public static bool GetKeyUp(KeyCode key)
    {
        return !GameTime.isPaused && Input.GetKeyUp(key);
    }
    public static bool GetKey(KeyCode key)
    {
        return !GameTime.isPaused && Input.GetKey(key);
    }
}
