using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    public KeyCode sprint_key
    {
        get { return KeyCode.LeftShift; }
    }

    public KeyCode interact_key
    {
        get { return KeyCode.E; }
    }

    public KeyCode use_weapon
    {
        get { return KeyCode.Mouse0; }
    }

    public KeyCode use_ability
    {
        get { return KeyCode.Mouse1; }
    }

    public KeyCode use_ultimate
    {
        get { return KeyCode.Q; }
    }
}
