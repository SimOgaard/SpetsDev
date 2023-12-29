using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Parrent controller of player.
/// </summary>
public class PlayerController : MonoBehaviour
{
    public enum Status { idle, walking, sprinting, sliding, usingWeapon, usingAbility, usingUltimate }
    public Status status;

    /// <summary>
    /// Changes current status of player.
    /// </summary>
    public void ChangeStatus(Status s)
    {
        if (status == s)
        {
            return;
        }
        status = s;
    }
}
